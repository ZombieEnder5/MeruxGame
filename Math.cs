using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merux.Mathematics
{
	public class Vector2
	{
		public double X = 0.0;
		public double Y = 0.0;

		public double MagnitudeSqr
		{
			get
			{
				return Dot(this);
			}
		}

		public double Magnitude
		{
			get
			{
				return Math.Sqrt(MagnitudeSqr);
			}
		}

		public Vector2 Unit
		{
			get
			{
				return this / Magnitude;
			}
		}

		public static Vector2 XAxis
		{
			get
			{
				return new Vector2(1, 0);
			}
		}

		public static Vector2 YAxis
		{
			get
			{
				return new Vector2(0, 1);
			}
		}

		public static Vector2 Zero
		{
			get
			{
				return new Vector2(0, 0);
			}
		}

		public static Vector2 One
		{
			get
			{
				return new Vector2(1, 1);
			}
		}

		public static Vector2 operator +(Vector2 left, Vector2 right)
		{
			return new Vector2(
				left.X + right.X,
				left.Y + right.Y
			);
		}

		public static Vector2 operator /(Vector2 left, Vector2 right)
		{
			return new Vector2(
				left.X / right.X,
				left.Y / right.Y
			);
		}

		public static Vector2 operator /(Vector2 left, double right)
		{
			return new Vector2(
				left.X / right,
				left.Y / right
			);
		}

		public static Vector2 operator /(double left, Vector2 right)
		{
			return new Vector2(
				left / right.X,
				left / right.Y
			);
		}

		public static Vector2 operator *(Vector2 left, Vector2 right)
		{
			return new Vector2(
				left.X * right.X,
				left.Y * right.Y
			);
		}

		public static Vector2 operator *(Vector2 left, double right)
		{
			return new Vector2(
				left.X * right,
				left.Y * right
			);
		}

		public static Vector2 operator *(double left, Vector2 right)
		{
			return new Vector2(
				right.X * left,
				right.Y * left
			);
		}

		public static Vector2 operator -(Vector2 left, Vector2 right)
		{
			return left + -right;
		}

		public static Vector2 operator -(Vector2 me)
		{
			return new Vector2(
				0.0 - me.X,
				0.0 - me.Y
			);
		}

		public Vector2(double X = 0, double Y = 0)
		{
			this.X = X;
			this.Y = Y;
		}

		public double Cross(Vector2 other)
		{
			return X * other.Y - Y * other.X;
		}

		public double Dot(Vector2 other)
		{
			return X * other.X + Y * other.Y;
		}

		public Vector2 Lerp(Vector2 other, double alpha)
		{
			return this + alpha * (other - this);
		}

		public OpenTK.Mathematics.Vector2 OpenTK()
		{
			return new OpenTK.Mathematics.Vector2(
				(float)X,
				(float)Y
			);
		}

		public static Vector2 FromOpenTK(OpenTK.Mathematics.Vector2 v)
		{
			return new Vector2(
				v.X,
				v.Y
			);
		}
	}

	public class Vector3
	{
		public double X = 0.0;
		public double Y = 0.0;
		public double Z = 0.0;

		public double MagnitudeSqr
		{
			get
			{
				return Dot(this);
			}
		}

		public double Magnitude
		{
			get
			{
				return Math.Sqrt(MagnitudeSqr);
			}
		}

		public Vector3 Unit
		{
			get
			{
				return this / Magnitude;
			}
		}

		public static Vector3 XAxis
		{
			get
			{
				return new Vector3(1, 0, 0);
			}
		}

		public static Vector3 YAxis
		{
			get
			{
				return new Vector3(0, 1, 0);
			}
		}

		public static Vector3 ZAxis
		{
			get
			{
				return new Vector3(0, 0, 1);
			}
		}

		public static Vector3 Zero
		{
			get
			{
				return new Vector3(0, 0, 0);
			}
		}

		public static Vector3 One
		{
			get
			{
				return new Vector3(1, 1, 1);
			}
		}

		public static Vector3 operator +(Vector3 left, Vector3 right)
		{
			return new Vector3(
				left.X + right.X,
				left.Y + right.Y,
				left.Z + right.Z
			);
		}

		public static Vector3 operator /(Vector3 left, Vector3 right)
		{
			return new Vector3(
				left.X / right.X,
				left.Y / right.Y,
				left.Z / right.Z
			);
		}

		public static Vector3 operator /(Vector3 left, double right)
		{
			return new Vector3(
				left.X / right,
				left.Y / right,
				left.Z / right
			);
		}

		public static Vector3 operator /(double left, Vector3 right)
		{
			return new Vector3(
				left / right.X,
				left / right.Y,
				left / right.Z
			);
		}

		public static Vector3 operator *(Vector3 left, Vector3 right)
		{
			return new Vector3(
				left.X * right.X,
				left.Y * right.Y,
				left.Z * right.Z
			);
		}

		public static Vector3 operator *(Vector3 left, double right)
		{
			return new Vector3(
				left.X * right,
				left.Y * right,
				left.Z * right
			);
		}

		public static Vector3 operator *(OpenTK.Mathematics.Matrix3 left, Vector3 right)
		{
			return FromOpenTK(left * right.OpenTK());
		}

		public static Vector3 operator *(double left, Vector3 right)
		{
			return new Vector3(
				right.X * left,
				right.Y * left,
				right.Z * left
			);
		}

		public static Vector3 operator -(Vector3 left, Vector3 right)
		{
			return left + -right;
		}

		public static Vector3 operator -(Vector3 me)
		{
			return new Vector3(
				0.0 - me.X,
				0.0 - me.Y,
				0.0 - me.Z
			);
		}

		public Vector3(double X=0, double Y=0, double Z=0)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}

		public Vector3(Vector3 v)
		{
			X = v.X;
			Y = v.Y;
			Z = v.Z;
		}

		public Vector3 Cross(Vector3 other)
		{
			return new Vector3(
				Y * other.Z - Z * other.Y,
				Z * other.X - X * other.Z,
				X * other.Y - Y * other.X
			);
		}

		public double Dot(Vector3 other)
		{
			return X * other.X + Y * other.Y + Z * other.Z;
		}

		public Vector3 Lerp(Vector3 other, double alpha)
		{
			return this + alpha * (other - this);
		}

		public OpenTK.Mathematics.Vector3 OpenTK()
		{
			return new(
				(float)X,
				(float)Y,
				(float)Z
			);
		}

		public static Vector3 FromOpenTK(OpenTK.Mathematics.Vector3 v)
		{
			return new Vector3(
				v.X,
				v.Y,
				v.Z
			);
		}

		public System.Numerics.Vector3 Numerics()
		{
			return new(
				(float)X,
				(float)Y,
				(float)Z
			);
		}

		public static Vector3 FromNumerics(System.Numerics.Vector3 v)
		{
			return new Vector3(
				v.X,
				v.Y,
				v.Z
			);
		}

		public override string ToString()
		{
			return $"{X}, {Y}, {Z}";
		}
	}

	// IMPORTANT: OpenTK's Matrix3 IS ROW-MAJOR. THIS IS COL-MAJOR.
	public class Matrix3
	{
		public double m00=1, m10=0, m20=0;
		public double m01=0, m11=1, m21=0;
		public double m02=0, m12=0, m22=1;

		public Vector3 XVector
		{
			get
			{
				return new Vector3(m00, m01, m02);
			}
			set
			{
				m00 = value.X;
				m01 = value.Y;
				m02 = value.Z;
			}
		}

		public Vector3 YVector
		{
			get
			{
				return new Vector3(m10, m11, m12);
			}
			set
			{
				m10 = value.X;
				m11 = value.Y;
				m12 = value.Z;
			}
		}

		public Vector3 ZVector
		{
			get
			{
				return new Vector3(m20, m21, m22);
			}
			set
			{
				m20 = value.X;
				m21 = value.Y;
				m22 = value.Z;
			}
		}

		public Vector3 Column0
		{
			get
			{
				return XVector;
			}
			set
			{
				XVector = value;
			}
		}

		public Vector3 Column1
		{
			get
			{
				return YVector;
			}
			set
			{
				YVector = value;
			}
		}

		public Vector3 Column2
		{
			get
			{
				return ZVector;
			}
			set
			{
				ZVector = value;
			}
		}

		public Vector3 Row0
		{
			get
			{
				return new Vector3(m00, m10, m20);
			}
			set
			{
				m00 = value.X;
				m10 = value.Y;
				m20 = value.Z;
			}
		}

		public Vector3 Row1
		{
			get
			{
				return new Vector3(m01, m11, m21);
			}
			set
			{
				m01 = value.X;
				m11 = value.Y;
				m21 = value.Z;
			}
		}

		public Vector3 Row2
		{
			get
			{
				return new Vector3(m02, m12, m22);
			}
			set
			{
				m02 = value.X;
				m12 = value.Y;
				m22 = value.Z;
			}
		}

		public double Determinant
		{
			get
			{
				return m00 * (m11 * m22 - m21 * m12) + m10 * (m21 * m02 - m01 * m22) + m20 * (m01 * m12 - m11 * m02);
			}
		}

		public static Matrix3 operator *(Matrix3 left, double right)
		{
			return new Matrix3(
				left.m00 * right,
				left.m01 * right,
				left.m02 * right,
				left.m10 * right,
				left.m11 * right,
				left.m12 * right,
				left.m20 * right,
				left.m21 * right,
				left.m22 * right
			);
		}

		public static Matrix3 operator /(Matrix3 left, double right)
		{
			return left * (1.0 / right);
		}

		public static Matrix3 operator *(Matrix3 left, Matrix3 right)
		{
			return new Matrix3(
				left.Row0.Dot(right.Column0), left.Row0.Dot(right.Column1), left.Row0.Dot(right.Column2),
				left.Row1.Dot(right.Column0), left.Row1.Dot(right.Column1), left.Row1.Dot(right.Column2),
				left.Row2.Dot(right.Column0), left.Row2.Dot(right.Column1), left.Row2.Dot(right.Column2)
			);
		}

		public static Matrix3 operator /(Matrix3 left, Matrix3 right)
		{
			return left * right.Inverse();
		}

		public static Vector3 operator *(Matrix3 left, Vector3 right)
		{
			return right.X * left.XVector + right.Y * left.YVector + right.Z * left.ZVector;
		}

		public Matrix3()
		{
		}

		public Matrix3(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
		{
			this.m00 = m00;
			this.m01 = m01;
			this.m02 = m02;
			this.m10 = m10;
			this.m11 = m11;
			this.m12 = m12;
			this.m20 = m20;
			this.m21 = m21;
			this.m22 = m22;
		}

		public Matrix3(Matrix3 mat)
		{
			m00 = mat.m00;
			m01 = mat.m01;
			m02 = mat.m02;
			m10 = mat.m10;
			m11 = mat.m11;
			m12 = mat.m12;
			m20 = mat.m20;
			m21 = mat.m21;
			m22 = mat.m22;
		}

		public static Matrix3 FromMatrix(Vector3 X, Vector3 Y, Vector3 Z)
		{
			return new Matrix3(X.X, X.Y, X.Z, Y.X, Y.Y, Y.Z, Z.X, Z.Y, Z.Z);
		}

		public Matrix3 Inverse()
		{
			return new Matrix3(
				m11 * m22 - m21 * m12, m20 * m12 - m10 * m22, m10 * m21 - m20 * m11,
				m21 * m02 - m01 * m22, m00 * m22 - m20 * m02, m20 * m01 - m00 * m21,
				m01 * m12 - m11 * m02, m10 * m02 - m00 * m12, m00 * m11 - m10 * m01
			).Transpose() / Determinant;
		}

		public Matrix3 Transpose()
		{
			return new Matrix3(
				m00, m10, m20,
				m01, m11, m21,
				m02, m12, m22
			);
		}

		public OpenTK.Mathematics.Matrix3 OpenTK()
		{
			return new OpenTK.Mathematics.Matrix3(
				(float)m00, (float)m10, (float)m20,
				(float)m01, (float)m11, (float)m21,
				(float)m02, (float)m12, (float)m22
			);
		}

		public override string ToString()
		{
			return $"{m00}, {m01}, {m02}, {m10}, {m11}, {m12}, {m20}, {m21}, {m22}";
		}
	}
}
