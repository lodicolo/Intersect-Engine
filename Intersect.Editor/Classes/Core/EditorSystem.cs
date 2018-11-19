﻿using System.Diagnostics;

namespace Intersect.Editor
{
    public class EditorSystem
    {
        public Stopwatch StopWatch = new Stopwatch();

        public EditorSystem()
        {
            StopWatch.Start();
        }

        public long GetTimeMs()
        {
            return StopWatch.ElapsedMilliseconds;
        }
    }
}