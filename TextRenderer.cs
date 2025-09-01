using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Merux
{
	internal class TextRenderer
	{
		SKTypeface face;

		public TextRenderer(Stream stream)
		{
			face = SKTypeface.FromStream(stream);
		}

		public int Render(string text, int width, int height, int size, int strokeWidth)
		{
			using SKBitmap bmp = new(width, height, true);
			using SKCanvas cnv = new(bmp);
			cnv.Clear(SKColors.Transparent);

			using var paint = new SKPaint
			{
				Typeface = face,
				TextSize = size,
				IsAntialias = false,
				Style = SKPaintStyle.Fill,
				Color = SKColors.White,
			};

			using var stroke = new SKPaint
			{
				Typeface = face,
				TextSize = size,
				IsAntialias = false,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = strokeWidth,
				Color = SKColors.Black,
				StrokeJoin = SKStrokeJoin.Round,
			};

			var bounds = new SKRect();
			stroke.MeasureText(text, ref bounds);
			float x = 10;
			float y = paint.FontMetrics.Ascent * -1 + 5;

			cnv.DrawText(text, x, y, stroke);
			cnv.DrawText(text, x, y, paint);

			using var img = SKImage.FromBitmap(bmp);
			using var imgData = img.Encode(SKEncodedImageFormat.Png, 100);
			File.WriteAllBytes("debug_output.png", imgData.ToArray());

			int handle = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, handle);

			GL.TexImage2D(
				TextureTarget.Texture2D,
				0, PixelInternalFormat.Rgba,
				bmp.Width, bmp.Height, 0,
				PixelFormat.Bgra, PixelType.UnsignedByte,
				bmp.PeekPixels().GetPixels()
			);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

			return handle;
		}
	}

	//internal enum TextAnchor
	//{
	//	TopLeft, TopCenter, TopRight,
	//	Left, Center, Right,
	//	BottomLeft, BottomCenter, BottomRight
	//}

	//internal class TextRenderer : IDisposable
	//{

	//	readonly int atlas;
	//	readonly int VAO, VBO, EBO;
	//	readonly int cellSize, cols, rows;
	//	readonly Font font;
	//	readonly Shader shader;

	//	public int Width => cols * cellSize;
	//	public int Height => rows * cellSize;

	//	public TextRenderer(byte[] fontData, float fontSize, int cellSize, int cols, int rows, Shader shader)
	//	{
	//		this.cellSize = cellSize;
	//		this.cols = cols;
	//		this.rows = rows;
	//		this.shader = shader;

			//var collect = new FontCollection();
	//		using var ms = new MemoryStream(fontData);
			//var family = collect.Add(ms);
	//		font = family.CreateFont(fontSize, FontStyle.Regular);

	//		using var atlasImg = new Image<Rgba32>(Width, Height);
	//		atlasImg.Mutate(ctx =>
	//		{
	//			for (int i = 0; i < cols * rows; i++)
	//			{
	//				char c = (char)(32 + i);
	//				int col = i % cols, row = i / cols;
	//				var loc = new PointF(col * cellSize, row * cellSize);
	//				ctx.DrawText(
	//					c.ToString(),
	//					font,
	//					Color.White,
	//					loc
	//				);
	//			}
	//		});

	//		atlas = GL.GenTexture();
	//		GL.BindTexture(TextureTarget.Texture2D, atlas);
	//		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
	//		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

	//		var pixels = new byte[Width * Height * 4];
	//		atlasImg.CopyPixelDataTo(pixels);
	//		GL.TexImage2D(
	//			TextureTarget.Texture2D,
	//			0, PixelInternalFormat.Rgba,
	//			Width, Height, 0,
	//			PixelFormat.Rgba, PixelType.UnsignedByte,
	//			pixels
	//		);

	//		float[] verts =
	//		{
	//			0, 0, 0, 0,
	//			1, 0, 1, 0,
	//			1, 1, 1, 1,
	//			0, 1, 0, 1,
	//		};
	//		uint[] idxs = { 0, 1, 2, 2, 3, 0 };

	//		VAO = GL.GenVertexArray();
	//		VBO = GL.GenBuffer();
	//		EBO = GL.GenBuffer();

	//		GL.BindVertexArray(VAO);
	//		GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
	//		GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);
	//		GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
	//		GL.BufferData(BufferTarget.ElementArrayBuffer, idxs.Length * sizeof(uint), idxs, BufferUsageHint.StaticDraw);

	//		int stride = 4 * sizeof(float);
	//		GL.EnableVertexAttribArray(0);
	//		GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
	//		GL.EnableVertexAttribArray(1);
	//		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
	//		GL.BindVertexArray(0);
	//	}

	//	public void DrawString(string text, float x, float y, TextAnchor anchor, Vector4 fillColor, Vector4 strokeColor, float strokeWidth)
	//	{
	//		int len = text.Length;
	//		if (len == 0) return;

	//		float totalWidth = len * cellSize;
	//		float totalHeight = cellSize;

	//		Vector2 pivot = anchor switch
	//		{
	//			TextAnchor.TopLeft => new(0f, 1f),
	//			TextAnchor.TopCenter => new(.5f, 1f),
	//			TextAnchor.TopRight => new(1f, 1f),
	//			TextAnchor.Left => new(0f, .5f),
	//			TextAnchor.Center => new(.5f, .5f),
	//			TextAnchor.Right => new(1f, .5f),
	//			TextAnchor.BottomLeft => new(0f, 0f),
	//			TextAnchor.BottomCenter => new(.5f, 0f),
	//			TextAnchor.BottomRight => new(1f, 0f),
	//			_ => new(0, 0)
	//		};

	//		float offX = x - totalWidth * pivot.X;
	//		float offY = y - totalHeight * pivot.Y;

	//		float[] inst = new float[len * 3];
	//		for (int i = 0; i < len; i++)
	//		{
	//			inst[i * 3] = text[i] - 32;
	//			inst[i * 3 + 1] = offX + i * cellSize;
	//			inst[i * 3 + 2] = offY;
	//		}

	//		int vboInst = GL.GenBuffer();
	//		GL.BindBuffer(BufferTarget.ArrayBuffer, vboInst);
	//		GL.BufferData(BufferTarget.ArrayBuffer, inst.Length * sizeof(float), inst, BufferUsageHint.DynamicDraw);

	//		GL.BindVertexArray(VAO);
	//		GL.EnableVertexAttribArray(2);
	//		GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
	//		GL.VertexBindingDivisor(2, 1);
	//		GL.EnableVertexAttribArray(3);
	//		GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 3 * sizeof(float), sizeof(float));
	//		GL.VertexBindingDivisor(3, 1);
	//		GL.EnableVertexAttribArray(4);
	//		GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, 3 * sizeof(float), 2 * sizeof(float));
	//		GL.VertexBindingDivisor(4, 1);

	//		GL.ActiveTexture(TextureUnit.Texture0);
	//		GL.BindTexture(TextureTarget.Texture2D, atlas);

	//		shader.Use();
	//		shader.SetInt("glyphMask", 0);
	//		shader.SetVector4("fillColor", fillColor);
	//		shader.SetVector4("strokeColor", strokeColor);
	//		shader.SetFloat("strokeWidth", strokeWidth);
	//		shader.SetFloat("cellSize", cellSize);
	//		shader.SetFloat("rows", rows);
	//		shader.SetFloat("cols", cols);

	//		GL.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero, len);

	//		GL.DeleteBuffer(vboInst);
	//		GL.BindVertexArray(0);
	//	}

	//	public void Dispose()
	//	{
	//		GL.DeleteTexture(atlas);
	//		GL.DeleteBuffer(VBO);
	//		GL.DeleteBuffer(EBO);
	//		GL.DeleteVertexArray(VAO);
	//	}
	//}
}
