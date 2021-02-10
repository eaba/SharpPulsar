﻿using System;
using System.Buffers;

namespace SharpPulsar.Helpers
{
    public static class ReadOnlySequenceExtensions
    {
        public static bool StartsWith<T>(this ReadOnlySequence<T> sequence, ReadOnlyMemory<T> target) where T : IEquatable<T>
        {
            if (target.Length > sequence.Length)
                return false;

            var targetIndex = 0;
            var targetSpan = target.Span;

            foreach (var memory in sequence)
            {
                var span = memory.Span;
                for (var i = 0; i < span.Length; ++i)
                {
                    if (!span[i].Equals(targetSpan[targetIndex]))
                        return false;

                    ++targetIndex;
                    if (targetIndex == targetSpan.Length)
                        return true;
                }
            }

            return false;
        }

        public static uint ReadUInt32(this ReadOnlySequence<byte> sequence, long start, bool isBigEndian)
        {
            if (sequence.Length < 4 + start)
                throw new ArgumentOutOfRangeException(nameof(start), start, "Sequence must be at least 4 bytes long from 'start' to end");

            var reverse = isBigEndian != BitConverter.IsLittleEndian;
            var union = new UIntUnion();
            var read = 0;

            foreach (var memory in sequence)
            {
                if (start > memory.Length)
                {
                    start -= memory.Length;
                    continue;
                }

                var span = memory.Span;
                for (var i = (int)start; i < span.Length; ++i, ++read)
                {
                    switch (read)
                    {
                        case 0:
                            if (reverse) union.B0 = span[i];
                            else union.B3 = span[i];
                            continue;
                        case 1:
                            if (reverse) union.B1 = span[i];
                            else union.B2 = span[i];
                            continue;
                        case 2:
                            if (reverse) union.B2 = span[i];
                            else union.B1 = span[i];
                            continue;
                        case 3:
                            if (reverse) union.B3 = span[i];
                            else union.B0 = span[i];
                            break;
                    }
                }

                if (read == 3)
                    break;
                start = 0;
            }

            return union.UInt;
        }
    }
}