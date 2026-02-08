shader_type canvas_item;
render_mode unshaded;

// Godot 4.x screen grab
uniform sampler2D screen_tex : hint_screen_texture;

// ==========================
// Pulse Controls
// ==========================
uniform float beat_speed : hint_range(0.0, 10.0) = 1.2;
uniform float pulse_strength : hint_range(0.0, 0.08) = 0.03;

// 0 = Smooth sine
// 1 = Single pulse
// 2 = Double pulse (lub-dub)
uniform int pulse_mode : hint_range(0, 2) = 2;

// ==========================
// Tint Controls
// ==========================
uniform vec3 tint_color : source_color = vec3(1.0, 0.15, 0.15);
uniform float tint_strength : hint_range(0.0, 0.4) = 0.15;

// ==========================
// Helpers
// ==========================
vec2 pulse_uv(vec2 uv, float scale) {
	return (uv - vec2(0.5)) / scale + vec2(0.5);
}

float sine_pulse(float t) {
	return sin(t * TAU) * 0.5 + 0.5;
}

float single_pulse(float t) {
	float c = fract(t);
	if (c < 0.2)
		return smoothstep(0.0, 0.2, c);
	if (c < 0.4)
		return (1.0 - (c - 0.2) / 0.2) * 0.6;
	return 0.0;
}

float double_pulse(float t) {
	float c = fract(t);
	float p = 0.0;

	if (c < 0.2) {
		p = smoothstep(0.0, 0.1, c)
		  * (1.0 - smoothstep(0.1, 0.2, c));
	}
	else if (c < 0.4) {
		p = smoothstep(0.2, 0.3, c)
		  * (1.0 - smoothstep(0.3, 0.4, c)) * 0.6;
	}

	return p;
}

// ==========================
// Fragment
// ==========================
void fragment() {
	float t = TIME * beat_speed;
	float pulse = 0.0;

	if (pulse_mode == 0)
		pulse = sine_pulse(t);
	else if (pulse_mode == 1)
		pulse = single_pulse(t);
	else
		pulse = double_pulse(t);

	// Screen zoom
	float scale = 1.0 + pulse * pulse_strength;
	vec2 uv = pulse_uv(SCREEN_UV, scale);

	vec4 screen_color = texture(screen_tex, uv);

	// Red tint scales with pulse
	vec3 final_color = mix(
		screen_color.rgb,
		screen_color.rgb + tint_color,
		pulse * tint_strength
	);

	COLOR = vec4(final_color, 1.0);
}
