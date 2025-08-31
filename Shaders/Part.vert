#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform vec3 size;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform int FACE_RIGHT;
uniform int FACE_LEFT;
uniform int FACE_TOP;
uniform int FACE_BOTTOM;
uniform int FACE_BACK;
uniform int FACE_FRONT;

out vec3 FragPos;
out vec3 Normal;
flat out int NormalID;
out vec3 Size;
out vec2 TexCoord;

void main() {
	FragPos = vec3(model * vec4(aPosition, 1.0));
	Normal = normalize(mat3(transpose(inverse(model))) * aNormal);
	TexCoord = aTexCoord;
	Size = size;
	
	vec3 eNorm = normalize(aNormal);
	vec3 aNorm = abs(eNorm);
	if (aNorm.x > aNorm.y && aNorm.x > aNorm.z)
		NormalID = eNorm.x > 0.0 ? FACE_RIGHT : FACE_LEFT;
	else if (aNorm.y > aNorm.x && aNorm.y > aNorm.z)
		NormalID = eNorm.y > 0.0 ? FACE_TOP : FACE_BOTTOM;
	else
		NormalID = eNorm.z > 0.0 ? FACE_BACK : FACE_FRONT;

	gl_Position = projection * view * model * vec4(aPosition, 1.0);
}