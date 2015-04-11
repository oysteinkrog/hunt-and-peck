using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace hap.NativeMethods
{
    public static class User32
    {
        [DllImport("user32.dll")]
        public static extern HWND GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern HWND GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern HWND GetWindowRect(HWND hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PhysicalToLogicalPoint(HWND hWnd, out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(HWND hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(HWND hWnd, int id);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(HWND hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(HWND hWnd);

        [DllImport("user32.dll")]
        public static extern HWND SetFocus(HWND hWnd);


        /// <summary>Delegate declaration that matches native WndProc signatures.</summary>
        public delegate IntPtr WndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// CS_*
        /// </summary>
        [Flags]
        public enum CS : uint
        {
            VREDRAW = 0x0001,
            HREDRAW = 0x0002,
            DBLCLKS = 0x0008,
            OWNDC = 0x0020,
            CLASSDC = 0x0040,
            PARENTDC = 0x0080,
            NOCLOSE = 0x0200,
            SAVEBITS = 0x0800,
            BYTEALIGNCLIENT = 0x1000,
            BYTEALIGNWINDOW = 0x2000,
            GLOBALCLASS = 0x4000,
            IME = 0x00010000,
            DROPSHADOW = 0x00020000
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public uint cbSize;
            public CS style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
            public IntPtr hIconSm;
        }


        /// <summary>
        /// A SafeHandle representing an HWND.
        /// </summary>
        /// <remarks>
        /// HWNDs have very loose ownership semantics. Unlike normal handles,
        /// there is no "CloseHandle" API for HWNDs. There are APIs like
        /// CloseWindow or DestroyWindow, but these actually affect the window,
        /// not just your handle to the window. This SafeHandle type does not
        /// actually do anything to release the handle in the finalizer, it
        /// simply provides type safety to the PInvoke signatures.
        ///
        /// The StrongHWND SafeHandle will actually destroy the HWND when it
        /// is disposed or finalized.
        ///
        /// Because of this loose ownership semantic, the same HWND value can
        /// be returned from multiple APIs and can be directly compared. Since
        /// SafeHandles are actually reference types, we have to override all
        /// of the comparison methods and operators. We also support equality
        /// between null and HWND(IntPtr.Zero).
        /// </remarks>
        public class HWND : SafeHandle
        {
            static HWND()
            {
                NULL = new HWND(IntPtr.Zero);
                BROADCAST = new HWND(new IntPtr(0xffff));
                MESSAGE = new HWND(new IntPtr(-3));
                DESKTOP = new HWND(new IntPtr(0));
                TOP = new HWND(new IntPtr(0));
                BOTTOM = new HWND(new IntPtr(1));
                TOPMOST = new HWND(new IntPtr(-1));
                NOTOPMOST = new HWND(new IntPtr(-2));

            }
            /// <summary>
            /// Public constructor to create an empty HWND SafeHandle instance.
            /// </summary>
            /// <remarks>
            /// This constructor is used by the marshaller. The handle value
            /// is then set directly.
            /// </remarks>
            public HWND()
                : base(invalidHandleValue: IntPtr.Zero, ownsHandle: false)
            {
            }

            /// <summary>
            /// Public constructor to create an HWND SafeHandle instance for
            /// an existing handle.
            /// </summary>
            public HWND(IntPtr hwnd)
                : this()
            {
                SetHandle(hwnd);
            }

            /// <summary>
            /// Constructor for derived classes to control whether or not the
            /// handle is owned.
            /// </summary>
            protected HWND(bool ownsHandle)
                : base(invalidHandleValue: IntPtr.Zero, ownsHandle: ownsHandle)
            {
            }

            /// <summary>
            /// Constructor for derived classes to specify a handle and to
            /// control whether or not the handle is owned.
            /// </summary>
            protected HWND(IntPtr hwnd, bool ownsHandle)
                : base(invalidHandleValue: IntPtr.Zero, ownsHandle: ownsHandle)
            {
                SetHandle(hwnd);
            }

            public static HWND NULL { get; private set; }
            public static HWND BROADCAST { get; private set; }
            public static HWND MESSAGE { get; private set; }
            public static HWND DESKTOP { get; private set; }
            public static HWND TOP { get; private set; }
            public static HWND BOTTOM { get; private set; }
            public static HWND TOPMOST { get; private set; }
            public static HWND NOTOPMOST { get; private set; }

            public override bool IsInvalid
            {
                get { return handle != IntPtr.Zero && !IsWindow(handle); }
            }

            protected override bool ReleaseHandle()
            {
                // This should never get called, since we specify ownsHandle:false
                // when constructed.
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                if (Object.ReferenceEquals(obj, null))
                {
                    return handle == IntPtr.Zero;
                }
                else
                {
                    HWND other = obj as HWND;
                    return other != null && Equals(other);
                }
            }

            public bool Equals(HWND other)
            {
                if (Object.ReferenceEquals(other, null))
                {
                    return handle == IntPtr.Zero;
                }
                else
                {
                    return other.handle == handle;
                }
            }

            public override int GetHashCode()
            {
                return handle.GetHashCode();
            }

            public static bool operator ==(HWND lvalue, HWND rvalue)
            {
                if (Object.ReferenceEquals(lvalue, null))
                {
                    return Object.ReferenceEquals(rvalue, null) || rvalue.handle == IntPtr.Zero;
                }
                else if (Object.ReferenceEquals(rvalue, null))
                {
                    return lvalue.handle == IntPtr.Zero;
                }
                else
                {
                    return lvalue.handle == rvalue.handle;
                }
            }

            public static bool operator !=(HWND lvalue, HWND rvalue)
            {
                return !(lvalue == rvalue);
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool IsWindow(IntPtr hwnd);
        }

        /// <summary>
        /// A SafeHandle representing an HWND with strong ownership semantics.
        /// </summary>
        /// <remarks>
        /// This class is located in the ComCtl32 library because of its
        /// dependency on WindowSubclass.
        /// </remarks>
        public class StrongHWND : HWND
        {
            public static StrongHWND CreateWindowEx(WindowStylesEx dwExStyle, string lpClassName, string lpWindowName, WindowStyles dwStyle, int x, int y, int nWidth, int nHeight, HWND hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam)
            {
                HWND hwnd = User32.CreateWindowEx(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
                return new StrongHWND(hwnd.DangerousGetHandle());
            }

            public StrongHWND()
                : base(true)
            {
                throw new InvalidOperationException("I need the HWND!");
            }

            public StrongHWND(IntPtr hwnd)
                : base(hwnd, ownsHandle: true)
            {
                _subclass = new StrongHWNDSubclass(this);
            }

            protected override bool ReleaseHandle()
            {
                _subclass.Dispose();
                return true;
            }

            // Called from StrongHWNDSubclass
            internal protected virtual void OnHandleReleased()
            {
                handle = IntPtr.Zero;
            }

            private StrongHWNDSubclass _subclass;
        }

        internal class StrongHWNDSubclass : WindowSubclass
        {
            public StrongHWNDSubclass(StrongHWND strongHwnd)
                : base(new HWND(strongHwnd.DangerousGetHandle()))
            {
                // Note that we passed a new "weak" HWND handle to the base class.
                // This is because we don't want the StrongHWNDSubclass processing
                // a partially disposed handle in its own Dispose methods.
                _strongHwnd = strongHwnd;
            }

            protected override void Dispose(bool disposing)
            {
                // call the base class to let it disconnect the window proc.
                HWND hwnd = Hwnd;
                base.Dispose(disposing);
                DestroyWindow(hwnd);
                _strongHwnd.OnHandleReleased();
            }

            private StrongHWND _strongHwnd;
        }

        /// <summary>
        /// WindowSubclass hooks into the stream of messages that are dispatched to
        /// a window.
        /// </summary>
        public abstract class WindowSubclass : CriticalFinalizerObject, IDisposable
        {
            static WindowSubclass()
            {
                _disposeMessage = RegisterWindowMessage("WindowSubclass.DisposeMessage");
            }

            /// <summary>
            /// Hooks into the stream of messages that are dispatched to the
            /// specified window.
            /// </summary>
            /// <remarks>
            /// The window must be owned by the calling thread.
            /// </remarks>
            public WindowSubclass(HWND hwnd)
            {
                if (!IsCorrectThread(hwnd))
                {
                    throw new InvalidOperationException("May not hook a window created by a different thread.");
                }
                _hwnd = hwnd;
                _wndproc = WndProcStub;
                _wndprocPtr = Marshal.GetFunctionPointerForDelegate(_wndproc);
                // Because our window proc is an instance method, it is connected
                // to this instance of WindowSubclass, where we can store state.
                // We do not need to store any extra state in native memory.
                SetWindowSubclass(hwnd, _wndproc, IntPtr.Zero, IntPtr.Zero);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                DisposeHelper(true);
            }

            ~WindowSubclass()
            {
                // The finalizer is our last chance! The finalizer is always on
                // the wrong thread, but we need to ensure that native code will
                // not try to call into us since we are being removed from memory.
                // Even though it is dangerous, and we risk a deadlock, we
                // send the dispose message to the WndProc to remove itself on
                // the correct thread.
                DisposeHelper(false);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_hwnd == null || !IsCorrectThread(_hwnd))
                {
                    throw new InvalidOperationException("Dispose virtual should only be called by WindowSubclass once on the correct thread.");
                }
                RemoveWindowSubclass(_hwnd, _wndproc, IntPtr.Zero);
                _hwnd = null;
            }

            protected virtual IntPtr WndProcOverride(HWND hwnd, WM msg, IntPtr wParam, IntPtr lParam, IntPtr id, IntPtr data)
            {
                // Call the next window proc in the subclass chain.
                return DefSubclassProc(hwnd, msg, wParam, lParam);
            }

            protected HWND Hwnd
            {
                get
                {
                    return _hwnd;
                }
            }

            private bool IsCorrectThread(HWND hwnd)
            {
                int processId;
                int threadId = GetWindowThreadProcessId(hwnd, out processId);
                return (processId == GetCurrentProcessId() && threadId == GetCurrentThreadId());
            }

            private void DisposeHelper(bool disposing)
            {
                HWND hwnd = _hwnd;
                if (hwnd != null)
                {
                    if (IsCorrectThread(hwnd))
                    {
                        // Call the virtual Dispose(bool)
                        Dispose(disposing);
                    }
                    else
                    {
                        // Send a message to the right thread to dispose for us.
                        SendMessage(hwnd, _disposeMessage, _wndprocPtr, disposing ? new IntPtr(1) : IntPtr.Zero);
                    }
                }
            }

            private IntPtr WndProcStub(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr id, IntPtr data)
            {
                HWND hwnd2 = new HWND(hwnd);
                return WndProc(hwnd2, (WM)msg, wParam, lParam, id, data);
            }

            private IntPtr WndProc(HWND hwnd, WM msg, IntPtr wParam, IntPtr lParam, IntPtr id, IntPtr data)
            {
                IntPtr retval = IntPtr.Zero;
                try
                {
                    retval = WndProcOverride(hwnd, msg, wParam, lParam, id, data);
                }
                finally
                {
                    if (_hwnd != HWND.NULL)
                    {
                        Debug.Assert(_hwnd == hwnd);
                        if (msg == WM.NCDESTROY)
                        {
                            Dispose();
                        }
                        else if (msg == _disposeMessage && wParam == _wndprocPtr)
                        {
                            DisposeHelper(lParam != IntPtr.Zero);
                        }
                    }
                }
                return retval;
            }

            private HWND _hwnd;
            private SUBCLASSPROC _wndproc;
            private IntPtr _wndprocPtr;
            private static readonly WM _disposeMessage;
        }

        // It would be great to use the HWND type for hwnd, but this is not
        // possible because you will get a MarshalDirectiveException complaining
        // that the unmanaged code cannot pass in a SafeHandle. This is because
        // native code is calling this callback, and native code doesn't know how
        // to create a managed SafeHandle for the native handle. I was a little
        // surprised that the marshaller can't do this automatically, but
        // apparently it can't.
        //
        // Instead, most classes that use a SUBCLASSPROC will expose their own
        // virtual that creates new HWND instances for the incomming handles.
        public delegate IntPtr SUBCLASSPROC(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr id, IntPtr data);

        /// <summary>
        /// Window message values, WM_*
        /// </summary>
        public enum WM
        {
            NULL = 0x0000,
            CREATE = 0x0001,
            DESTROY = 0x0002,
            MOVE = 0x0003,
            SIZE = 0x0005,
            ACTIVATE = 0x0006,
            SETFOCUS = 0x0007,
            KILLFOCUS = 0x0008,
            ENABLE = 0x000A,
            SETREDRAW = 0x000B,
            SETTEXT = 0x000C,
            GETTEXT = 0x000D,
            GETTEXTLENGTH = 0x000E,
            PAINT = 0x000F,
            CLOSE = 0x0010,
            QUERYENDSESSION = 0x0011,
            QUIT = 0x0012,
            QUERYOPEN = 0x0013,
            ERASEBKGND = 0x0014,
            SYSCOLORCHANGE = 0x0015,
            SHOWWINDOW = 0x0018,
            CTLCOLOR = 0x0019,
            WININICHANGE = 0x001A,
            SETTINGCHANGE = 0x001A,
            ACTIVATEAPP = 0x001C,
            SETCURSOR = 0x0020,
            MOUSEACTIVATE = 0x0021,
            CHILDACTIVATE = 0x0022,
            QUEUESYNC = 0x0023,
            GETMINMAXINFO = 0x0024,

            WINDOWPOSCHANGING = 0x0046,
            WINDOWPOSCHANGED = 0x0047,

            CONTEXTMENU = 0x007B,
            STYLECHANGING = 0x007C,
            STYLECHANGED = 0x007D,
            DISPLAYCHANGE = 0x007E,
            GETICON = 0x007F,
            SETICON = 0x0080,
            NCCREATE = 0x0081,
            NCDESTROY = 0x0082,
            NCCALCSIZE = 0x0083,
            NCHITTEST = 0x0084,
            NCPAINT = 0x0085,
            NCACTIVATE = 0x0086,
            GETDLGCODE = 0x0087,
            SYNCPAINT = 0x0088,
            NCMOUSEMOVE = 0x00A0,
            NCLBUTTONDOWN = 0x00A1,
            NCLBUTTONUP = 0x00A2,
            NCLBUTTONDBLCLK = 0x00A3,
            NCRBUTTONDOWN = 0x00A4,
            NCRBUTTONUP = 0x00A5,
            NCRBUTTONDBLCLK = 0x00A6,
            NCMBUTTONDOWN = 0x00A7,
            NCMBUTTONUP = 0x00A8,
            NCMBUTTONDBLCLK = 0x00A9,

            SYSKEYDOWN = 0x0104,
            SYSKEYUP = 0x0105,
            SYSCHAR = 0x0106,
            SYSDEADCHAR = 0x0107,
            COMMAND = 0x0111,
            SYSCOMMAND = 0x0112,

            MOUSEMOVE = 0x0200,
            LBUTTONDOWN = 0x0201,
            LBUTTONUP = 0x0202,
            LBUTTONDBLCLK = 0x0203,
            RBUTTONDOWN = 0x0204,
            RBUTTONUP = 0x0205,
            RBUTTONDBLCLK = 0x0206,
            MBUTTONDOWN = 0x0207,
            MBUTTONUP = 0x0208,
            MBUTTONDBLCLK = 0x0209,
            MOUSEWHEEL = 0x020A,
            XBUTTONDOWN = 0x020B,
            XBUTTONUP = 0x020C,
            XBUTTONDBLCLK = 0x020D,
            MOUSEHWHEEL = 0x020E,
            PARENTNOTIFY = 0x0210,

            CAPTURECHANGED = 0x0215,
            POWERBROADCAST = 0x0218,
            DEVICECHANGE = 0x0219,

            ENTERSIZEMOVE = 0x0231,
            EXITSIZEMOVE = 0x0232,

            IME_SETCONTEXT = 0x0281,
            IME_NOTIFY = 0x0282,
            IME_CONTROL = 0x0283,
            IME_COMPOSITIONFULL = 0x0284,
            IME_SELECT = 0x0285,
            IME_CHAR = 0x0286,
            IME_REQUEST = 0x0288,
            IME_KEYDOWN = 0x0290,
            IME_KEYUP = 0x0291,

            NCMOUSELEAVE = 0x02A2,

            TABLET_DEFBASE = 0x02C0,
            //WM_TABLET_MAXOFFSET = 0x20,

            TABLET_ADDED = TABLET_DEFBASE + 8,
            TABLET_DELETED = TABLET_DEFBASE + 9,
            TABLET_FLICK = TABLET_DEFBASE + 11,
            TABLET_QUERYSYSTEMGESTURESTATUS = TABLET_DEFBASE + 12,

            CUT = 0x0300,
            COPY = 0x0301,
            PASTE = 0x0302,
            CLEAR = 0x0303,
            UNDO = 0x0304,
            RENDERFORMAT = 0x0305,
            RENDERALLFORMATS = 0x0306,
            DESTROYCLIPBOARD = 0x0307,
            DRAWCLIPBOARD = 0x0308,
            PAINTCLIPBOARD = 0x0309,
            VSCROLLCLIPBOARD = 0x030A,
            SIZECLIPBOARD = 0x030B,
            ASKCBFORMATNAME = 0x030C,
            CHANGECBCHAIN = 0x030D,
            HSCROLLCLIPBOARD = 0x030E,
            QUERYNEWPALETTE = 0x030F,
            PALETTEISCHANGING = 0x0310,
            PALETTECHANGED = 0x0311,
            HOTKEY = 0x0312,
            PRINT = 0x0317,
            PRINTCLIENT = 0x0318,
            APPCOMMAND = 0x0319,
            THEMECHANGED = 0x031A,

            DWMCOMPOSITIONCHANGED = 0x031E,
            DWMNCRENDERINGCHANGED = 0x031F,
            DWMCOLORIZATIONCOLORCHANGED = 0x0320,
            DWMWINDOWMAXIMIZEDCHANGE = 0x0321,

            GETTITLEBARINFOEX = 0x033F,
            #region Windows 7
            DWMSENDICONICTHUMBNAIL = 0x0323,
            DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,
            #endregion

            USER = 0x0400,

            // This is the hard-coded message value used by WinForms for Shell_NotifyIcon.
            // It's relatively safe to reuse.
            TRAYMOUSEMESSAGE = 0x800, //WM_USER + 1024
            APP = 0x8000,
        }



        // Depending on the message, callers may want to call GetLastError based on the return value.
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(HWND hWnd, WM Msg, IntPtr wParam, IntPtr lParam);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport("user32.dll", EntryPoint = "UnregisterClass", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _UnregisterClassAtom(IntPtr lpClassName, IntPtr hInstance);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport("user32.dll", EntryPoint = "UnregisterClass", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _UnregisterClassName(string lpClassName, IntPtr hInstance);
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "UnregisterClassW",
                CharSet = CharSet.Unicode)]
        public static extern bool UnregisterClassAtom_(IntPtr lpClassName, [In] IntPtr hInstance);

        public static bool UnregisterClassAtom(Int16 atom, IntPtr hInstance)
        {
            // #define MAKEINTATOM(i)  (LPTSTR)((ULONG_PTR)((WORD)(i)))
            var word = (UInt16)atom;
            var p = (UInt32)word;
            return UnregisterClassAtom_((IntPtr)word, hInstance);
        }

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "RegisterClassExW", CharSet = CharSet.Unicode)]
        public static extern WindowClassAtom RegisterClassEx(ref WNDCLASSEX lpwcx);

        public class WindowClassAtom : SafeHandleZeroOrMinusOneIsInvalid
        {
            [UsedImplicitly]
            private WindowClassAtom() : base(true) { }

            [UsedImplicitly]
            public WindowClassAtom(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
            {
                this.SetHandle(preexistingHandle);
            }

            protected override bool ReleaseHandle()
            {
                var r = UnregisterClassAtom((short)handle, IntPtr.Zero);
                return r;
            }
        }

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        public static extern HWND CreateWindowEx(
               WindowStylesEx dwExStyle,
               [MarshalAs(UnmanagedType.LPWStr)]
               string lpClassName,
               [MarshalAs(UnmanagedType.LPWStr)]
               string lpWindowName,
               WindowStyles dwStyle,
               Int32 x,
               Int32 y,
               Int32 nWidth,
               Int32 nHeight,
               HWND hWnd,
               IntPtr hMenu,
               IntPtr hInstance,
               IntPtr lpParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DestroyWindow(HWND hwnd);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetWindowSubclass(HWND hWnd, SUBCLASSPROC pfnSubclass, IntPtr uIdSubclass, ref IntPtr pdwRefData);
        [DllImport("comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowSubclass(HWND hwnd, SUBCLASSPROC callback, IntPtr id, IntPtr data);
        [DllImport("comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RemoveWindowSubclass(HWND hwnd, SUBCLASSPROC callback, IntPtr id);
        [DllImport("comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr DefSubclassProc(HWND hwnd, WM msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(HWND hWnd, out int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetCurrentProcessId();
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetCurrentThreadId();
                    [Flags]
            public enum WindowStylesEx : uint
            {
                /// <summary>
                /// Specifies that a window created with this style accepts drag-drop files.
                /// </summary>
                WS_EX_ACCEPTFILES = 0x00000010,

                /// <summary>
                /// Forces a top-level window onto the taskbar when the window is visible.
                /// </summary>
                WS_EX_APPWINDOW = 0x00040000,

                /// <summary>
                /// Specifies that a window has a border with a sunken edge.
                /// </summary>
                WS_EX_CLIENTEDGE = 0x00000200,

                /// <summary>
                /// Windows XP: Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. 
                /// </summary>
                WS_EX_COMPOSITED = 0x02000000,

                /// <summary>
                /// Includes a question mark in the title bar of the window. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window.
                /// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
                /// </summary>
                WS_EX_CONTEXTHELP = 0x00000400,

                /// <summary>
                /// The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
                /// </summary>
                WS_EX_CONTROLPARENT = 0x00010000,

                /// <summary>
                /// Creates a window that has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
                /// </summary>
                WS_EX_DLGMODALFRAME = 0x00000001,

                /// <summary>
                /// Windows 2000/XP: Creates a layered window. Note that this cannot be used for child windows. Also, this cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. 
                /// </summary>
                WS_EX_LAYERED = 0x00080000,

                /// <summary>
                /// Arabic and Hebrew versions of Windows 98/Me, Windows 2000/XP: Creates a window whose horizontal origin is on the right edge. Increasing horizontal values advance to the left. 
                /// </summary>
                WS_EX_LAYOUTRTL = 0x00400000,

                /// <summary>
                /// Creates a window that has generic left-aligned properties. This is the default.
                /// </summary>
                WS_EX_LEFT = 0x00000000,

                /// <summary>
                /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
                /// </summary>
                WS_EX_LEFTSCROLLBAR = 0x00004000,

                /// <summary>
                /// The window text is displayed using left-to-right reading-order properties. This is the default.
                /// </summary>
                WS_EX_LTRREADING = 0x00000000,

                /// <summary>
                /// Creates a multiple-document interface (MDI) child window.
                /// </summary>
                WS_EX_MDICHILD = 0x00000040,

                /// <summary>
                /// Windows 2000/XP: A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window. 
                /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
                /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
                /// </summary>
                WS_EX_NOACTIVATE = 0x08000000,

                /// <summary>
                /// Windows 2000/XP: A window created with this style does not pass its window layout to its child windows.
                /// </summary>
                WS_EX_NOINHERITLAYOUT = 0x00100000,

                /// <summary>
                /// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
                /// </summary>
                WS_EX_NOPARENTNOTIFY = 0x00000004,

                /// <summary>
                /// Combines the WS_EX_CLIENTEDGE and WS_EX_WINDOWEDGE styles.
                /// </summary>
                WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

                /// <summary>
                /// Combines the WS_EX_WINDOWEDGE, WS_EX_TOOLWINDOW, and WS_EX_TOPMOST styles.
                /// </summary>
                WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

                /// <summary>
                /// The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
                /// Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
                /// </summary>
                WS_EX_RIGHT = 0x00001000,

                /// <summary>
                /// Vertical scroll bar (if present) is to the right of the client area. This is the default.
                /// </summary>
                WS_EX_RIGHTSCROLLBAR = 0x00000000,

                /// <summary>
                /// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
                /// </summary>
                WS_EX_RTLREADING = 0x00002000,

                /// <summary>
                /// Creates a window with a three-dimensional border style intended to be used for items that do not accept user input.
                /// </summary>
                WS_EX_STATICEDGE = 0x00020000,

                /// <summary>
                /// Creates a tool window; that is, a window intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE. 
                /// </summary>
                WS_EX_TOOLWINDOW = 0x00000080,

                /// <summary>
                /// Specifies that a window created with this style should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
                /// </summary>
                WS_EX_TOPMOST = 0x00000008,

                /// <summary>
                /// Specifies that a window created with this style should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
                /// To achieve transparency without these restrictions, use the SetWindowRgn function.
                /// </summary>
                WS_EX_TRANSPARENT = 0x00000020,

                /// <summary>
                /// Specifies that a window has a border with a raised edge.
                /// </summary>
                WS_EX_WINDOWEDGE = 0x00000100
            }

            /// <summary>
            /// Window Styles.
            /// The following styles can be specified wherever a window style is required. After the control has been created, these styles cannot be modified, except as noted.
            /// </summary>
            [Flags()]
            public enum WindowStyles : uint
            {
                /// <summary>The window has a thin-line border.</summary>
                WS_BORDER = 0x800000,

                /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
                WS_CAPTION = 0xc00000,

                /// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
                WS_CHILD = 0x40000000,

                /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
                WS_CLIPCHILDREN = 0x2000000,

                /// <summary>
                /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
                /// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
                /// </summary>
                WS_CLIPSIBLINGS = 0x4000000,

                /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
                WS_DISABLED = 0x8000000,

                /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
                WS_DLGFRAME = 0x400000,

                /// <summary>
                /// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
                /// The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
                /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
                /// </summary>
                WS_GROUP = 0x20000,

                /// <summary>The window has a horizontal scroll bar.</summary>
                WS_HSCROLL = 0x100000,

                /// <summary>The window is initially maximized.</summary> 
                WS_MAXIMIZE = 0x1000000,

                /// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary> 
                WS_MAXIMIZEBOX = 0x10000,

                /// <summary>The window is initially minimized.</summary>
                WS_MINIMIZE = 0x20000000,

                /// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
                WS_MINIMIZEBOX = 0x20000,

                /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
                WS_OVERLAPPED = 0x0,

                /// <summary>The window is an overlapped window.</summary>
                WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

                /// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
                WS_POPUP = 0x80000000u,

                /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
                WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

                /// <summary>The window has a sizing border.</summary>
                WS_SIZEFRAME = 0x40000,

                /// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
                WS_SYSMENU = 0x80000,

                /// <summary>
                /// The window is a control that can receive the keyboard focus when the user presses the TAB key.
                /// Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.  
                /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
                /// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
                /// </summary>
                WS_TABSTOP = 0x10000,

                /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
                WS_VISIBLE = 0x10000000,

                /// <summary>The window has a vertical scroll bar.</summary>
                WS_VSCROLL = 0x200000,
                
                WS_THICKFRAME = 0x00040000
            }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessage", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint _RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static WM RegisterWindowMessage(string lpString)
        {
            uint iRet = _RegisterWindowMessage(lpString);
            if (iRet == 0)
            {
                throw new Win32Exception();
            }
            return (WM)iRet;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "DefWindowProcW", CharSet = CharSet.Unicode)]
        public static extern IntPtr DefWindowProc(User32.HWND hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetMessage(out MSG lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        /// <summary>
        /// Overload that does automatic error-checking (and throws Win32Exception if there are any)
        /// </summary>
        /// <param name="lpMsg"></param>
        /// <param name="hWnd"></param>
        /// <param name="wMsgFilterMin"></param>
        /// <param name="wMsgFilterMax"></param>
        /// <returns>True if not WM_QUIT</returns>
        public static bool GetMessageSafe(out MSG lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
        {
            var returnValue = GetMessage(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
            if (returnValue == -1)
            {
                // An error occured
                throw new Win32Exception();
            }
            return returnValue != 0;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PostMessage(HWND hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static void PostMessageSafe(HWND hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            bool returnValue = PostMessage(hWnd, msg, wParam, lParam);
            if (!returnValue)
            {
                // An error occured
                throw new Win32Exception();
            }
        }
    }
}
