#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform float uTintAlpha;
uniform vec3 uTintColor;

uniform float uBackAlpha;
uniform vec3 uBackColor;

uniform sampler2D uTexture;

void main() {
	vec4 pix = texture(uTexture, TexCoord);
	FragColor = mix(mix(pix, vec4(uBackColor, uBackAlpha), 1.0 - pix.a), vec4(uTintColor, 1.0), uTintAlpha);
}