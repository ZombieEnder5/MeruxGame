#version 330 core

layout(location = 0) in vec3 aPos;

out vec3 TexCoord;

uniform mat4 uView;
uniform mat4 uProj;

void main()
{
	mat4 view = mat4(mat3(uView));
	vec4 pos = uProj * view * vec4(aPos, 1.0);
	gl_Position = pos.xyww;
	TexCoord = aPos;
}