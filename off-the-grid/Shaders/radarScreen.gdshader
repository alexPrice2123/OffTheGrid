shader_type spatial;
render_mode unshaded, blend_mix;

uniform sampler2D base_texture : source_color;

uniform vec4 hue : source_color = vec4(1.0);
uniform bool flip = false;
uniform bool run = true;

uniform float uv_mult : hint_range(1.0, 50.0, 0.5) = 6.5;
uniform float lines_speed : hint_range(0.1, 2.0, 0.01) = 0.4;
uniform float line_alpha : hint_range(0.0, 1.0, 0.01) = 0.5;
uniform float distortion : hint_range(-1.0, 1.0, 0.01) = 1.0;
uniform float intensity : hint_range(0.0, 2.0, 0.01) = 2.0;
uniform float line_definition : hint_range(0.0, 1.0, 0.01) = 0.98;
uniform float min_thickness : hint_range(0.0, 1.0, 0.01) = 0.03;
uniform float vignette : hint_range(0.0, 1.0, 0.01) = 0.0;

float map_range(float val, float min1, float max1, float min2, float max2) {
	if (val < min1) return min2;
	if (val > max1) return max2;

	float scale = (max2 - min2) / (max1 - min1);
	return val * scale + (-min1 * scale + min2);
}

float smooth_compare(float val, float ref, float delta, float hardness) {
	float diff = abs(val - ref);
	return map_range(diff, hardness * 0.5, delta * 0.5, 0.0, 1.0);
}

void fragment() {
	float coord = fract(UV.y * uv_mult + TIME * lines_speed * float(run));

	float mask = smooth_compare(coord, 0.5, 1.0 - line_definition, min_thickness);
	if (flip) {
		mask = 1.0 - mask;
	}

	float lines_mult = mask + (1.0 - mask) * (1.0 - line_alpha);

	vec2 base_uv = UV;
	base_uv.x += (distortion * 0.01) * lines_mult;

	vec4 tex = texture(base_texture, base_uv);
	vec3 color = tex.rgb * hue.rgb * lines_mult * intensity;

	/* ---------- Vignette ---------- */

	// UV distance from center
	vec2 centered_uv = UV - vec2(0.5);
	float dist = length(centered_uv);

	// Soft vignette curve
	float vignette_mask = smoothstep(0.2, 0.65, dist);

	// Apply strength
	color *= mix(1.0, 1.0 - vignette_mask, vignette);

	ALBEDO = color;
}


