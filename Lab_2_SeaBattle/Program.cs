using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Lab_2_SeaBattle {

	class Program {

		private const int MF_BYCOMMAND = 0x00000000;
		public const int SC_CLOSE = 0xF060;
		public const int SC_MINIMIZE = 0xF020;
		public const int SC_MAXIMIZE = 0xF030;
		public const int SC_SIZE = 0xF000;

		[DllImport("user32.dll")]
		public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		[DllImport("kernel32.dll", ExactSpelling = true)]
		private static extern IntPtr GetConsoleWindow();



		[STAThread]
		static void Main(string[] args) {

			Game game;
			IntPtr handle = GetConsoleWindow();
			IntPtr sysMenu = GetSystemMenu(handle, false);

			if (handle != IntPtr.Zero) {
				DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
				DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
			}

			game = Game.getInstance();
			Console.SetWindowSize(120, 40);
			Console.OutputEncoding = UnicodeEncoding.Unicode;
			game.Start();
		}
	}
}
