using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Merux
{
	public class Cubemap : IDisposable
	{
		public int Handle { get; private set; }

		public Cubemap(string[] paths)
		{
			Handle = GL.GenTexture();
			GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
			for (int i = 0; i < 6; i++)
			{
				using Stream stream = Merux.LoadStream(paths[i]);
				using var img = Image.Load<Rgb24>(stream);
				int width = img.Width, height = img.Height;
				var pixels = new byte[width * height * 3];
				img.CopyPixelDataTo(pixels);
				GL.TexImage2D(
					TextureTarget.TextureCubeMapPositiveX + i,
					0,
					PixelInternalFormat.Rgb,
					width, height,
					0,
					PixelFormat.Rgb, PixelType.UnsignedByte,
					pixels
				);
			}
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
		}

		public void Bind(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
		}

		public void Dispose()
		{
			GL.DeleteTexture(Handle);
			GC.SuppressFinalize(this);
		}
	}
}
