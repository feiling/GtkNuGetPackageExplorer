using System;
using Mono.TextEditor;
using Gtk;
using System.Collections.Generic;
using NuGet;
using System.IO;
using System.Text;

namespace GtkNuGetPackageExplorer
{
    public class FileContentEditor
    {
        private VBox _vbox;
        private ComboBox _fileTypeCombobox;
        private TextEditor _textEditor;
        private Label _encodingInfo;

        // key is the text displayed in the combobox, value is the mime type
        Dictionary<string, string> _fileTypes;

        // key is the file extension, value is the text displayed in the combobox
        Dictionary<string, string> _fileExtensionToType;

        // List of file extensions known to be binary
        HashSet<string> _knownBinaryFileExtension;

        private const string mimeTypeCSharp = "text/x-csharp";
        private const string mimeTypeXml = "application/xml";
        private const string mimeTypeJavaScript = "text/javascript";
        private const string mimeTypeHtml = "text/html";

        public Widget Widget
        {
            get
            {
                return _vbox;
            }
        }

        protected virtual void Build()
        {
            _fileTypeCombobox = ComboBox.NewText();
            _encodingInfo = new Label();
            var hbox = new HBox();
            hbox.Spacing = 5;
            hbox.PackStart(_fileTypeCombobox, expand: false, fill: false, padding: 0);
            hbox.PackStart(_encodingInfo, expand: false, fill: false, padding: 0);

            _textEditor = new TextEditor();
            _textEditor.Options.ShowLineNumberMargin = true;

            var scrolledWindow = new ScrolledWindow();
            scrolledWindow.ShadowType = ShadowType.EtchedIn;
            scrolledWindow.Add(_textEditor);

            _vbox = new VBox();
            _vbox.PackStart(hbox, expand: false, fill: false, padding: 0);
            _vbox.PackEnd(scrolledWindow);
            _vbox.ShowAll();
        }

        public FileContentEditor()
        {
            this.Build();

            InitFileTypes();
            foreach (var t in _fileTypes.Keys)
            {
                _fileTypeCombobox.AppendText(t);
            }

            _fileTypeCombobox.Changed += OnFileTypeComboboxChanged;
            _textEditor.Document.ReadOnly = true;
            _textEditor.Options.EnableSyntaxHighlighting = true;
            _textEditor.Document.MimeType = "";
        }

        private void InitFileTypes()
        {
            _fileTypes = new Dictionary<string, string>();
            _fileTypes.Add("C#", mimeTypeCSharp);
            _fileTypes.Add("XML", mimeTypeXml);
            _fileTypes.Add("JavaScript", mimeTypeJavaScript);
            _fileTypes.Add("Html", mimeTypeHtml);
            _fileTypes.Add("Text", "");

            _fileExtensionToType = new Dictionary<string, string>(
                StringComparer.InvariantCultureIgnoreCase);
            _fileExtensionToType.Add(".cs", "C#");
            _fileExtensionToType.Add(".js", "JavaScript");
            _fileExtensionToType.Add(".xml", "XML");
            _fileExtensionToType.Add(".html", "Html");
            _fileExtensionToType.Add(".htm", "Html");

            _knownBinaryFileExtension = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            _knownBinaryFileExtension.Add(".exe");
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
            if (_knownBinaryFileExtension.Contains(extension))
            {
                _textEditor.Text = "Binary file";
                _encodingInfo.Text = "";
            }
            else
            {
                GetEncodingInfo(packageFile.GetStream());
                using (TextReader r = new StreamReader(packageFile.GetStream()))
                {
                    var fileContent = r.ReadToEnd();
                    SetFileType(extension);
                    _textEditor.Text = fileContent;
                }
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
        }
    }
}

