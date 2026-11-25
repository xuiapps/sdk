namespace Xui.Compiler.Tests.Wasm.TestApp;

/// <summary>Canvas text and path operations for the real mobile-shell frame gate.</summary>
internal static class BrowserCanvasHost
{
    public const string JavaScript = """
        function canvasImports(context, commands) {
            const color = (r,g,b,a) => `rgba(${Math.round(r*255)},${Math.round(g*255)},${Math.round(b*255)},${a})`;
            let pathRect = [0,0,0,0], transform = [1,0,0,1], controls = [0,0,0,0], arc = [0,0,0], dash = [];
            let gradient, gradientColor, ellipse, arcToControls;
            let cachedText, metrics; const fonts = [];
            let font = { size: 12, weight: 400, stretch: 100, height: 14.4, family: 'Inter' };
            return { Command(name,a,b,c,d,text) {
                commands.push([name,a,b,c,d,text]); metrics = undefined;
                switch (name) {
                    case 'Error': throw new Error(text);
                    case 'Save': fonts.push(font); context?.save(); break;
                    case 'Restore': font = fonts.pop(); context?.restore(); break;
                    case 'GlobalAlpha': if (context) context.globalAlpha = a; break;
                    case 'LineCap': if (context) context.lineCap = ['butt','round','square'][a]; break;
                    case 'LineJoin': if (context) context.lineJoin = ['miter','round','bevel'][a]; break;
                    case 'LineWidth': if (context) context.lineWidth = a; break;
                    case 'MiterLimit': if (context) context.miterLimit = a; break;
                    case 'LineDashOffset': if (context) context.lineDashOffset = a; break;
                    case 'TextAlign': if (context) context.textAlign = ['start','end','left','right','center'][a]; break;
                    case 'TextBaseline': if (context) context.textBaseline = ['top','hanging','middle','alphabetic','ideographic','bottom'][a]; break;
                    case 'SetLineDash': context?.setLineDash([]); break;
                    case 'LinearGradient': gradient = context?.createLinearGradient(a,b,c,d); break;
                    case 'GradientColor': gradientColor = color(a,b,c,d); break;
                    case 'GradientStop': gradient?.addColorStop(a, gradientColor); break;
                    case 'FillGradient': if (context) context.fillStyle = gradient; break;
                    case 'StrokeGradient': if (context) context.strokeStyle = gradient; break;
                    case 'SetStroke': if (context) context.strokeStyle = color(a,b,c,d); break;
                    case 'SetFill': if (context) context.fillStyle = color(a,b,c,d); break;
                    case 'FillRect': context?.fillRect(a,b,c,d); break;
                    case 'FillText': context?.fillText(text,a,b); break;
                    case 'StrokeRect': context?.strokeRect(a,b,c,d); break;
                    case 'MoveTo': context?.moveTo(a,b); break;
                    case 'LineTo': context?.lineTo(a,b); break;
                    case 'QuadraticTo': context?.quadraticCurveTo(a,b,c,d); break;
                    case 'CurveControls': controls = [a,b,c,d]; break;
                    case 'CubicTo': context?.bezierCurveTo(...controls,a,b); break;
                    case 'ClosePath': context?.closePath(); break;
                    case 'Stroke': context?.stroke(); break;
                    case 'Rect': context?.rect(a,b,c,d); break;
                    case 'Rotate': context?.rotate(a); break;
                    case 'TransformLinear': transform = [a,b,c,d]; break;
                    case 'TransformTranslation': context?.transform(...transform,a,b); break;
                    case 'SetTransformTranslation': context?.setTransform(...transform,a,b); break;
                    case 'EllipseGeometry': ellipse = [a,b,c,d]; break;
                    case 'EllipseAngles': context?.ellipse(...ellipse,a,b,c,d !== 0); break;
                    case 'ArcToControls': arcToControls = [a,b,c,d]; break;
                    case 'ArcTo': context?.arcTo(...arcToControls,a); break;
                    case 'ArcOrigin': arc = [a,b,c]; break;
                    case 'ArcAngles': context?.arc(...arc,a,b,c !== 0); break;
                    case 'RoundRectCorners': context?.roundRect(...pathRect, [a,b,c,d]); break;
                    case 'DashBegin': dash = []; break;
                    case 'DashValue': dash.push(a); break;
                    case 'DashEnd': context?.setLineDash(dash); break;
                    case 'ClearFilter': if (context) context.filter = 'none'; break;
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
                        if (context) context.font = `${a ? 'italic ' : b ? `oblique ${c}deg ` : ''}${font.weight} ${font.size}px ${JSON.stringify(font.family)}`;
                        break;
                    default: throw new Error(`Unsupported Canvas command: ${name}`);
                }
            }, Measure(text, index) {
                if (metrics === undefined || cachedText !== text) {
                    cachedText = text;
                    // Node uses deterministic metrics to exercise layout; browsers use
                    // their actual Canvas font shaping and bounding boxes.
                    const m = context ? context.measureText(text) : { width: text.length * font.size * .55 };
                    const ascent = m.fontBoundingBoxAscent ?? font.size * .8;
                    const descent = m.fontBoundingBoxDescent ?? font.size * .2;
                    metrics = [m.width, m.actualBoundingBoxLeft ?? 0, m.actualBoundingBoxRight ?? m.width,
                        m.actualBoundingBoxAscent ?? ascent, m.actualBoundingBoxDescent ?? descent,
                        ascent, descent, m.emHeightAscent ?? ascent, m.emHeightDescent ?? descent,
                        -(m.alphabeticBaseline ?? 0), -(m.hangingBaseline ?? ascent * .8), -(m.ideographicBaseline ?? -descent)];
                }
                if (!Number.isInteger(index) || index < 0 || index >= metrics.length) throw new Error('Invalid text metric');
                return metrics[index];
            } };
        }
        """;
}
