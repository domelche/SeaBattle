using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2_SeaBattle {

	public class Board {

		private static Random _rnd;

		public int size { get; }

		public Cell[,] cells { get; private set; }
		public List<Cell> availableCells { get; }



		static Board() {
			_rnd = new Random();
		}

		private Board() { }

		public Board(int size) {
			this.size = size;
			cells = new Cell[this.size, this.size];
			for (int x = 0; x < size; ++x)
				for (int y = 0; y < size; ++y)
					cells[x, y] = new Cell(x, y);
			availableCells = new List<Cell>(size * size);
			foreach (Cell cell in cells)
				availableCells.Add(cell);
		}



		public bool IsNearShip(int[] start, int[] end) {

			for (int i = -1; i < 2; ++i)
				for (int j = -1; j < 2; ++j)
					try {
						if (cells[start[0] + i, start[1] + j].type == Cell.Types.Ship ||
							cells[end[0] + i, end[1] + j].type == Cell.Types.Ship)
							return (true);
					}
					catch (Exception) { }
			return (false);
		}

		private void ExcludeCells(Ship ship) {

			Cell first,
				 last;

			first = ship.cells.First();
			last = ship.cells.Last();
			for (int i = -1; i < 2; ++i)
				for (int j = -1; j < 2; ++j) {
					availableCells.RemoveAll(
						cell => cell.x == first.x + i && cell.y == first.y + j);
					availableCells.RemoveAll(
						cell => cell.x == last.x + i && cell.y == last.y + j);
				}
		}

		public void PutShip(Display display, Bot bot, Ship ship, Cell cell, int dir) {

			int x;
			int y;
			int xStep;
			int yStep;

			x = cell.x;
			y = cell.y;
			xStep = 0;
			yStep = 0;
			switch (dir) {
				case 0:		xStep =  0; yStep = -1;		break;
				case 1:		xStep =  1; yStep =  0;		break;
				case 2:		xStep =  0; yStep =  1;		break;
				case 3:		xStep = -1; yStep =  0;		break;
			}

			for (int i = 0;
				 i < ship.size;
				 ++i, x += xStep, y += yStep) {

				this.cells[x, y].type = Cell.Types.Ship;
				this.cells[x, y].ship = ship;
				if (bot != null)
					bot.enemyBoard.cells[x, y].ship = ship;

				ship.cells.Add(this.cells[x, y]);
				if (display != null) {
					display.PutStr(
						display.playersBoardStart[0] + x * 2,
						display.playersBoardStart[1] + y,
						Cell.typeStr[(int)Cell.Types.Ship]);
					display.Print();
				}
			}
			ExcludeCells(ship);
		}

		private bool CheckDirection(int size, Cell cell, int xStep, int yStep) {

			int tmp;
			int x;
			int y;

			if ((tmp = cell.x + size * xStep) < 0 || tmp >= this.size ||
				(tmp = cell.y + size * yStep) < 0 || tmp >= this.size)
				return (false);

			x = cell.x + xStep;
			y = cell.y + yStep;
			for (tmp = 1;
				 tmp < size;
				 ++tmp, x += xStep, y += yStep) {

				if (this.cells[x, y].type == Cell.Types.Ship)
					return (false);
			}
			return (!IsNearShip(new int[2] { cell.x, cell.y }, new int[2] { x - xStep, y - yStep }));
		}

		private bool TryPlaceShip(Display display, Ship ship, Cell cell) {

			int direction;

			direction = _rnd.Next() % 4;
			for (int i = 0; i < 4; ++i) {
				switch (direction = (direction + i) % 4) {
					case 0:
						if (CheckDirection(ship.size, cell, 0, -1)) {
							PutShip(display, null, ship, cell, direction);
							return (true);
						}
						break;
					case 1:
						if (CheckDirection(ship.size, cell, 1, 0)) {
							PutShip(display, null, ship, cell, direction);
							return (true);
						}
						break;
					case 2:
						if (CheckDirection(ship.size, cell, 0, 1)) {
							PutShip(display, null, ship, cell, direction);
							return (true);
						}
						break;
					case 3:
						if (CheckDirection(ship.size, cell, -1, 0)) {
							PutShip(display, null, ship, cell, direction);
							return (true);
						}
						break;
				}
			}
			return (false);
		}

		public void PlaceShip(Display display, Ship ship) {
			int i;

			i = _rnd.Next(availableCells.Count);
			foreach (Cell cell in availableCells.Skip(i))
				if (TryPlaceShip(display, ship, cell))
					return;
			foreach (Cell cell in availableCells.Take(i))
				if (TryPlaceShip(display, ship, cell))
					return;
		}

		private static void DestroyShip() {

		}

		//public bool ReceiveShot(Cell cell) {
		//	if (cells[cell.x, cell.y].type == Cell.Types.Ship) {
		//		cells[cell.x, cell.y].ship.decHp();

		//	}
		//}
	}
}
