using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2_SeaBattle {

	public class Ship {

		public int hp { get; private set; }
		public readonly int size;
		public List<Cell> cells { get; }



		private Ship() { }

		public Ship(int size) {
			this.size = size;
			hp = size;
			cells = new List<Cell>(size);
		}



		//public void 
		public void decHp() { --hp; }
	}
}
