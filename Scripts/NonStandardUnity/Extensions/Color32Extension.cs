using UnityEngine;

namespace NonStandard.Extension {
	public static class Color32Extension {
		public static bool EqualRgba(this Color32 a, Color32 b) { return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a; }
	}
}