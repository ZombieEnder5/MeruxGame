using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Merux
{
	public class Texture2DArray
	{
		public int Handle { get; private set; }
		public Vector2i Size { get; private set; }
		public int LayerCount { get; private set; }

		public Texture2DArray(string[] paths)
		{
			using Stream firstStream = Merux.LoadStream(paths[0]);
			using var first = Image.Load<Rgba32>(firstStream);
			int width = first.Width;
			int height = first.Height;
			Size = new Vector2i(width, height);
			LayerCount = paths.Length;
			Handle = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2DArray, Handle);
			GL.TexImage3D(
				TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
				width, height, LayerCount, 0,
				PixelFormat.Rgba, PixelType.UnsignedByte,
				IntPtr.Zero
			);
			for (int i = 0; i < LayerCount; i++)
			{
				using Stream stream = Merux.LoadStream(paths[i]);
				using var img = Image.Load<Rgba32>(stream);
				var pixels = new byte[width * height * 4];
				img.CopyPixelDataTo(pixels);
				GL.TexSubImage3D(
					TextureTarget.Texture2DArray,
					0, 0, 0, i,
					width, height, 1,
					PixelFormat.Rgba, PixelType.UnsignedByte,
					pixels
				);
			}
			GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			//GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
		}

		public void Bind(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2DArray, Handle);
		}

		public void Dispose()
		{
			GL.DeleteTexture(Handle);
			GC.SuppressFinalize(this);
		}
	}
}
