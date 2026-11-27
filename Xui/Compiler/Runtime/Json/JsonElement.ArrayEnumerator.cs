using System.Collections;

namespace Xui.Compiler.Runtime.Json;

public readonly partial struct JsonElement
{
    /// <summary>A value cursor; enumeration copies retain the same document and independent positions.</summary>
    public struct ArrayEnumerator : IEnumerable<JsonElement>, IEnumerator<JsonElement>
    {
        private readonly JsonElement array;
        private int position;
        private readonly int length;
        internal ArrayEnumerator(JsonElement array) { this.array = array; position = -1; length = array.GetArrayLength(); }
        public JsonElement Current => position < 0 || position >= length ? default : array[position];
        object IEnumerator.Current => Current;
        public bool MoveNext() { if (position >= length) return false; position++; return position < length; }
        public void Reset() => position = -1;
        public void Dispose() => position = length;
        public ArrayEnumerator GetEnumerator() => new(array);
        IEnumerator<JsonElement> IEnumerable<JsonElement>.GetEnumerator() => new Cursor(array);
        IEnumerator IEnumerable.GetEnumerator() => new Cursor(array);

        private sealed class Cursor(JsonElement array) : IEnumerator<JsonElement>
        {
            private ArrayEnumerator iterator = new(array);
            public JsonElement Current => iterator.Current;
            object IEnumerator.Current => Current;
            public bool MoveNext() => iterator.MoveNext();
            public void Reset() => iterator.Reset();
            public void Dispose() => iterator.Dispose();
        }
    }
}
