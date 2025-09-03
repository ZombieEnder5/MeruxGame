using Merux.EnumTypes;
using Merux.Mathematics;

namespace Merux
{
	public class CFrame
	{
		public Vector3 Position {
			get {
				return new Vector3(_realPosition);
			}
			set {
				_realPosition = new Vector3(value);
			}
		}
		public Vector3 p
		{
			get
			{
				return new Vector3(_realPosition);
			}
			set
			{
				_realPosition = new Vector3(value);
			}
		}
		private Vector3 _realPosition;

		public CFrame Rotation
		{
			get
			{
				return new CFrame(new Vector3(0, 0, 0), _rotation);
			}
		}
		private Matrix3 _rotation;

		public Vector3 XVector
		{
			get
			{
				return _rotation.XVector;
			}
			set
			{
				_rotation.XVector = value;
			}
		}
		public Vector3 YVector
		{
			get
			{
				return _rotation.YVector;
			}
			set
			{
				_rotation.YVector = value;
			}
		}
		public Vector3 ZVector
		{
			get
			{
				return _rotation.ZVector;
			}
			set
			{
				_rotation.ZVector = value;
			}
		}

		public Vector3 RightVector
		{
			get
			{
				return XVector;
			}
		}
		public Vector3 UpVector
		{
			get
			{
				return YVector;
			}
		}
		public Vector3 LookVector
		{
			get
			{
				return -ZVector;
			}
		}

		public static CFrame Identity 
		{ 
			get
			{
				return new CFrame(new Vector3(), new Matrix3());
			}
		}

		public static Vector3 operator *(CFrame left, Vector3 right)
		{
			Vector3 result = left.p + left._rotation * right;
			return result;
		}

		public static CFrame operator *(CFrame left, CFrame right)
		{
			CFrame result = new CFrame(left * right.p, left._rotation * right._rotation);
			return result;
		}

		public static CFrame operator +(CFrame left, Vector3 right)
		{
			CFrame result = new CFrame(left.p + right, left._rotation);
			return result;
		}

		public static CFrame operator -(CFrame left, Vector3 right)
		{
			return left + -right;
		}

		public CFrame(Vector3 position, Matrix3 rotation)
		{
			_realPosition = new Vector3(position);
			_rotation = new Matrix3(rotation);
		}

		public CFrame(Vector3 position, double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
		{
			_realPosition = new Vector3(position);
			_rotation = new Matrix3(m00, m01, m02, m10, m11, m12, m20, m21, m22);
		}

		public CFrame(CFrame cframe)
		{
			_realPosition = cframe.p;
			_rotation = cframe.GetRotationMatrix();
		}

		public static CFrame fromMatrix(Vector3 position, Vector3 xVector, Vector3 yVector, Vector3 zVector)
		{
			return new CFrame(
				position,
				xVector.X, yVector.X, zVector.X,
				xVector.Y, yVector.Y, zVector.Y,
				xVector.Z, yVector.Z, zVector.Z
			);
		}

		// VBRS success
		public CFrame Inverse()
		{
			Matrix3 inv = _rotation.Inverse();
			return new CFrame(inv * p, inv);
		}

		// this one is relatively simple compared to ToEulerAngles. who would've known.
		public static CFrame FromEulerAngles(double rx, double ry, double rz, EulerOrder order)
		{
			double sx = Math.Sin(rx);
			double sy = Math.Sin(ry);
			double sz = Math.Sin(rz);
			double cx = Math.Cos(rx);
			double cy = Math.Cos(ry);
			double cz = Math.Cos(rz);

			Matrix3 X = new Matrix3(1, 0, 0, 0, cx, -sx, 0, sx, cx);
			Matrix3 Y = new Matrix3(cy, 0, sy, 0, 1, 0, -sy, 0, cy);
			Matrix3 Z = new Matrix3(cz, -sz, 0, sz, cz, 0, 0, 0, 1);

			if (order == EulerOrder.XYZ) return new CFrame(new Vector3(0, 0, 0), X * Y * Z);
			if (order == EulerOrder.XZY) return new CFrame(new Vector3(0, 0, 0), X * Z * Y);
			if (order == EulerOrder.YXZ) return new CFrame(new Vector3(0, 0, 0), Y * X * Z);
			if (order == EulerOrder.YZX) return new CFrame(new Vector3(0, 0, 0), Y * Z * X);
			if (order == EulerOrder.ZXY) return new CFrame(new Vector3(0, 0, 0), Z * X * Y);
			if (order == EulerOrder.ZYX) return new CFrame(new Vector3(0, 0, 0), Z * Y * X);

			throw new NotImplementedException("how did we even get here");
		}

		// you have no idea how much math and VBRS to get this right
		// https://docs.google.com/document/d/1K29oLjsdRzlNjzkhIK9mUdZHgN0k5cQvJw8GyoeU5w4/edit?usp=sharing
		public Vector3 ToEulerAngles(EulerOrder order)
		{
			double rx=0, ry=0, rz=0;
			if (order == EulerOrder.XYZ)
			{
				ry = Math.Asin(ZVector.X);
				rx = Math.Atan2(-ZVector.Y, ZVector.Z);
				rz = Math.Atan2(-YVector.X, XVector.X);
			}
			if (order == EulerOrder.XZY)
			{
				rz = -Math.Asin(YVector.X);
				rx = Math.Atan2(YVector.Z, YVector.Y);
				ry = Math.Atan2(ZVector.X, XVector.X);
			}
			if (order == EulerOrder.YXZ)
			{
				rx = -Math.Asin(ZVector.Y);
				ry = Math.Atan2(ZVector.X, ZVector.Z);
				rz = Math.Atan2(XVector.Y, YVector.Y);
			}
			if (order == EulerOrder.YZX)
			{
				rz = Math.Asin(XVector.Y);
				rx = Math.Atan2(-ZVector.Y, YVector.Y);
				ry = Math.Atan2(-XVector.Z, XVector.X);
			}
			if (order == EulerOrder.ZXY)
			{
				rx = Math.Asin(YVector.Z);
				ry = Math.Atan2(-XVector.Z, ZVector.Z);
				rz = Math.Atan2(-YVector.X, YVector.Y);
			}
			if (order == EulerOrder.ZYX)
			{
				ry = -Math.Asin(XVector.Z);
				rx = Math.Atan2(YVector.Z, ZVector.Z);
				rz = Math.Atan2(XVector.Y, XVector.X);
			}
			return new Vector3(rx, ry, rz);
		}

		// using just common sense, you can figure out that ToEulerAnglesXYZ uses the ToEulerAngles function with rotation order XYZ
		public Vector3 ToEulerAnglesXYZ()
		{
			return ToEulerAngles(EulerOrder.XYZ);
		}

		// as with ToEulerAnglesXYZ, same with ToEulerAnglesYXZ. except the rotation order is YXZ.
		public Vector3 ToEulerAnglesYXZ()
		{
			return ToEulerAngles(EulerOrder.YXZ);
		}

		// https://roblox.fandom.com/wiki/CFrame
		// "Creates a CFrame rotated around the three axes, relative to the CFrame, in X, Y, Z order using the angles (rx, ry, rz)."
		// did VBRS, too. it's not lying.
		public Vector3 ToOrientation()
		{
			return ToEulerAnglesYXZ();
		}

		// yadayadayada
		public static CFrame FromEulerAnglesXYZ(double rx, double ry, double rz)
		{
			return FromEulerAngles(rx, ry, rz, EulerOrder.XYZ);
		}

		// yadayadayada YXZ
		public static CFrame FromEulerAnglesYXZ(double rx, double ry, double rz)
		{
			return FromEulerAngles(rx, ry, rz, EulerOrder.YXZ);
		}

		public static CFrame FromOrientation(double rx, double ry, double rz)
		{
			return FromEulerAnglesYXZ(rx, ry, rz);
		}

		public Matrix3 GetRotationMatrix()
		{
			return new Matrix3(_rotation);
		}

		public static CFrame FromNumericsMatrix(System.Numerics.Matrix4x4 mat)
		{
			return new(
				Vector3.FromNumerics(mat.Translation), new Matrix3(
					mat.M11, mat.M12, mat.M13,
					mat.M21, mat.M22, mat.M23,
					mat.M31, mat.M32, mat.M33
				)
			);
		}

		// row-major matrices are the bane of my existence.
		public System.Numerics.Matrix4x4 ToNumericsMatrix()
		{
			return new(
				(float)_rotation.m00, (float)_rotation.m01, (float)_rotation.m02, 0f,
				(float)_rotation.m10, (float)_rotation.m11, (float)_rotation.m12, 0f,
				(float)_rotation.m20, (float)_rotation.m21, (float)_rotation.m22, 0f,
				(float)_realPosition.X, (float)_realPosition.Y, (float)_realPosition.Z, 0f
			);
		}

		// I had to google this since I could not figure out how Roblox did it.
		// https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
		// wikipedia is obviously not the best source, but it was one of the only good-ish ones
		public static CFrame FromAxisAngle(Vector3 axis, double angle)
		{
			axis = axis.Unit;
			double x = axis.X, y = axis.Y, z = axis.Z;
			double c = Math.Cos(angle);
			double s = Math.Sin(angle);
			double t = 1 - c;

			return new CFrame(
				new Vector3(0, 0, 0),
				t * x * x + c, t * x * y - s * z, t * x * z + s * y,
				t * x * y + s * z, t * y * y + c, t * y * z - s * x,
				t * x * z - s * y, t * y * z + s * x, t * z * z + c
			);
		}

		public Vector3 AxisFromIndex(int i)
		{
			switch (i)
			{
				case 0:
					return XVector;
				case 1:
					return YVector;
				case 2:
					return ZVector;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public CFrame Copy()
		{
			return new CFrame(_realPosition, _rotation);
		}

		public override string ToString()
		{
			return $"{p}, {_rotation}";
		}

		Vector3 orthoproject(Vector3 u, Vector3 v)
		{
			return u.MagnitudeSqr > 0.0 ? u * v.Dot(u) / u.MagnitudeSqr : Vector3.Zero;
		}

		public void Orthonormalize()
		{
			XVector = XVector.Unit;
			YVector = (YVector - orthoproject(XVector, YVector)).Unit;
			ZVector = XVector.Cross(YVector).Unit;
		}

		public CFrame Orthonormalized()
		{
			CFrame cf = new CFrame(_realPosition, _rotation);
			cf.Orthonormalize();
			return cf;
		}

		public static CFrame LookAt(Vector3 at, Vector3 lookAt)
		{
			Vector3 z = (at - lookAt).Unit;
			Vector3 x = Vector3.YAxis.Cross(z);
			if (x.MagnitudeSqr == 0.0)
				x = Vector3.XAxis.Cross(z);
			x = x.Unit;
			Vector3 y = z.Cross(x);
			y = y.Unit;
			return fromMatrix(at, x, y, z);
		}
	}
}
