using System;
using System.Windows;
using System.Windows.Automation;
using hap.Extensions;

namespace hap.Models
{
    /// <summary>
    /// Represents a Windows UI Automation based hint
    /// </summary>
    internal class UiAutomationHint : Hint
    {
        private readonly Lazy<InvokePattern> _invokePattern;

        public UiAutomationHint(IntPtr owningWindow, Rect windowBounds, AutomationElement automationElement)
            : base(owningWindow, GetBoundingRect(owningWindow, windowBounds, automationElement))
        {
            AutomationElement = automationElement;

            if (BoundingRectangle.IsEmpty) return;

            _invokePattern = new Lazy<InvokePattern>(() => TryGetInvokePattern(automationElement));
        }

        private static Rect GetBoundingRect(IntPtr owningWindow, Rect windowBounds, AutomationElement automationElement)
        {
            var boundingRectObject = automationElement.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty, true);

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

        public override void Invoke()
        {
            _invokePattern.Value.Invoke();
        }
    }
}
