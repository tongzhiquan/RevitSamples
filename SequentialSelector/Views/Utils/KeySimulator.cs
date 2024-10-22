using System.Runtime.InteropServices;

namespace SequentialSelector.Views.Utils
{
    internal class KeySimulator
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        // 定义虚拟键码
        private const byte VK_ESCAPE = 0x1B;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static void PressEscape()
        {
            // 模拟按下 ESC 键
            keybd_event(VK_ESCAPE, 0, 0, UIntPtr.Zero);
            // 模拟释放 ESC 键
            keybd_event(VK_ESCAPE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
