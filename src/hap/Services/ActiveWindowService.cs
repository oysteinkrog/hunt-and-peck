using System;
using System.Runtime.InteropServices;
using System.Text;
using hap.Services.Interfaces;

namespace hap.Services
{
    public class ActiveWindowService : IActiveWindowService, IDisposable
    {
        private WinEventDelegate _lpfnWinEventProc;
        private IntPtr _mHhook;

        public ActiveWindowService()
        {
            _lpfnWinEventProc = WinEventProc;
            _mHhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _lpfnWinEventProc,
                0, 0,
                WINEVENT_OUTOFCONTEXT);
        }

        ~ActiveWindowService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool managed)
        {
            if (_lpfnWinEventProc != null)
            {
                UnhookWinEvent(_lpfnWinEventProc);
                _lpfnWinEventProc = null;
            }
        }

        private delegate void WinEventDelegate(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
            uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(
            WinEventDelegate hWinEventHook
            );

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            ActiveWindowChanged?.Invoke(hwnd);
        }

        public event Action<IntPtr> ActiveWindowChanged;
    }
}