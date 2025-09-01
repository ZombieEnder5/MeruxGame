#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D uTex;

void main() {
//	vec4 tex = texture(uTex, TexCoord);
//	if (tex.a < 0.01)
//		discard;
//	FragColor = tex;
	FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}