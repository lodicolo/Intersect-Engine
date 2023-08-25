using System;

namespace Intersect.Client.Framework.Gwen
{
    /// <summary>
    ///     Represents relative position.
    /// </summary>
    [Flags]
    public enum Pos
    {
        None = 0,

        Left = 1 << 1,

        Right = 1 << 2,

        Top = 1 << 3,

        Bottom = 1 << 4,

        CenterV = 1 << 5,

        CenterH = 1 << 6,

        Fill = 1 << 7,

        Center = CenterV | CenterH
    }

    public static class PosExtensions
    {
        public static Pos Simplify(this Pos pos)
        {
            if (pos == Pos.None)
            {
                return pos;
            }

            if (pos.HasFlag(Pos.Fill))
            {
                return Pos.Fill;
            }

            var simplified = Pos.None;

            if (pos.HasFlag(Pos.Left) && pos.HasFlag(Pos.Right))
            {
                simplified |= Pos.CenterH;
            }

            if (pos.HasFlag(Pos.Top) && pos.HasFlag(Pos.Bottom))
            {
                simplified |= Pos.CenterV;
            }

            if (simplified == Pos.Center)
            {
                return simplified;
            }

            if (simplified.HasFlag(Pos.CenterV))
            {
                if (pos.HasFlag(Pos.Left))
                {
                    simplified |= Pos.Left;
                }

                if (pos.HasFlag(Pos.Right))
                {
                    simplified |= Pos.Right;
                }

                return simplified;
            }

            if (simplified.HasFlag(Pos.CenterH))
            {
                if (pos.HasFlag(Pos.Top))
                {
                    simplified |= Pos.Top;
                }

                if (pos.HasFlag(Pos.Bottom))
                {
                    simplified |= Pos.Bottom;
                }

                return simplified;
            }

            simplified |= pos;

            return simplified;
        }
    }
}