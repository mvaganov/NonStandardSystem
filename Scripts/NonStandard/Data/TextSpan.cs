using System;
using System.Collections.Generic;

namespace NonStandard.Data {
	[System.Serializable] public struct TextSpan : IComparable<TextSpan> {
		/// <summary>
		/// inclusive
		/// </summary>
		public Coord start, end;
		//public object tag;
		public string tag;

		public TextSpan(Coord whereCursorStarted, Coord whereCursorEnded, object tag) : this() {
			this.start = whereCursorStarted;
			this.end = whereCursorEnded;
			//this.tag = tag;
			this.tag = tag.ToString();
		}

		public int CompareTo(TextSpan other) {
			int cmp = start.CompareTo(other.start);
			if (cmp == 0) {
				cmp = -end.CompareTo(other.end);
			}
			return cmp;
		}

		public bool Contains(Coord coord) {
			if (coord.row < start.row || coord.row > end.row)    { return false; }
			if (coord.row == start.row && coord.col < start.col) { return false; }
			if (coord.row == end.row && coord.col > start.col)   { return false; }
			return true;
		}

		public static void GetSpans(IEnumerable<TextSpan> orderedListOfSpans, Coord coord, List<TextSpan> out_found) {
			foreach (TextSpan span in orderedListOfSpans) {
				if (span.Contains(coord)) {
					out_found.Add(span);
				}
				// assume spans are in order, so stop once they start out of range.
				if (span.start.row > coord.row) { break; }
			}
		}
	}
}
