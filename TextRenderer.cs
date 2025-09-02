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

		public Texture2D Render(string text, int width, int height, int strokeWidth, int padding)
		{
			width += padding * 2;
			height += padding * 2;
			using SKBitmap bmp = new(width, height, isOpaque: false);
			using SKCanvas cnv = new(bmp);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = SKColors.White,
			};

			using var stroke = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = strokeWidth,
				Color = SKColors.Black,
				StrokeJoin = SKStrokeJoin.Round,
			};

			font.MeasureText(text, out var bounds);
			float x = padding;
			float y = padding;

			cnv.DrawText(text, x, y, SKTextAlign.Left, font, stroke);
			cnv.DrawText(text, x, y, SKTextAlign.Left, font, paint);

			using var img = SKImage.FromBitmap(bmp);

			return new Texture2D(img.PeekPixels().GetPixels(), img.Width, img.Height);
		}

		public Texture2D RenderAutoBounds(string text, int strokeWidth, int paddingX, int paddingY, Vector3 textColor)
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

			float x = paddingX;
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
				cnv.DrawText(text, x, y, SKTextAlign.Left, font, stroke);
			}

			cnv.DrawText(text, x, y, SKTextAlign.Left, font, paint);

			using var img = SKImage.FromBitmap(bmp);

			return new Texture2D(img.PeekPixels().GetPixels(), w, h);
		}

		public T RenderScreenImage<T>(string text, int strokeWidth, int paddingX, int paddingY, Vector3 textColor) where T : ScreenImage
		{
			var tex = RenderAutoBounds(text, strokeWidth, paddingX, paddingY, textColor);
			T result = Instance.Create<T>();
			result.Texture = tex;
			result.Size = Vector2.FromOpenTK(tex.Size);
			return result;
		}

		public T RenderScreenImage<T>(string text, int strokeWidth, int paddingX, int paddingY) where T : ScreenImage
		{
			return RenderScreenImage<T>(text, strokeWidth, paddingX, paddingY, Vector3.One);
		}

		public T RenderScreenImage<T>(string text, int strokeWidth, int padding) where T : ScreenImage
		{
			return RenderScreenImage<T>(text, strokeWidth, padding, padding);
		}
	}
}
