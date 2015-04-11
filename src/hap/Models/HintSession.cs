using System;
using System.Collections.Generic;
using System.Windows;
using hap.NativeMethods;

namespace hap.Models
{
    public class HintSession
    {
        /// <summary>
        /// The hints
        /// </summary>
        public IList<Hint> Hints { get; set; }

        /// <summary>
        /// Owning window for the hints
        /// </summary>
        public User32.HWND OwningWindow { get; set; }

        /// <summary>
        /// Bounds of the owning window in logical screen coordinates
        /// </summary>
        public Rect OwningWindowBounds { get; set; }
    }
}
