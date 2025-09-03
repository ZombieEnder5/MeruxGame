using BulletSharp;
using Merux.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merux.Instances
{
	public class World : Instance
	{
		public new string Name = "Workspace";
		public float Gravity
		{
			get
			{
				return PhysWorld.Gravity.Y;
			}
			set
			{
				PhysWorld.Gravity = new(0f, -value, 0f);
			}
		}
		readonly DefaultCollisionConfiguration physConf;
		readonly CollisionDispatcher physDispatcher;
		readonly BroadphaseInterface physBroadphase;
		readonly SequentialImpulseConstraintSolver physSolver;

		public Dictionary<RigidBody, Part> rigidReference = new Dictionary<RigidBody, Part>();
		public DiscreteDynamicsWorld PhysWorld { get; }

		public World() : base()
		{
			Name = "Workspace";
			physConf = new();
			physDispatcher = new(physConf);
			physBroadphase = new DbvtBroadphase();
			physSolver = new();
			PhysWorld = new DiscreteDynamicsWorld(physDispatcher, physBroadphase, physSolver, physConf)
			{
				Gravity = new(0f, -50f, 0f)
			};
			DescendantRemoving += (_, o) =>
			{
				if (o is Part part)
					removeRigidBody(part.rigidBody);
			};
			DescendantAdded += (_, o) =>
			{
				if (o is Part part)
				{
					if (part.rigidBody == null || part.rigidBody.IsDisposed)
						part.setupRigidbody();
					addRigidBody(part, part.rigidBody);
				}
			};
		}

		internal void addRigidBody(Part sender, RigidBody body)
		{
			rigidReference[body] = sender;
			PhysWorld.AddRigidBody(body);
		}

		internal void removeRigidBody(RigidBody body)
		{
			rigidReference.Remove(body);
			PhysWorld.RemoveRigidBody(body);
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);
			PhysWorld.StepSimulation(deltaTime);
		}

		public override void Dispose()
		{
			for (int i = PhysWorld.NumCollisionObjects - 1; i >= 0; --i)
			{
				var o = PhysWorld.CollisionObjectArray[i];
				var body = o as RigidBody;
				if (body != null)
					PhysWorld.RemoveRigidBody(body);
				else
					PhysWorld.RemoveCollisionObject(o);
			}
			foreach (var kv in Part.boxCache)
				kv.Value.Dispose();
			Part.boxCache.Clear();
			PhysWorld.Dispose();
			physSolver.Dispose();
			physDispatcher.Dispose();
			physBroadphase.Dispose();
			physConf.Dispose();
			base.Dispose();
		}

		public bool Raycast(Vector3 _start, Vector3 _dir, out RayHit contact)
		{
			var start = _start.Numerics();
			var dir = _dir.Numerics();
			var back = new ClosestRayResultCallback(ref start, ref dir);
			PhysWorld.RayTest(start, dir, back);
			contact = new RayHit();
			if (back.HasHit)
			{
				contact.point = Vector3.FromNumerics(back.HitPointWorld);
				contact.normal = Vector3.FromNumerics(back.HitNormalWorld);
				RigidBody? key = back.CollisionObject as RigidBody;
				if (key != null)
					contact.part = rigidReference[key];
				return true;
			}
			return false;
		}
	}
}
