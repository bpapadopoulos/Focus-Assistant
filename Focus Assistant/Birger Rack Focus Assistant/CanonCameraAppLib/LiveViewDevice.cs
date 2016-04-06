using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanonCameraAppLib
{
    [Flags]
    public enum LiveViewDevice : uint
    {
        None = 0,
        Camera = 1,
        Host = 2
    }
}
