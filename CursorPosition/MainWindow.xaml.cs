using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CursorPosition
{
    public partial class MainWindow : Window
    {
        private Thread thread;
        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern POINT GetCursorPos(ref POINT pos);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(
            string lpClassName, // class name 
            string lpWindowName // window name 
        );

        void FindMouse()
        {
            IntPtr hwndOld = GetForegroundWindow();
            IntPtr hwndThis = FindWindow(null, "Cursor Position");
            bool status = false;
            
            while (true)
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                        POINT pos = new POINT();
                        RECT rct = new RECT(){Bottom = 99999,Left = 0,Right = 99999,Top = 0};
                        GetCursorPos(ref pos);
                        var tmpPos = pos;
                        IntPtr hwnd = GetForegroundWindow();
                        if (status)
                        {
                            GetWindowRect(hwnd, out rct);
                            ScreenToClient(hwnd, ref pos);
                        }

                        label1.Background = ((tmpPos.x > rct.Right) || (tmpPos.x < rct.Left))
                            ? new SolidColorBrush(Color.FromRgb(255, 78, 78))
                            : new SolidColorBrush(Color.FromRgb(214, 209, 209));

                        label2.Background = ((tmpPos.y > rct.Bottom) || (tmpPos.y < rct.Top))
                            ? new SolidColorBrush(Color.FromRgb(255, 78, 78))
                            : new SolidColorBrush(Color.FromRgb(214, 209, 209));

                        if (hwnd == hwndThis)
                        {
                            status = false;
                            textBlock.Text = "Весь экран";
                        }
                        else
                        {
                            status = true;
                            hwndOld = hwnd;
                            StringBuilder Buff = new StringBuilder(256);

                            GetWindowText(hwnd, Buff, 256);
                            textBlock.Text = Buff.ToString();
                        }
                        
                        label1.Content = "X: " + pos.x;
                        label2.Content = "Y: " + pos.y;
                    }), DispatcherPriority.Render);
                    Thread.Sleep(50);
                }
            }   
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBlock.Text = "Весь экран";
            thread = new Thread(FindMouse); // создание отдельного потока
            thread.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            thread.Abort();
        }
    }

    
}
