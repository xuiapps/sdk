#version 300 es
precision highp float;
precision highp int;
#if defined(XUI_VERTEX) == defined(XUI_FRAGMENT)
#error Define exactly one XUI shader stage
#endif
struct x_Vertex {
    vec3 x_Position;
    vec4 x_Color;
};
struct x_Varyings {
    vec4 x_Position;
    vec4 x_Color;
};
struct x_Bindings {
    mat4x4 x_MVP;
};
struct x_FragmentOutput {
    vec4 x_Color;
};
layout(std140, row_major) uniform XuiBindings { x_Bindings x_bindings; };
#ifdef XUI_VERTEX
layout(location = 0) in vec3 x_attribute_0;
layout(location = 1) in vec4 x_attribute_1;
smooth out vec4 x_varying_0;
x_Varyings x_execute(x_Vertex x_input) {
    vec4 x_pos4 = vec4(x_input.x_Position, 1.0);
    x_Varyings x_output = x_Varyings(vec4(0.0), vec4(0.0));
    x_output.x_Position = (x_bindings.x_MVP * x_pos4);
    x_output.x_Color = x_input.x_Color;
    return x_output;
}
void main() {
    x_Vertex x_input;
    x_input.x_Position = x_attribute_0;
    x_input.x_Color = x_attribute_1;
    x_Varyings x_result = x_execute(x_input);
    gl_Position = x_result.x_Position;
    x_varying_0 = x_result.x_Color;
    gl_Position.z = 2.0 * gl_Position.z - gl_Position.w;
}
#endif
#ifdef XUI_FRAGMENT
smooth in vec4 x_varying_0;
layout(location = 0) out vec4 x_color_0;
x_FragmentOutput x_execute(x_Varyings x_input) {
    x_FragmentOutput x_output = x_FragmentOutput(vec4(0.0));
    x_output.x_Color = x_input.x_Color;
    return x_output;
}
void main() {
    x_Varyings x_input;
    x_input.x_Position = gl_FragCoord;
    x_input.x_Color = x_varying_0;
    x_FragmentOutput x_result = x_execute(x_input);
    x_color_0 = x_result.x_Color;
}
#endif
