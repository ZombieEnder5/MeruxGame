using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Merux
{
	internal class Debug
	{
		public static void Print(params object[] args)
		{
			MethodBase? methodOptional = new StackFrame(1).GetMethod();
			MethodBase method;
			if (methodOptional == null)
				return;
			method = methodOptional;
			string output = $"[{method.DeclaringType.Name}.{method.Name}]: ";
			foreach (object o in args)
				if (o == null)
					output += "null ";
				else
					output += o.ToString() + " ";
			Console.WriteLine(output);
		}
	}
}
