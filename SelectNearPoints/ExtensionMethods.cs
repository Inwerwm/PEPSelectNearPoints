using PEPlugin.SDX;
using System;

namespace SelectNearPoints
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// 指定された閉区間の範囲内であるかを判断します。
        /// </summary>
        /// <param name="i"></param>
        /// <param name="lower">下限（自身も含む）</param>
        /// <param name="upper">上限（自身も含む）</param>
        /// <returns></returns>
        public static bool IsWithin<T>(this T i, T lower, T upper) where T : IComparable
        {
            if (upper.CompareTo(lower) < 0)
                throw new ArgumentOutOfRangeException("IsWithin<T>:下限値が上限値よりも大きいです。");
            return i.CompareTo(lower) * upper.CompareTo(i) >= 0;
        }

        /// <summary>
        /// 指定された開区間の範囲内であるかを判断します。
        /// </summary>
        /// <param name="i"></param>
        /// <param name="lower">下限（自身は含まない）</param>
        /// <param name="upper">上限（自身は含まない）</param>
        /// <returns></returns>
        public static bool IsInside<T>(this T i, T lower, T upper) where T : IComparable
        {
            if (upper.CompareTo(lower) < 0)
                throw new ArgumentOutOfRangeException("IsInside<T>:下限値が上限値よりも大きいです。");
            return i.CompareTo(lower) * upper.CompareTo(i) > 0;
        }

        /// <summary>
        /// 末尾に改行文字を加えてstringに変換する
        /// </summary>
        public static string ToStringL(this object value)
        {
            return value.ToString() + Environment.NewLine;
        }

        /// <summary>
        /// 「名前 = 値↲」の形で出力する
        /// </summary>
        /// <param name="name">名前</param>
        public static string ToStringN(this object value, string name)
        {
            return name + " = " + value.ToStringL();
        }

        public static float[] ToArray(this V3 v)
        {
            return new float[] { v.X, v.Y, v.Z };
        }

        public static float[] ToArray(this V2 v)
        {
            return new float[] { v.X, v.Y };
        }
    }
}
