using hap.NativeMethods;
using System;
using Caliburn.Micro;
using hap.Services.Interfaces;

namespace hap.Services
{
    internal class KeyListenerService : IKeyListenerService, IDisposable
    {
        public event EventHandler OnHotKeyActivated;

        /// <summary>
        /// Current hotkey reference id
        /// </summary>
        private int _hotKeyId = 0;

        /// <summary>
        /// Whether a hotkey has been currently registered
        /// </summary>
        private bool _currentlyRegistered;

        /// <summary>
        /// The hotkey
        /// </summary>
        private HotKey _hotKey;

        private MessageWindow _messageWindow;
        private MessageWindow.WndProcNullableRet _handleWindowMessage;

        public KeyListenerService()
        {
            _handleWindowMessage = HandleWindowMessage;
            _messageWindow = new MessageWindow("MessageWindow", _handleWindowMessage);
        }

        private IntPtr? HandleWindowMessage(User32.HWND hWnd, uint msg, IntPtr wparam, IntPtr lparam)
        {
            if (msg == Constants.WM_HOTKEY)
            {
                var e = new HotKeyEventArgs(lparam);

                if (e.Key == _hotKey.Keys &&
                    e.Modifiers == _hotKey.Modifier)
                {
                    OnHotKeyActivated?.Invoke(this, new EventArgs());
                }
            }

            return null;
        }

        ~KeyListenerService()
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
            if (_currentlyRegistered)
            {
                User32.UnregisterHotKey(_messageWindow.Handle, _hotKeyId);
                _currentlyRegistered = false;
            }

            if(managed)
            {
                DisposeUtils.Dispose(ref _messageWindow);
            }
        }

        /// <summary>
        /// Re-registers the current hotkey, unregistering any previous key
        /// </summary>
        private void ReRegisterHotkey()
        {
            if (_currentlyRegistered)
            {
                User32.UnregisterHotKey(_messageWindow.Handle, _hotKeyId);
                _currentlyRegistered = false;
            }

            _hotKeyId++;
            User32.RegisterHotKey(_messageWindow.Handle, _hotKeyId, (uint) _hotKey.Modifier, (uint) _hotKey.Keys);
            _currentlyRegistered = true;
        }

        /// <summary>
        /// Gets/sets the current hotkey
        /// </summary>
        /// <remarks>Changing this will cause the current hotkey to be unregistered</remarks>
        public HotKey HotKey
        {
            set
            {
                _hotKey = value;
                ReRegisterHotkey();
            }
            get { return _hotKey; }
        }
    }
}