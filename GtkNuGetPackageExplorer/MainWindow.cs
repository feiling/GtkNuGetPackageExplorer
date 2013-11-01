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
        _fileContentEditor.Visible = true;

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

    private void OpenPackageFile(string fileName)
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
        _fileContentEditor.Text = "";

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
            using (TextReader r = new StreamReader(packageFile.GetStream()))
            {
                var fileContent = r.ReadToEnd();
                _fileContentEditor.SetFileType(System.IO.Path.GetExtension(e.FilePath));
                _fileContentEditor.Text = fileContent;
            }

            if (_fileDetail.Child != _fileContentEditor)
            {
                _fileDetail.Remove(_fileInfoView);
                _fileDetail.Add(_fileContentEditor);
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

            if (_fileDetail.Child != _fileInfoView)
            {
                _fileDetail.Remove(_fileContentEditor);
                _fileDetail.Add(_fileInfoView);
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
