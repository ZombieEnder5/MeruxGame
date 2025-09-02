#version 330 core

// Part.frag
// in this shader, it is assumed that the mesh this applies to is ALWAYS a non-uniform cube.

flat in int NormalID;
in vec3 Normal;
in vec3 Size;
in vec2 TexCoord;

uniform sampler2DArray atlas;
uniform vec3 baseColor;
uniform vec3 borderColor;
uniform vec2 textureSize;
uniform vec2 tileSize = vec2(2.0, 2.0);
uniform int FACE_RIGHT;
uniform int FACE_LEFT;
uniform int FACE_TOP;
uniform int FACE_BOTTOM;
uniform int FACE_BACK;
uniform int FACE_FRONT;
uniform float transparency;

out vec4 FragColor;

void main() {
	vec2 invTileSize = 1.0 / tileSize;
	vec2 outlineUV = 1.0 / textureSize;
	vec2 uvScale = invTileSize * (
		NormalID == FACE_RIGHT ? Size.zy :
		NormalID == FACE_LEFT ? Size.yz :
		NormalID == FACE_BACK ? Size.yx :
		NormalID == FACE_FRONT ? Size.xy :
		Size.xz
	);
	vec2 texScale = uvScale * TexCoord;
	bool isBorder = texScale.x < 0.02 || texScale.x > uvScale.x - 0.02 ||
					texScale.y < 0.02 || texScale.y > uvScale.y - 0.02;
	if (isBorder)
		FragColor = vec4(borderColor, 1.0 - transparency);
	else {
		vec2 tiledUV = fract(texScale);
		vec4 tex = texture(atlas,vec3(tiledUV,NormalID));
		FragColor = vec4(mix(mix(baseColor, tex.rgb, tex.a), vec3(0,0,0), .25*(dot(Normal, vec3(0,-1,0))+1)), 1.0 - transparency);
	}
}