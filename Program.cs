using HonorsContractSpring2025;
using System;
using System.Windows.Forms;

namespace AlgorithmVisualizer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}