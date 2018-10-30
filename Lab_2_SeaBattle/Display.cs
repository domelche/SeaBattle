using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Lab_2_SeaBattle {

	public class Display {

		// output stuff with kernel32.dll

		

		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern SafeFileHandle CreateFile(
			string fileName,
			[MarshalAs(UnmanagedType.U4)] uint fileAccess,
			[MarshalAs(UnmanagedType.U4)] uint fileShare,
			IntPtr securityAttributes,
			[MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
			[MarshalAs(UnmanagedType.U4)] int flags,
			IntPtr template);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool WriteConsoleOutput(
			SafeFileHandle hConsoleOutput,
			CharInfo[] lpBuffer,
			Coord dwBufferSize,
			Coord dwBufferCoord,
			ref SmallRect lpWriteRegion);

		[StructLayout(LayoutKind.Sequential)]
		public struct Coord {
			public short X;
			public short Y;

			public Coord(short X, short Y) {
				this.X = X;
				this.Y = Y;
			}
		};

		[StructLayout(LayoutKind.Explicit)]
		public struct CharUnion {
			[FieldOffset(0)] public char UnicodeChar;
			[FieldOffset(0)] public byte AsciiChar;
		}

		[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
		public struct CharInfo {
			[FieldOffset(0)] public char UnicodeChar;
			[FieldOffset(0)] public byte AsciiChar;
			[FieldOffset(2)] public short Attributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SmallRect {
			public short Left;
			public short Top;
			public short Right;
			public short Bottom;
		}



		// Display class



		public static short defaultAttr;

		public int width { get; }
		public int height { get; }
		public int boardSize { get; }

		private SafeFileHandle file;
		private CharInfo[] buf;
		private SmallRect rect;
		private Coord c1;
		private Coord c2;

		public Queue<String> logs;
		public int[] playersBoardStart { get; }
		public int[] botsBoardStart { get; }



		static Display() {
			defaultAttr = Color.BlackGrey;
		}

		private Display() { }

		public Display(int width, int height) {

			this.width = width;
			this.height = height;
			this.boardSize = 0;

			this.file = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
			this.rect = new SmallRect() {
				Left = 0,
				Top = 0,
				Right = (short)width,
				Bottom = (short)height
			};
			this.c1 = new Coord() { X = (short)width, Y = (short)height };
			this.c2 = new Coord() { X = 0, Y = 0 };

			this.playersBoardStart = new int[] { 0, 0 };
			this.botsBoardStart = new int[] { 0, 0 };
			this.logs = null;

			this.buf = new CharInfo[width * height];
			for (int i = 0; i < buf.Length; ++i) {
				buf[i].UnicodeChar = ' ';
				buf[i].Attributes = 0;
			}
		}

		public Display(int width, int height, int boardSize) {

			this.width = width;
			this.height = height;
			this.boardSize = boardSize;

			this.file = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
			this.rect = new SmallRect() {
				Left = 0, Top = 0, Right = (short)width, Bottom = (short)height
			};
			this.c1 = new Coord() { X = (short)width, Y = (short)height };
			this.c2 = new Coord() { X = 0, Y = 0 };

			this.playersBoardStart = new int[] { 10, 6 };
			this.botsBoardStart = new int[] { 75, 6 };
			this.logs = new Queue<String>(10);
			for (int i = 0; i < 10; ++i)
				logs.Enqueue("");
			InitInterface(width, height);
		}



		private void InitInterface(int width, int height) {

			this.buf = new CharInfo[width * height];
			for (int i = 0; i < buf.Length; ++i) {
				buf[i].UnicodeChar = ' ';
				buf[i].Attributes = 0;
			}

			PutStr(playersBoardStart[0], playersBoardStart[1] - 4, "Your map", Color.BlackGreen);
			PutStr(botsBoardStart[0], botsBoardStart[1] - 4, "Enemy map", Color.BlackRed);
			DrawBoard(boardSize, playersBoardStart[0], playersBoardStart[1], defaultAttr);
			DrawBoard(boardSize, botsBoardStart[0], botsBoardStart[1], defaultAttr);

			PutStr(0, playersBoardStart[1] + 18, new String('=', width));
			UpdateLog();
			PutStr(0, playersBoardStart[1] + 29, new String('=', width));

			Console.SetCursorPosition(0, playersBoardStart[1] + 31);
		}



		private void UpdateLog() {

			int y;
			String empty;

			y = playersBoardStart[1] + 19;
			empty = new String(' ', width);
			foreach (String log in logs) {
				PutStr(0, y, empty);
				PutStr(0, y, "> ");
				if (log.IndexOf("Error:") != -1) {
					PutStr(2, y, "Error:", Color.BlackRed);
					PutStr(9, y, log.Substring(7));
				} else {
					PutStr(2, y, log);
				}
				++y;
			}
			Print();
		}

		public void ClearLog() {

			for (int i = 0; i < 10; ++i) {
				logs.Dequeue();
				logs.Enqueue("");
			}
			UpdateLog();
		}

		public void Log(String log) {
			logs.Dequeue();
			logs.Enqueue(log);
			UpdateLog();
		}



		public void PutChar(int x, int y, char c) => PutChar(x, y, c, defaultAttr);

		public void PutChar(int x, int y, char c, short attr) {
			try {
				buf[y * width + x].UnicodeChar = c;
				buf[y * width + x].Attributes = attr;
			}
			catch (Exception) { }
		}

		public void PutChar(int x, int y, CharInfo c) {
			if (x < 0 || y < 0)
				return;
			try { buf[y * width + x] = c; } catch (Exception) { }
		}

		public void PutStr(int x, int y, String str) => PutStr(x, y, str, defaultAttr);

		public void PutStr(int x, int y, String str, short attr) {
			for (int i = 0; i < str.Length; ++i)
				PutChar(x + i, y, str[i], attr);
		}

		public void PutStr(int x, int y, CharInfo[] str) {
			for (int i = 0; i < str.Length; ++i)
				PutChar(x + i, y, str[i]);
		}

		public void PutStr(int x, int y, int width, String str) => PutStr(x, y, width, str, defaultAttr);

		public void PutStr(int x, int y, int width, String str, short attr) {

			for (int i = y; true; ++i) {
				for (int j = x; j < x + width; ++j) {
					if ((i - y) * width + j - x >= str.Length) return;
					if (i < 0) continue;
					try { PutChar(j, i, str[(i - y) * width + j - x], attr); }
					catch (Exception) { }
				}
			}
		}

		public void PutStr(int x, int y, int width, CharInfo[] str) {

			for (int i = y; true; ++i) {
				for (int j = x; j < x + width; ++j) {
					if ((i - y) * width + j - x >= str.Length) return;
					if (i < 0) continue;
					try { PutChar(j, i, str[(i - y) * width + j - x]); }
					catch (Exception) { }
				}
			}
		}

		public void PutStr(int x, int y, int width, char[] str) => PutStr(x, y, width, str, defaultAttr);

		public void PutStr(int x, int y, int width, char[] str, short attr) {

			for (int i = y; true; ++i) {
				for (int j = x; j < x + width; ++j) {
					if ((i - y) * width + j - x >= str.Length) return;
					if (i < 0) continue;
					try { PutChar(j, i, str[(i - y) * width + j - x], attr); }
					catch (Exception) { }
				}
			}
		}

		public void PutStrMenu (int x, int y, String str) {

			for (int i = y; true; ++i) {
				for (int j = x; j < x + width; ++j) {
					if ((i - y) * width + j - x >= str.Length) return;
					if (i < 0) continue;
					PutChar(j, i, str[(i - y) * width + j - x],
						(str[(i - y) * width + j - x] == '$') ? Color.GreyGrey : defaultAttr);
				}
			}
		}



		public void DrawBoard(int size, int x, int y, short attr) {

			x -= 2;
			y -= 2;

			for (int i = 0; i < size; ++i) {
				PutChar(x + i * 2 + 2, y, (char)('a' + i), attr);
			}
			PutStr(x, y + 1, " +" + new String('-', size * 2) + "+", attr);
			for (int i = 0; i < size; ++i) {
				PutStr(x - 1, y + 2 + i, (i + 1).ToString(), attr);
				PutChar(x + 1, y + 2 + i, '|', attr);
				PutChar(x + 2 + size * 2, y + 2 + i, '|', attr);
			}
			PutStr(x + 1, y + 2 + size, "+" + new String('-', size * 2) + "+", attr);
		}

		public void Print() {
			WriteConsoleOutput(file, buf, c1, c2, ref rect);
		}



		private void FillBuf(int x, int y, int width, int height, CharInfo[] buf) {

			for (int i = 0; i < width; ++i)
				for (int j = 0; j < height; ++j) {
					if (y + j < 0) {
						buf[j * width + i].UnicodeChar = ' ';
						buf[j * width + i].Attributes = defaultAttr;
					}
					else
						buf[j * width + i] = this.buf[(y + j) * this.width + x + i];
				}
		}



		public void AnimateShot(int x, int y) {

			int shellY;
			CharInfo[] buf;
			CharInfo[] leftSmoke;
			CharInfo[] rightSmoke;

			shellY = -1;
			buf = new CharInfo[12] {
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr }
			};
			leftSmoke = new CharInfo[2] {
				new CharInfo() { UnicodeChar = ' ', Attributes = Color.GreyGrey },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr }
			};
			rightSmoke = new CharInfo[2] {
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = Color.GreyGrey }
			};

			while (shellY != y) {
				FillBuf(x, shellY - 5, 2, 6, buf);
				PutStr(x, shellY - 5, 2, (shellY % 2 == 0) ? leftSmoke : rightSmoke);
				PutStr(x, shellY - 4, 2, (shellY % 2 == 1) ? leftSmoke : rightSmoke);
				PutStr(x, shellY - 3, 2, (shellY % 2 == 0) ? leftSmoke : rightSmoke);
				PutStr(x, shellY - 2, 2, "TT[]\\/", Color.BlackDarkYellow);
				Print();
				++shellY;
				System.Threading.Thread.Sleep(50);
				PutStr(x, shellY - 6, 2, buf);
			}
			Print();
		}

		//
		//  ||
		// \__/
		//

		//
		//    **   *  *
		//   ****   ** 
		//

		public void AnimateMiss(int x, int y) {

			CharInfo[] buf;
			int delay;

			buf = new CharInfo[8] {
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr }
			};
			delay = 100;

			FillBuf(x - 1, y - 1, 4, 2, buf);
			PutStr(x - 1, y, "\\__/", Color.BlackBlue);
			Print();
			System.Threading.Thread.Sleep(delay);
			PutStr(x - 1, y - 1, " || ", Color.BlackBlue);
			Print();
			System.Threading.Thread.Sleep(delay);
			PutStr(x - 1, y - 1, "    ");
			Print();
			System.Threading.Thread.Sleep(delay);
			PutStr(x - 1, y - 1, 4, buf);
			Print();
		}

		public void AnimateExplosion(int x, int y) {

			CharInfo[] buf;
			int delay;

			buf = new CharInfo[8] {
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr },
				new CharInfo() { UnicodeChar = ' ', Attributes = defaultAttr }
			};
			delay = 100;

			FillBuf(x - 1, y - 1, 4, 2, buf);
			PutStr(x - 1, y - 1, " ** ", Color.BlackRed);
			PutStr(x - 1, y, "*  *", Color.BlackRed);
			PutStr(x, y, "**", Color.BlackYellow);
			Print();
			System.Threading.Thread.Sleep(delay);
			PutStr(x - 1, y - 1, "*  *", Color.BlackRed);
			PutStr(x - 1, y, " ** ", Color.BlackYellow);
			Print();
			System.Threading.Thread.Sleep(delay);
			PutStr(x - 1, y - 1, " ** ", Color.BlackRed);
			PutStr(x - 1, y, "*  *", Color.BlackRed);
			PutStr(x, y, "**", Color.BlackYellow);
			Print();
			System.Threading.Thread.Sleep(delay);
			PutStr(x - 1, y - 1, 4, buf);
			Print();
		}
	}
}
