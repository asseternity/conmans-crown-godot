shader_type canvas_item;
render_mode blend_mix, unshaded;

// This grants access to the rendered screen behind this overlay
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform vec4 tint_color = vec4(1.0, 0.95, 0.85, 1.0);
uniform float vignette_strength = 0.8;
uniform float vignette_smoothness = 0.3;
uniform float scanline_intensity = 0.15;
uniform float time_speed = 0.5;

void fragment() {
    // Sample from the screen
    vec3 scene = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0).rgb;

    float t = TIME * time_speed;

    // micro flicker / shift
    vec3 micro = scene * (0.95 + 0.05 * sin(t + UV.y * 6.0 + UV.x * 3.0));

    // tint
    vec3 tinted = micro * tint_color.rgb;

    // vignette
    vec2 center = SCREEN_UV - vec2(0.5);
    float dist = length(center);
    float v = smoothstep(0.0, vignette_smoothness, dist);
    tinted *= mix(1.0, 1.0 - vignette_strength, v);

    // scanlines
    float scan = (sin(SCREEN_UV.y * 1000.0 + t * 20.0) * 0.5 + 0.5);
    tinted *= 1.0 - scan * scanline_intensity;

    COLOR = vec4(tinted, 1.0);
}
