using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

namespace Xui.Middleware.DevTools.Client;

/// <summary>Startup settings and portable-PDB symbolization for exact, scoped allocation snapshots.</summary>
public static class AllocationStacks
{
    public const string ProfilerId = "{59A7E9F2-76C5-4D77-9146-8D19EFBA3572}";

    public static IReadOnlyDictionary<string, string?> EnvironmentFor(string nativeLibrary) => new Dictionary<string, string?>
    {
        ["CORECLR_ENABLE_PROFILING"] = "1",
        ["CORECLR_PROFILER"] = ProfilerId,
        ["CORECLR_PROFILER_PATH"] = Path.GetFullPath(nativeLibrary),
        ["CORECLR_PROFILER_PATH_64"] = Path.GetFullPath(nativeLibrary),
        ["XUI_ALLOCATION_PROFILER"] = Path.GetFullPath(nativeLibrary),
        // Keep native-to-IL maps stable during the capture and JIT allocation helpers
        // with the profiler enabled. This mode is diagnostic, not a timing benchmark.
        ["DOTNET_ReadyToRun"] = "0",
        ["DOTNET_TieredCompilation"] = "0",
    };

    /// <summary>Returns empty text only when an enabled collector observed no allocations or capture errors.</summary>
    public static string Format(JsonElement profile)
    {
        if (!profile.TryGetProperty("allocationTrace", out var trace) || trace.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException("Allocation stack capture is not enabled for this app.");
        using var document = JsonDocument.Parse(trace.GetString()!);
        var root = document.RootElement;
        var modules = new Dictionary<string, ModuleSymbols?>(StringComparer.Ordinal);
        try
        {
            ModuleSymbols? Module(string path)
            {
                if (modules.TryGetValue(path, out var cached)) return cached;
                try { cached = string.IsNullOrEmpty(path) ? null : new ModuleSymbols(path); }
                catch (Exception ex) when (ex is IOException or BadImageFormatException or UnauthorizedAccessException) { cached = null; }
                modules[path] = cached;
                return cached;
            }
            var groups = new SortedDictionary<string, (long Count, long Bytes)>(StringComparer.Ordinal);
            foreach (var allocation in root.GetProperty("allocations").EnumerateArray())
            {
                var typeModule = allocation.GetProperty("typeModule").GetString()!;
                var token = allocation.GetProperty("typeToken").GetInt32();
                var rank = allocation.GetProperty("rank").GetInt32();
                var element = allocation.GetProperty("elementType").GetInt32();
                var type = Module(typeModule)?.TypeName(token) ?? PrimitiveName(element) ?? "<unresolved type>";
                if (rank > 0) type += "[" + new string(',', rank - 1) + "]";
                var phase = allocation.GetProperty("phase").GetInt32() switch { 1 => "input", 2 => "animation", 3 => "render", _ => "unknown" };
                var text = new StringBuilder().Append(phase).Append(": ").Append(type).Append('\n');
                var status = allocation.GetProperty("stackStatus").GetInt32();
                if (status < 0) text.Append("  <capture error 0x").Append(status.ToString("X8")).Append(">\n");
                var frames = allocation.GetProperty("frames");
                if (frames.GetArrayLength() == 0) text.Append("  <stack unavailable>\n");
                foreach (var frame in frames.EnumerateArray())
                {
                    var path = frame.GetProperty("module").GetString()!;
                    var method = frame.GetProperty("token").GetInt32();
                    var offset = frame.GetProperty("ilOffset").GetInt32();
                    text.Append("  ").Append(Module(path)?.Frame(method, offset) ?? $"<unresolved method 0x{method:X8} in {Path.GetFileName(path)}>").Append('\n');
                }
                if (allocation.GetProperty("truncated").GetBoolean()) text.Append("  <stack truncated>\n");
                var key = text.ToString();
                groups.TryGetValue(key, out var total);
                groups[key] = (total.Count + 1, total.Bytes + allocation.GetProperty("bytes").GetInt64());
            }
            var report = new StringBuilder();
            var dropped = root.GetProperty("dropped").GetInt32();
            if (dropped != 0) report.Append("CAPTURE INCOMPLETE: ").Append(dropped).Append(" allocations dropped.\n\n");
            foreach (var (stack, total) in groups)
            {
                report.Append(total.Count).Append(" allocation(s), ").Append(total.Bytes).Append(" bytes\n");
                report.Append(stack).Append('\n');
            }
            return report.ToString();
        }
        finally { foreach (var module in modules.Values) module?.Dispose(); }
    }

    private static string? PrimitiveName(int type) => type switch
    {
        2 => "System.Boolean", 3 => "System.Char", 4 => "System.SByte", 5 => "System.Byte",
        6 => "System.Int16", 7 => "System.UInt16", 8 => "System.Int32", 9 => "System.UInt32",
        10 => "System.Int64", 11 => "System.UInt64", 12 => "System.Single", 13 => "System.Double",
        14 => "System.String", 24 => "System.IntPtr", 25 => "System.UIntPtr", 28 => "System.Object", _ => null,
    };

    private sealed class ModuleSymbols : IDisposable
    {
        private readonly PEReader pe;
        private readonly MetadataReader metadata;
        private readonly MetadataReaderProvider? pdbProvider;
        private readonly MetadataReader? pdb;

        public ModuleSymbols(string path)
        {
            pe = new PEReader(File.OpenRead(path));
            try
            {
                metadata = pe.GetMetadataReader();
                // Supports adjacent portable PDBs and embedded portable PDBs, with
                // identity verification performed by PEReader before accepting them.
                if (pe.TryOpenAssociatedPortablePdb(path, file => File.Exists(file) ? File.OpenRead(file) : null,
                    out var provider, out _) && provider is not null)
                {
                    pdbProvider = provider;
                    pdb = provider.GetMetadataReader();
                }
            }
            catch { pe.Dispose(); throw; }
        }

        public string? TypeName(int token)
        {
            if ((token & unchecked((int)0xff000000)) != 0x02000000) return null;
            try { return TypeName(MetadataTokens.TypeDefinitionHandle(token & 0x00ffffff)); }
            catch (BadImageFormatException) { return null; }
        }

        private string TypeName(TypeDefinitionHandle handle)
        {
            var type = metadata.GetTypeDefinition(handle);
            var name = metadata.GetString(type.Name);
            if (!type.GetDeclaringType().IsNil) return TypeName(type.GetDeclaringType()) + "+" + name;
            var ns = metadata.GetString(type.Namespace);
            return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
        }

        public string Frame(int token, int offset)
        {
            if ((token & unchecked((int)0xff000000)) != 0x06000000) return $"<unresolved method 0x{token:X8}>";
            var handle = MetadataTokens.MethodDefinitionHandle(token & 0x00ffffff);
            var method = metadata.GetMethodDefinition(handle);
            var name = TypeName(method.GetDeclaringType()) + "." + metadata.GetString(method.Name);
            if (pdb is not null && offset >= 0)
            {
                var debug = pdb.GetMethodDebugInformation(handle);
                SequencePoint? selected = null;
                foreach (var point in debug.GetSequencePoints())
                {
                    if (point.Offset > offset) break;
                    if (!point.IsHidden) selected = point;
                }
                if (selected is { } location)
                {
                    var document = location.Document.IsNil ? debug.Document : location.Document;
                    if (!document.IsNil)
                    {
                        var file = pdb.GetString(pdb.GetDocument(document).Name);
                        return $"{file}({location.StartLine},{location.StartColumn}): {name} [IL_{offset:X4}]";
                    }
                }
            }
            return $"{name} [source unavailable; IL_{offset:X4}]";
        }

        public void Dispose() { pdbProvider?.Dispose(); pe.Dispose(); }
    }
}
