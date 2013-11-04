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

            var hbox = new HBox();
            hbox.PackStart(new Label() { Text = "Font" }, expand: false, fill: false, padding: 5);

            var fontButton = new FontButton(_userSettings.UIFont.ToString());
            fontButton.FontSet += (obj, e) =>
            {
                _userSettings.UIFont = Pango.FontDescription.FromString(fontButton.FontName);
            };

            hbox.PackStart(fontButton, expand: true, fill: true, padding: 5);
            this.VBox.PackStart(hbox, expand: false, fill: false, padding: 5);

            hbox = new HBox();
            hbox.PackStart(new Label() { Text = "Text Editor Font" }, expand: false, fill: false, padding: 5);

            var editorFontButton = new FontButton(_userSettings.TextEditorFont.ToString());            
            editorFontButton.FontSet += (obj, e) =>
            {
                _userSettings.TextEditorFont = Pango.FontDescription.FromString(editorFontButton.FontName);
            };
            hbox.PackStart(editorFontButton, expand: true, fill: true, padding: 5);
            this.VBox.PackStart(hbox, expand: false, fill: false, padding: 5);

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
