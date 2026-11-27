#version 300 es
precision highp float;
precision highp int;
#if defined(XUI_VERTEX) == defined(XUI_FRAGMENT)
#error Define exactly one XUI shader stage
#endif
struct x_Vertex {
    vec4 x_PositionUv;
    vec4 x_Detail;
};
struct x_Varyings {
    vec4 x_Position;
    vec4 x_Texture;
};
struct x_Bindings {
    vec4 x_Camera;
    vec4 x_Fade;
};
struct x_FragmentOutput {
    vec4 x_Color;
};
layout(std140, row_major) uniform XuiBindings { x_Bindings x_bindings; };
uniform highp sampler2D x_texture_Atlas_Sampler;
#ifdef XUI_VERTEX
layout(location = 0) in vec4 x_attribute_0;
layout(location = 1) in vec4 x_attribute_1;
smooth out vec4 x_varying_0;
x_Varyings x_execute(x_Vertex x_input) {
    x_Varyings x_output = x_Varyings(vec4(0.0), vec4(0.0));
    x_output.x_Position = vec4((((x_input.x_PositionUv.x * x_bindings.x_Camera.x) + x_bindings.x_Camera.z) + (x_input.x_Detail.y * x_bindings.x_Fade.z)), (((x_input.x_PositionUv.y * x_bindings.x_Camera.y) + x_bindings.x_Camera.w) + (x_input.x_Detail.z * x_bindings.x_Fade.w)), 0.0, 1.0);
    x_output.x_Texture = vec4(x_input.x_PositionUv.z, x_input.x_PositionUv.w, (x_input.x_Detail.x + x_bindings.x_Fade.y), (x_bindings.x_Fade.x * x_input.x_Detail.w));
    return x_output;
}
void main() {
    x_Vertex x_input;
    x_input.x_PositionUv = x_attribute_0;
    x_input.x_Detail = x_attribute_1;
    x_Varyings x_result = x_execute(x_input);
    gl_Position = x_result.x_Position;
    x_varying_0 = x_result.x_Texture;
    gl_Position.z = 2.0 * gl_Position.z - gl_Position.w;
}
#endif
#ifdef XUI_FRAGMENT
smooth in vec4 x_varying_0;
layout(location = 0) out vec4 x_color_0;
x_FragmentOutput x_execute(x_Varyings x_input) {
    vec4 x_color = textureLod(x_texture_Atlas_Sampler, vec2(x_input.x_Texture.x, x_input.x_Texture.y), x_input.x_Texture.z);
    x_FragmentOutput x_output = x_FragmentOutput(vec4(0.0));
    x_output.x_Color = (x_color * x_input.x_Texture.w);
    return x_output;
}
void main() {
    x_Varyings x_input;
    x_input.x_Position = gl_FragCoord;
    x_input.x_Texture = x_varying_0;
    x_FragmentOutput x_result = x_execute(x_input);
    x_color_0 = x_result.x_Color;
}
#endif
