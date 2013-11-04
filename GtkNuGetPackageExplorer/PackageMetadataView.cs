using System;
using Mono.TextEditor;
using Gtk;
using System.Collections.Generic;
using NuGet;
using System.IO;
using System.Text;

namespace GtkNuGetPackageExplorer
{
    class PackageMetadataView
    {
        private ScrolledWindow _scrolledWindow;
        private TextView _metaDataView;

        public PackageMetadataView()
        {
            _metaDataView = new TextView();
            _metaDataView.Editable = false;
            _metaDataView.WrapMode = WrapMode.Word;
            _metaDataView.LeftMargin = 5;
            _metaDataView.RightMargin = 5;

            _scrolledWindow = new ScrolledWindow();
            _scrolledWindow.ShadowType = ShadowType.EtchedIn;
            _scrolledWindow.Add(_metaDataView);

            var textBuffer = _metaDataView.Buffer;
            var tag = new TextTag("bold") { Weight = Pango.Weight.Bold };
            textBuffer.TagTable.Add(tag);

            tag = new TextTag("italic") { Style = Pango.Style.Italic };
            textBuffer.TagTable.Add(tag);
        }

        public Pango.FontDescription Font
        {
            get
            {
                return _metaDataView.Style.FontDesc;               
            }
            set
            {
                _metaDataView.ModifyFont(value);
            }
        }

        public Widget Widget
        {
            get
            {
                return _scrolledWindow;
            }
        }

        public void Update(IPackage package)
        {
            var textBuffer = _metaDataView.Buffer;
            textBuffer.Clear();

            textBuffer.AddWithTag("bold", "Id: ");
            textBuffer.Add("{0}\n", package.Id);

            textBuffer.AddWithTag("bold", "Version: ");
            textBuffer.Add("{0}\n", package.Version);

            if (!string.IsNullOrEmpty(package.Title))
            {
                textBuffer.AddWithTag("bold", "Version: ");
                textBuffer.Add("{0}\n", package.Title);
            }

            textBuffer.AddWithTag("bold", "Development Dependency: ");
            textBuffer.Add("{0}\n", package.DevelopmentDependency);

            if (!package.Authors.IsEmpty())
            {
                textBuffer.AddWithTag("bold", "Authors: ");
                textBuffer.Add("{0}\n", String.Join(",", package.Authors));
            }

            if (!package.Owners.IsEmpty())
            {
                textBuffer.AddWithTag("bold", "Owners: ");
                textBuffer.Add("{0}\n", String.Join(",", package.Owners));
            }

            if (!string.IsNullOrEmpty(package.Tags))
            {
                textBuffer.AddWithTag("bold", "Tags: ");
                textBuffer.Add("{0}\n", package.Tags);
            }

            if (!string.IsNullOrEmpty(package.Language))
            {
                textBuffer.AddWithTag("bold", "Language: ");
                textBuffer.Add("{0}\n", package.Language);
            }

            textBuffer.AddWithTag("bold", "Requires License Acceptance: ");
            textBuffer.Add("{0}\n", package.RequireLicenseAcceptance ? "Yes" : "No");

            if (package.LicenseUrl != null)
            {
                textBuffer.AddWithTag("bold", "License Url: ");
                textBuffer.Add("{0}\n", package.LicenseUrl);
            }

            if (package.ProjectUrl != null)
            {
                textBuffer.AddWithTag("bold", "Project Url: ");
                textBuffer.Add("{0}\n", package.ProjectUrl);
            }

            if (!string.IsNullOrEmpty(package.Summary))
            {
                textBuffer.AddWithTag("bold", "Summary: ");
                textBuffer.Add("\n{0}\n", package.Summary);
            }

            textBuffer.AddWithTag("bold", "Description: ");
            textBuffer.Add("\n{0}\n", package.Description);

            if (!string.IsNullOrEmpty(package.ReleaseNotes))
            {
                textBuffer.AddWithTag("bold", "Release notes: ");
                textBuffer.Add("\n{0}\n", package.ReleaseNotes);
            }

            textBuffer.AddWithTag("bold", "Dependencies: ");
            bool hasDependency = false;
            foreach (var dependencySet in package.DependencySets)
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
    }
}
