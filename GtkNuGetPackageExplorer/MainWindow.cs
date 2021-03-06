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
using System.Threading.Tasks;

public class MainWindow: Gtk.Window
{	
    IPackage _package;
    PackageMetadataView _metadataView;
    PackageFileListView _treeViewManager;
    FileContentEditor _fileContentEditor;
    OpenFileFromFeedDialog _openFileFromFeedDialog;
    FileChooserDialog _saveAsDialog;
    MenuItem _saveAsMenuItem;
    UserSettings _userSettings;
    UserPrefs _userPrefs;
    HPaned _hpaned;
    VPaned _rightPane;

    public MainWindow()
        : base(Gtk.WindowType.Toplevel)
    {
        LoadUserPrefs();
        Build();
        DragDropSetup();        
        LoadUserSettings();
    }

    void LoadUserPrefs()
    {
        _userPrefs = new UserPrefs();
        _userPrefs.Load();
    }

    void LoadUserSettings()
    {
        _userSettings = new UserSettings();
        _userSettings.Load();

        if (_userSettings.UIFont == null)
        {
            _userSettings.UIFont = _metadataView.Font;
        }
        if (_userSettings.TextEditorFont == null)
        {
            var f = _fileContentEditor.TextEditorFont.Copy();            
            if (f.Family == "Mono")
            {
                f.Family = "Monospace";
            }
            _userSettings.TextEditorFont = f.ToString();
        }

        ApplySettings();
    }

    protected virtual void Build()
    {
        // create menu
        var openMenuItem = new MenuItem("Open");
        openMenuItem.Activated += (o, e) => OpenFile();

        var openFromFeedMenuItem = new MenuItem("Open from feed ...");
        openFromFeedMenuItem.Activated += (o, e) => OpenFileFromFeed();

        _saveAsMenuItem = new MenuItem("Save as ...");
        _saveAsMenuItem.Activated += (o, e) => SaveAs();
        _saveAsMenuItem.Sensitive = false;

        var settingsMenuItem = new MenuItem("Settings");
        settingsMenuItem.Activated += SettingsMenuItem_Activated;

        var fileMenu = new Menu();
        fileMenu.Append(openMenuItem);
        fileMenu.Append(openFromFeedMenuItem);
        fileMenu.Append(_saveAsMenuItem);
        fileMenu.Append(settingsMenuItem);

        var fileMenuItem = new MenuItem("File");
        fileMenuItem.Submenu = fileMenu;

        var menuBar = new MenuBar();
        menuBar.Append(fileMenuItem);
        
        _metadataView = new PackageMetadataView();
        _hpaned = new HPaned();
        _hpaned.Position = _userPrefs.HPanedPosition;
        _hpaned.Add1(_metadataView.Widget);
        
        // tree view manager
        _treeViewManager = new PackageFileListView();
        _treeViewManager.FileSelected += HandleFileSelected;

        // file content
        _fileContentEditor = new FileContentEditor();

        _rightPane = new VPaned();
        _rightPane.Position = _userPrefs.VPanedPosition;
        _rightPane.Add1(_treeViewManager.Widget);
        _rightPane.Add2(_fileContentEditor.Widget);
        
        _hpaned.Add2(_rightPane);

        var vbox = new VBox();
        vbox.PackStart(menuBar, expand: false, fill: false, padding: 0);
        vbox.PackEnd(_hpaned);

        this.Add(vbox);
        vbox.ShowAll();

        this.DefaultWidth = _userPrefs.WindowWidth;
        this.DefaultHeight = _userPrefs.WindowHeight;
        this.Show();
        this.DeleteEvent += new global::Gtk.DeleteEventHandler(this.OnDeleteEvent);

        _openFileFromFeedDialog = new OpenFileFromFeedDialog(this);
    }

    void SettingsMenuItem_Activated(object sender, EventArgs e)
    {
        var userSettingsDialog = new UserSettingsDialog(this, _userSettings);
        int r = userSettingsDialog.Run();
        if (r == (int)ResponseType.Ok)
        {
            userSettingsDialog.Destroy();
            _userSettings = userSettingsDialog.UserSettings;
            _userSettings.Save();
            ApplySettings();
        }
        else
        {
            userSettingsDialog.Destroy();
        }
    }

    private void ApplySettings()
    {
        _metadataView.Font = _userSettings.UIFont;
        _treeViewManager.Font = _userSettings.UIFont;
        _fileContentEditor.Font = _userSettings.UIFont;
        _fileContentEditor.TextEditorFont = Pango.FontDescription.FromString(_userSettings.TextEditorFont);
    }

    private void SaveAs()
    {
        if (_package == null)
        {
            return;
        }

        if (_saveAsDialog == null)
        {
            _saveAsDialog = new FileChooserDialog(
                "Save as",
                this,
                FileChooserAction.Save,
                Stock.Cancel, ResponseType.Cancel,
                Stock.Ok, ResponseType.Ok);
        }
        _saveAsDialog.CurrentName = string.Format("{0}.{1}.nupkg", _package.Id, _package.Version);
        _saveAsDialog.DoOverwriteConfirmation = true;
        var r = _saveAsDialog.Run();
        _saveAsDialog.Hide();
        if (r != (int)ResponseType.Ok)
        {
            return;
        }

        var fileName = _saveAsDialog.Filename;
        using (var f = new FileStream(fileName, FileMode.Create))
        {
            using (var inputStream = _package.GetStream())
            {
                inputStream.CopyTo(f);
            }
        }
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
        SaveUserPrefs();
		Application.Quit();
		a.RetVal = true;
	}

    private void OpenPackage(IPackage package)
    {
        _package = package;
        _saveAsMenuItem.Sensitive = true;
        _metadataView.Update(_package);
        _treeViewManager.Package = _package;
        _fileContentEditor.Clear();
        _fileContentEditor.SetMode(FileContentEditorMode.FileInfo);
    }

    public void OpenPackageFile(string fileName)
    {
        try
        {
            _package = new OptimizedZipPackage(fileName);
            OpenPackage(_package);
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

    private void OpenFileFromFeed()
    {
        int r = _openFileFromFeedDialog.Run();
        _openFileFromFeedDialog.Hide();
        if (r != (int)ResponseType.Ok)
        {
            return;
        }

        // load package in the background
        var package = _openFileFromFeedDialog.Package;
        var message = string.Format(
            "loading pacakge {0} {1}",
            package.Id, package.Version);
        var waitDialog = new WaitDialog(message, this);
        waitDialog.Show();
        
        var task = Task.Factory.StartNew(() =>
            {
                package.GetFiles();
            });
        GLib.Timeout.Add(100,
            () =>
            {
                if (task.IsCompleted)
                {
                    // show package
                    waitDialog.Destroy();
                    OpenPackage(package);

                    return false;
                }

                waitDialog.Pulse();
                return true;
            });
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

    void HandleFileSelected(object sender, FileSelectedEventArgs e)
    {
        _fileContentEditor.Clear();
        if (e.FilePath == null)
        {
            return;
        }

        var packageFile = _package.GetFiles().FirstOrDefault(f => f.Path == e.FilePath);
        _fileContentEditor.OpenFile(packageFile);
    }

    void SaveUserPrefs()
    {
        int h, w;
        this.GetSize(out w, out h);
        _userPrefs.WindowHeight = h;
        _userPrefs.WindowWidth = w;
        _userPrefs.HPanedPosition = _hpaned.Position;
        _userPrefs.VPanedPosition = _rightPane.Position;

        _userPrefs.Save();
    }
}
