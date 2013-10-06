using System;
using Gtk;
using NuGet;
using System.Text;
using GtkNuGetPackageExplorer;
using System.Collections.Generic;
using System.IO;

public partial class MainWindow: Gtk.Window
{	
    IPackage _package;

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		this.OpenAction.Activated += (object sender, EventArgs e) => OpenFile();
        InitViews();
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

    private void InitViews()
    {
        // init metadata view
        var textBuffer = _metaDataView.Buffer;
        var tag = new TextTag("bold") { Weight =  Pango.Weight.Bold };
        textBuffer.TagTable.Add(tag);

        tag = new TextTag("italic") { Style = Pango.Style.Italic };
        textBuffer.TagTable.Add(tag);

		// init tree view
        treeview1.HeadersVisible = false;
        treeview1.AppendColumn("", new CellRendererText(), "text", 0);
    }

	private void OpenFile()
	{
		FileChooserDialog fc = new FileChooserDialog(
			"Open package file", 
			this, 
			FileChooserAction.Open,
			"Cancel", ResponseType.Cancel, 
			"OK", ResponseType.Ok);
        if (fc.Run() == (int)ResponseType.Ok)
        {
            _package = new OptimizedZipPackage(fc.Filename);
            fc.Destroy();
            UpdateMetadataView();
            UpdateTreeView();
        }
        else
        {
            fc.Destroy();
        }
	}

    private void UpdateMetadataView()
    {
        var textBuffer = _metaDataView.Buffer;
        textBuffer.Clear();

        textBuffer.AddWithTag("bold", "Id: ");
        textBuffer.Add("{0}\n", _package.Id);

        textBuffer.AddWithTag("bold", "Version: ");
        textBuffer.Add("{0}\n", _package.Version);

        if (!string.IsNullOrEmpty(_package.Title))
        {
            textBuffer.AddWithTag("bold", "Version: ");
            textBuffer.Add("{0}\n", _package.Title);
        }

        if (!_package.Authors.IsEmpty())
        {
            textBuffer.AddWithTag("bold", "Authors: ");
            textBuffer.Add("{0}\n", String.Join(",", _package.Authors));
        }

        if (!_package.Owners.IsEmpty())
        {
            textBuffer.AddWithTag("bold", "Owners: ");
            textBuffer.Add("{0}\n", String.Join(",", _package.Owners));
        }

        if (!string.IsNullOrEmpty(_package.Tags))
        {
            textBuffer.AddWithTag("bold", "Tags: ");
            textBuffer.Add("{0}\n", _package.Tags);
        }

        if (!string.IsNullOrEmpty(_package.Language))
        {
            textBuffer.AddWithTag("bold", "Language: ");
            textBuffer.Add("{0}\n", _package.Language);
        }

        textBuffer.AddWithTag("bold", "Requires License Acceptance: ");
        textBuffer.Add("{0}\n", _package.RequireLicenseAcceptance ? "Yes" : "No");

        if (_package.LicenseUrl != null)
        {
            textBuffer.AddWithTag("bold", "License Url: ");
            textBuffer.Add("{0}\n", _package.LicenseUrl);
        }

        if (_package.ProjectUrl != null)
        {
            textBuffer.AddWithTag("bold", "Project Url: ");
            textBuffer.Add("{0}\n", _package.ProjectUrl);
        }

        if (!string.IsNullOrEmpty(_package.Summary))
        {
            textBuffer.AddWithTag("bold", "Summary: ");
            textBuffer.Add("\n{0}\n", _package.Summary);
        }

        textBuffer.AddWithTag("bold", "Description: ");
        textBuffer.Add("\n{0}\n", _package.Description);

        if (!string.IsNullOrEmpty(_package.ReleaseNotes))
        {
            textBuffer.AddWithTag("bold", "Release notes: ");
            textBuffer.Add("\n{0}\n", _package.ReleaseNotes);
        }

        textBuffer.AddWithTag("bold", "Dependencies: ");
        bool hasDependency = false;
        foreach (var dependencySet in _package.DependencySets)
        {
            foreach (var dependency in dependencySet.Dependencies)
            {
                hasDependency = true;
                textBuffer.Add("\n{0}\n", dependency.ToString());
            }
        }

        if (!hasDependency)
        {
            textBuffer.AddWithTag("italic", "\n\t{0}\n", "No dependencies");
        }
    }

    private void UpdateTreeView()
    {
        var builder = new DirectoryTreeBuilder();
        var root = builder.Create(_package);       

        var store = new TreeStore(typeof(string), typeof(string));
        foreach (var c in root.Children)
        {
            AddTreeNode(store, TreeIter.Zero, c);
        }

        treeview1.Model = store;
    }

    private void AddTreeNode(TreeStore store, TreeIter parent, GtkNuGetPackageExplorer.TreeNode n)
    {
        TreeIter iter;
        if (parent.Equals(TreeIter.Zero))
        {
            iter = store.AppendValues(n.Name, n.Name);
        }
        else
        {
			iter = store.AppendNode(parent);
			store.SetValues(iter, n.Name, n.Name);
        }

        foreach (var c in n.Children)
        {
            AddTreeNode(store, iter, c);
        }
    }
}
