using System;
using System.Collections.Generic;

namespace NonStandard.Data {
	public struct TextSpan : IComparable<TextSpan> {
		/// <summary>
		/// inclusive
		/// </summary>
		public Coord start, end;
		public object tag;

		public TextSpan(Coord whereCursorStarted, Coord whereCursorEnded, object tag) : this() {
			this.start = whereCursorStarted;
			this.end = whereCursorEnded;
			this.tag = tag;
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

		public static List<TextSpan> GetSpans(IEnumerable<TextSpan> spans, Coord coord) {
			List<TextSpan> found = new List<TextSpan>();
			foreach (TextSpan span in spans) {
				if (span.Contains(coord)) {
					found.Add(span);
				}
				// assume spans are in order, so stop once they start out of range.
				if (span.start.row > coord.row) { break; }
			}
			return found;
		}
	}
}
