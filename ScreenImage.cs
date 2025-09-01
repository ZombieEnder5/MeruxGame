using Merux.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merux
{
	internal class ScreenImage : IDisposable
	{
		Shader IMAGE_SHADER = Shader.FromPath("Shaders.Image");

		float[] QUAD_VERTS =
		{
			0f, 0f, 0f, 1f,
			1f, 0f, 1f, 1f,
			1f, 1f, 1f, 0f,
			0f, 1f, 0f, 0f
		};

		uint[] QUAD_IDXS =
		{
			0, 1, 2,
			2, 3, 0
		};

		Texture2D texture;
		int VAO, VBO, EBO;

		public Vector2 AnchorPoint = new Vector2(0.5, 0.5);
		public GuiDim Position = new GuiDim(0, 0, 0, 0);
		public Vector2 Size = new Vector2(64, 64);

		public float TintAlpha = 0f;
		public Vector3 TintColor = new Vector3(0, 0, 0);

		public ScreenImage(Stream stream)
		{
			texture = new Texture2D(stream);
			Debug.Print(texture.Handle);

			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			EBO = GL.GenBuffer();

			GL.BindVertexArray(VAO);

			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, QUAD_VERTS.Length * sizeof(float), QUAD_VERTS, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, QUAD_IDXS.Length * sizeof(uint), QUAD_IDXS, BufferUsageHint.StaticDraw);

			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
			GL.EnableVertexAttribArray(1);

			GL.BindVertexArray(0);
		}

		public void Render()
		{
			var ScrSize = Merux.Game.windowExtents;
			var p = (Position.Offset + Position.Window * ScrSize - AnchorPoint * Size).OpenTK();

			texture.Bind();

			IMAGE_SHADER.Use();
			IMAGE_SHADER.SetVector2("uPos", p);
			IMAGE_SHADER.SetVector2("uSize", Size.OpenTK());
			IMAGE_SHADER.SetVector2("uScrSize", ScrSize.OpenTK());
			IMAGE_SHADER.SetInt("uTexture", 0);

			IMAGE_SHADER.SetFloat("uTintAlpha", TintAlpha);
			IMAGE_SHADER.SetVector3("uTintColor", TintColor.OpenTK());

			GL.Disable(EnableCap.CullFace);

			GL.BindVertexArray(VAO);
			GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);

			GL.Enable(EnableCap.CullFace);
		}

		public void Dispose()
		{
			texture.Dispose();
			GL.DeleteVertexArray(VAO);
			GL.DeleteBuffer(VBO);
			GL.DeleteBuffer(EBO);
		}
	}
}
