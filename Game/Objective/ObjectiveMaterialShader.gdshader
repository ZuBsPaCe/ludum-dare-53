// NOTE: Shader automatically converted from Godot Engine 4.0.2.stable.mono's StandardMaterial3D.

shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_disabled,diffuse_burley,specular_schlick_ggx;
uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color,filter_linear_mipmap,repeat_enable;
uniform float point_size : hint_range(0,128);
uniform float roughness : hint_range(0,1);
uniform sampler2D texture_metallic : hint_default_white,filter_linear_mipmap,repeat_enable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_r,filter_linear_mipmap,repeat_enable;
uniform float specular;
uniform float metallic;
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;

uniform sampler2D texture_noise : source_color,filter_linear_mipmap,repeat_enable;


void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
}






void fragment() {
	vec2 base_uv = UV;
	
	float noise = texture(texture_noise, base_uv * vec2(1.0, mix(1.0, 10.0, base_uv.y)) + vec2(0, TIME * 2.0)).r;
	noise = clamp(base_uv.y * 2.0, 0.0, 1.0) * noise;
	
//	alpha = mix(1.0, alpha)
	
//	alpha = clamp(alpha * 2.0, 0.0, 1.0);
	float alpha = clamp(base_uv.y * 2.0, 0.0, 1.0) * noise;
	//alpha = base_uv.y;
//	alpha = min(mix(1.0, alpha, base_uv.y) * 2.0, 1.0);

	alpha *= 0.75;
	
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	float metallic_tex = dot(texture(texture_metallic,base_uv),metallic_texture_channel);
	METALLIC = metallic_tex * metallic;
	vec4 roughness_texture_channel = vec4(1.0,0.0,0.0,0.0);
	float roughness_tex = dot(texture(texture_roughness,base_uv),roughness_texture_channel);
	ROUGHNESS = roughness_tex * roughness;
	SPECULAR = specular;
	
	ALPHA = alpha;
}
