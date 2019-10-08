using System.Windows.Forms;
using mstat.Core;
using Console = System.Console;

namespace mstat.Cli
{
    class Program
    {

        static void Main()
        {
            var window = WinHook.CreateForegroundChangedEventArg().ProcessName;

            WinHook.ForegroundWindowChanged += (sender, arg) => { window = arg.ProcessName; };

            WinHook.MouseChanged += (sender, arg) =>
            {
                var click = arg.Msg == 513 ? "WM_LBUTTONDOWN" : "WM_LBUTTONUP";
                Console.WriteLine($"{window} {click} @ ({arg.X},{arg.Y})");
            };

            Application.Run();
        }
    }
}