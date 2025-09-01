using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Merux
{
	internal class MeruxWindow : GameWindow
	{
		public MeruxWindow() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
		{
			Image<Rgba32> iconImage;
			using (Stream stream = Merux.LoadStream("Textures.favicon.png"))
				iconImage = (Image<Rgba32>)SixLabors.ImageSharp.Image.Load(stream);
			var iconBytes = new byte[iconImage.Width * iconImage.Height * 4];
			iconImage.CopyPixelDataTo(iconBytes);
			Icon = new OpenTK.Windowing.Common.Input.WindowIcon(new OpenTK.Windowing.Common.Input.Image(iconImage.Width, iconImage.Height, iconBytes));
			if (SupportsRawMouseInput)
				RawMouseInput = true;
			Merux.window = this;
		}

		protected override void OnLoad()
		{
			base.OnLoad();
			Merux.OnLoad();
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			Merux.OnRenderFrame(args);
			base.OnRenderFrame(args);
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			Merux.OnResize(e);
			base.OnResize(e);
		}

		protected override void OnUnload()
		{
			Merux.OnUnload();
			base.OnUnload();
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			Merux.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			Merux.OnMouseUp(e);
		}
	}

	internal class Merux
	{
		static Vector2 mousePos;
		static Vector2 lastMousePos;
		static ScreenImage cursorIcon;
		private static int _grabTimer = 0;
		public static int grabTimer { get { return _grabTimer; } private set { _grabTimer = value; } }

		internal static Assembly gameAssembly = Assembly.GetExecutingAssembly();

		static public Game Game;
		static public MeruxWindow window;

		public static Stream LoadStream(string path)
		{
			if (gameAssembly == null)
				throw new NullReferenceException("how did we get here?");
			Stream stream = gameAssembly.GetManifestResourceStream("Merux." + path);
			if (stream == null)
				throw new NullReferenceException($"THERE'S NO STREAM! AAAAAA!! THE GHOST OF Merux.{path}");
			return stream;
		}

		internal static void OnLoad()
		{
			window.Title = "Merux";
			window.CursorState = CursorState.Hidden;
			using (var stream = LoadStream("Textures.cursor.png"))
				cursorIcon = new ScreenImage(stream);
			Game = new();
		}

		public static Vector2 GetMousePosition()
		{
			return new Vector2(mousePos.X, mousePos.Y); // just to make sure nobody modifies it
		}

		internal static void OnRenderFrame(FrameEventArgs args)
		{
			Game.TickAndRender((float)args.Time, window.ClientSize);
			if (!window.MouseState.IsButtonDown(MouseButton.Right))
			{
				lastMousePos = mousePos;
				mousePos = new Vector2(window.MouseState.X, window.MouseState.Y);
				grabTimer = 0;
			}
			else
			{
				mousePos = lastMousePos;
				grabTimer += 1;
			}
			var mPos = new Mathematics.GuiDim(0, 0, mousePos.X, mousePos.Y);
			cursorIcon.Position = mPos;
			GL.Disable(EnableCap.DepthTest);
			cursorIcon.Render();
			GL.Enable(EnableCap.DepthTest);
			window.SwapBuffers();
		}

		internal static void OnUnload()
		{
			cursorIcon.Dispose();
			Game.Dispose();
		}

		internal static void OnResize(ResizeEventArgs e)
		{
			int fbWidth, fbHeight;
			unsafe
			{
				GLFW.GetFramebufferSize(window.WindowPtr, out fbWidth, out fbHeight);
			}
			GL.Viewport(0, 0, fbWidth, fbHeight);
		}

		internal static void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.Button == MouseButton.Right)
				window.CursorState = CursorState.Grabbed;
		}

		internal static void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.Button == MouseButton.Right)
				window.CursorState = CursorState.Hidden;
		}
	}

	class Program
	{
		static void Main()
		{
			using var game = new MeruxWindow();
			game.Run();
		}
	}
}