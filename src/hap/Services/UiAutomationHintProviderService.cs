using hap.Extensions;
using hap.Models;
using hap.NativeMethods;
using hap.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;

namespace hap.Services
{
    internal class UiAutomationHintProviderService : IHintProviderService
    {
        /// <summary>
        /// Enumerate the available hints for the current foreground window
        /// </summary>
        /// <returns>The hint session containing the available hints</returns>
        public HintSession EnumHints()
        {
            var desktopHandle = User32.GetForegroundWindow();
            var hints =  EnumHints(desktopHandle);
            return hints;
        }

        /// <summary>
        /// Enumerate the available hints for the given window
        /// </summary>
        /// <param name="hWnd">The window handle of window to enumerate hints in</param>
        /// <returns>The hint session containing the available hints</returns>
        public HintSession EnumHints(IntPtr hWnd)
        {
            var elements = EnumElements(hWnd);

            // Window bounds
            var rawWindowBounds = new RECT();
            User32.GetWindowRect(hWnd, ref rawWindowBounds);
            Rect windowBounds = rawWindowBounds;

            var result = new List<Hint>();
            foreach (AutomationElement element in elements)
            {
                result.Add(new UiAutomationHint(hWnd, windowBounds, element));
            }
            return new HintSession
            {
                Hints = result.Where(hint => !hint.BoundingRectangle.IsEmpty).ToList(),
                OwningWindow = hWnd,
                OwningWindowBounds = windowBounds,
            };
        }

        /// <summary>
        /// Enumerates the automation elements from the given window
        /// </summary>
        /// <param name="hWnd">The window handle</param>
        /// <returns>All of the automation elements found</returns>
        private AutomationElementCollection EnumElements(IntPtr hWnd)
        {
            var automationElement = AutomationElement.FromHandle(hWnd);
            var condition = new AndCondition(new PropertyCondition(AutomationElement.IsOffscreenProperty, false),
                                             new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                                             // Filter out non-invoke patterns to speed this up as this can be slow for large windows
                                             new PropertyCondition(AutomationElement.IsInvokePatternAvailableProperty, true)
                                             );

            return automationElement.FindAll(TreeScope.Descendants, condition);
        }
    }
}
