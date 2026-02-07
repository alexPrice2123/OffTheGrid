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
uniform float roll_speed : hint_range(-2.0, 2.0, 0.01) = 0.25;
uniform float roll_height : hint_range(0.01, 0.5, 0.01) = 0.12;
uniform float roll_offset : hint_range(0.0, 0.5, 0.01) = 0.08;
uniform float roll_warp : hint_range(0.0, 0.05, 0.001) = 0.02;
uniform float screen_warp : hint_range(0.0, 2.5, 0.01) = 0.18;
uniform float screen_warp_power : hint_range(1.0, 4.0, 0.1) = 2.0;
uniform float edge_fade : hint_range(0.0, 1.0, 0.01) = 0.05;
uniform float roll_interval : hint_range(0.5, 10.0, 0.1) = 3.0;
uniform float roll_duration : hint_range(0.1, 2.0, 0.05) = 0.6;


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
	vec2 uv = UV;

	/* ---------- Screen Curvature ---------- */
	vec2 centered = uv - vec2(0.5);
	float r = length(centered);
	uv += centered * pow(r, screen_warp_power) * screen_warp;

	/* ---------- Roll Timing ---------- */
	float t = TIME * float(run);
	float roll_timer = mod(t, roll_interval);

	// stable on/off window
	float roll_active = step(0.0, roll_timer)
	                  * step(roll_timer, roll_duration);

	/* ---------- Vertical Sync Roll ---------- */
	float roll_pos = fract(t * roll_speed);

	// wrapped distance
	float d1 = abs(uv.y - roll_pos);
	float d2 = abs(uv.y - (roll_pos - 1.0));
	float band_dist = min(d1, d2);

	float band = smoothstep(roll_height, 0.0, band_dist) * roll_active;

	// vertical tear
	uv.y += band * roll_offset;

	// horizontal wobble
	float wobble = sin((uv.y + TIME) * 40.0);
	uv.x += wobble * band * roll_warp;

	/* ---------- Clamp AFTER distortion ---------- */
	vec2 clamped_uv = clamp(uv, vec2(0.0), vec2(1.0));

	/* ---------- Scanlines ---------- */
	float coord = fract(clamped_uv.y * uv_mult + TIME * lines_speed * float(run));

	float mask = smooth_compare(coord, 0.5, 1.0 - line_definition, min_thickness);
	if (flip) mask = 1.0 - mask;

	float lines_mult = mask + (1.0 - mask) * (1.0 - line_alpha);

	/* ---------- Minor Distortion ---------- */
	clamped_uv.x += (distortion * 0.01) * lines_mult;

	vec4 tex = texture(base_texture, clamped_uv);
	vec3 color = tex.rgb * hue.rgb * lines_mult * intensity;

	/* ---------- Vignette ---------- */
	vec2 centered_uv = UV - vec2(0.5);
	float dist = length(centered_uv);
	float vignette_mask = smoothstep(0.2, 0.65, dist);
	color *= mix(1.0, 1.0 - vignette_mask, vignette);

	/* ---------- Edge Fade ---------- */
	float edge_x = smoothstep(0.0, edge_fade, clamped_uv.x)
	             * smoothstep(0.0, edge_fade, 1.0 - clamped_uv.x);

	float edge_y = smoothstep(0.0, edge_fade, clamped_uv.y)
	             * smoothstep(0.0, edge_fade, 1.0 - clamped_uv.y);

	color *= edge_x * edge_y;

	ALBEDO = color;
}





