// Numeric opcode order matches BrowserDrawingContext.Op. Consume memory views synchronously.
export function createPlayer(context, images, gpu) {
    let buffer = new Float64Array(4096);
    const colors = new Map();
    const radii = [0, 0, 0, 0];
    function color(value) {
        let css = colors.get(value);
        if (css === undefined) {
            const packed = value >>> 0;
            css = `rgba(${packed >>> 24},${packed >>> 16 & 255},${packed >>> 8 & 255},${(packed & 255) / 255})`;
            if (colors.size >= 1024) colors.clear();
            colors.set(value, css);
        }
        return css;
    }
    return function submit(view, strings) {
        if (buffer.length < view.length) buffer = new Float64Array(Math.max(view.length, buffer.length * 2));
        view.copyTo(buffer);
        const c = context, b = buffer, length = view.length;
        let i = 0;
        while (i < length) {
            const op = b[i++];
            switch (op) {
                case 0: c.save(); break;
                case 1: c.restore(); break;
                case 2: c.beginPath(); break;
                case 3: c.closePath(); break;
                case 4: c.moveTo(b[i++], b[i++]); break;
                case 5: c.lineTo(b[i++], b[i++]); break;
                case 6: c.quadraticCurveTo(b[i++], b[i++], b[i++], b[i++]); break;
                case 7: c.bezierCurveTo(b[i++], b[i++], b[i++], b[i++], b[i++], b[i++]); break;
                case 8: c.arc(b[i++], b[i++], b[i++], b[i++], b[i++], !!b[i++]); break;
                case 9: c.arcTo(b[i++], b[i++], b[i++], b[i++], b[i++]); break;
                case 10: c.ellipse(b[i++], b[i++], b[i++], b[i++], b[i++], b[i++], b[i++], !!b[i++]); break;
                case 11: c.rect(b[i++], b[i++], b[i++], b[i++]); break;
                case 12: {
                    const x = b[i++], y = b[i++], w = b[i++], h = b[i++];
                    for (let r = 0; r < 4; r++) radii[r] = b[i++];
                    c.roundRect(x, y, w, h, radii); break;
                }
                case 13: c.clip(); break;
                case 14: c.fill(b[i++] ? 'evenodd' : 'nonzero'); break;
                case 15: c.stroke(); break;
                case 16: c.fillRect(b[i++], b[i++], b[i++], b[i++]); break;
                case 17: c.strokeRect(b[i++], b[i++], b[i++], b[i++]); break;
                case 18: c.transform(b[i++], b[i++], b[i++], b[i++], b[i++], b[i++]); break;
                case 19: c.setTransform(b[i++], b[i++], b[i++], b[i++], b[i++], b[i++]); break;
                case 20: c.fillStyle = color(b[i++]); break;
                case 21: c.strokeStyle = color(b[i++]); break;
                case 22: c.globalAlpha = b[i++]; break;
                case 23: c.lineWidth = b[i++]; break;
                case 24: c.lineCap = ['butt', 'round', 'square'][b[i++]]; break;
                case 25: c.lineJoin = ['miter', 'round', 'bevel'][b[i++]]; break;
                case 26: c.miterLimit = b[i++]; break;
                case 27: c.lineDashOffset = b[i++]; break;
                case 28: { const n = b[i++]; c.setLineDash(b.subarray(i, i + n)); i += n; break; }
                case 29: c.font = strings[b[i++]]; break;
                case 30: c.textAlign = ['start', 'end', 'left', 'right', 'center'][b[i++]]; break;
                case 31: c.textBaseline = ['alphabetic', 'top', 'hanging', 'middle', 'ideographic', 'bottom'][b[i++]]; break;
                case 32: c.fillText(strings[b[i++]], b[i++], b[i++]); break;
                case 33:
                case 34: {
                    const stroke = b[i++];
                    const gradient = op === 33
                        ? c.createLinearGradient(b[i++], b[i++], b[i++], b[i++])
                        : c.createRadialGradient(b[i++], b[i++], b[i++], b[i++], b[i++], b[i++]);
                    const n = b[i++];
                    for (let stop = 0; stop < n; stop++) gradient.addColorStop(b[i++], color(b[i++]));
                    c[stroke ? 'strokeStyle' : 'fillStyle'] = gradient;
                    break;
                }
                case 35: {
                    const image = images.get(b[i++])?.source;
                    if (image && b[i + 2] > 0 && b[i + 3] > 0) {
                        const alpha = c.globalAlpha;
                        c.globalAlpha *= b[i + 8];
                        c.drawImage(image, b[i], b[i+1], b[i+2], b[i+3], b[i+4], b[i+5], b[i+6], b[i+7]);
                        c.globalAlpha = alpha;
                    }
                    i += 9; break;
                }
                case 36: {
                    const image = images.get(b[i++])?.source;
                    const repeat = ['repeat', 'repeat-x', 'repeat-y', 'no-repeat'][b[i++]];
                    const stroke = b[i++];
                    c[stroke ? 'strokeStyle' : 'fillStyle'] = image ? c.createPattern(image, repeat) : 'transparent';
                    break;
                }
                case 37: gpu.present(b[i++], c, b[i++], b[i++], b[i++], b[i++]); break;
                default: throw new Error(`Unknown Xui canvas opcode ${op} at ${i - 1}`);
            }
        }
    };
}
