using System;
using Gtk;
using NuGet;

namespace GtkNuGetPackageExplorer
{
    // The class used to manage the tree view that displays the files in a package
    public class PackageFileListView
    {
        public IPackage Package
        { 
            get 
            {
                return _package;
            }
            set
            { 
                _package = value; 
                Refresh(); 
            }
        }

        private ScrolledWindow _scrolledWindow;
        private TreeView _treeView;
        private IPackage _package;
        private VBox _vbox;

        public event EventHandler<FileSelectedEventArgs> FileSelected;

        public PackageFileListView()
        {
            HBox hbox = new HBox();
            Button button = new Button("Collapse All");
            button.Clicked += (obj, e) =>
            {
                _treeView.CollapseAll();
            };
            hbox.PackStart(button, expand: false, fill: false, padding: 5);

            button = new Button("Expand All");
            button.Clicked += (obj, e) =>
            {
                _treeView.ExpandAll();
            };
            hbox.PackStart(button, expand: false, fill: false, padding: 5);

            _treeView = new TreeView();
            _treeView.CanFocus = true;
            _treeView.HeadersVisible = false;
            _treeView.AppendColumn("", new CellRendererText(), "text", 0);
            _treeView.CursorChanged += HandleCursorChanged;

            _scrolledWindow = new ScrolledWindow();
            _scrolledWindow.ShadowType = ShadowType.EtchedIn;
            _scrolledWindow.Add(_treeView);

            _vbox = new VBox();
            _vbox.PackStart(hbox, expand: false, fill: true, padding: 5);
            _vbox.PackStart(_scrolledWindow, expand: true, fill: true, padding: 5);
        }

        public Pango.FontDescription Font
        {
            get
            {
                return _treeView.Style.FontDesc;
            }
            set
            {
                _treeView.ModifyFont(value);
            }
        }

        public Widget Widget
        {
            get
            {
                return _vbox;
            }
        }

        private void Refresh()
        {
            var builder = new DirectoryTreeBuilder();
            var root = builder.Create(_package);       

            var store = new TreeStore(typeof(string), typeof(string));
            foreach (var c in root.Children)
            {
                AddTreeNode(store, TreeIter.Zero, c);
            }

            _treeView.Model = store;
            _treeView.ExpandAll();
        }        

        private void AddTreeNode(TreeStore store, TreeIter parent, GtkNuGetPackageExplorer.TreeNode n)
        {
            TreeIter iter;
            if (parent.Equals(TreeIter.Zero))
            {
                iter = store.AppendValues(n.Name, n.FilePath);
            }
            else
            {
                iter = store.AppendNode(parent);
                store.SetValues(iter, n.Name, n.FilePath);
            }

            foreach (var c in n.Children)
            {
                AddTreeNode(store, iter, c);
            }
        }        

        // Called when a row is selected in the tree view
        void HandleCursorChanged (object sender, EventArgs e)
        {
            TreeModel model;
            TreeIter iter;
            if (!_treeView.Selection.GetSelected(out model, out iter))
            {
                return;
            }

            var filePath = model.GetValue(iter, 1) as string;
            if (FileSelected != null)
            {
                FileSelected(this, new FileSelectedEventArgs(filePath));
            }
        }
    }

    public class FileSelectedEventArgs : EventArgs
    {
        // null if a directory is selected.
        public string FilePath { get; set; }

        public FileSelectedEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}

