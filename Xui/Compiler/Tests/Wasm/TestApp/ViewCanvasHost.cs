namespace Xui.Compiler.Tests.Wasm.TestApp;

/// <summary>The explicit JS Canvas boundary for the real View first-frame test.</summary>
internal static class ViewCanvasHost
{
    public const string JavaScript = """
        function canvasImports(context, commands) {
            const color = (r,g,b,a) => `rgba(${Math.round(r*255)},${Math.round(g*255)},${Math.round(b*255)},${a})`;
            let pathRect = [0,0,0,0];
            let font = { size: 12, weight: 400, stretch: 100, height: 14.4, family: 'Inter' };
            return { Command(name,a,b,c,d,text) {
                commands.push([name,a,b,c,d,text]);
                switch (name) {
                    case 'Save': context?.save(); break;
                    case 'Restore': context?.restore(); break;
                    case 'GlobalAlpha': if (context) context.globalAlpha = a; break;
                    case 'LineCap': if (context) context.lineCap = ['butt','round','square'][a]; break;
                    case 'LineJoin': if (context) context.lineJoin = ['miter','round','bevel'][a]; break;
                    case 'LineWidth': if (context) context.lineWidth = a; break;
                    case 'MiterLimit': if (context) context.miterLimit = a; break;
                    case 'LineDashOffset': if (context) context.lineDashOffset = a; break;
                    case 'TextAlign': if (context) context.textAlign = ['start','end','left','right','center'][a]; break;
                    case 'TextBaseline': if (context) context.textBaseline = ['top','hanging','middle','alphabetic','ideographic','bottom'][a]; break;
                    case 'SetLineDash': context?.setLineDash([]); break;
                    case 'SetStroke': if (context) context.strokeStyle = color(a,b,c,d); break;
                    case 'SetFill': if (context) context.fillStyle = color(a,b,c,d); break;
                    case 'FillRect': context?.fillRect(a,b,c,d); break;
                    case 'BeginPath': context?.beginPath(); break;
                    case 'PathRect': pathRect = [a,b,c,d]; break;
                    case 'RoundRect': context?.roundRect(...pathRect, a); break;
                    case 'Translate': context?.translate(a,b); break;
                    case 'Scale': context?.scale(a,b); break;
                    case 'Fill': context?.fill(a === 0 ? 'nonzero' : 'evenodd'); break;
                    case 'Clip': context?.clip(); break;
                    case 'Font':
                        if (c !== 100) throw new Error('This first-frame host supports normal font stretch only.');
                        font = { size:a, weight:b, stretch:c, height:d, family:text };
                        break;
                    case 'FontStyle':
                        if (a !== 0 || b !== 0) throw new Error('This first-frame host supports upright fonts only.');
                        if (context) context.font = `${font.weight} ${font.size}px/${font.height}px ${JSON.stringify(font.family)}`;
                        break;
                    default: throw new Error(`Unsupported Canvas command: ${name}`);
                }
            } };
        }
        """;
}
