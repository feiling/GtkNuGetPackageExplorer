using System;
using Gtk;

namespace GtkNuGetPackageExplorer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
            if (args.Length > 0)
            {
                win.OpenPackageFile(args[0]);
            }

			Application.Run ();
		}
	}
}
