using System;

namespace ShadowMUD
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var mud = new ShadowMud();

            mud.Initialize();

            mud.MainLoop();
        }
    }
}