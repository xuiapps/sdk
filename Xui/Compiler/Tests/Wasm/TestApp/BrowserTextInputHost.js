// A native single-line input receives browser keyboard/paste/composition events;
// Xui's Window and TextEditingState remain the document and selection owners.
function createTextInputHost(canvas, api, session, invalidate, fail) {
    if (!api.TextClient) return null;
    const input = document.createElement('input');
    input.dataset.xuiTextInput = '';
    input.setAttribute('aria-label', 'Focused Xui text field');
    input.autocomplete = 'off'; input.spellcheck = false;
    Object.assign(input.style, { position:'fixed', width:'1px', height:'1px', opacity:'0', pointerEvents:'none', left:'0', top:'0', padding:'0', border:'0' });
    document.body.appendChild(input);
    let client = null, syncing = false, composing = false, compositionClient = null, start = 0, end = 0;
    let committed = null, commitTimer = 0;
    const keys = { Backspace:8, Tab:9, Enter:13, Escape:27, Home:36, End:35, ArrowLeft:37, ArrowRight:39, Delete:46 };
    const safely = action => event => { try { action(event); } catch(error) { fail(new Error(api.TextError?.() || String(error))); } };
    function sync(focus = false) {
        const next = api.TextClient(session);
        if (next !== client) { composing = false; client = next; }
        input.disabled = client === null;
        if (!client) { if(document.activeElement===input) input.blur(); input.value=''; return; }
        if (composing) return;
        syncing = true;
        const type=api.TextSecure(session) ? 'password' : 'text';
        if(input.type!==type) input.type=type;
        const value = api.TextValue(session);
        if(input.value !== value) input.value=value;
        const selectionStart=api.TextStart(session),selectionEnd=api.TextEnd(session);
        if(input.selectionStart!==selectionStart || input.selectionEnd!==selectionEnd) input.setSelectionRange(selectionStart,selectionEnd);
        if(focus && document.activeElement!==input) input.focus({preventScroll:true});
        syncing = false;
    }
    function selected() {
        if(!syncing && !composing && client && document.activeElement===input &&
            (api.TextStart(session)!==input.selectionStart || api.TextEnd(session)!==input.selectionEnd)) {
            api.TextSelect(session,input.selectionStart,input.selectionEnd); return true;
        }
        return false;
    }
    function changed() { sync(true); invalidate(); }
    function insert(text) { selected(); api.TextInsert(session,text); changed(); }
    const handlers = {
        beforeinput: safely(event => {
            if(!client || composing || event.isComposing) return;
            event.preventDefault();
            if(committed !== null && (event.inputType==='insertFromComposition' || event.data===committed)) return;
            selected();
            if(event.inputType==='deleteContentBackward') api.TextKey(session,8,0,0);
            else if(event.inputType==='deleteContentForward') api.TextKey(session,46,0,0);
            else if(event.inputType==='deleteByCut') api.TextInsert(session,'');
            else if(event.inputType==='insertLineBreak' || event.inputType==='insertParagraph') api.TextInsert(session,'\n');
            else if(event.inputType.startsWith('insert') && event.data!==null) api.TextInsert(session,event.data);
            changed();
        }),
        keydown: safely(event => {
            if(!client || composing || event.isComposing) return;
            if((event.ctrlKey || event.metaKey) && event.key.toLowerCase()==='a') {
                event.preventDefault(); api.TextSelect(session,0,api.TextValue(session).length); changed(); return;
            }
            const key=keys[event.key];
            if(key && !event.ctrlKey && !event.metaKey && !event.altKey) {
                selected();
                if(api.TextKey(session,key,event.shiftKey?1:0,event.repeat?1:0)) event.preventDefault();
                changed();
            }
        }),
        paste: safely(event => { if(client && !composing) { event.preventDefault(); insert(event.clipboardData.getData('text/plain')); } }),
        select: safely(() => { if(selected()) invalidate(); }),
        compositionstart: safely(() => {
            selected(); composing=true; compositionClient=client;
            start=api.TextStart(session); end=api.TextEnd(session);
        }),
        compositionend: safely(event => {
            composing=false;
            // Commit once. Provisional native composition is not yet painted by Xui.
            if(compositionClient===api.TextClient(session) && event.data) {
                api.TextSelect(session,start,end); api.TextInsert(session,event.data);
            }
            committed=event.data; clearTimeout(commitTimer);
            commitTimer=setTimeout(()=>{committed=null;},0);
            changed();
        }),
        input: safely(() => { if(!composing) sync(); }),
    };
    for(const [name,handler] of Object.entries(handlers)) input.addEventListener(name,handler);
    return {
        sync,
        dispose() {
            clearTimeout(commitTimer);
            for(const [name,handler] of Object.entries(handlers)) input.removeEventListener(name,handler);
            input.value=''; input.remove(); client=null; compositionClient=null; session=null; api=null;
        }
    };
}
