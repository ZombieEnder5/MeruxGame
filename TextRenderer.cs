using Merux.Instances;
using Merux.Mathematics;
using SkiaSharp;

namespace Merux
{
	internal class TextRenderer
	{
		SKTypeface face;
		SKFont font;

		public TextRenderer(string fontPath, float textSize)
		{
			var stream = Merux.LoadStream(fontPath);
			face = SKTypeface.FromStream(stream);
			font = new SKFont(face)
			{
				Size = textSize,
			};
		}

		public Texture2D Render(string text, int width, int height, int strokeWidth, Vector3 textColor, SKTextAlign align)
		{
			font.MeasureText(text, out var bounds);
			using SKBitmap bmp = new(width, height, isOpaque: false);
			using SKCanvas cnv = new(bmp);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = new SKColor((byte)(textColor.X * 255f), (byte)(textColor.Y * 255f), (byte)(textColor.Z * 255f)),
			};

			float x = width * align switch
			{
				SKTextAlign.Left => 0f,
				SKTextAlign.Center => 0.5f,
				SKTextAlign.Right => 1f,
				_ => 0f,
			};
			float y = -bounds.MidY + height * .5f;

			if (strokeWidth > 0)
			{
				using var stroke = new SKPaint
				{
					IsAntialias = true,
					Style = SKPaintStyle.Stroke,
					StrokeWidth = strokeWidth,
					Color = SKColors.Black,
					StrokeJoin = SKStrokeJoin.Round,
				};
				cnv.DrawText(text, x, y, align, font, stroke);
			}

			cnv.DrawText(text, x, y, align, font, paint);

			using var img = SKImage.FromBitmap(bmp);

			return new Texture2D(img.PeekPixels().GetPixels(), width, height);
		}

		public Texture2D RenderAutoBounds(string text, int strokeWidth, int paddingX, int paddingY, Vector3 textColor, SKTextAlign align)
		{
			font.MeasureText(text, out var bounds);
			int w = (int)bounds.Width + paddingX * 2;
			int h = (int)bounds.Height + paddingY * 2;

			using SKBitmap bmp = new(w, h, isOpaque: false);
			using SKCanvas cnv = new(bmp);
			cnv.Clear(SKColors.Transparent);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = new SKColor((byte)(textColor.X * 255f), (byte)(textColor.Y * 255f), (byte)(textColor.Z * 255f)),
			};

			float x = paddingX + bounds.Right * align switch
			{
				SKTextAlign.Left => 0f,
				SKTextAlign.Center => 0.5f,
				SKTextAlign.Right => 1f,
				_ => 0f,
			};
			float y = paddingY - bounds.Top;

			if (strokeWidth > 0)
			{
				using var stroke = new SKPaint
				{
					IsAntialias = true,
					Style = SKPaintStyle.Stroke,
					StrokeWidth = strokeWidth,
					Color = SKColors.Black,
					StrokeJoin = SKStrokeJoin.Round,
				};
				cnv.DrawText(text, x, y, align, font, stroke);
			}

			cnv.DrawText(text, x, y, align, font, paint);

			using var img = SKImage.FromBitmap(bmp);

			return new Texture2D(img.PeekPixels().GetPixels(), w, h);
		}

		public T RenderScreenImage<T>(string text, int strokeWidth, int paddingX, int paddingY, Vector3 textColor, SKTextAlign align) where T : ScreenImage
		{
			var tex = RenderAutoBounds(text, strokeWidth, paddingX, paddingY, textColor, align);
			T result = Instance.Create<T>();
			result.Texture = tex;
			result.Size = Vector2.FromOpenTK(tex.Size);
			return result;
		}

		public T RenderScreenImage<T>(string text, int strokeWidth, int paddingX, int paddingY, SKTextAlign align) where T : ScreenImage
		{
			return RenderScreenImage<T>(text, strokeWidth, paddingX, paddingY, Vector3.One, align);
		}

		public T RenderScreenImage<T>(string text, int strokeWidth, int padding, SKTextAlign align) where T : ScreenImage
		{
			return RenderScreenImage<T>(text, strokeWidth, padding, padding, align);
		}
	}
}
