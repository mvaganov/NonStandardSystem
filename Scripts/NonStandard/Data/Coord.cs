﻿using System;

namespace NonStandard.Data {
	public interface IPosition { Coord GetPosition(); }

	[System.Serializable]
	public partial struct Coord : IComparable<Coord> {
		public short row, col;

		public Coord(int col, int row) {
			this.col = (short)col;
			this.row = (short)row;
		}

		public int Col { get => col; set => col = (short)value; }
		public int Row { get => row; set => row = (short)value; }
		public int X { get => col; set => col = (short)value; }
		public int Y { get => row; set => row = (short)value; }
		public int x { get => col; set => col = (short)value; }
		public int y { get => row; set => row = (short)value; }
		public int Area { get { return X * Y; } }

		public static readonly Coord Zero = new Coord(0, 0);
		public static readonly Coord One = new Coord(1, 1);
		public static readonly Coord NegativeOne = new Coord(-1, -1);
		public static readonly Coord Two = new Coord(2, 2);
		public static readonly Coord Up = new Coord(0, -1);
		public static readonly Coord Left = new Coord(-1, 0);
		public static readonly Coord Down = new Coord(0, 1);
		public static readonly Coord Right = new Coord(1, 0);
		public static readonly Coord Max = new Coord(short.MaxValue, short.MaxValue);
		public static readonly Coord Min = new Coord(short.MinValue, short.MinValue);

		public static Coord[] CardinalDirs = new Coord[] { Coord.Up, Coord.Left, Coord.Right, Coord.Down };
		private static Coord[] CardinalDirsAll = new Coord[] {
			Left+Up  , Up ,  Up+Right,
			Left,               Right,
			Left+Down,Down,Down+Right
		};
		public enum CardinalDirsEnum { Up = 0, Left = 1, Right = 2, Down = 3 }
		public enum CardinalDirsAllEnum { UpLeft = 0, Up = 1, UpRight = 2, Left = 3, Right = 4, DownLeft = 5, Down = 6, DownRight = 7 }

		public override string ToString() => "(" + col + "," + row + ")";
		public override int GetHashCode() => row * 0x00010000 + col;
		public override bool Equals(object o) {
			return (o == null || o.GetType() != typeof(Coord)) ? false : Equals((Coord)o);
		}
		public bool Equals(Coord c) => row == c.row && col == c.col;

		public static bool operator ==(Coord a, Coord b) => a.row == b.row && a.col == b.col;
		public static bool operator !=(Coord a, Coord b) => a.row != b.row || a.col != b.col;
		public static Coord operator +(Coord a, Coord b) => new Coord(a.col + b.col, a.row + b.row);
		public static Coord operator *(Coord a, Coord b) => new Coord(a.col * b.col, a.row * b.row);
		public static Coord operator *(Coord a, int n) => new Coord(a.col * n, a.row * n);
		public static Coord operator -(Coord a, Coord b) => new Coord(a.col - b.col, a.row - b.row);
		public static Coord operator -(Coord a) => new Coord(-a.col, -a.row);

		public Coord Scale(Coord scale) { col *= scale.col; row *= scale.row; return this; }
		public Coord InverseScale(Coord scale) { col /= scale.col; row /= scale.row; return this; }

		/// <param name="min">inclusive starting point</param>
		/// <param name="max">exclusive limit</param>
		/// <returns>if this is within the given range</returns>
		public bool IsWithin(Coord min, Coord max) {
			return row >= min.row && row < max.row && col >= min.col && col < max.col;
		}

		/// <param name="max">exclusive limit</param>
		/// <returns>IsWithin(<see cref="Coord.Zero"/>, max)</returns>
		public bool IsWithin(Coord max) => IsWithin(Zero, max);
		public bool Contains(Coord c) => c.IsWithin(Zero, this);

		public bool IsGreaterThan(Coord other) => col > other.col && row > other.row;
		public bool IsGreaterThanOrEqualTo(Coord other) => col >= other.col && row >= other.row;

		public void Clamp(Coord min, Coord max) {
			col = (col < min.col) ? min.col : (col > max.col) ? max.col : col;
			row = (row < min.row) ? min.row : (row > max.row) ? max.row : row;
		}

		public static Coord SizeOf(Array map) {
			return new Coord { row = (short)map.GetLength(0), col = (short)map.GetLength(1) };
		}

		/// <summary>
		/// use in a do-while, since it increments
		/// </summary>
		/// <param name="max"></param>
		/// <param name="mincol"></param>
		/// <returns></returns>
		public bool Iterate(Coord max, short mincol = 0) {
			if (++col >= max.col) {
				if (++row >= max.row) { return false; }
				col = mincol;
			}
			return true;
		}
		public bool Increment(Coord max, short mincol = 0) { return Iterate(max,mincol); }
		public static void ForEach(Coord min, Coord max, Action<Coord> action) {
			Coord cursor = min;
			for (cursor.row = min.row; cursor.row < max.row; ++cursor.row) {
				for (cursor.col = min.col; cursor.col < max.col; ++cursor.col) {
					action(cursor);
				}
			}
		}

		public void ForEach(Action<Coord> action) => ForEach(Zero, this, action);

		/// <summary>
		/// stops iterating as soon as action returns true
		/// </summary>
		/// <param name="action">runs till the first return true</param>
		/// <returns>true if action returned true even once</returns>
		public static bool ForEach(Coord min, Coord max, Func<Coord, bool> action) {
			Coord cursor = min;
			for (cursor.row = min.row; cursor.row < max.row; ++cursor.row) {
				for (cursor.col = min.col; cursor.col < max.col; ++cursor.col) {
					if (action(cursor)) { return true; }
				}
			}
			return false;
		}

		public bool ForEach(Func<Coord, bool> action) => ForEach(Zero, this, action);

		public static void ForEachInclusive(Coord start, Coord end, Action<Coord> action) {
			bool colIncrease = start.col < end.col, rowIncrease = start.row < end.row;
			Coord cursor = start;
			do {
				cursor.col = start.col;
				do {
					action(cursor);
					if (cursor.col == end.col || (colIncrease ? cursor.col > end.col : cursor.col < end.col)) { break; }
					if (colIncrease) { ++cursor.col; } else { --cursor.col; }
				} while (true);
				if (cursor.row == end.row || (rowIncrease ? cursor.row > end.row : cursor.row < end.row)) { break; }
				if (rowIncrease) { ++cursor.row; } else { --cursor.row; }
			} while (true);
		}

		public float GetMagnitude() { return (float)Math.Sqrt(col*col+row*row); }
		public static int ManhattanDistance(Coord a, Coord b) {
			Coord delta = b - a;
			return Math.Abs(delta.col) + Math.Abs(delta.row);
		}

		public void SetConsoleCursorPosition() => Console.SetCursorPosition(col, row);
		public static Coord GetCursorPosition() => new Coord(Console.CursorLeft, Console.CursorTop);

		public int CompareTo(Coord other) {
			if (row < other.row) return -1;
			if (row > other.row) return 1;
			if (col < other.col) return -1;
			if (col > other.col) return 1;
			return 0;
		}

		public short this[int i] {
			get {
				switch (i) {
				case 0: return col;
				case 1: return row;
				default: throw new Exception("must be 0 or 1, not " + i);
				}
			}
			set {
				switch (i) {
				case 0: col = value; break;
				case 1: row = value; break;
				default: throw new Exception("must be 0 or 1, not " + i);
				}
			}
		}
	}

	public static class IPositionExtension {
		public static int CompareTo(this IPosition self, IPosition other) {
			return self.GetPosition().CompareTo(other.GetPosition());
		}
		public static int ManhattanDistance(this IPosition self, IPosition other) {
			return Coord.ManhattanDistance(self.GetPosition(), other.GetPosition());
		}
	}

	public static class MatrixCoordExtension {
		public static Coord GetSize<TYPE>(this TYPE[,] matrix) {
			return new Coord(matrix.GetLength(1), matrix.GetLength(0));
		}
		public static TYPE At<TYPE>(this TYPE[,] matrix, Coord coord) {
			return matrix[coord.row, coord.col];
		}
		public static void SetAt<TYPE>(this TYPE[,] matrix, Coord position, TYPE value) {
			matrix[position.row, position.col] = value;
		}
		public static void SetAt<TYPE>(this TYPE[,] matrix, Coord position, Coord size, TYPE value) {
			Coord cursor;
			for (cursor.row = 0; cursor.row < size.row; ++cursor.row) {
				for (cursor.col = 0; cursor.col < size.col; ++cursor.col) {
					matrix.SetAt(cursor + position, value); ;
				}
			}
		}
		public static void Fill<TYPE>(this TYPE[,] matrix, TYPE value) {
			SetAt(matrix, Coord.Zero, matrix.GetSize(), value);
		}
	}
}