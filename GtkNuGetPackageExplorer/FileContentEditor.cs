using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Gtk;
using Mono.TextEditor;
using NuGet;

namespace GtkNuGetPackageExplorer
{
    public enum FileContentEditorMode
    {
        FileInfo,
        TextEditor
    }

    public class FileContentEditor
    {
        private VBox _widget;

        private VBox _editorContainer;
        private ComboBox _fileTypeCombobox;
        private TextEditor _textEditor;
        private Label _encodingInfo;

        // key is the text displayed in the combobox, value is the mime type
        Dictionary<string, string> _fileTypes;

        // key is the file extension, value is the text displayed in the combobox
        Dictionary<string, string> _fileExtensionToType;

        // List of file extensions known to be binary
        HashSet<string> _knownBinaryFileExtension;

        // there are two modes:
        // text editor mode, or file info mode
        private FileContentEditorMode _mode;

        private TextView _fileInfoView;
        private ScrolledWindow _fileInfoContainer;

        public Widget Widget
        {
            get
            {
                return _widget;
            }
        }

        protected virtual void Build()
        {
            _fileTypeCombobox = ComboBox.NewText();
            _fileTypeCombobox.Changed += OnFileTypeComboboxChanged;

            _encodingInfo = new Label();
            var hbox = new HBox();
            hbox.Spacing = 5;
            hbox.PackStart(_fileTypeCombobox, expand: false, fill: false, padding: 0);
            hbox.PackStart(_encodingInfo, expand: false, fill: false, padding: 0);

            _textEditor = new TextEditor();
            _textEditor.Document.ReadOnly = true;
            _textEditor.Options.EnableSyntaxHighlighting = true;
            _textEditor.Document.MimeType = "";
            _textEditor.Options.ShowLineNumberMargin = true;

            var scrolledWindow = new ScrolledWindow();
            scrolledWindow.ShadowType = ShadowType.EtchedIn;
            scrolledWindow.Add(_textEditor);

            _editorContainer = new VBox();
            _editorContainer.PackStart(hbox, expand: false, fill: false, padding: 0);
            _editorContainer.PackEnd(scrolledWindow);
            _editorContainer.ShowAll();

            _fileInfoView = new TextView()
            {
                Editable = false,
                WrapMode = WrapMode.Word,
                LeftMargin = 5,
                RightMargin = 5                
            };

            _fileInfoContainer = new ScrolledWindow()
            {
                ShadowType = Gtk.ShadowType.EtchedIn
            };
            _fileInfoContainer.Add(_fileInfoView);
            _fileInfoContainer.ShowAll();

            _widget = new VBox();
            _widget.Add(_fileInfoContainer);
            _mode = FileContentEditorMode.FileInfo;            
        }

        public FileContentEditor()
        {
            this.Build();

            InitFileTypes();
            foreach (var t in _fileTypes.Keys)
            {
                _fileTypeCombobox.AppendText(t);
            }            
        }

        private void InitFileTypes()
        {
            _fileTypes = new Dictionary<string, string>();
            _fileTypes.Add("C#", "text/x-csharp");
            _fileTypes.Add("CSS", "text/css");
            _fileTypes.Add("Html", "text/html");
            _fileTypes.Add("JavaScript", "text/javascript");
            _fileTypes.Add("XML", "application/xml");
            _fileTypes.Add("Text", "");

            _fileExtensionToType = new Dictionary<string, string>(
                StringComparer.InvariantCultureIgnoreCase);
            _fileExtensionToType.Add(".cs", "C#");
            _fileExtensionToType.Add(".js", "JavaScript");
            _fileExtensionToType.Add(".xml", "XML");
            _fileExtensionToType.Add(".html", "Html");
            _fileExtensionToType.Add(".htm", "Html");
            _fileExtensionToType.Add(".css", "CSS");

            _knownBinaryFileExtension = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            _knownBinaryFileExtension.AddRange(new[] { ".exe", ".pdb" });
        }

        protected void OnFileTypeComboboxChanged(object sender, EventArgs e)
        {
            TreeIter iter;
            if (!_fileTypeCombobox.GetActiveIter(out iter))
            {
                return;
            }

            var type = (string)_fileTypeCombobox.Model.GetValue(iter, 0);
            var mimeType = _fileTypes[type];
            SetMimeType(mimeType);
        }

        private void SetFileType(string fileExtension)
        {
            string fileType;
            if (!_fileExtensionToType.TryGetValue(fileExtension, out fileType))
            {
                fileType = "Text";
            }

            TreeIter iter;
            _fileTypeCombobox.Model.IterChildren(out iter);
            for (; ; )
            {
                var t = (string)_fileTypeCombobox.Model.GetValue(iter, 0);
                if (t == fileType)
                {
                    _fileTypeCombobox.SetActiveIter(iter);
                    return;
                }

                if (!_fileTypeCombobox.Model.IterNext(ref iter))
                {
                    break;
                }
            }
        }

        void SetMimeType(string mimeType)
        {
            _textEditor.Document.MimeType = mimeType;

            // Change document text to force syntax highlighting update
            var text = _textEditor.Document.Text;
            _textEditor.Document.Text = "";
            _textEditor.Document.Text = text;
        }

        public void OpenFile(IPackageFile packageFile)
        {
            var extension = Path.GetExtension(packageFile.Path);
            if (String.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                ShowDllFileInfo(packageFile);
                SetMode(FileContentEditorMode.FileInfo);
            }
            else if (_knownBinaryFileExtension.Contains(extension))
            {
                _fileInfoView.Buffer.Text = "This is a binary file";
                SetMode(FileContentEditorMode.FileInfo);
            }
            else
            {
                SetMode(FileContentEditorMode.TextEditor);
                GetEncodingInfo(packageFile.GetStream());
                using (TextReader r = new StreamReader(packageFile.GetStream()))
                {
                    var fileContent = r.ReadToEnd();
                    SetFileType(extension);
                    _textEditor.Text = fileContent;
                }
            }
        }

        public void SetMode(FileContentEditorMode mode)
        {
            if (_mode == mode)
            {
                return;
            }

            _mode = mode;
            if (_mode == FileContentEditorMode.FileInfo)
            {
                _widget.Remove(_editorContainer);
                _widget.Add(_fileInfoContainer);
            }
            else
            {
                // Editor info mode
                _widget.Remove(_fileInfoContainer);
                _widget.Add(_editorContainer);
            }
        }

        private void GetEncodingInfo(Stream stream)
        {
            byte[] buffer = new byte[4];
            var bufferSize = stream.Read(buffer, 0, buffer.Length);

            if (ContainsBom(Encoding.UTF8.GetPreamble(), buffer, bufferSize))
            {
                _encodingInfo.Text = "UTF-8 with BOM";
            }
            else if (ContainsBom(Encoding.Unicode.GetPreamble(), buffer, bufferSize))
            {
                _encodingInfo.Text = "UTF-16 LE with BOM";
            }
            else if (ContainsBom(Encoding.BigEndianUnicode.GetPreamble(), buffer, bufferSize))
            {
                _encodingInfo.Text = "UTF-16 BE with BOM";
            }
            else if (ContainsBom(Encoding.UTF32.GetPreamble(), buffer, bufferSize))
            {
                _encodingInfo.Text = "UTF-32 LE with BOM";
            }
            else if (ContainsBom(new byte[] {00, 00, 0xFE, 0xFF}, buffer, bufferSize))
            {
                _encodingInfo.Text = "UTF-32 BE with BOM";
            }
            else
            {
                _encodingInfo.Text = "No BOM. Treated as UTF-8";
            }
        }

        private bool ContainsBom(byte[] preamble, byte[] buffer, int bufferSize)
        {
            if (preamble.Length > bufferSize)
            {
                return false;
            }

            for (int i = 0; i < preamble.Length; ++i)
            {
                if (preamble[i] != buffer[i])
                {
                    return false;
                }
            }

            return true;
        }

        public void Clear()
        {
            _textEditor.Text = "";
            _encodingInfo.Text = "";
            _fileInfoView.Buffer.Clear();
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

        public Pango.FontDescription TextEditorFont 
        { 
            get
            {
                return _textEditor.Options.Font;
            }
            set
            {
                _textEditor.Options.FontName = value.ToString();
            }
        }

        public Pango.FontDescription Font
        {
            get
            {
                return _fileInfoView.Style.FontDesc;
            }
            set
            {
                _fileInfoView.ModifyFont(value);
            }
        }
    }
}

