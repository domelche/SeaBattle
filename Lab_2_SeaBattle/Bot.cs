using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2_SeaBattle {

	public class Bot : Player {

		private static Random _rnd;

		private List<Cell> _currentSuccessShots;
		private List<Cell> _possibleShots;
		private Player _enemy;



		static Bot() {
			_rnd = new Random();
		}

		public Bot(Display display, int boardSize, Player enemy) : base(boardSize) {
			_currentSuccessShots = new List<Cell>(3);
			_possibleShots = new List<Cell>(boardSize * boardSize);
			_enemy = enemy;
			enemy.SetEnemy(this);
			for (int x = 0; x < boardSize; ++x)
				for (int y = 0; y < boardSize; ++y)
					_possibleShots.Add(enemyBoard.cells[x, y]);

			for (int i = 0, j; i < 4; ++i)
				for (j = 0; j < i + 1; ++j) {
					ships.Add(new Ship(4 - i));
					board.PlaceShip(null, ships.Last());
				}
		}



		public override void Shoot(Display display) {
			if (_currentSuccessShots.Count == 0)
				ShootRandom(display);
			else if (_currentSuccessShots.Count == 1)
				ShootAround(display);
			else
				ShootFinish(display);
		}

		private void ShootRandom(Display display) {

			int i;
			bool? res;

			i = _rnd.Next(_possibleShots.Count);
			if ((res = HandleShot(display, _enemy, _possibleShots[i], display.playersBoardStart)) == true) {
				if (_possibleShots[i].ship.hp == 0) {
					RemoveAround(_possibleShots[i]);
					if (destroyedShips == 10) {
						Game.getInstance().Finish(false);
						return;
					}
				}
				else {
					_currentSuccessShots.Add(_possibleShots[i]);
					_possibleShots.RemoveAt(i);
				}
				Shoot(display);
			}
			else if (res == null)
				Shoot(display);
			else {
				_possibleShots.RemoveAt(i);
				_enemy.Shoot(display);
			}
		}

		private bool? InvokeHandleShot(Display display, Cell cell, int x, int y) {
			try {
				return (HandleShot(display, _enemy,
					enemyBoard.cells[cell.x + x, cell.y + y], display.playersBoardStart));
			}
			catch (Exception) { return (null); }
		}

		private void RemoveAround(Cell cell) {

			for (int i = -1; i < 2; ++i)
				for (int j = -1; j < 2; ++j) {
					try {
						_possibleShots.RemoveAt(
							_possibleShots.FindIndex(c => c.x == cell.x + i && c.y == cell.y + j));
					}
					catch (Exception) { }
				}
		}

		private void ShootAround(Display display) {

			int dir;
			bool? res;
			int[] coord;
			Cell shot;

			res = null;
			coord = null;
			dir = _rnd.Next(4);
			for (int i = 0; i < 4; ++i) {
				switch (dir = (dir + i) % 4) {
					case 0:
						res = InvokeHandleShot(display, _currentSuccessShots[0], 0, -1);
						coord = new int[2] { 0, -1 };
						break;

					case 1:
						res = InvokeHandleShot(display, _currentSuccessShots[0], 1, 0);
						coord = new int[2] { 1, 0 };
						break;

					case 2:
						res = InvokeHandleShot(display, _currentSuccessShots[0], 0, 1);
						coord = new int[2] { 0, 1 };
						break;

					case 3:
						res = InvokeHandleShot(display, _currentSuccessShots[0], -1, 0);
						coord = new int[2] { -1, 0 };
						break;
				}
				if (res != null)
					break;
			}

			shot = _possibleShots.Find(c => c.x == _currentSuccessShots[0].x + coord[0] &&
											c.y == _currentSuccessShots[0].y + coord[1]);
			shot = enemyBoard.cells[shot.x, shot.y];

			if (res == true) {
				if (shot.ship.hp == 0) {
					RemoveAround(_currentSuccessShots.First());
					RemoveAround(_currentSuccessShots.Last());
					if (destroyedShips == 10) {
						Game.getInstance().Finish(false);
						return;
					}
					_currentSuccessShots.RemoveAll(x => true);
				}
				else
					_currentSuccessShots.Add(shot);
				_possibleShots.Remove(shot);
				Shoot(display);
			}
			else {
				_possibleShots.Remove(shot);
				_enemy.Shoot(display);
			}
		}

		private void ShootFinish(Display display) {

			Cell shot,
				 start,
				 end;
			bool? res;
			bool isTurned;
			int[] coord;

			start = _currentSuccessShots.First();
			end = _currentSuccessShots.Last();
			coord = new int[2] { Math.Sign(end.x - start.x), Math.Sign(end.y - start.y) };

			res = InvokeHandleShot(display, end, coord[0], coord[1]);
			isTurned = false;

			if (res == null) {
				res = InvokeHandleShot(display, start, -coord[0], -coord[1]);
				isTurned = true;
			}

			shot = (!isTurned)
				? _possibleShots.Find(c => c.x == end.x + coord[0] &&
										   c.y == end.y + coord[1])
				: _possibleShots.Find(c => c.x == start.x - coord[0] &&
										   c.y == start.y - coord[1]);
			shot = enemyBoard.cells[shot.x, shot.y];

			if (res == true) {
				if (shot.ship.hp == 0) {
					RemoveAround(_currentSuccessShots.First());
					RemoveAround(_currentSuccessShots.Last());
					if (destroyedShips == 10) {
						Game.getInstance().Finish(false);
						return;
					}
					_currentSuccessShots.RemoveAll(x => true);
				}
				else if (!isTurned)
					_currentSuccessShots.Add(shot);
				else
					_currentSuccessShots.Insert(0, shot);
				_possibleShots.Remove(shot);
				Shoot(display);
			}
			else {
				_possibleShots.Remove(shot);
				_enemy.Shoot(display);
			}
		}
	}
}
