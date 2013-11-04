using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Pango;

namespace GtkNuGetPackageExplorer
{
    struct UserSettings
    {
        public string TextEditorFont
        {
            get;
            set;
        }

        public FontDescription UIFont
        {
            get;
            set;
        }

        public void Load()
        {
            try
            {
                var fn = GetFileName();
                XDocument doc = XDocument.Load(fn);

                var v = GetValue(doc, "UIFont");
                if (!String.IsNullOrWhiteSpace(v))
                {
                    UIFont = Pango.FontDescription.FromString(v);                    
                }

                v = GetValue(doc, "TextEditorFont");
                if (!String.IsNullOrWhiteSpace(v))
                {
                    TextEditorFont = v;
                }
            }
            catch
            {
            }
        }

        public static string GetValue(XDocument doc, string key)
        {
            var d = doc.Root.Descendants().Where(
                e =>
                {
                    if (e.Name != "setting")
                    {
                        return false;
                    }

                    var a = e.Attribute("key");
                    if (a == null || a.Value != key)
                    {
                        return false;
                    }

                    return true;
                }).FirstOrDefault();

            if (d == null)
            {
                return null;
            }

            var attr = d.Attribute("value");
            if (attr == null)
            {
                return null;
            }

            return attr.Value;
        }

        public void Save()
        {
            try
            {
                XDocument doc = new XDocument(new XElement("settings"));
                var e = new XElement("setting");
                e.Add(new XAttribute("key", "UIFont"));
                e.Add(new XAttribute("value", UIFont.ToString()));
                doc.Root.Add(e);

                e = new XElement("setting");
                e.Add(new XAttribute("key", "TextEditorFont"));
                e.Add(new XAttribute("value", TextEditorFont.ToString()));
                doc.Root.Add(e);

                var fn = GetFileName();
                doc.Save(fn);
            }
            catch
            {
            }
        }

        private string GetFileName()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GtkNuGetPackageExplorer.settings");
        }
    }
}
