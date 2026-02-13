shader_type spatial;
render_mode unshaded, blend_mix;

uniform sampler2D base_texture : source_color;

uniform vec4 hue : source_color = vec4(1.0);
uniform bool run = true;

/* 0 = clean, 1 = fully disturbed */
uniform float disturbed : hint_range(0.0, 1.0, 0.01) = 0.0;

/* 0 = black & white, 1 = normal color */
uniform float saturation : hint_range(0.0, 1.0, 0.01) = 1.0;

uniform float uv_mult = 6.5;
uniform float lines_speed = 0.4;
uniform float line_alpha = 0.5;
uniform float distortion = 1.0;
uniform float intensity = 2.0;
uniform float line_definition = 0.98;
uniform float min_thickness = 0.03;
uniform float vignette = 0.0;
uniform float roll_speed = 0.25;
uniform float roll_height = 0.12;
uniform float roll_offset = 0.08;
uniform float roll_warp = 0.02;
uniform float screen_warp = 0.18;
uniform float screen_warp_power = 2.0;
uniform float edge_fade = 0.05;
uniform float roll_interval = 3.0;
uniform float roll_duration = 0.6;

float rand(vec2 p) {
	return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

float map_range(float val, float min1, float max1, float min2, float max2) {
	float scale = (max2 - min2) / (max1 - min1);
	return clamp(val * scale + (-min1 * scale + min2), min2, max2);
}

float smooth_compare(float val, float ref, float delta, float hardness) {
	float diff = abs(val - ref);
	return map_range(diff, hardness * 0.5, delta * 0.5, 0.0, 1.0);
}

void fragment() {
	vec2 uv = UV;

	float d = clamp(disturbed, 0.0, 1.0);
	float sat = clamp(saturation, 0.0, 1.0);

	float time = TIME;

	/* ---------- Screen Curvature ---------- */
	vec2 centered = uv - vec2(0.5);
	float r = length(centered);
	uv += centered * pow(r, screen_warp_power)
		* mix(screen_warp, screen_warp * 3.0, d);

	/* ---------- UV Chaos ---------- */
	float noise = rand(vec2(time, uv.y)) - 0.5;
	uv.x += noise * mix(0.0, 0.25, d);
	uv.y += sin(time * mix(0.0, 14.0, d) + uv.x * 50.0)
		* mix(0.0, 0.035, d);

	/* ---------- Roll Timing ---------- */
	float t = time * float(run);
	float roll_timer = mod(t, roll_interval);
	float roll_active = step(0.0, roll_timer) * step(roll_timer, roll_duration);

	/* ---------- Vertical Roll ---------- */
	float roll_pos = fract(t * roll_speed);
	float band_dist = min(abs(uv.y - roll_pos), abs(uv.y - (roll_pos - 1.0)));
	float band = smoothstep(roll_height, 0.0, band_dist) * roll_active;

	uv.y += band * mix(roll_offset, roll_offset * 2.5, d);
	uv.x += sin((uv.y + time) * 40.0)
		* band * mix(roll_warp, roll_warp * 3.0, d);

	/* ---------- Clamp ---------- */
	vec2 clamped_uv = clamp(uv, vec2(0.0), vec2(1.0));

	/* ---------- Scanlines ---------- */
	float coord = fract(
		clamped_uv.y * mix(uv_mult, uv_mult * 2.0, d)
		+ time * mix(lines_speed * float(run), lines_speed * 2.5, d)
	);

	float mask = smooth_compare(coord, 0.5, 1.0 - line_definition, min_thickness);

	float lines_mult = mix(
		mask + (1.0 - mask) * (1.0 - line_alpha),
		mask,
		d
	);

	/* ---------- Distortion ---------- */
	clamped_uv.x += mix(distortion * 0.01, 0.08, d) * lines_mult;

	vec4 tex = texture(base_texture, clamped_uv);

	vec3 color = tex.rgb
		* hue.rgb
		* lines_mult
		* mix(intensity, intensity * 1.6, d);

	/* ---------- Color Channel Glitch ---------- */
	color.r *= 1.0 + rand(vec2(time, uv.y)) * 0.4 * d;
	color.b *= 1.0 - rand(vec2(uv.x, time)) * 0.3 * d;

	/* ---------- Saturation ---------- */
	float luminance = dot(color, vec3(0.299, 0.587, 0.114));
	vec3 grayscale = vec3(luminance);
	color = mix(grayscale, color, sat);

	/* ---------- Vignette ---------- */
	vec2 cuv = UV - vec2(0.5);
	float dist = length(cuv);
	float vmask = smoothstep(0.2, 0.65, dist);
	color *= mix(1.0, 1.0 - vmask, mix(vignette, 0.6, d));

	/* ---------- Edge Fade ---------- */
	float edge_x = smoothstep(0.0, edge_fade, clamped_uv.x)
	             * smoothstep(0.0, edge_fade, 1.0 - clamped_uv.x);
	float edge_y = smoothstep(0.0, edge_fade, clamped_uv.y)
	             * smoothstep(0.0, edge_fade, 1.0 - clamped_uv.y);

	color *= edge_x * edge_y;

	ALBEDO = color;
}
