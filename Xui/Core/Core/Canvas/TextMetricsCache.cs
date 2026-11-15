namespace Xui.Core.Canvas;

/// <summary>
/// Bounded LRU of string measurements. Text keys use object identity, so stable
/// labels never need character hashing or comparison. All storage is allocated
/// once; misses and eviction do not allocate cache entries. Each measurement
/// context owns its cache; clear it if native font resolution changes.
/// </summary>
public sealed class TextMetricsCache
{
    private readonly Entry[] entries;
    private int count;
    private ulong sequence;

    /// <summary>The standard maximum number of retained string measurements.</summary>
    public const int DefaultCapacity = 128;

    /// <summary>Creates a context-owned cache with fixed storage.</summary>
    public TextMetricsCache(int capacity = DefaultCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        entries = new Entry[capacity];
    }

    /// <summary>Looks up the same string instance and measurement settings, updating its recency.</summary>
    public bool TryGet(string text, in Font font, TextAlign align, TextBaseline baseline, out TextMetrics metrics)
    {
        for (int i = 0; i < count; i++)
        {
            ref var entry = ref entries[i];
            if (!entry.Matches(text, in font, align, baseline)) continue;
            entry.LastUse = ++sequence;
            metrics = entry.Metrics;
            return true;
        }
        metrics = default;
        return false;
    }

    /// <summary>Stores metrics, replacing the least recently used entry when full.</summary>
    public void Set(string text, in Font font, TextAlign align, TextBaseline baseline, in TextMetrics metrics)
    {
        int candidate = 0;
        ulong oldest = ulong.MaxValue;
        for (int i = 0; i < count; i++)
        {
            ref var entry = ref entries[i];
            if (entry.Matches(text, in font, align, baseline))
            {
                entry.Metrics = metrics;
                entry.LastUse = ++sequence;
                return;
            }
            if (entry.LastUse < oldest) { oldest = entry.LastUse; candidate = i; }
        }
        if (count < entries.Length) candidate = count++;
        entries[candidate] = new Entry
        {
            Text = text, Font = font, Align = align, Baseline = baseline,
            Metrics = metrics, LastUse = ++sequence,
        };
    }

    /// <summary>Clears measurements and releases all retained string and font references.</summary>
    public void Clear()
    {
        // Release strings and font-family references when the owning window closes.
        Array.Clear(entries);
        count = 0;
        sequence = 0;
    }

    private struct Entry
    {
        internal string? Text;
        internal Font Font;
        internal TextAlign Align;
        internal TextBaseline Baseline;
        internal TextMetrics Metrics;
        internal ulong LastUse;

        internal readonly bool Matches(string text, in Font font, TextAlign align, TextBaseline baseline) =>
            ReferenceEquals(Text, text) && Font == font && Align == align && Baseline == baseline;
    }
}
