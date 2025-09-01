using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.IO;
using System.Text;

namespace Merux
{
	public class Shader
	{
		public int Handle { get; private set; }

		public Shader(string vSrc, string fSrc)
		{
			int vSh = GL.CreateShader(ShaderType.VertexShader);
			GL.ShaderSource(vSh, vSrc);
			GL.CompileShader(vSh);
			assessShaderStatus(vSh);

			int fSh = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(fSh, fSrc);
			GL.CompileShader(fSh);
			assessShaderStatus(fSh);

			Handle = GL.CreateProgram();
			GL.AttachShader(Handle, vSh);
			GL.AttachShader(Handle, fSh);
			GL.LinkProgram(Handle);
			assessProgramStatus();

			GL.DeleteShader(vSh);
			GL.DeleteShader(fSh);
		}

		public static Shader FromPath(string vPath, string fPath)
		{
			using (Stream vertStream = Merux.LoadStream(vPath))
				using (Stream fragStream = Merux.LoadStream(fPath))
			{
				var vBytes = new byte[vertStream.Length];
				var fBytes = new byte[fragStream.Length];
				vertStream.Read(vBytes, 0, vBytes.Length);
				fragStream.Read(fBytes, 0, fBytes.Length);
				string vSrc = Encoding.UTF8.GetString(vBytes, 0, vBytes.Length);
				string fSrc = Encoding.UTF8.GetString(fBytes, 0, fBytes.Length);
				return new Shader(vSrc, fSrc);
			}
		}

		public static Shader FromPath(string path)
		{
			return FromPath(path + ".vert", path + ".frag");
		}

		public void Use()
		{
			GL.UseProgram(Handle);
		}

		public void SetMatrix4(string name, Matrix4 mat)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.UniformMatrix4(loc, false, ref mat);
		}

		public void SetInt(string name, int val)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.Uniform1(loc, val);
		}

		public void SetFloat(string name, float val)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.Uniform1(loc, val);
		}

		public void SetVector2(string name, Vector2 vec)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.Uniform2(loc, vec);
		}

		public void SetVector3(string name, Vector3 vec)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.Uniform3(loc, vec);
		}

		public void SetVector4(string name, Vector4 vec)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.Uniform4(loc, vec);
		}

		private void assessProgramStatus()
		{
			GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
			if (success == 0)
			{
				string log = GL.GetProgramInfoLog(Handle);
				throw new Exception($"Shader - link fail\n\n{log}");
			}
		}

		private void assessShaderStatus(int shader)
		{
			GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
			if (success == 0)
			{
				string log = GL.GetShaderInfoLog(shader);
				throw new Exception($"Shader - compile fail\n\n{log}");
			}
		}
	}
}
