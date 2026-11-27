namespace Xui.Compiler.Tests.Wasm.TestApp;

/// <summary>Canvas text and path operations for the real mobile-shell frame gate.</summary>
internal static class BrowserCanvasHost
{
    public const string JavaScript = """
        function canvasImports(context, commands, baseTransform = null) {
            const color = (r,g,b,a) => `rgba(${Math.round(r*255)},${Math.round(g*255)},${Math.round(b*255)},${a})`;
            let pathRect = [0,0,0,0], transform = [1,0,0,1], controls = [0,0,0,0], arc = [0,0,0], dash = [];
            let selectedGpuTarget;
            let gradient, gradientDefinition, fillBrush, strokeBrush, gradientColor, ellipse, arcToControls, selectedImage, imageSource, imageOpacity;
            const imageEntries = Object.entries(globalThis.xuiImageAssets ?? {});
            const imageIds = new Map(imageEntries.map(([uri], i) => [uri, i+1]));
            const image = id => { const asset = imageEntries[id-1]?.[1]; if (!asset) throw new Error(`Invalid image handle: ${id}`); return asset; };
            let cachedText, metrics; const fonts = [];
            let font = { size: 12, weight: 400, stretch: 100, height: 14.4, family: 'Inter' };
            const paintProperties = ['globalAlpha','globalCompositeOperation','fillStyle','strokeStyle','lineWidth','lineCap','lineJoin','miterLimit','lineDashOffset','font','textAlign','textBaseline','direction','filter','imageSmoothingEnabled','imageSmoothingQuality'];
            const makeBrush = brush => {
                if (!brush) return null;
                const value = context.createLinearGradient(...brush.args);
                for (const stop of brush.stops) value.addColorStop(...stop);
                return {value, definition:brush};
            };
            return { State() { return {font, fillBrush, strokeBrush, paint:context ? Object.fromEntries(paintProperties.map(k=>[k,context[k]])) : {}, dash:context?.getLineDash() ?? []}; },
            SetState(state) {
                font=state.font; fillBrush=state.fillBrush; strokeBrush=state.strokeBrush; metrics=undefined;
                for (const key of paintProperties) context[key]=state.paint[key];
                context.setLineDash(state.dash);
                const fill=makeBrush(fillBrush), stroke=makeBrush(strokeBrush);
                if(fill){context.fillStyle=fill.value;fillBrush=fill.definition;}
                if(stroke){context.strokeStyle=stroke.value;strokeBrush=stroke.definition;}
            }, Group(key) { if (key === null) throw new Error('A group requires a stable key'); return 0; }, EndGroup() { return 0; },
            Command(name,a,b,c,d,text) {
                commands.push([name,a,b,c,d,text]); metrics = undefined;
                if (name.startsWith('Group')) return; // Consumed by the DOM compositor; immediate hosts use the C# fallback.
                switch (name) {
                    case 'GpuTarget': selectedGpuTarget=a; break;
                    case 'GpuDraw': globalThis.xuiGpuHost.present(selectedGpuTarget,context,a,b,c,d); break;
                    case 'Image': selectedImage = image(a); imageOpacity = b; imageSource = null; break;
                    case 'ImageSource': imageSource = [a,b,c,d]; break;
                    case 'DrawImage':
                        if (context) {
                            if (!selectedImage?.decoded) throw new Error('Image must be decoded before drawing');
                            context.save();
                            try { context.globalAlpha *= imageOpacity; context.drawImage(selectedImage.decoded, ...(imageSource ?? []), a,b,c,d); }
                            finally { context.restore(); }
                        }
                        break;
                    case 'FillPattern': case 'StrokePattern': {
                        if(name === 'FillPattern') fillBrush=null; else strokeBrush=null;
                        const asset = image(a), repetition = ['repeat','repeat-x','repeat-y','no-repeat'][b];
                        if (!repetition) throw new Error(`Unsupported image repetition: ${b}`);
                        if (context) {
                            if (!asset.decoded) throw new Error('Image must be decoded before pattern creation');
                            const pattern = context.createPattern(asset.decoded, repetition);
                            if (!pattern) throw new Error('Image pattern creation failed');
                            if (name === 'FillPattern') context.fillStyle = pattern; else context.strokeStyle = pattern;
                        }
                        break;
                    }
                    case 'Error': throw new Error(text);
                    case 'Save': fonts.push({font,fillBrush,strokeBrush}); context?.save(); break;
                    case 'Restore': ({font,fillBrush,strokeBrush}=fonts.pop()); context?.restore(); break;
                    case 'GlobalAlpha': if (context) context.globalAlpha = a; break;
                    case 'LineCap': if (context) context.lineCap = ['butt','round','square'][a]; break;
                    case 'LineJoin': if (context) context.lineJoin = ['miter','round','bevel'][a]; break;
                    case 'LineWidth': if (context) context.lineWidth = a; break;
                    case 'MiterLimit': if (context) context.miterLimit = a; break;
                    case 'LineDashOffset': if (context) context.lineDashOffset = a; break;
                    case 'TextAlign': if (context) context.textAlign = ['start','end','left','right','center'][a]; break;
                    case 'TextBaseline': if (context) context.textBaseline = ['top','hanging','middle','alphabetic','ideographic','bottom'][a]; break;
                    case 'SetLineDash': context?.setLineDash([]); break;
                    case 'LinearGradient': gradient = context?.createLinearGradient(a,b,c,d); gradientDefinition={args:[a,b,c,d],stops:[]}; break;
                    case 'GradientColor': gradientColor = color(a,b,c,d); break;
                    case 'GradientStop': gradient?.addColorStop(a, gradientColor); gradientDefinition.stops.push([a,gradientColor]); break;
                    case 'FillGradient': fillBrush=gradientDefinition; if (context) context.fillStyle = gradient; break;
                    case 'StrokeGradient': strokeBrush=gradientDefinition; if (context) context.strokeStyle = gradient; break;
                    case 'SetStroke': strokeBrush=null; if (context) context.strokeStyle = color(a,b,c,d); break;
                    case 'SetFill': fillBrush=null; if (context) context.fillStyle = color(a,b,c,d); break;
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
                    case 'SetTransformTranslation': if (context) { const matrix = new DOMMatrix([...transform,a,b]); context.setTransform(baseTransform ? baseTransform.multiply(matrix) : matrix); } break;
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
            }, LoadImage(uri) { return imageIds.get(uri) ?? 0; },
            ImageSize(handle, dimension) { if (dimension !== 0 && dimension !== 1) throw new Error('Invalid image dimension'); const asset = image(handle); return dimension === 0 ? asset.width : asset.height; },
            Measure(text, index) {
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
