﻿using System;
using System.Windows;
using hap.NativeMethods;

namespace hap.Models
{
    /// <summary>
    /// Represents a hint that has 1 or more capabilities
    /// </summary>
    public abstract class Hint
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="owningWindow">The owning window</param>
        /// <param name="boundingRectangle">The bounding rectangle of the hint in owner window coordinates</param>
        protected Hint(User32.HWND owningWindow, Rect boundingRectangle)
        {
            OwningWindow = owningWindow;
            BoundingRectangle = boundingRectangle;
        }

        /// <summary>
        /// The bounding rectangle for the hint in Window coordinates for the owning window
        /// </summary>
        public Rect BoundingRectangle { get; private set; }

        /// <summary>
        /// The window handle of the owning window
        /// </summary>
        public User32.HWND OwningWindow { get; private set; }

        /// <summary>
        /// Invokes the hint
        /// </summary>
        public abstract void Invoke();

        public abstract string AccessKey { get; }
    }
}
