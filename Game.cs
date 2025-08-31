using Merux.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using Merux.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.CompilerServices;
using OpenTK.Audio.OpenAL;

namespace Merux
{
    public class Game
	{
		public static Workspace Workspace = new Workspace();
		public static Shader SHADER_PART = Shader.FromPath("Shaders.Part");
		public static Shader SHADER_SKY = Shader.FromPath("Shaders.Sky");
		public static float Time = 0f;
		public static Random randomSource = new Random();

		static Part selector;
		static private bool hovering = false;

		static private bool debounce1 = false;
		static private bool debounce2 = false;

		static int PUT_SND_SRC;
		static int DST_SND_SRC;
		static int CLK_SND_SRC;

		public static FontSystem fontSys = new FontSystem();
		public static QFont GAME_FONT;
		
		static QFontDrawing createText = new QFontDrawing();
		static QFontDrawing deleteText = new QFontDrawing();

		protected static Cubemap skyCube = new Cubemap(new string[] {
			"Textures.sky_ri.png", // right
			"Textures.sky_le.png", // left
			"Textures.sky_to.png", // top
			"Textures.sky_bo.png", // bottom
			"Textures.sky_fr.png", // back
			"Textures.sky_ba.png", // front
		});

		static Camera mainCamera = new Camera();

		static internal Merux window { get; private set; }
		public static Vector2 windowExtents { 
			get
			{
				unsafe
				{
					GLFW.GetFramebufferSize(window.WindowPtr, out int fbWidth, out int fbHeight);
					return new Vector2(fbWidth, fbHeight);
				}
			}
		}

		internal static void SetWindow(Merux _window)
		{
			FontStashSharp.Rasterizers.StbTrueTypeSharp.
			if (window != null) throw new UnauthorizedAccessException("Window already set.");
			window = _window;
		}

		internal static int BufferFromWaveform(Stream stream)
		{
			using var reader = new NAudio.Wave.WaveFileReader(stream);
			var buffer = AL.GenBuffer();
			ALFormat fmt = reader.WaveFormat.Channels switch
			{
				1 => ALFormat.Mono16,
				2 => ALFormat.Stereo16,
				_ => throw new NotImplementedException("not mono or stereo!?")
			};
			var data = new byte[reader.Length];
			reader.Read(data, 0, data.Length);
			AL.BufferData(buffer, fmt, data, reader.WaveFormat.SampleRate);
			return buffer;
		}

		public static void Start()
		{
			using (var stream = Merux.LoadStream("Fonts.meruxfont.otf"))
			{
				var buffer = new byte[(int)stream.Length];
				stream.Read(buffer, 0, (int)stream.Length);

				GAME_FONT = new(buffer, 32f, config);
			}

			ALDevice sdev = ALC.OpenDevice(null);
			ALContext ctx = ALC.CreateContext(sdev, (int[]?)null);
			ALC.MakeContextCurrent(ctx);
			int sbuf1 = BufferFromWaveform(Merux.LoadStream("Sounds.put.wav"));
			int sbuf2 = BufferFromWaveform(Merux.LoadStream("Sounds.click.wav"));
			int sbuf3 = BufferFromWaveform(Merux.LoadStream("Sounds.destroy.wav"));
			PUT_SND_SRC = AL.GenSource();
			CLK_SND_SRC = AL.GenSource();
			DST_SND_SRC = AL.GenSource();
			AL.Source(PUT_SND_SRC, ALSourcei.Buffer, sbuf1);
			AL.Source(CLK_SND_SRC, ALSourcei.Buffer, sbuf2);
			AL.Source(DST_SND_SRC, ALSourcei.Buffer, sbuf3);

			GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
			mainCamera.SetParent(Workspace);
			selector = new Part();
			selector.SetParent(Workspace);
			selector.CanCollide = false;
			selector.Color = new(0, 1, 1);
			selector.Size = new(4, 1, 2);
			selector.Anchored = true;
			selector.Transparency = 0.5f;
			selector.Name = "selector";
			Part baseplate = new Part();
			baseplate.SetParent(Workspace);
			baseplate.Size = new Vector3(64f, 10f, 64f);
			baseplate.Position = new Vector3(0f, -5f, 0f);
			baseplate.Anchored = true;
			baseplate.Name = "Baseplate I";
			baseplate.Locked = true;
			Part baseplate2 = new Part();
			baseplate2.SetParent(Workspace);
			baseplate2.Size = new Vector3(64f, 32f, 4f);
			baseplate2.Position = new Vector3(0f, -5f, 34f);
			baseplate2.Anchored = true;
			baseplate2.Name = "Baseplate II";
			baseplate2.Locked = true;
			Part baseplate3 = new Part();
			baseplate3.SetParent(Workspace);
			baseplate3.Size = new Vector3(64f, 32f, 4f);
			baseplate3.Position = new Vector3(0f, -5f, -34f);
			baseplate3.Anchored = true;
			baseplate3.Name = "Baseplate III";
			baseplate3.Locked = true;
			Part baseplate4 = new Part();
			baseplate4.SetParent(Workspace);
			baseplate4.Size = new Vector3(4f, 32f, 64f);
			baseplate4.Position = new Vector3(34f, -5f, 0f);
			baseplate4.Anchored = true;
			baseplate4.Name = "Baseplate IV";
			baseplate4.Locked = true;
			Part baseplate5 = new Part();
			baseplate5.SetParent(Workspace);
			baseplate5.Size = new Vector3(4f, 32f, 64f);
			baseplate5.Position = new Vector3(-34f, -5f, 0f);
			baseplate5.Anchored = true;
			baseplate5.Name = "Baseplate V";
			baseplate5.Locked = true;
			mainCamera.SetLook(new Vector3(1, 0, 0));
		}

		public static void TickAndRender(float deltaTime, OpenTK.Mathematics.Vector2i winSize)
		{
			Time += deltaTime;
			var mouse = window.MouseState;
			var mouseDelta = mouse.Delta;
			if (mouse.IsButtonDown(MouseButton.Right) && window.grabTimer > 1)
			{
				if (mouseDelta.LengthSquared > 0)
				{
					mainCamera.TurnCamera(Vector2.FromOpenTK(mouseDelta));
				}
			}
			if (mouse.ScrollDelta.Y > 0)
				mainCamera.ZoomIn(CLK_SND_SRC);
			else if (mouse.ScrollDelta.Y < 0)
				mainCamera.ZoomOut(CLK_SND_SRC);

			var view =
				OpenTK.Mathematics.Matrix4.CreateTranslation(-mainCamera.CFrame.p.OpenTK())
				* new OpenTK.Mathematics.Matrix4(mainCamera.CFrame.GetRotationMatrix().OpenTK());
			float ratio = (float)windowExtents.X / (float)windowExtents.Y;
			float near = 0.1f;
			float far = 10000f;
			var fov = MathF.PI * 70f / 180f;
			var projection = OpenTK.Mathematics.Matrix4.CreatePerspectiveFieldOfView(fov, ratio, near, far);

			var mpos = window.GetMousePosition();
			double vx = 2.0 * mpos.X / windowExtents.X - 1.0;
			double vy = 1.0 - 2.0 * mpos.Y / windowExtents.Y;

			double tanHY = Math.Tan(fov * .5);
			double tanHX = tanHY * ratio;

			var localDir = new Vector3(vx * tanHX, vy * tanHY, -1).Unit;
			hovering = Workspace.Raycast(mainCamera.CFrame.p, mainCamera.CFrame.Rotation * localDir * 100_000, out RayHit contact);
			if (hovering)
			{
				var half = selector.Size * .5;
				Vector3 x = half.X * Vector3.XAxis, y = half.Y * Vector3.YAxis, z = half.Z * Vector3.ZAxis;
				var vert = EpsilonMaxBy(new[] {
					-x - y - z,
					-x - y + z,
					-x + y - z,
					-x + y + z,
					x - y - z,
					x - y + z,
					x + y - z,
					x + y + z,
				},v => v.Dot(contact.normal),1e-2);
				selector.Position = contact.point + vert;
				Debug.Print(contact.part);
			}

			bool leftMBDown = mouse.IsButtonDown(MouseButton.Left);
			bool midMBDown = mouse.IsButtonDown(MouseButton.Middle);
			if (leftMBDown ^ debounce1)
			{
				debounce1 = leftMBDown;
				if (leftMBDown && hovering)
				{
					var inst = new Part();
					inst.SetParent(Workspace);
					inst.Position = selector.Position;
					inst.Color = selector.Color;
					inst.Size = new Vector3(4, 1, 2);
					selector.Color = new Vector3(randomSource.NextDouble(), randomSource.NextDouble(), randomSource.NextDouble());
					AL.SourcePlay(PUT_SND_SRC);
				}
			}
			if (midMBDown ^ debounce2)
			{
				debounce2 = midMBDown;
				if (midMBDown && hovering && !contact.part.Locked)
				{
					contact.part.Destroy();
					AL.SourcePlay(DST_SND_SRC);
				}
			}

			Workspace.Tick(deltaTime);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.DepthFunc(DepthFunction.Lequal);
			GL.Disable(EnableCap.CullFace);
			SHADER_SKY.Use();
			SHADER_SKY.SetMatrix4("uView", view);
			SHADER_SKY.SetMatrix4("uProj", projection);
			GL.BindVertexArray(Mesh.CubeMesh.VAO);
			skyCube.Bind();
			GL.DrawElements(PrimitiveType.Triangles, Mesh.CubeMesh.IndexCount, DrawElementsType.UnsignedShort, 0);
			GL.DepthFunc(DepthFunction.Less);
			GL.Enable(EnableCap.CullFace);
			var list = Workspace.GetDescendantsOfClass<Part>().ToList();
			var opaque = list.Where(p => p.Transparency <= 0f);
			var transparent = list.Where(p => p.Transparency > 0f).OrderByDescending(p => (p.Position - mainCamera.CFrame.p).MagnitudeSqr);
			foreach (Part part in opaque)
				part.Render(view, projection);
			if (transparent.Count() > 0)
			{
				GL.DepthMask(false);
				foreach (Part part in transparent)
					part.Render(view, projection);
				GL.DepthMask(true);
			}
		}

		public static void Dispose()
		{
			skyCube.Dispose();
			Workspace.Dispose();
		}

		internal static T EpsilonMaxBy<T>(T[] values, Func<T, double> selectFunc, double epsilon)
		{
			T best = values[0];
			double amt = selectFunc(best);
			if (values.Length > 1)
				foreach(T value in values)
				{
					double cur = selectFunc(value);
					if (Math.Abs(cur - amt) > epsilon && cur > amt)
					{
						amt = cur;
						best = value;
					}
				}
			return best;
		}
	}
}
