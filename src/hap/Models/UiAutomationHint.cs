﻿using System;
using System.Windows;
using System.Windows.Automation;
using hap.Extensions;
using hap.NativeMethods;

namespace hap.Models
{
    /// <summary>
    /// Represents a Windows UI Automation based hint
    /// </summary>
    internal class UiAutomationHint : Hint
    {
        private readonly Lazy<InvokePattern> _invokePattern;
        private readonly Lazy<string> _accessKey;

        public UiAutomationHint(User32.HWND owningWindow, Rect windowBounds, AutomationElement automationElement)
            : base(owningWindow, GetBoundingRect(owningWindow, windowBounds, automationElement))
        {
            AutomationElement = automationElement;

            if (BoundingRectangle.IsEmpty) return;

            _invokePattern = new Lazy<InvokePattern>(() => TryGetInvokePattern(automationElement));
            _accessKey =
                new Lazy<string>(
                    () =>
                        automationElement.GetCurrentPropertyValue(AutomationElement.AccessKeyProperty, true) as string);
        }

        private static Rect GetBoundingRect(User32.HWND owningWindow, Rect windowBounds, AutomationElement automationElement)
        {
            var boundingRectObject =
                automationElement.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty, true);

            if (boundingRectObject == AutomationElement.NotSupported)
            {
                // Not supported
                return Rect.Empty;
            }

            var boundingRect = (Rect) boundingRectObject;
            if (boundingRect.IsEmpty)
            {
                // Not currently displaying UI
                return Rect.Empty;
            }

            // Convert the bounding rect to logical coords
            var logicalRect = boundingRect.PhysicalToLogicalRect(owningWindow);
            if (!logicalRect.IsEmpty)
            {
                var windowCoords = boundingRect.ScreenToWindowCoordinates(windowBounds);
                return windowCoords;
            }
            return Rect.Empty;
        }

        private InvokePattern TryGetInvokePattern(AutomationElement automationElement)
        {
            object invokePattern;
            if (automationElement.TryGetCurrentPattern(InvokePattern.Pattern, out invokePattern))
            {
                return invokePattern as InvokePattern;
            }
            return null;
        }

        /// <summary>
        /// The underlying automation element
        /// </summary>
        public AutomationElement AutomationElement { get; private set; }

        public override string AccessKey
        {
            get { return _accessKey.Value; }
        }

        public override void Invoke()
        {
            _invokePattern.Value.Invoke();
        }
    }
}