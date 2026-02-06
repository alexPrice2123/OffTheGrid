shader_type canvas_item;
render_mode unshaded;

// REQUIRED in Godot 4.x
uniform sampler2D screen_texture : hint_screen_texture;

// 振動の強さ
uniform float shake_power = 0.03;
// 振動ブロックサイズ
uniform float shake_block_size = 30.5;
// 色の分離率
uniform float fade : hint_range(0.0, 1.0) = 0.01;

// RGB separation directions
uniform vec2 direction_r = vec2(1.0, 0.0);
uniform vec2 direction_g = vec2(0.4, 1.0);
uniform vec2 direction_b = vec2(-0.7, -0.3);

float random(float seed) {
	return fract(
		543.2543 * sin(
			dot(vec2(seed, seed), vec2(3525.46, -54.3415))
		)
	);
}

void fragment() {
	vec2 fixed_uv = SCREEN_UV;

	float block_y = floor(SCREEN_UV.y * shake_block_size) / shake_block_size;

	fixed_uv.x += (
		random(block_y + TIME) - 0.5
	) * shake_power * (fade * 12.0);

	vec3 col;
	col.r = texture(screen_texture, fixed_uv + normalize(direction_r) * fade).r;
	col.g = texture(screen_texture, fixed_uv + normalize(direction_g) * fade).g;
	col.b = texture(screen_texture, fixed_uv + normalize(direction_b) * fade).b;

	COLOR = vec4(col * (1.0 - fade), 1.0);
}
