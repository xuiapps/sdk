using System.Buffers.Binary;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Xui.Compiler.Tests.Wasm.TestApp;

/// <summary>Injects packaged image metadata for Node and embedded, predecoded assets for standalone browser pages.</summary>
internal static class BrowserAssets
{
    public static string Script(bool embed, params string[] files)
    {
        var catalogue = files.ToDictionary(file => "asset://" + file, file => {
            var bytes = File.ReadAllBytes(Path.Combine(Frontend.SourceProject.XuiRoot, "Apps/TestApp/Assets", file));
            return new {
                width = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4)),
                height = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4)),
                data = embed ? "data:image/png;base64," + Convert.ToBase64String(bytes) : null
            };
        });
        string json = JsonSerializer.Serialize(catalogue, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        return "globalThis.xuiImageAssets = " + json + ";\n" + (embed ? """
            globalThis.xuiPrepareAssets = async () => {
                await Promise.all(Object.values(xuiImageAssets).map(async asset => {
                    const image = new Image(); image.src = asset.data; await image.decode();
                    if(image.naturalWidth !== asset.width || image.naturalHeight !== asset.height) throw new Error('Decoded asset size differs from its catalogue');
                    asset.decoded = image;
                }));
            };
            """ : "");
    }
}
