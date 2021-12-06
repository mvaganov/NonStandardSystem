﻿using System;
using System.Collections.Generic;

namespace NonStandard.Extension {
    public static class MathExtension {
        public static float Clamp01(this float f) => f < 0 ? 0 : f > 1 ? 1 : f;
        public static float Clamp0(this float f, float max) => f < 0 ? 0 : f > max ? max : f;

        public static float Clamp(this float f, float min, float max) => f < min ? min : f > max ? max : f;
        public static int Clamp(this int val, int min, int max) => (val < min) ? min : ((val > max) ? max : val);
        public static byte Clamp(this byte val, byte min, byte max) => (val < min) ? min : ((val > max) ? max : val);
        public static int RoundToInt(this float val) => (int) (val + 0.5f);
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;
            return val;
        }
    }
}