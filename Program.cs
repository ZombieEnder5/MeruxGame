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
	class Merux : GameWindow
	{
		float[] mVerts =
		{
			0f, 0f, 0f, 1f,
			1f, 0f, 1f, 1f,
			1f, 1f, 1f, 0f,
			0f, 1f, 0f, 0f
		};
		uint[] mIdxs =
		{
			0, 1, 2,
			2, 3, 0
		};
		int mVAO, mVBO, mEBO;
		Shader mShader;
		Texture2D mTex;
		Vector2 mousePos;
		Vector2 lastMousePos;
		private int _grabTimer = 0;
		public int grabTimer { get { return _grabTimer; } private set { _grabTimer = value; } }

		internal static Assembly gameAssembly = Assembly.GetExecutingAssembly();

		public static Stream LoadStream(string path)
		{
			if (gameAssembly == null)
				throw new NullReferenceException("how did we get here?");
			Stream stream = gameAssembly.GetManifestResourceStream("Merux." + path);
			if (stream == null)
				throw new NullReferenceException($"THERE'S NO STREAM! AAAAAA!! THE GHOST OF {path}");
			return stream;
		}

		public Merux() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
		{
			Image<Rgba32> iconImage;
			using (Stream stream = LoadStream("Textures.favicon.png"))
				iconImage = (Image<Rgba32>)SixLabors.ImageSharp.Image.Load(stream);
			var iconBytes = new byte[iconImage.Width * iconImage.Height * 4];
			iconImage.CopyPixelDataTo(iconBytes);
			Icon = new OpenTK.Windowing.Common.Input.WindowIcon(new OpenTK.Windowing.Common.Input.Image(iconImage.Width, iconImage.Height, iconBytes));
			if (SupportsRawMouseInput)
				RawMouseInput = true;
		}

		protected override void OnLoad()
		{
			base.OnLoad();
			Title = "Merux";
			Game.SetWindow(this);
			CursorState = CursorState.Hidden;
			mTex = new Texture2D("Textures.cursor.png");
			mVAO = GL.GenVertexArray();
			mVBO = GL.GenBuffer();
			mEBO = GL.GenBuffer();
			GL.BindVertexArray(mVAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, mVerts.Length * sizeof(float), mVerts, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, mEBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, mIdxs.Length * sizeof(uint), mIdxs, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
			GL.EnableVertexAttribArray(1);
			mShader = Shader.FromPath("Shaders.Mouse");
			Game.Start();
		}

		public Vector2 GetMousePosition()
		{
			return new Vector2(mousePos.X, mousePos.Y); // just to make sure nobody modifies it
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			Game.TickAndRender((float)args.Time, ClientSize);
			int fbWidth, fbHeight;
			unsafe
			{
				GLFW.GetFramebufferSize(WindowPtr, out fbWidth, out fbHeight);
			}
			if (!MouseState.IsButtonDown(MouseButton.Right))
			{
				lastMousePos = mousePos;
				mousePos = new Vector2(MouseState.X, MouseState.Y);
				grabTimer = 0;
			}
			else
			{
				mousePos = lastMousePos;
				grabTimer += 1;
			}
			Debug.Print(MousePosition);
			Vector2 mPos = new Vector2(mousePos.X, fbHeight - mousePos.Y);
			mShader.Use();
			mShader.SetVector2("uPos", mPos);
			mShader.SetVector2("uSize", new Vector2(64, 64));
			mShader.SetVector2("uScrSize", new Vector2(fbWidth, fbHeight));
			mTex.Bind();
			GL.BindVertexArray(mVAO);
			GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);
			SwapBuffers();
			base.OnRenderFrame(args);
		}

		protected override void OnUnload()
		{
			mTex.Dispose();
			Game.Dispose();
			base.OnUnload();
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			int fbWidth, fbHeight;
			unsafe
			{
				GLFW.GetFramebufferSize(WindowPtr, out fbWidth, out fbHeight);
			}
			GL.Viewport(0, 0, fbWidth, fbHeight);
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButton.Right)
				CursorState = CursorState.Grabbed;
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			if (e.Button == MouseButton.Right)
				CursorState = CursorState.Hidden;
		}
	}

	class Program
	{
		static void Main()
		{
			using var game = new Merux();
			game.Run();
		}
	}
}