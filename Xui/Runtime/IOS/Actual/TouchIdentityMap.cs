using System.Collections.Generic;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Stable pointer IDs for native touch handles throughout their contact lifetime.</summary>
internal sealed class TouchIdentityMap
{
    private readonly Dictionary<nint, int> contacts = new(capacity: 10);

    public bool HasContacts => contacts.Count != 0;

    public bool TryBegin(nint handle, out int id)
    {
        if (contacts.TryGetValue(handle, out id)) return false;
        id = 0;
        while (contacts.ContainsValue(id)) id++;
        contacts.Add(handle, id);
        return true;
    }

    public bool TryGet(nint handle, out int id) => contacts.TryGetValue(handle, out id);
    public void End(nint handle) => contacts.Remove(handle);
}
