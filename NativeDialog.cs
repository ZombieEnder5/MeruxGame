using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Merux
{
	internal class NativeDialog
	{
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, uint uType);

		const uint MB_YESNO = 4;
		const uint MB_ICONQUESTION = 0x20;
		const int IDYES = 6;
		const int IDNO = 7;

		public static bool ShowYesNo(string text, string caption)
		{
			int result = MessageBoxW(IntPtr.Zero, text, caption, MB_YESNO | MB_ICONQUESTION);
			return result == IDYES;
		}

		public static void ShowAlert(string text, string caption)
		{
			MessageBoxW(IntPtr.Zero, text, caption, 0x10);
		}
	}
}
