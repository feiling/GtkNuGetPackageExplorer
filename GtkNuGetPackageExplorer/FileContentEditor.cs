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

        public FileContentEditor()
        {
            this.Build();

            InitFileTypes();
            foreach (var t in _fileTypes.Keys)
            {
                _fileTypeCombobox.AppendText(t);
            }

            _textEditor.Document.ReadOnly = true;
            _textEditor.Options.EnableSyntaxHighlighting = true;
            _textEditor.Document.MimeType = "";
        }

        private void InitFileTypes()
        {
            _fileTypes = new Dictionary<string, string>();
            _fileTypes.Add("C#", "text/x-csharp");
            _fileTypes.Add("XML", "application/xml");
            _fileTypes.Add("JavaScript", "text/javascript");
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

