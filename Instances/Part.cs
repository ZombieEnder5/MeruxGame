using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System;
using Merux;
using Merux.Mathematics;
using System.ComponentModel.DataAnnotations;
using Merux.EnumTypes;
using BulletSharp;
using BulletSharp.SoftBody;

namespace Merux.Instances
{
	public class Part : Instance
	{
		protected static int MAX_GJK_ITER = 20;
		private static float DEG_TO_RAD = 0.01745329252f;
		private static float RAD_TO_DEG = 57.2957795131f;

		private static int ENTIRE_DIRTY =	0b0001;
		private static int CFRAME_DIRTY =	0b0010;
		private static int ANCHOR_DIRTY =	0b0100;
		private static int COLLIDE_DIRTY =	0b1000;

		public static Vector3[] BOX_CORNERS = new Vector3[]
		{
			new Vector3(-.5f, -.5f, -.5f),
			new Vector3(-.5f, -.5f, .5f),
			new Vector3(-.5f, .5f, -.5f),
			new Vector3(-.5f, .5f, .5f),
			new Vector3(.5f, -.5f, -.5f),
			new Vector3(.5f, -.5f, .5f),
			new Vector3(.5f, .5f, -.5f),
			new Vector3(.5f, .5f, .5f),
		};

		private Vector3 _color = new Vector3(0.7, 0.7, 0.7);
		public Vector3 Color
		{
			get
			{
				return new Vector3(_color);
			}
			set
			{
				_color = new Vector3(value);
			}
		}

		public float Transparency = 0f;
		public bool Locked = false;

		protected static Shader shader = Game.SHADER_PART;
		protected Texture2DArray texture = new Texture2DArray(new string[] {
			"Textures.smooth.png",
			"Textures.smooth.png",
			"Textures.studs.png",
			"Textures.inlet.png",
			"Textures.smooth.png",
			"Textures.smooth.png",
		});
		protected Mesh mesh = Mesh.CubeMesh;

		protected CFrame realCFrame = CFrame.Identity;
		public CFrame CFrame
		{
			get
			{
				return new CFrame(realCFrame);
			}
			set
			{
				realCFrame = value;
				dirty |= CFRAME_DIRTY;
			}
		}
		public Vector3 Position
		{
			get
			{
				return new Vector3(realCFrame.p);
			}
			set
			{
				realCFrame.p = value;
				dirty |= CFRAME_DIRTY;
			}
		}
		public Vector3 Rotation
		{
			get
			{
				return RAD_TO_DEG * realCFrame.ToEulerAnglesXYZ();
			}
			set
			{
				value *= DEG_TO_RAD;
				realCFrame = CFrame.FromEulerAnglesXYZ(value.X, value.Y, value.Z) + realCFrame.p;
				dirty |= CFRAME_DIRTY;
			}
		}
		public Vector3 Orientation
		{
			get
			{
				return RAD_TO_DEG * realCFrame.ToOrientation();
			}
			set
			{
				value *= DEG_TO_RAD;
				realCFrame = CFrame.FromOrientation(value.X, value.Y, value.Z) + realCFrame.p;
				dirty |= CFRAME_DIRTY;
			}
		}
		internal int dirty = COLLIDE_DIRTY;
		private Vector3 _size = new Vector3(4, 1, 2);
		private bool _anchored = false;
		public Vector3 Velocity
		{
			get
			{
				return Vector3.FromNumerics(rigidBody.LinearVelocity);
			}
			set
			{
				rigidBody.LinearVelocity = value.Numerics();
			}
		}
		public Vector3 AngularVelocity
		{
			get
			{
				return Vector3.FromNumerics(rigidBody.AngularVelocity);
			}
			set
			{
				rigidBody.AngularVelocity = value.Numerics();
			}
		}
		public Vector3 Size
		{
			get
			{
				return new Vector3(_size);
			}
			set
			{
				_size = value;
				dirty |= ENTIRE_DIRTY;
			}
		}
		public bool Anchored
		{
			get
			{
				return _anchored;
			}
			set
			{
				_anchored = value;
				dirty |= ANCHOR_DIRTY;
			}
		}
		public double Mass
		{
			get
			{
				return Size.X * Size.Y * Size.Z * 0.7;
			}
		}

		public float Restitution {
			get
			{
				return rigidBody.Restitution;
			}
			set
			{
				rigidBody.Restitution = value;
			}
		}
		public float Friction
		{
			get
			{
				return rigidBody.Friction;
			}
			set
			{
				rigidBody.Friction = value;
			}
		}
		private bool _canCollide = true;
		public bool CanCollide
		{
			get
			{
				return _canCollide;
			}
			set
			{
				_canCollide = value;
				dirty |= COLLIDE_DIRTY;
			}
		}

		internal RigidBody rigidBody;
		MotionState motionState;

		static internal readonly Dictionary<(int, int, int), BoxShape> boxCache = new();

		public Part() : base()
		{
			Name = "Part";

			setupRigidbody();
		}

		private void disposeRigidbody()
		{
			var list = Game.Workspace.rigidReference.Where(p => p.Value == this).ToList();
			foreach (var item in list)
				Game.Workspace.removeRigidBody(item.Key);
			motionState.Dispose();
			rigidBody.Dispose();
		}

		private void setupRigidbody()
		{
			var start = realCFrame.ToNumericsMatrix();
			var shape = GetBoxShape(Size.Numerics() * .5f);

			motionState = new DefaultMotionState(start);
			shape.CalculateLocalInertia((float)Mass, out var localInertia);

			var rbInfo = new RigidBodyConstructionInfo((float)Mass, motionState, shape, localInertia)
			{
				Friction = 0.5f,
				Restitution = 0.7f,
				LinearDamping = 0.0f,
				AngularDamping = 0.0f,
			};

			rigidBody = new RigidBody(rbInfo);
			rbInfo.Dispose();
		}

		public static CollisionShape GetBoxShape(System.Numerics.Vector3 halfSize)
		{
			var key = ((int)(halfSize.X * 1000f), (int)(halfSize.Y * 1000f), (int)(halfSize.Z * 1000f));
			if (!boxCache.ContainsKey(key))
				boxCache[key] = new BoxShape(halfSize);
			return boxCache[key];
		}

		public override void Dispose()
		{
			base.Dispose();
			texture.Dispose();
			disposeRigidbody();
		}

		public void Render(OpenTK.Mathematics.Matrix4 view, OpenTK.Mathematics.Matrix4 projection)
		{
			Debug.Print("heya",Name,Position);
			OpenTK.Mathematics.Matrix4 model =
				OpenTK.Mathematics.Matrix4.CreateScale(Size.OpenTK()) *
				new OpenTK.Mathematics.Matrix4(CFrame.GetRotationMatrix().OpenTK().Inverted()) *
				OpenTK.Mathematics.Matrix4.CreateTranslation(Position.OpenTK())
				;

			shader.Use();
			shader.SetVector3("size", Size.OpenTK());
			shader.SetMatrix4("model", model);
			shader.SetMatrix4("view", view);
			shader.SetMatrix4("projection", projection);
			Type normType = typeof(NormalId);
			//i'll fix this later. it's not a big problem right now
			foreach (int i in Enum.GetValues(normType))
				shader.SetInt($"FACE_{Enum.GetName(normType, i).ToUpper()}", i);

			texture.Bind();
			shader.SetInt("atlas", 0);
			shader.SetVector3("baseColor", Color.OpenTK());
			shader.SetFloat("transparency", Transparency);
			shader.SetVector2("textureSize", texture.Size);

			GL.BindVertexArray(mesh.VAO);
			GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedShort, 0);
		}

		private bool Vector3HasNaN(Vector3 v)
		{
			return double.IsNaN(v.X) || double.IsNaN(v.Y) || double.IsNaN(v.Z);
		}

		private void keepMeSafe()
		{
			if (Vector3HasNaN(CFrame.XVector) || Vector3HasNaN(CFrame.YVector) || Vector3HasNaN(CFrame.ZVector))
				CFrame = CFrame.Identity + Position;
			if (Vector3HasNaN(AngularVelocity))
				AngularVelocity = new Vector3(0, 0, 0);
			if (Vector3HasNaN(Velocity))
				Velocity = new Vector3(0, 0, 0);
		}

		internal bool dirtyFlagSet(int flag)
		{
			return (dirty & flag) == flag;
		}

		public override void Tick(float deltaTime)
		{
			keepMeSafe();
			if (!rigidBody.IsDisposed)
			{
				if (dirtyFlagSet(COLLIDE_DIRTY))
				{
					Debug.Print(CanCollide);
					if (!CanCollide && !rigidBody.IsDisposed)
					{
						Debug.Print("A");
						if (IsDescendantOf(Game.Workspace))
							Game.Workspace.removeRigidBody(rigidBody);
						disposeRigidbody();
					}
					else if (!rigidBody.IsDisposed)
					{
						Debug.Print("B");
						setupRigidbody();
						if (IsDescendantOf(Game.Workspace))
							Game.Workspace.addRigidBody(this, rigidBody);
					}
				}
			}
			if (!rigidBody.IsDisposed)
			{
				if (dirtyFlagSet(ENTIRE_DIRTY))
				{
					if (IsDescendantOf(Game.Workspace))
					{
						Game.Workspace.removeRigidBody(rigidBody);
					}
					disposeRigidbody();
					setupRigidbody();
					if (IsDescendantOf(Game.Workspace))
					{
						Game.Workspace.addRigidBody(this, rigidBody);
					}
				}
				if (dirtyFlagSet(CFRAME_DIRTY))
				{
					rigidBody.WorldTransform = realCFrame.ToNumericsMatrix();
				}
				if (dirtyFlagSet(ANCHOR_DIRTY))
				{
					if (Anchored)
					{
						rigidBody.CollisionFlags |= CollisionFlags.KinematicObject;
						rigidBody.ActivationState = ActivationState.DisableDeactivation;
					}
					else
					{
						rigidBody.CollisionFlags &= ~CollisionFlags.KinematicObject;
						rigidBody.ActivationState = ActivationState.ActiveTag;
						rigidBody.CollisionShape.CalculateLocalInertia((float)Mass, out var localInertia);
						rigidBody.SetMassProps((float)Mass, localInertia);
					}
				}
				realCFrame = CFrame.FromNumericsMatrix(rigidBody.WorldTransform);
			}
			dirty = 0;
		}

		public override string ToString()
		{
			return new string(Name);
		}
	}
}


