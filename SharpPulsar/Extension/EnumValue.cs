﻿using System;
using System.Linq;

namespace SharpPulsar.Extension
{
    public static class EnumValue
    {
        public static object GetCompressionTypeValue(this Api.ICompressionType compression)
        {
            //return Enum.GetValues(typeof(object)).Cast().ToList()[(int) compression];
            return null;
        }
    }
}
