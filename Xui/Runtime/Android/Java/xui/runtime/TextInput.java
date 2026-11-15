package xui.runtime;

import android.content.Context;
import android.text.*;
import android.view.*;
import android.view.inputmethod.*;
import java.nio.*;

/** InputConnection uses Xui's document as the authority, including validation and grapheme ranges. */
final class TextInput {
    private final Host host;
    private final InputMethodManager ime;
    private final SpannableStringBuilder text=new SpannableStringBuilder();
    private ByteBuffer events=ByteBuffer.allocateDirect(256).order(ByteOrder.LITTLE_ENDIAN);
    private boolean active;
    private int generation;
    private int keyboard,content,action;
    private boolean password;
    TextInput(Host host) {this.host=host;ime=(InputMethodManager)host.getContext().getSystemService(Context.INPUT_METHOD_SERVICE);}
    boolean active(){return active;}
    void sync(ByteBuffer data) {
        active=data.getInt()!=0;boolean restart=data.getInt()!=0;
        if(active) {
            keyboard=data.getInt();content=data.getInt();action=data.getInt();password=data.getInt()!=0;
            String value=Host.string(data);int start=data.getInt(),end=data.getInt(),markedStart=data.getInt(),markedEnd=data.getInt();
            text.replace(0,text.length(),value);BaseInputConnection.removeComposingSpans(text);Selection.setSelection(text,start,end);
            if(markedStart>=0&&markedEnd>=markedStart)text.setSpan(composing,markedStart,markedEnd,Spanned.SPAN_EXCLUSIVE_EXCLUSIVE|Spanned.SPAN_COMPOSING);
            ime.updateSelection(host,start,end,markedStart,markedEnd);
        } else {text.clear();text.clearSpans();}
        if(restart){generation++;host.requestFocus();ime.restartInput(host);}
    }
    private final Object composing=new Object();
    void keyboard(boolean show) {
        if(show&&active){host.requestFocus();ime.showSoftInput(host,InputMethodManager.SHOW_IMPLICIT);}
        else ime.hideSoftInputFromWindow(host.getWindowToken(),0);
    }
    private boolean edit(int op,int a,int b,CharSequence value) {
        if(!active)return false;
        int count=value==null?0:value.length(),size=16+2*count;
        if(events.capacity()<size)events=ByteBuffer.allocateDirect(Math.max(size,events.capacity()*2)).order(ByteOrder.LITTLE_ENDIAN);
        events.clear();events.putInt(op).putInt(a).putInt(b).putInt(count);for(int i=0;i<count;i++)events.putChar(value.charAt(i));
        host.textEvent(events,events.position());return true;
    }
    boolean key(KeyEvent event) {
        if(!active)return false;
        if(event.getAction()!=KeyEvent.ACTION_DOWN)return true;
        switch(event.getKeyCode()) {
            case KeyEvent.KEYCODE_DEL:return edit(8,0,0,null);
            case KeyEvent.KEYCODE_FORWARD_DEL:return edit(9,0,0,null);
            case KeyEvent.KEYCODE_ENTER:return edit(7,0,0,null);
            default:int code=event.getUnicodeChar();return code==0?false:edit(1,1,0,new String(Character.toChars(code)));
        }
    }
    InputConnection connect(EditorInfo info) {
        if(!active)return null;
        int type=InputType.TYPE_CLASS_TEXT;
        if(keyboard==2||keyboard==3)type=InputType.TYPE_CLASS_NUMBER|(keyboard==3?InputType.TYPE_NUMBER_FLAG_DECIMAL:0);
        else if(password)type|=InputType.TYPE_TEXT_VARIATION_PASSWORD;
        else if(content==1)type|=InputType.TYPE_TEXT_VARIATION_PERSON_NAME|InputType.TYPE_TEXT_FLAG_CAP_WORDS;
        else if(keyboard==1)type|=InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD;
        info.inputType=type;info.imeOptions=EditorInfo.IME_FLAG_NO_EXTRACT_UI|(action==2?EditorInfo.IME_ACTION_NEXT:action==1?EditorInfo.IME_ACTION_PREVIOUS:EditorInfo.IME_ACTION_DONE);
        if(keyboard==1)info.imeOptions|=EditorInfo.IME_FLAG_FORCE_ASCII;
        info.initialSelStart=Selection.getSelectionStart(text);info.initialSelEnd=Selection.getSelectionEnd(text);
        final int session=generation;
        return new BaseInputConnection(host,true) {
            private boolean edit(int op,int a,int b,CharSequence value) {return session==generation&&TextInput.this.edit(op,a,b,value);}
            private final Editable disconnected=new SpannableStringBuilder();
            @Override public Editable getEditable(){return session==generation?text:disconnected;}
            @Override public boolean commitText(CharSequence value,int cursor){return edit(1,cursor,0,value);}
            @Override public boolean setComposingText(CharSequence value,int cursor){return edit(2,cursor,0,value);}
            @Override public boolean finishComposingText(){return edit(3,0,0,null);}
            @Override public boolean deleteSurroundingText(int before,int after){return edit(4,before,after,null);}
            @Override public boolean deleteSurroundingTextInCodePoints(int before,int after) {
                if(session!=generation||!active||before<0||after<0)return false;
                int start=Selection.getSelectionStart(text),end=Selection.getSelectionEnd(text);
                int left=Character.offsetByCodePoints(text,start,-Math.min(before,Character.codePointCount(text,0,start)));
                int right=Character.offsetByCodePoints(text,end,Math.min(after,Character.codePointCount(text,end,text.length())));
                return edit(4,start-left,right-end,null);
            }
            @Override public boolean setSelection(int start,int end){return edit(5,start,end,null);}
            @Override public boolean setComposingRegion(int start,int end){return edit(6,start,end,null);}
            @Override public boolean performEditorAction(int action){return edit(7,0,0,null);}
            @Override public ExtractedText getExtractedText(ExtractedTextRequest request,int flags) {
                if(session!=generation||!active)return null;
                ExtractedText result=new ExtractedText();result.text=text.toString();result.selectionStart=Selection.getSelectionStart(text);result.selectionEnd=Selection.getSelectionEnd(text);result.flags=ExtractedText.FLAG_SINGLE_LINE;return result;
            }
            @Override public boolean sendKeyEvent(KeyEvent event) {
                return session==generation&&key(event);
            }
        };
    }
}
