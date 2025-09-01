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
using System.Runtime.InteropServices;
using static OpenTK.Graphics.OpenGL.GL;
using Merux.Tools;

/*
 * before I start, I'd like to admit shamefully that I did use ChatGPT for some parts of the code in this project.
 * however, in my defense, Google does not provide relevant results very often. neither does Bing.
 * and occasionally I just get completely stuck at parts that I cannot figure out at all.
 * THOSE are the only times I use ChatGPT. no other time.
 * I'd really like to keep this project going without a brick wall stopping me in my tracks permanently.
 */

namespace Merux
{

    public class Game : Instance
	{
		public Workspace Workspace = new Workspace();
		public static Shader SHADER_PART = Shader.FromPath("Shaders.Part");
		public static Shader SHADER_SKY = Shader.FromPath("Shaders.Sky");
		public static Shader SHADER_TEXT = Shader.FromPath("Shaders.Text");
		public float Time = 0f;
		public Random RandomSource = new Random();

		public Part selector;
		public bool hovering = false;

		private bool debounce = false;

		public int PUT_SND_SRC;
		public int DST_SND_SRC;
		public int CLK_SND_SRC;

		public event Action? MouseLeftClickEvent;
		public event Action? MouseLeftReleaseEvent;

		readonly internal TextRenderer rend;

		static readonly int TOOL_ICON_SIZE = 200;

		ScreenImage helpText = new ScreenImage(Merux.LoadStream("Textures.helptext.png"))
		{
			Position = new GuiDim(1, 1, 0, 0),
			AnchorPoint = new Vector2(1, 1),
			Size = new Vector2(512, 64)
		};

		ToolScript[] tools;
		Dictionary<ToolScript, ScreenImage> toolIcons = new();
		int toolMode = 0;
		int lastToolMode = -1;

		RayHit? mouseHitCache = null;

		protected static Cubemap skyCube = new Cubemap(new string[] {
			"Textures.sky_ri.png", // right
			"Textures.sky_le.png", // left
			"Textures.sky_to.png", // top
			"Textures.sky_bo.png", // bottom
			"Textures.sky_fr.png", // back
			"Textures.sky_ba.png", // front
		});

		Camera mainCamera = new Camera();

		internal Merux window { get; private set; }
		public Vector2 windowExtents { 
			get
			{
				unsafe
				{
					GLFW.GetFramebufferSize(Merux.window.WindowPtr, out int fbWidth, out int fbHeight);
					return new Vector2(fbWidth, fbHeight);
				}
			}
		}

		internal Game()
		{
			tools = new ToolScript[]
			{
				Create<CreateTool>(Workspace),
				Create<DeleteTool>(Workspace),
			};

			for (int i = 0; i < tools.Length; i++)
			{
				var tool = tools[i];
				using (var stream = Merux.LoadStream(tool.GetIconPath()))
				{
					ScreenImage icon = new(stream);
					icon.Size = Vector2.One * TOOL_ICON_SIZE;
					icon.Position = new GuiDim(0, 1, TOOL_ICON_SIZE * i, 0);
					icon.AnchorPoint = new Vector2(0, 1);
					toolIcons[tool] = icon;
				}
			}

			using (var stream = Merux.LoadStream("Fonts.meruxfont.ttf"))
			{
				//var buffer = new byte[(int)stream.Length];
				//stream.Read(buffer, 0, buffer.Length);
				rend = new(stream);
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
			mainCamera.Parent = Workspace;
			selector = Instance.Create<Part>(Workspace);
			selector.CanCollide = false;
			selector.Color = new(0, 1, 1);
			selector.Size = new(4, 1, 2);
			selector.Anchored = true;
			selector.Transparency = 0.5f;
			selector.Name = "selector";
			Part baseplate = Instance.Create<Part>(Workspace);
			baseplate.Size = new Vector3(64f, 10f, 64f);
			baseplate.Position = new Vector3(0f, -5f, 0f);
			baseplate.Anchored = true;
			baseplate.Name = "Baseplate I";
			baseplate.Locked = true;
			Part baseplate2 = Instance.Create<Part>(Workspace);
			baseplate2.Size = new Vector3(64f, 32f, 4f);
			baseplate2.Position = new Vector3(0f, -5f, 34f);
			baseplate2.Anchored = true;
			baseplate2.Name = "Baseplate II";
			baseplate2.Locked = true;
			Part baseplate3 = Instance.Create<Part>(Workspace);
			baseplate3.Size = new Vector3(64f, 32f, 4f);
			baseplate3.Position = new Vector3(0f, -5f, -34f);
			baseplate3.Anchored = true;
			baseplate3.Name = "Baseplate III";
			baseplate3.Locked = true;
			Part baseplate4 = Instance.Create<Part>(Workspace);
			baseplate4.Size = new Vector3(4f, 32f, 64f);
			baseplate4.Position = new Vector3(34f, -5f, 0f);
			baseplate4.Anchored = true;
			baseplate4.Name = "Baseplate IV";
			baseplate4.Locked = true;
			Part baseplate5 = Instance.Create<Part>(Workspace);
			baseplate5.Size = new Vector3(4f, 32f, 64f);
			baseplate5.Position = new Vector3(-34f, -5f, 0f);
			baseplate5.Anchored = true;
			baseplate5.Name = "Baseplate V";
			baseplate5.Locked = true;
			mainCamera.SetLook(new Vector3(1, 0, 0));
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

		public RayHit? GetMouseHit()
		{
			if (mouseHitCache.HasValue) return mouseHitCache;
			float ratio = (float)windowExtents.X / (float)windowExtents.Y;

			var mpos = Merux.GetMousePosition();
			double vx = 2.0 * mpos.X / windowExtents.X - 1.0;
			double vy = 1.0 - 2.0 * mpos.Y / windowExtents.Y;

			double tanHY = Math.Tan(mainCamera.FieldOfView * .5);
			double tanHX = tanHY * ratio;

			var localDir = new Vector3(vx * tanHX, vy * tanHY, -1).Unit;
			hovering = Workspace.Raycast(mainCamera.CFrame.p, mainCamera.CFrame.Rotation * localDir * 100_000, out RayHit contact);
			if (hovering)
				return contact;
			return null;
		}

		public void TickAndRender(float deltaTime, OpenTK.Mathematics.Vector2i winSize)
		{
			mouseHitCache = null;

			Time += deltaTime;
			var mouse = Merux.window.MouseState;
			var mouseDelta = mouse.Delta;
			if (mouse.IsButtonDown(MouseButton.Right) && Merux.grabTimer > 1)
			{
				if (mouseDelta.LengthSquared > 0)
				{
					mainCamera.TurnCamera(Vector2.FromOpenTK(mouseDelta));
				}
			}
			if (Merux.window.KeyboardState.IsKeyPressed(Keys.D1))
				toolMode = 0;
			else if(Merux.window.KeyboardState.IsKeyPressed(Keys.D2))
				toolMode = 1;
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
			var projection = OpenTK.Mathematics.Matrix4.CreatePerspectiveFieldOfView(mainCamera.FieldOfView, ratio, near, far);

			if (lastToolMode != toolMode)
			{
				if (lastToolMode > -1)
				{
					var last = tools[lastToolMode];
					last.Unequip();
				}
				if (toolMode > -1)
				{
					var equipping = tools[toolMode];
					equipping.Equip();
				}
				lastToolMode = toolMode;
			}

			bool leftMBDown = mouse.IsButtonDown(MouseButton.Left);
			if (leftMBDown ^ debounce)
			{
				debounce = leftMBDown;
				if (leftMBDown)
					MouseLeftClickEvent?.Invoke();
				else
					MouseLeftReleaseEvent?.Invoke();
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
			//GL.DepthFunc(DepthFunction.Less);
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

			helpText.Render();

			for(int i = 0; i < tools.Length; i++)
			{
				var tool = tools[i];
				var icon = toolIcons[tool];
				icon.TintAlpha = i == toolMode ? 0f : 0.2f;
				icon.Render();
			}
		}

		public override void Dispose()
		{
			helpText.Dispose();
			skyCube.Dispose();
			base.Dispose();
		}

		internal T EpsilonMaxBy<T>(T[] values, Func<T, double> selectFunc, double epsilon)
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
