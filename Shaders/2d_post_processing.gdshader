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

    // chromatic aberration
    vec2 offset = (SCREEN_UV - 0.5) * 0.01;
    float r = texture(SCREEN_TEXTURE, SCREEN_UV + offset * 0.5).r;
    float g = texture(SCREEN_TEXTURE, SCREEN_UV).g;
    float b = texture(SCREEN_TEXTURE, SCREEN_UV - offset * 0.5).b;
    vec3 chroma = vec3(r, g, b);

    // film grain
    float grain = fract(sin(dot(UV * vec2(12.9898,78.233), vec2(43758.5453))) * 43758.5453);
    chroma += (grain - 0.5) * 0.05;

    // color grading
    chroma = pow(chroma, vec3(1.1)); // subtle contrast

    // crt
    vec2 warp = SCREEN_UV;
    warp.x += sin(warp.y * 3.14159) * 0.02;
    chroma = texture(SCREEN_TEXTURE, warp).rgb;

    // adaptive vignette based on brightness
    float brightness = dot(chroma, vec3(0.299, 0.587, 0.114));
    float adaptive_vig = mix(vignette_strength, vignette_strength * 0.5, brightness);
    chroma *= mix(1.0, 1.0 - adaptive_vig, v);

    COLOR = vec4(tinted, 1.0);
}
