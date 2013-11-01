using System;
using Mono.TextEditor;
using Gtk;
using System.Collections.Generic;

namespace GtkNuGetPackageExplorer
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class FileContentEditor : Gtk.Bin
    {
        // key is the text displayed in the combobox, value is the mime type
        Dictionary<string, string> _fileTypes;

        // key is the file extension, value is the text displayed in the combobox
        Dictionary<string, string> _fileExtensionToType;

        private const string mimeTypeCSharp = "text/x-csharp";
        private const string mimeTypeXml = "application/xml";
        private const string mimeTypeJavaScript = "text/javascript";
        private const string mimeTypeHtml = "text/html";

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
        }

        public string Text
        {
            get
            {
                return _textEditor.Text;
            }
            set
            {
                _textEditor.Text = value;
            }
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

        public void SetFileType(string fileExtension)
        {
            string fileType;
            if (!_fileExtensionToType.TryGetValue(fileExtension, out fileType))
            {
                fileType = "Text";
            }

            TreeIter iter;
            _fileTypeCombobox.Model.IterChildren(out iter);
            for (;;)
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
    }
}

