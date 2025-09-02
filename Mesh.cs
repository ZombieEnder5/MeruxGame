using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;
using OpenTK.Windowing.Common;
using SharpGLTF.Schema2;

namespace Merux
{
	public class Mesh
	{
		public int VAO { get; private set; }
		public int VBO { get; private set; }
		public int EBO { get; private set; }
		public int IndexCount { get; private set; }

		public static Mesh CubeMesh = FromResource("Meshes.cube.glb");

		public Mesh(float[] vertices, ushort[] indices)
		{
			IndexCount = indices.Length;

			VAO = GL.GenVertexArray();
			GL.BindVertexArray(VAO);

			VBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			EBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, IndexCount * sizeof(uint), indices, BufferUsageHint.StaticDraw);

			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

			GL.EnableVertexAttribArray(2);
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

			GL.BindVertexArray(0);
		}

		public void Dispose()
		{
			GL.DeleteVertexArray(VAO);
			GL.DeleteBuffer(VBO);
			GL.DeleteBuffer(EBO);
		}

		public static Mesh FromResource(string path)
		{
			using Stream stream = Merux.LoadStream(path);
			return FromModel(ModelRoot.ReadGLB(stream));
		}

		public static Mesh FromFile(string path)
		{
			return FromModel(ModelRoot.Load(path));
		}

		public static Mesh FromModel(ModelRoot model)
		{
			var mesh = model.LogicalMeshes[0];

			var prim = mesh.Primitives[0];
			var vbo = prim.GetVertexAccessor("POSITION").AsVector3Array().ToArray();

			Vector3 min = new Vector3(float.MaxValue);
			Vector3 max = new Vector3(float.MinValue);
			foreach (var v in vbo)
			{
				min = new Vector3(MathF.Min(min.X, v.X), MathF.Min(min.Y, v.Y), MathF.Min(min.Z, v.Z));
				max = new Vector3(MathF.Max(max.X, v.X), MathF.Max(max.Y, v.Y), MathF.Max(max.Z, v.Z));
			}

			var norm = prim.GetVertexAccessor("NORMAL").AsVector3Array().ToArray();
			var uv = prim.GetVertexAccessor("TEXCOORD_0").AsVector2Array().ToArray();
			ushort[] indices = prim.IndexAccessor.AsIndicesArray().Select(i => (ushort)i).ToArray();

			var flat = new float[vbo.Length * 8];
			for (int i = 0; i < vbo.Length; i++)
			{
				flat[i * 8] = vbo[i].X;
				flat[i * 8 + 1] = vbo[i].Y;
				flat[i * 8 + 2] = vbo[i].Z;
				flat[i * 8 + 3] = norm[i].X;
				flat[i * 8 + 4] = norm[i].Y;
				flat[i * 8 + 5] = norm[i].Z;
				flat[i * 8 + 6] = uv[i].X;
				flat[i * 8 + 7] = uv[i].Y;
			}

			return new Mesh(flat, indices);
		}
	}
}
