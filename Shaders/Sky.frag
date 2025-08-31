#version 330 core

in vec3 TexCoord;
out vec4 FragColor;

uniform samplerCube uSkybox;

void main() {
	FragColor = texture(uSkybox, TexCoord);
}