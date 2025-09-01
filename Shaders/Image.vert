#version 330 core

layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTex;
out vec2 TexCoord;

uniform vec2 uPos;
uniform vec2 uSize;
uniform vec2 uScrSize;

void main()
{
	vec2 iconSize = uSize * vec2(1,-1);
	vec2 scrSize = uScrSize * vec2(1,-1);
	vec2 ndc = (uPos + (aPos - vec2(0, 1)) * iconSize) / scrSize * 2 - vec2(1,-1);
	gl_Position = vec4(ndc.x, ndc.y, 0.0, 1.0);
	//gl_Position = vec4(uPos/uScrSize+aPos, 0.0, 1.0);
	TexCoord = aTex;
}