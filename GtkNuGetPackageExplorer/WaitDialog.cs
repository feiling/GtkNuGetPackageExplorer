using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;

namespace GtkNuGetPackageExplorer
{
    class WaitDialog : Window
    {
        ProgressBar _progressBar;

        public WaitDialog(string text, Window parent) : base(WindowType.Popup)
        {
            Modal = true;
            this.TransientFor = parent;
            this.WindowPosition = Gtk.WindowPosition.CenterOnParent;
            DefaultSize = new Gdk.Size(300, 50);
            _progressBar = new ProgressBar();
            _progressBar.Text = text;
            var vbox = new VBox();
            vbox.PackStart(_progressBar, expand: true, fill: true, padding: 5);
            this.Add(vbox);
            this.Child.ShowAll();
        }

        public void Pulse()
        {
            _progressBar.Pulse();
        }
    }
}
