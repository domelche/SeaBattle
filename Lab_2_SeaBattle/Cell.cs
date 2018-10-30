using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2_SeaBattle
{
	public class Cell
	{
		public enum Types { Empty, Shooted, Dead, Ship };
		public static String[] typeStr = { "  ", "<>", "><", "[]" };



		public int x;
		public int y;
		public Types type { get; set; }
		public Ship ship { get; set; }



		private Cell() { }

		public Cell(int x, int y) {
			type = Types.Empty;
			this.x = x;
			this.y = y;
		}



		public static Cell operator + (Cell a, Cell b) => new Cell(a.x + b.x, a.y + b.y);
		public static Cell operator - (Cell a, Cell b) => new Cell(a.x - b.x, a.y - b.y);
	}
}
