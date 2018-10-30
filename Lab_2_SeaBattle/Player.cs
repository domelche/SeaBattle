using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2_SeaBattle
{
	public class Player {

		public Board board { get; }
		public Board enemyBoard { get; }
		public List<Ship> ships { get; }
		public int destroyedShips { get; private set; }

		private Bot _enemy;



		public Player(int boardSize) {
			board = new Board(boardSize);
			enemyBoard = new Board(boardSize);
			ships = new List<Ship>(10);
			destroyedShips = 0;
		}



		public void SetEnemy(Bot enemy) {
			_enemy = enemy;
		}



		private static int[] ParseCoord(String str, int i, int boardSize, Display display) {

			int[] xy;

			xy = new int[] { -1, -1 };
			if (str[i] < 'A' ||
				(str[i] > 'A' + boardSize && str[i] < 'a') ||
				str[i] > 'a' + boardSize) {

				display.Log("Error: First coordinate should be a board letter");
				return (xy);
			}
			xy[0] = str[i] - (Char.IsUpper(str[i]) ? 'A' : 'a');
			++i;

			try {
				xy[1] = Int32.Parse(str.Substring(
					i, ((str.Length != i + 1 && Char.IsDigit(str[i + 1])) ? 2 : 1)));
			}
			catch (Exception) { }

			--xy[1];
			if (xy[1] < 0 || xy[1] >= boardSize) {
				display.Log("Error: Second coordinate should be a board number");
				return (xy);
			}

			return (xy);
		}

		public void AskPlaceShips(Display display) {

			int[] counters = { 0, 0, 0, 0 };
			String str;
			String empty;
			int i;
			int dist;
			int dir;
			int[] start;
			int[] end;

			str = null;
			i = 0;
			empty = new String(' ', display.width);

			display.Log("Place your ships, determining the start and the end cells of the ship, using next format:");
			display.Log("  'x1y1-x2y2' (example: a3-c3). Also, you can place several ships with one input,");
			display.Log("  delimiting coordinates with coma and space (b5-b6, a8-a8).");

			while (ships.Count != 10) {
				if (str == null || i >= str.Length || i == 1) {
					str = Console.ReadLine();
					Console.SetCursorPosition(0, Console.CursorTop - 1);
					display.PutStr(0, Console.CursorTop, empty);
					display.Print();
					i = 0;
				}

				if (str.Length < 5) {
					i = str.IndexOf(',', i) + 2;
					display.Log("Error: wrong format of coordinates");
					continue;
				}
				start = ParseCoord(str, i, board.size, display);
				if (start[0] < 0 || start[1] < 0) {
					i = str.IndexOf(',', i) + 2;
					continue;
				}

				if (str[2] == '-')
					i += 3;
				else if (str[3] == '-')
					i += 4;
				else {
					i = str.IndexOf(',', i) + 2;
					display.Log("Error: coordinates should be devided with '-'");
					continue;
				}

				end = ParseCoord(str, i, board.size, display);
				if (end[0] < 0 || end[1] < 0) {
					i = str.IndexOf(',', i) + 2;
					continue;
				}

				dist = Math.Abs(start[0] - end[0] + start[1] - end[1]) + 1;
				if ((start[0] != end[0] && start[1] != end[1]) ||
					dist > 4 || counters[dist - 1] == 5 - dist) {

					i = str.IndexOf(',', i) + 2;
					display.Log("Error: impossible to place ship with such coordinates or size");
					continue;
				}

				if (board.IsNearShip(start, end)) {
					i = str.IndexOf(',', i) + 2;
					display.Log("Error: too close to another ship");
					continue;
				}

				dir = 0;
				if (start[0] == end[0])
					dir = (start[1] - end[1] < 0) ? 2 : 0;
				else if (start[1] == end[1])
					dir = (start[0] - end[0] < 0) ? 1 : 3;

				ships.Add(new Ship(dist));
				board.PutShip(display, _enemy, ships.Last(), board.cells[start[0], start[1]], dir);
				++counters[dist - 1];
				i = str.IndexOf(',', i) + 2;
				display.Log("Ship was successfully placed");
			}
			display.ClearLog();
			display.Log("Now, enter your shooting coordinates in 'xy' format (example: d6)");
			display.Log("And let The Battle begins!");
		}

		protected void HandleDeadShip(Display display, Player enemy, Ship ship, Cell start, Cell end, int[] boardStart) {

			for (int i = -1; i < 2; ++i)
				for (int j = -1; j < 2; ++j) {
					try {
						if (enemy.board.cells[start.x + i, start.y + j].type == Cell.Types.Empty) {
							enemy.board.cells[start.x + i, start.y + j].type = Cell.Types.Shooted;
							enemyBoard.cells[start.x + i, start.y + j].type = Cell.Types.Shooted;
							display.PutStr(
								boardStart[0] + (start.x + i) * 2,
								boardStart[1] + start.y + j,
								Cell.typeStr[(int)Cell.Types.Shooted],
								Color.BlackBlue);
						}
					}
					catch (Exception) { }
					try {
						if (enemy.board.cells[end.x + i, end.y + j].type == Cell.Types.Empty) {
							enemy.board.cells[end.x + i, end.y + j].type = Cell.Types.Shooted;
							enemyBoard.cells[end.x + i, end.y + j].type = Cell.Types.Shooted;
							display.PutStr(
								boardStart[0] + (end.x + i) * 2,
								boardStart[1] + end.y + j,
								Cell.typeStr[(int)Cell.Types.Shooted],
								Color.BlackBlue);
						}
					}
					catch (Exception) { }
				}
			foreach (Cell c in ship.cells) {
				display.AnimateExplosion(boardStart[0] + c.x * 2, boardStart[1] + c.y);
				c.ship = null;
			}
			++destroyedShips;
			display.Print();
		}

		protected bool? HandleShot(Display display, Player enemy, Cell cell, int[] boardStart) {

			if (enemyBoard.cells[cell.x, cell.y].type != Cell.Types.Empty) {
				if (display.botsBoardStart[0] == boardStart[0])
					display.Log("Error: unable to shoot selected cell");
				return (null);
			}
			display.AnimateShot(boardStart[0] + cell.x * 2, boardStart[1] + cell.y);
			if (enemy.board.cells[cell.x, cell.y].type == Cell.Types.Ship) {
				enemy.board.cells[cell.x, cell.y].ship.decHp();
				enemyBoard.cells[cell.x, cell.y].type = Cell.Types.Dead;
				display.PutStr(
					boardStart[0] + cell.x * 2,
					boardStart[1] + cell.y,
					Cell.typeStr[(int)Cell.Types.Dead],
					Color.BlackRed);
				display.Print();
				if (enemy.board.cells[cell.x, cell.y].ship.hp == 0) {
					display.Log("Destroyed!");
					HandleDeadShip(
						display, enemy, enemy.board.cells[cell.x, cell.y].ship,
						enemy.board.cells[cell.x, cell.y].ship.cells.First(),
						enemy.board.cells[cell.x, cell.y].ship.cells.Last(), boardStart);
				}
				else {
					display.Log("Hit!");
					display.AnimateExplosion(boardStart[0] + cell.x * 2, boardStart[1] + cell.y);
				}
				return (true);
			}
			else {
				enemyBoard.cells[cell.x, cell.y].type = Cell.Types.Shooted;
				display.Log("Miss!");
				display.AnimateMiss(boardStart[0] + cell.x * 2, boardStart[1] + cell.y);
				display.PutStr(
					boardStart[0] + cell.x * 2,
					boardStart[1] + cell.y,
					Cell.typeStr[(int)Cell.Types.Shooted],
					Color.BlackBlue);
				display.Print();
				return (false);
			}
		}

		public virtual void Shoot(Display display) {

			String str;
			String empty;
			int[] coord;
			bool? res;

			empty = new String(' ', display.width);
			str = Console.ReadLine();
			Console.SetCursorPosition(0, Console.CursorTop - 1);
			display.PutStr(0, Console.CursorTop, empty);
			display.Print();

			coord = ParseCoord(str, 0, board.size, display);
			if (coord[0] == -1 || coord[1] == -1) {
				Shoot(display);
				return;
			}
			res = HandleShot(display, _enemy, enemyBoard.cells[coord[0], coord[1]],
				display.botsBoardStart);
			if (destroyedShips == 10) {
				Game.getInstance().Finish(true);
				return;
			}
			if (res == true || res == null)
				Shoot(display);
			else
				_enemy.Shoot(display);
		}







		public void PrintEnemyMap(Display display) {

			for (int x = 0; x < board.size; ++x)
				for (int y = 0; y < board.size; ++y) {
					if (_enemy.board.cells[x, y].type == Cell.Types.Ship)
						display.PutStr(display.botsBoardStart[0] + x * 2, display.botsBoardStart[1] + y, "[]");
				}
			display.Print();
		}
	}
}
