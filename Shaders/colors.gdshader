shader_type canvas_item;

uniform float color_r : hint_range(0.0, 1.0, 0.1);
uniform float color_g : hint_range(0.0, 1.0, 0.1);
uniform float color_b : hint_range(0.0, 1.0, 0.1);
uniform float opacity : hint_range(0.0, 1.0, 0.1);

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    vec4 tint = vec4(color_r, color_g, color_b, opacity);
    COLOR = tex_color * tint;
}
