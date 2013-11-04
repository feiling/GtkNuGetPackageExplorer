using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GtkNuGetPackageExplorer
{
    class UserPrefs
    {
        public int WindowHeight { get; set; }
        public int WindowWidth { get; set; }
        public int HPanedPosition { get; set; }
        public int VPanedPosition { get; set; }

        public UserPrefs()
        {
            WindowHeight = 500;
            WindowWidth = 600;
            HPanedPosition = 200;
            VPanedPosition = 200;
        }

        public void Load()
        {
            try
            {
                var fn = GetFileName();
                XDocument doc = XDocument.Load(fn);

                var v = UserSettings.GetValue(doc, "WindowHeight");
                int result;
                if (Int32.TryParse(v, out result))
                {
                    WindowHeight = result;
                }

                v = UserSettings.GetValue(doc, "WindowWidth");
                if (Int32.TryParse(v, out result))
                {
                    WindowWidth = result;
                }

                v = UserSettings.GetValue(doc, "HPanedPosition");
                if (Int32.TryParse(v, out result))
                {
                    HPanedPosition = result;
                }

                v = UserSettings.GetValue(doc, "VPanedPosition");
                if (Int32.TryParse(v, out result))
                {
                    VPanedPosition = result;
                }
            }
            catch
            {
            }
        }

        public void Save()
        {
            try
            {
                XDocument doc = new XDocument(new XElement("settings"));
                var e = new XElement("setting");
                e.Add(new XAttribute("key", "WindowHeight"));
                e.Add(new XAttribute("value", WindowHeight.ToString()));
                doc.Root.Add(e);

                e = new XElement("setting");
                e.Add(new XAttribute("key", "WindowWidth"));
                e.Add(new XAttribute("value", WindowWidth.ToString()));
                doc.Root.Add(e);

                e = new XElement("setting");
                e.Add(new XAttribute("key", "HPanedPosition"));
                e.Add(new XAttribute("value", HPanedPosition.ToString()));
                doc.Root.Add(e);

                e = new XElement("setting");
                e.Add(new XAttribute("key", "VPanedPosition"));
                e.Add(new XAttribute("value", VPanedPosition.ToString()));
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
                "GtkNuGetPackageExplorer.prefs");
        }
    }
}
