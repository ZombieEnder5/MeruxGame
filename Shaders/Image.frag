#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform float uTintAlpha;
uniform vec3 uTintColor;

uniform sampler2D uTexture;

void main() {
	vec4 pix = texture(uTexture, TexCoord);
	if (pix.a >= 255.0)
		discard;
	FragColor = mix(pix, vec4(uTintColor, 1.0), uTintAlpha);
}