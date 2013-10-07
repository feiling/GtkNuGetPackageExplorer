using System;
using NuGet;
using Gtk;

namespace GtkNuGetPackageExplorer
{
    // The class used to manage the tree view that displays the files in a package
    public class TreeViewManager
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

        private TreeView _treeView;
        private IPackage _package;

        public event EventHandler<FileSelectedEventArgs> FileSelected;

        public TreeViewManager(TreeView treeView)
        {
            _treeView = treeView;

            // init tree view
            _treeView.HeadersVisible = false;
            _treeView.AppendColumn("", new CellRendererText(), "text", 0);
            _treeView.CursorChanged += HandleCursorChanged;
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

