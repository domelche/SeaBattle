using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2_SeaBattle
{
	public class Game
	{
		private static Game _instance;



		private Game() { }



		public static Game getInstance() {
			if (_instance == null)
				_instance = new Game();
			return (_instance);
		}



		private static void PrintMenu(Display display, int selected, int res) {

			short[] attrs;

			attrs = new short[3] {
				Display.defaultAttr,
				Display.defaultAttr,
				Display.defaultAttr
			};
			attrs[selected] = Color.GreyBlack;

			display.PutStr(0, 21, new String(' ', 58) + "START" + new String(' ', 57), attrs[0]);
			display.PutStr(0, 22, new String(' ', 49) + "SELECT MAP SIZE:   " + res + new String(' ', 50), attrs[1]);
			display.PutChar(66, 22, (res == 10) ? ' ' : '<', attrs[1]);
			display.PutChar(71, 22, (res == 15) ? ' ' : '>', attrs[1]);
			display.PutStr(0, 23, new String(' ', 58) + "EXIT" + new String(' ', 58), attrs[2]);
			display.Print();
		}

		private static int HandleMenu(Display display) {

			ConsoleKeyInfo ki;
			int i;
			int res;
			int x;
			int y;
			String bar;

			i = 0;
			res = 10;
			bar = new String('=', display.width);

			x = 18;
			y = 5;
			display.PutStrMenu(x, y + 0, "$$$$$$$$$$                      $$$$$$$$$$                                    ");
			display.PutStrMenu(x, y + 1, "$$$$$$$$$$                      $$$$    $$                          $$        ");
			display.PutStrMenu(x, y + 2, "$$$$                            $$$$    $$                          $$        ");
			display.PutStrMenu(x, y + 3, "$$$$                            $$$$    $$                          $$        ");
			display.PutStrMenu(x, y + 4, "$$$$$$$$$$                      $$$$$$$$$$            $$      $$    $$        ");
			display.PutStrMenu(x, y + 5, "$$$$$$$$$$  $$$$$$  $$$$$$      $$$$$$$$$$  $$$$$$  $$$$$$  $$$$$$  $$  $$$$$$");
			display.PutStrMenu(x, y + 6, "      $$$$  $$  $$      $$      $$$$    $$      $$    $$      $$    $$  $$  $$");
			display.PutStrMenu(x, y + 7, "      $$$$  $$$$$$  $$$$$$      $$$$    $$  $$$$$$    $$      $$    $$  $$$$$$");
			display.PutStrMenu(x, y + 8, "$$$$$$$$$$  $$      $$  $$      $$$$    $$  $$  $$    $$      $$    $$  $$    ");
			display.PutStrMenu(x, y + 9, "$$$$$$$$$$  $$$$$$  $$$$$$      $$$$$$$$$$  $$$$$$    $$$$    $$$$  $$  $$$$$$");
			display.PutStr(0, y + 14, bar);
			display.PutStr(0, y + 20, bar);

			PrintMenu(display, i, res);

			while (true) {
				ki = Console.ReadKey();
				switch (ki.Key) {
					case ConsoleKey.UpArrow:		PrintMenu(display, ((i == 0) ? i : --i), res);					break;
					case ConsoleKey.DownArrow:		PrintMenu(display, ((i == 2) ? i : ++i), res);					break;
					case ConsoleKey.LeftArrow:		if (i == 1) PrintMenu(display, i, (res == 10) ? res : --res);	break;
					case ConsoleKey.RightArrow:		if (i == 1) PrintMenu(display, i, (res == 15) ? res : ++res);	break;
					case ConsoleKey.Enter:			if (i == 0) return (res); else if (i == 2) return (0);			break;
				}
			}
		}

		public void Start() {

			int size;
			Display mainDisplay;
			Player player;
			Bot bot;

			size = HandleMenu(new Display(Console.WindowWidth, Console.WindowHeight));
			if (size == 0)
				return ;

			Console.Clear();

			mainDisplay = new Display(Console.WindowWidth, Console.WindowHeight, size);

			player = new Player(size);
			bot = new Bot(mainDisplay, size, player);

			mainDisplay.Print();


			//			player.PrintEnemyMap(mainDisplay);


			player.AskPlaceShips(mainDisplay);
			player.Shoot(mainDisplay);
		}

		public void Finish(bool isPlayer) {

			Display finish;
			int x;

			finish = new Display(Console.WindowWidth, Console.WindowHeight);
			x = 7;

			if (!isPlayer) {
				finish.PutStrMenu(x , 5, "$$$$        $$$$                                                                                  $$");
				finish.PutStrMenu(x,  6, "$$$$        $$$$                  $$                                                              $$");
				finish.PutStrMenu(x,  7, "$$$$$$    $$$$$$                  $$                                                              $$");
				finish.PutStrMenu(x,  8, "$$$$$$    $$$$$$                  $$                                                              $$");
				finish.PutStrMenu(x,  9, "$$$$$$$$$$$$$$$$                  $$                                                              $$");
				finish.PutStrMenu(x, 10, "$$$$$$$$$$$$$$$$  $$$$$$  $$$$$$  $$$$$$  $$  $$$$$$  $$$$$$  $$$$$$      $$  $$  $$  $$  $$$$$$  $$");
				finish.PutStrMenu(x, 11, "$$$$  $$$$  $$$$      $$  $$      $$  $$      $$  $$  $$  $$  $$          $$  $$  $$      $$  $$  $$");
				finish.PutStrMenu(x, 12, "$$$$  $$$$  $$$$  $$$$$$  $$      $$  $$  $$  $$  $$  $$$$$$  $$$$$$      $$  $$  $$  $$  $$  $$  $$");
				finish.PutStrMenu(x, 13, "$$$$  $$$$  $$$$  $$  $$  $$      $$  $$  $$  $$  $$  $$          $$      $$  $$  $$  $$  $$  $$    ");
				finish.PutStrMenu(x, 14, "$$$$  $$$$  $$$$  $$$$$$  $$$$$$  $$  $$  $$  $$  $$  $$$$$$  $$$$$$      $$$$$$$$$$  $$  $$  $$  $$");
			} else {
				finish.PutStrMenu(x,  5, "$$$$  $$$$                                                                                              $$");
				finish.PutStrMenu(x,  6, "$$$$  $$$$                                                                                              $$");
				finish.PutStrMenu(x,  7, "$$$$  $$$$                                                                                              $$");
				finish.PutStrMenu(x,  8, "$$$$  $$$$                                            $$                                                $$");
				finish.PutStrMenu(x,  9, "$$$$$$$$$$                                            $$                                                $$");
				finish.PutStrMenu(x, 10, "$$$$$$$$$$  $$  $$  $$$$$$$$$$  $$$$$$  $$$$$$  $$  $$$$$$  $$  $$      $$  $$  $$  $$  $$$$$$  $$$$$$  $$");
				finish.PutStrMenu(x, 11, "$$$$  $$$$  $$  $$  $$  $$  $$      $$  $$  $$        $$    $$  $$      $$  $$  $$  $$  $$  $$  $$      $$");
				finish.PutStrMenu(x, 12, "$$$$  $$$$  $$  $$  $$  $$  $$  $$$$$$  $$  $$  $$    $$    $$  $$      $$  $$  $$  $$  $$  $$  $$$$$$  $$");
				finish.PutStrMenu(x, 13, "$$$$  $$$$  $$  $$  $$  $$  $$  $$  $$  $$  $$  $$    $$    $$  $$      $$  $$  $$  $$  $$  $$      $$    ");
				finish.PutStrMenu(x, 14, "$$$$  $$$$  $$$$$$  $$  $$  $$  $$$$$$  $$  $$  $$    $$    $$$$$$      $$$$$$$$$$  $$  $$  $$  $$$$$$  $$");
				finish.PutStrMenu(x, 15, "                                                                $$                                        ");
				finish.PutStrMenu(x, 16, "                                                            $$$$$$                                        ");
			}
			finish.Print();

			System.Threading.Thread.Sleep(3000);

			getInstance().Start();
		}
	}
}
