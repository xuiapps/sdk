using System.Buffers.Binary;
using System.Text;
using Xui.Core.Actual;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Runtime.Android.Composition;
using Xui.Runtime.Android.Interop;
namespace Xui.Runtime.Android.Actual;

/// <summary>Synchronizes Android's InputConnection with the focused Xui editor.</summary>
internal sealed class AndroidTextInput(nint host) : ITextInputSession, IDisposable
{
    private ITextEditingClient? client;
    private readonly WireBuffer packet = new();
    private bool changing;
    public void SetClient(ITextEditingClient? value)
    {
        if(ReferenceEquals(client,value))return;
        if(client is not null){client.Editor.Changed-=Changed;client.Editor.UnmarkText();}
        client=value;
        if(client is not null)client.Editor.Changed+=Changed;
        Sync(true);
    }
    // Xui retains touch selection. This adapter owns IME editing, not native handles/loupe.
    public void CancelSelectionGesture() { }
    private void Changed(TextEditingChange _) {if(!changing)Sync(false);}
    private void Sync(bool restart)
    {
        packet.Clear();packet.Int(client is null?0:1);packet.Int(restart?1:0);
        if(client is not null)
        {
            var options=client.InputOptions;var editor=client.Editor;
            packet.Int((int)options.Keyboard);packet.Int((int)options.ContentType);packet.Int((int)options.ReturnAction);packet.Int(client.IsPassword?1:0);
            packet.Text(editor.Text);packet.Int(editor.Selection.Start);packet.Int(editor.Selection.End);
            packet.Int(editor.MarkedRange?.Start??-1);packet.Int(editor.MarkedRange?.End??-1);
        }
        AndroidBridge.Invoke(host,7,packet);
    }
    internal void Edit(ReadOnlySpan<byte> bytes)
    {
        if(client is null)return;
        if(bytes.Length<16)throw new InvalidOperationException("Invalid IME packet.");
        int op=BinaryPrimitives.ReadInt32LittleEndian(bytes),a=BinaryPrimitives.ReadInt32LittleEndian(bytes[4..]),b=BinaryPrimitives.ReadInt32LittleEndian(bytes[8..]);
        int length=BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]);
        if(length<0||length!=(bytes.Length-16)/2)throw new InvalidOperationException("Invalid IME text length.");
        var value=Encoding.Unicode.GetString(bytes[16..]);var editor=client.Editor;
        changing=true;
        try
        {
            if(op==7)client.PerformInputAction(TextInputAction.Default);
            else Apply(editor,op,a,b,value);
        }
        finally{changing=false;Sync(false);}
    }
    internal static void Apply(TextEditingState editor,int op,int a,int b,string value)
    {
            switch(op)
            {
                case 1: // Commit text; Android cursor offset is relative to the inserted range.
                    int start=(editor.MarkedRange??editor.Selection).Start;
                    if(editor.Insert(value)) {int caret=a>0?editor.Selection.End+a-1:start+a;editor.Selection=new(caret,caret);}break;
                case 2:
                    int cursor=a>0?value.Length+a-1:a;editor.SetMarkedText(value,new(cursor,cursor));break;
                case 3:editor.UnmarkText();break;
                case 4:
                    editor.UnmarkText();editor.Replace(new(Math.Max(0,editor.Selection.Start-Math.Max(0,a)),Math.Min(editor.Text.Length,editor.Selection.End+Math.Max(0,b))),"");break;
                case 5:editor.Selection=new(a,b);break;
                case 6:
                    editor.UnmarkText();var range=editor.Normalize(new(a,b));var selected=editor.Selection;
                    string marked=editor.Text[range.Start..range.End];editor.Selection=range;
                    editor.SetMarkedText(marked,new(selected.Start-range.Start,selected.End-range.Start));break;
                case 8:editor.DeleteBackward();break;
                case 9:editor.DeleteForward();break;
                default:throw new InvalidOperationException("Unknown IME operation.");
            }

    }
    public void Dispose()
    {
        if(client is not null)client.Editor.Changed-=Changed;client=null;packet.Dispose();
    }
}
