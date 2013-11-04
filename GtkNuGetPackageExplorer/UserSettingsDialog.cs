using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;

namespace GtkNuGetPackageExplorer
{
    class UserSettingsDialog : Dialog
    {
        UserSettings _userSettings;

        public UserSettingsDialog(Window parent, UserSettings userSettings)
            : base("Settings", parent, DialogFlags.DestroyWithParent)
        {
            _userSettings = userSettings;

            Gtk.Table table = new Table(2, 2, homogeneous: false);
            var label = new Label() { Text = "Font" };
            label.SetAlignment(0, 0.5f);
            table.Attach(
                label, 
                0, 1, 0, 1, 
                xoptions: AttachOptions.Fill,
                yoptions: 0,
                xpadding: 5,
                ypadding: 5);

            var fontButton = new FontButton(_userSettings.UIFont.ToString());
            fontButton.FontSet += (obj, e) =>
            {
                _userSettings.UIFont = Pango.FontDescription.FromString(fontButton.FontName);
            };
            table.Attach(
                fontButton,
                1, 2, 0, 1,
                xoptions: AttachOptions.Expand | AttachOptions.Fill,
                yoptions: 0,
                xpadding: 5,
                ypadding: 5);

            label = new Label() { Text = "Text Editor Font" };
            label.SetAlignment(0, 0.5f);
            table.Attach(
                label, 
                0, 1, 1, 2, 
                xoptions: 0,
                yoptions: 0,
                xpadding: 5,
                ypadding: 5);

            var editorFontButton = new FontButton(_userSettings.TextEditorFont.ToString());            
            editorFontButton.FontSet += (obj, e) =>
            {
                _userSettings.TextEditorFont = editorFontButton.FontName;
            };
            table.Attach(
                editorFontButton,
                1, 2, 1, 2,
                xoptions: AttachOptions.Expand | AttachOptions.Fill,
                yoptions: 0,
                xpadding: 5,
                ypadding: 5);

            this.VBox.PackStart(table, expand: true, fill: true, padding: 5);
            AddButton("OK", ResponseType.Ok);
            AddButton("Cancel", ResponseType.Cancel);
            this.ShowAll();
        }

        public UserSettings UserSettings
        {
            get
            {
                return _userSettings;
            }
        }
    }
}
