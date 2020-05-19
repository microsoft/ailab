using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Speaker.Recorder.Helpers
{
    public class NvApiLoader
    {
        public static bool TryLoad()
        {
            var libName = Environment.Is64BitProcess ? "nvapi64.dll" : "nvapi.dll";
            return NativeLibrary.TryLoad(libName, out _);
        }
    }
}
