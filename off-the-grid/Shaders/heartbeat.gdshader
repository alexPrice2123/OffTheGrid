shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_tex : hint_screen_texture;

// ==========================
// MANUAL BEAT CONTROL
// ==========================
// Set this yourself (0.0 → 1.0 → 0.0)
uniform float beat : hint_range(0.0, 1.0) = 0.0;

// 0 = Smooth
// 1 = Single hit
// 2 = Double hit (lub-dub)
uniform int pulse_mode : hint_range(0, 2) = 2;

// Strength
uniform float pulse_strength : hint_range(0.0, 0.08) = 0.03;

// ==========================
// Tint
// ==========================
uniform vec3 tint_color : source_color = vec3(1.0, 0.15, 0.15);
uniform float tint_strength : hint_range(0.0, 0.4) = 0.15;

// ==========================
// Helpers
// ==========================
vec2 pulse_uv(vec2 uv, float scale) {
	return (uv - vec2(0.5)) / scale + vec2(0.5);
}

float smooth_pulse(float t) {
	return sin(t * PI);
}

float single_pulse(float t) {
	return smoothstep(0.0, 0.3, t) *
	       (1.0 - smoothstep(0.3, 0.6, t));
}

float double_pulse(float t) {
	float p1 = smoothstep(0.0, 0.2, t) *
	           (1.0 - smoothstep(0.2, 0.4, t));
	float p2 = smoothstep(0.45, 0.6, t) *
	           (1.0 - smoothstep(0.6, 0.8, t)) * 0.6;
	return max(p1, p2);
}

// ==========================
// Fragment
// ==========================
void fragment() {
	float pulse = 0.0;

	if (pulse_mode == 0)
		pulse = smooth_pulse(beat);
	else if (pulse_mode == 1)
		pulse = single_pulse(beat);
	else
		pulse = double_pulse(beat);

	float scale = 1.0 + pulse * pulse_strength;
	vec2 uv = pulse_uv(SCREEN_UV, scale);

	vec4 screen_color = texture(screen_tex, uv);

	vec3 final_color = mix(
		screen_color.rgb,
		screen_color.rgb + tint_color,
		pulse * tint_strength
	);

	COLOR = vec4(final_color, 1.0);
}
