using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Merux
{
	public class Texture2D : IDisposable
	{
		public int Handle { get; private set; }
		public Vector2 Size { get; private set; }

		public Texture2D(Stream stream)
		{
			Image<Rgba32> img = Image.Load<Rgba32>(stream);
			//img.Mutate(x => x.Flip(FlipMode.Vertical));
			int w = img.Width;
			int h = img.Height;
			var pxls = new byte[w * h * 4];
			img.CopyPixelDataTo(pxls);
			Handle = GL.GenTexture();
			Size = new Vector2(w, h);
			GL.BindTexture(TextureTarget.Texture2D, Handle);
			GL.TexImage2D(
				TextureTarget.Texture2D,
				0,
				PixelInternalFormat.Rgba,
				w,
				h,
				0,
				PixelFormat.Rgba,
				PixelType.UnsignedByte,
				pxls
				);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			img.Dispose();
			//GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
		}

		public Texture2D(IntPtr ptr, int w, int h)
		{
			Handle = GL.GenTexture();
			Size = new Vector2(w, h);
			GL.BindTexture(TextureTarget.Texture2D, Handle);
			GL.TexImage2D(
				TextureTarget.Texture2D,
				0,
				PixelInternalFormat.Rgba,
				w,
				h,
				0,
				PixelFormat.Rgba,
				PixelType.UnsignedByte,
				ptr
				);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		}

		public static Texture2D FromPath(string path)
		{
			using (var stream = Merux.LoadStream(path))
				return new Texture2D(stream);
		}

		public void Bind(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2D, Handle);
		}

		public void Dispose()
		{
			GL.DeleteTexture(Handle);
		}
	}
}
