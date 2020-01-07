using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CMOConverter
{
    public static class ModifyProgressBarColor
    {
        public const uint WM_USER = 0x400;
        public const uint PBM_SETSTATE = WM_USER + 16;
        public const int PBST_NORMAL = 0x0001;
        public const int PBST_ERROR = 0x0002;
        public const int PBST_PAUSED = 0x0003;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
        public static void SetState(this ProgressBar pBar, int state)
        {
            SendMessage(pBar.Handle, PBM_SETSTATE, (IntPtr)state, IntPtr.Zero);
        }
    }

}