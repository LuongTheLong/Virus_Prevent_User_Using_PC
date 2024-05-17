using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CheckPC
{
    public partial class Form1 : Form
    {
        Form2 displayForm = new Form2();
        private const int WH_KEYBOARD_LL = 13; //keycap alt
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_TAB = 0x09;
        private const int VK_MENU = 0x12;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == VK_TAB && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    // Ngăn chặn sự kiện Alt + Tab
                    return (IntPtr)1;
                }
                if (vkCode == VK_MENU && (Control.ModifierKeys & Keys.Tab) == Keys.Tab)
                {
                    // Ngăn chặn sự kiện Alt down
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void customControl11_Click(object sender, EventArgs e)
        {
            // Lấy kích thước của màn hình
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            // Tạo một bitmap để lưu ảnh chụp màn hình
            Bitmap screenshot = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);

            // Tạo một đối tượng Graphics từ bitmap
            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                // Copy màn hình vào bitmap
                graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size, CopyPixelOperation.SourceCopy);
            }

            // Đường dẫn đầy đủ của thư mục đích
            string folderPath = @"C:\Users\ADMIN\source\repos\TestForm";

            // Kiểm tra thư mục tồn tại hay không, nếu không thì tạo mới
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            // Lưu ảnh chụp màn hình vào thư mục
            string filePath = System.IO.Path.Combine(folderPath, "screenshot.png");
            screenshot.Save(filePath, ImageFormat.Png);

            // Hiển thị ảnh chụp màn hình trong một cửa sổ không có khung

            displayForm.StartPosition = FormStartPosition.Manual;
            displayForm.FormBorderStyle = FormBorderStyle.None;
            displayForm.BackgroundImage = screenshot;
            displayForm.ClientSize = screenshot.Size;

            // Đặt cửa sổ vào góc cuối cùng của màn hình
            displayForm.Location = new Point(screenBounds.Width - screenshot.Width, screenBounds.Height - screenshot.Height);

            // Hiển thị cửa sổ dưới dạng hộp thoại modal
            displayForm.ShowDialog();

            Console.WriteLine("Chụp màn hình đã được lưu tại: " + filePath);

            _hookID = SetHook(_proc);
            //Application.Run();
            UnhookWindowsHookEx(_hookID);

        }
    }
}
