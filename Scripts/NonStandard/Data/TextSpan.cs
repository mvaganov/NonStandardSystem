using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Data {
	public struct TextSpan : IComparable<TextSpan> {
		/// <summary>
		/// inclusive
		/// </summary>
		public Coord start, end;

		public int CompareTo(TextSpan other) { return start.CompareTo(other.start); }

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
