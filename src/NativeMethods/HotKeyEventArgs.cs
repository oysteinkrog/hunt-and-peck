using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace hap.NativeMethods
{
    public class HotKeyEventArgs : EventArgs
    {
        public readonly Keys Key;
        public readonly KeyModifier Modifiers;

        public HotKeyEventArgs(Keys key, KeyModifier modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam)
        {
            var param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifier)(param & 0x0000ffff);
        }
    }

    public class MessageWindow : IDisposable
    {
        #region Delegates

        //
        /// <summary>
        /// This delegate is nullable on purpose, if null is returned the "handler"
        /// does not wish to _handle_ the message and the default wndproc is used
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate IntPtr? WndProcNullableRet(User32.HWND hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion

        private readonly string _name;
        private readonly WndProcNullableRet _wndProc;
        private User32.WindowClassAtom _classAtom;
        private string _className;

        private bool _disposed;
        private IntPtr _hInstance;
        private User32.WndProc _internalWndProc;
        private string _windowName;
        private User32.WNDCLASSEX _wndClassEx;
        private User32.HWND _handle;

        public MessageWindow(string name, WndProcNullableRet wndProc)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("class_name is empty or null");
            }
            if (wndProc == null)
            {
                throw new ArgumentNullException("wndProc");
            }
            _name = name;
            _wndProc = wndProc;

            //Log.General.Debug("MessageWindow() name: " + name);

            _hInstance = Marshal.GetHINSTANCE(GetType()
                .Module);
            if (_hInstance == new IntPtr(-1))
            {
                throw new Win32Exception("Couldn't get modules instance");
            }

            int UniqueID = 1;
            _className = _name + " class " + UniqueID;
            if (_className.Length > 255)
            {
                throw new ArgumentException("class name too long");
            }

            //Log.General.Debug("MessageWindow.Create classname: " + _className);

            _internalWndProc = WndProc;

            _wndClassEx = new User32.WNDCLASSEX
            {
                cbSize = (uint) Marshal.SizeOf(typeof (User32.WNDCLASSEX)),
                style =
                    User32.CS.OWNDC | User32.CS.HREDRAW |
                    User32.CS.VREDRAW,
                lpfnWndProc = _internalWndProc,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = _hInstance,
                hIcon = IntPtr.Zero,
                hCursor = IntPtr.Zero,
                hbrBackground = IntPtr.Zero,
                lpszMenuName = null,
                lpszClassName = _className,
                hIconSm = IntPtr.Zero
            };

            _classAtom = User32.RegisterClassEx(ref _wndClassEx);
            if (_classAtom.IsInvalid)
            {
                throw new Win32Exception("Could not register window class");
            }

            _windowName = _name + " wnd " + UniqueID;

            //Log.General.Debug("MessageWindow.Create windowname: " + _windowName);

            // Create window
            _handle =
                User32.StrongHWND.CreateWindowEx(
                    User32.WindowStylesEx.WS_EX_CLIENTEDGE | User32.WindowStylesEx.WS_EX_APPWINDOW,
                    _className,
                    _windowName,
                    User32.WindowStyles.WS_OVERLAPPEDWINDOW,
                    // position
                    0,
                    0,
                    // size
                    0,
                    0,
                    // no parent
                    User32.HWND.NULL,
                    // no menu
                    IntPtr.Zero,
                    _hInstance,
                    IntPtr.Zero);

            if (Handle.IsInvalid)
            {
                throw new Win32Exception("Could not create window");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool managed)
        {
            if (!_disposed)
            {
                StopPump();

                if (managed)
                {
                    // Dispose managed resources
                    DisposeUtils.Dispose(ref _handle);
                    DisposeUtils.Dispose(ref _classAtom);
                }

                // Dispose unmanaged resources

                _disposed = true;
            }
        }

        public User32.HWND Handle
        {
            get { return _handle; }
        }

        public void PostMessage(uint message, IntPtr zero, IntPtr intPtr)
        {
            if (Handle.IsInvalid)
            {
                return;
            }

            /* Post the thread to our message pump */
            User32.PostMessageSafe(Handle, message, IntPtr.Zero, IntPtr.Zero);
        }

        public void Pump()
        {
            // Begin the pump 
            MSG msg;
            // this is a blocking call, so it will wait/block until a new message is received
            while (User32.GetMessageSafe(out msg, User32.HWND.NULL, 0, 0))
            {
                var handle = Handle;
                if (handle == null || handle.IsInvalid)
                {
                    return;
                }

                /* Try to translate the message.  
                 * Only here for completeness */
                User32.TranslateMessage(ref msg);

                /* Dispatches the win32 message to the window message proc */
                User32.DispatchMessage(ref msg);
            }
        }

        private void StopPump()
        {
            PostMessage((uint)User32.WM.QUIT, IntPtr.Zero, IntPtr.Zero);
        }

        private IntPtr WndProc(IntPtr rawHwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            //WM_CREATE must always succeed
            var hwnd = new User32.HWND(rawHwnd);
            if (msg == (uint)User32.WM.CREATE || msg == (uint)User32.WM.NCCREATE)
            {
                return User32.DefWindowProc(hwnd, msg, wParam, lParam);
            }

            IntPtr? ret = _wndProc(hwnd, msg, wParam, lParam);
            if (ret.HasValue)
            {
                return ret.Value;
            }
            return User32.DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }
}
