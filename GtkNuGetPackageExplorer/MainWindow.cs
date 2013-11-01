using System;
using Gtk;
using NuGet;
using System.Text;
using GtkNuGetPackageExplorer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public partial class MainWindow: Gtk.Window
{	
    private global::Gtk.Action FileAction;
	private global::Gtk.Action OpenAction;
	// private global::Gtk.MenuBar menubar2;
	private global::Gtk.ScrolledWindow GtkScrolledWindow1;
	private global::Gtk.TextView _metaDataView;
	private global::Gtk.VPaned _rightPane;
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	private global::Gtk.TreeView treeview1;
	
	
    IPackage _package;
    TextView _fileInfoView;

    TreeViewManager _treeViewManager;
    ScrolledWindow _fileDetail;

    FileContentEditor _fileContentEditor;

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
        Build ();

        DragDropSetup();

        _fileContentEditor = new FileContentEditor();
        _treeViewManager = new TreeViewManager(treeview1);
        _treeViewManager.FileSelected += HandleFileSelected;

        _fileInfoView = new TextView()
        {
            Visible = true,
            Editable = false,
        };
        _fileDetail = new ScrolledWindow()
        {
            Visible = true,
            ShadowType = ShadowType.EtchedIn
        };
        _fileDetail.Add(_fileInfoView);
		_rightPane.Add2(_fileDetail);

        this.OpenAction.Activated += (object sender, EventArgs e) => OpenFile();
        InitViews();
	}

    protected virtual void Build()
    {
        // create menu
        var openMenuItem = new MenuItem("Open");
        openMenuItem.Activated += (o, e) => OpenFile();

        var fileMenu = new Menu();
        fileMenu.Append(openMenuItem);

        var fileMenuItem = new MenuItem("File");
        fileMenuItem.Submenu = fileMenu;

        var menuBar = new MenuBar();
        menuBar.Append(fileMenuItem);        
        
        // Widget MainWindow
        global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup("Default");
        this.FileAction = new global::Gtk.Action("FileAction", global::Mono.Unix.Catalog.GetString("File"), null, null);
        this.FileAction.ShortLabel = global::Mono.Unix.Catalog.GetString("File");
        w1.Add(this.FileAction, null);
        this.OpenAction = new global::Gtk.Action("OpenAction", global::Mono.Unix.Catalog.GetString("Open"), null, null);
        this.OpenAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Open");
        w1.Add(this.OpenAction, null);
        this.Name = "MainWindow";
        this.Title = global::Mono.Unix.Catalog.GetString("MainWindow");
        this.WindowPosition = ((global::Gtk.WindowPosition)(4));



        /*
        // Container child vbox1.Gtk.Box+BoxChild
        this.UIManager.AddUiFromString ("<ui><menubar name=\'menubar2\'><menu name=\'FileAction\' action=\'FileAction\'><menuite" +
            "m name=\'OpenAction\' action=\'OpenAction\'/></menu></menubar></ui>");
        this.menubar2 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar2")));
        this.menubar2.Name = "menubar2"; 
        this.vbox1.Add (this.menubar2); 
        global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.menubar2]));
        w2.Position = 0;
        w2.Expand = false;
        w2.Fill = false; */



        // Container child hpaned1.Gtk.Paned+PanedChild
        this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
        this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
        this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
        // Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
        this._metaDataView = new global::Gtk.TextView();
        this._metaDataView.CanFocus = true;
        this._metaDataView.Name = "_metaDataView";
        this._metaDataView.Editable = false;
        this._metaDataView.WrapMode = ((global::Gtk.WrapMode)(2));
        this._metaDataView.LeftMargin = 5;
        this._metaDataView.RightMargin = 5;
        this.GtkScrolledWindow1.Add(this._metaDataView);

        // Container child vbox1.Gtk.Box+BoxChild
        var hpaned1 = new global::Gtk.HPaned();
        hpaned1.CanFocus = true;
        hpaned1.Position = 157;
        hpaned1.Add(this.GtkScrolledWindow1);
        global::Gtk.Paned.PanedChild w4 = ((global::Gtk.Paned.PanedChild)(hpaned1[this.GtkScrolledWindow1]));
        w4.Resize = false;

        // Container child hpaned1.Gtk.Paned+PanedChild
        this._rightPane = new global::Gtk.VPaned();
        this._rightPane.CanFocus = true;
        this._rightPane.Name = "_rightPane";
        this._rightPane.Position = 133;
        // Container child _rightPane.Gtk.Paned+PanedChild
        this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
        this.GtkScrolledWindow.Name = "GtkScrolledWindow";
        this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
        // Container child GtkScrolledWindow.Gtk.Container+ContainerChild
        this.treeview1 = new global::Gtk.TreeView();
        this.treeview1.CanFocus = true;
        this.treeview1.Name = "treeview1";
        this.GtkScrolledWindow.Add(this.treeview1);
        this._rightPane.Add(this.GtkScrolledWindow);
        global::Gtk.Paned.PanedChild w6 = ((global::Gtk.Paned.PanedChild)(this._rightPane[this.GtkScrolledWindow]));
        w6.Resize = false;
        hpaned1.Add(this._rightPane);

        var vbox1 = new VBox();
        vbox1.PackStart(menuBar, expand: false, fill: false, padding: 0);
        vbox1.PackEnd(hpaned1);

        this.Add(vbox1);
        vbox1.ShowAll();

        this.DefaultWidth = 575;
        this.DefaultHeight = 530;
        this.Show();
        this.DeleteEvent += new global::Gtk.DeleteEventHandler(this.OnDeleteEvent);
    }
	
    private void DragDropSetup()
    {
        var targetEntries = new TargetEntry[]
        {
            new TargetEntry("text/uri-list", 0, 0)
        };
        Drag.DestSet(
            this,
            DestDefaults.All,
            targetEntries,
            Gdk.DragAction.Copy);
        this.DragDataReceived += HandleDragDataReceived;
    }

    void HandleDragDataReceived (object o, DragDataReceivedArgs args)
    {
        byte[] data = args.SelectionData.Data;
        string s = System.Text.Encoding.UTF8.GetString(data);
        string[] fileList = Regex.Split(s, "\r\n");
        string fileName = fileList[0];
        Uri uri;
        if (!Uri.TryCreate(fileName, UriKind.Absolute, out uri))
        {
            return;
        }

        if (!uri.IsFile)
        {
            return;
        }

        OpenPackageFile(uri.LocalPath);
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

        _fileInfoView.WrapMode = WrapMode.Word;
    }

    public void OpenPackageFile(string fileName)
    {
        try
        {
            _package = new OptimizedZipPackage(fileName);
            UpdateMetadataView();
            _treeViewManager.Package = _package;
        }
        catch (Exception ex)
        {
            var errorMessage = String.Format(
                "Error while openning file {0}: {1} ", 
                fileName, 
                ex.Message);
            var m = new MessageDialog(
                this, 
                DialogFlags.DestroyWithParent, 
                MessageType.Error, 
                ButtonsType.Ok,
                errorMessage);
            m.Run();
            m.Destroy();
        }
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
            var fileName = fc.Filename;
            fc.Destroy();
            OpenPackageFile(fileName);
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

        textBuffer.AddWithTag("bold", "Development Dependency: ");
        textBuffer.Add("{0}\n", _package.DevelopmentDependency);

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

    void HandleFileSelected(object sender, FileSelectedEventArgs e)
    {
        _fileInfoView.Buffer.Clear();
        _fileContentEditor.Clear();
        if (e.FilePath == null)
        {
            return;
        }

        var packageFile = _package.GetFiles().FirstOrDefault(f => f.Path == e.FilePath);
        if (string.Equals(System.IO.Path.GetExtension(e.FilePath), ".dll", StringComparison.OrdinalIgnoreCase))
        {
            ShowDllFileInfo(packageFile);
        }
        else
        {
            _fileContentEditor.OpenFile(packageFile);
			if (_rightPane.Child2 != _fileContentEditor.Widget)
			{
				_rightPane.Remove(_rightPane.Child2);
				_rightPane.Add2(_fileContentEditor.Widget);
			}
        }
    }

    void ShowDllFileInfo(IPackageFile packageFile)
    {
        byte[] rawAssembly;
        using (var stream = packageFile.GetStream())
        {
            rawAssembly = stream.ReadAllBytes();
        }

        AppDomain temp = AppDomain.CreateDomain("temp_for_loading_metadata");
        try
        {
            AssemblyLoader al = (AssemblyLoader)temp.CreateInstanceAndUnwrap(
                typeof(AssemblyLoader).Assembly.FullName,
                typeof(AssemblyLoader).FullName);
            var s = al.GetMetadata(rawAssembly);
            var b = _fileInfoView.Buffer;
            b.Clear();
            b.Add(s);

            if (_rightPane.Child2 != _fileDetail) 
            {
				_rightPane.Remove(_rightPane.Child2);
                _rightPane.Add2(_fileDetail);
            }
        }
        finally
        {
            AppDomain.Unload(temp);
        }
    }

    public class AssemblyLoader : MarshalByRefObject
    {
        public string GetMetadata(byte[] rawAssembly)
        {
            var assembly = Assembly.ReflectionOnlyLoad(rawAssembly);
            var sb = new StringBuilder();
            sb.AppendFormat("Full Name: {0}\n", assembly.FullName);

            return sb.ToString();
        }
    }
}
