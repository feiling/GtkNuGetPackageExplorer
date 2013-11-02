using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using NuGet;

namespace GtkNuGetPackageExplorer
{
    class OpenFileFromFeedDialog : Dialog
    {
        private Entry _packageSource;
        private TreeView _packageList;
        private Label _info;
        private ListStore _store;

        private const int _pageSize = 15;

        public OpenFileFromFeedDialog()
            : base("Open from feed", null, DialogFlags.Modal)
        {
            var hbox = new HBox();
            hbox.PackStart(
                new Label() { Text = "Package source: " }, 
                expand: false, fill: false, padding: 5);

            _packageSource = new Entry()
            {
                Text = "https://nuget.org/api/v2/"
            };
            hbox.PackStart(_packageSource, expand: true, fill: true, padding: 5);
            
            var reloadButton = new Button("Reload");
            reloadButton.Clicked += (o, e) => Reload();

            hbox.PackStart(reloadButton, expand: false, fill: false, padding: 5);
            this.VBox.PackStart(hbox, expand: false, fill: false, padding: 5);

            hbox = new HBox();
            _info = new Label();
            hbox.PackStart(_info, expand: false, fill: false, padding: 5);
            this.VBox.PackStart(hbox, expand: false, fill: false, padding: 5);

            _packageList = new TreeView();
            var column = new TreeViewColumn("Id", new CellRendererText(), "text", 0)
            {
                Resizable = true
            };
            _packageList.AppendColumn(column);

            column = new TreeViewColumn("Version", new CellRendererText(), "text", 1)
            {
                Resizable = true
            };
            _packageList.AppendColumn(column);
            column = new TreeViewColumn("Authors", new CellRendererText(), "text", 2)
            {
                Resizable = true
            };
            _packageList.AppendColumn(column);
            column = new TreeViewColumn("Downloads", new CellRendererText(), "text", 3)
            {
                Resizable = true
            };
            _packageList.AppendColumn(column);

            _store = new ListStore(
                typeof(string),
                typeof(string), 
                typeof(string), 
                typeof(string));
            _packageList.Model = _store;
            
            var scrolledWindow = new ScrolledWindow()
            {
                ShadowType = Gtk.ShadowType.EtchedIn
            };
            scrolledWindow.Add(_packageList);
            this.VBox.PackStart(scrolledWindow, expand: true, fill: true, padding: 5);

            this.AddButton("Close", ResponseType.Close);
            this.VBox.ShowAll();
        }

        private void Reload()
        {
            var source = _packageSource.Text;
            Uri sourceUri;
            if (!Uri.TryCreate(source, UriKind.Absolute, out sourceUri))
            {
                var m = new MessageDialog(
                    this,
                    DialogFlags.Modal,
                    MessageType.Error,
                    ButtonsType.Ok,
                    "The package source provided is not a valid uri");
                m.Run();
                m.Destroy();
                return;
            }

            var packageRepo = new DataServicePackageRepository(sourceUri);
            var packages = packageRepo.GetPackages().OrderByDescending(p => p.DownloadCount);
            var totalCount = packages.Count();
            _info.Text = string.Format("total {0}", totalCount);
            foreach (var p in packages.Take(_pageSize))
            {
                _store.AppendValues(
                    p.Id,
                    p.Version.ToString(),
                    String.Join(", ", p.Authors),
                    p.DownloadCount.ToString());
            }
        }
    }
}
