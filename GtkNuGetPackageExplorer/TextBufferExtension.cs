using System;
using Gtk;

namespace GtkNuGetPackageExplorer
{
    public static class TextBufferExtension
    {
        public static void AddWithTag(this TextBuffer textBuffer, string tag, string format, params object[] args)
        {
            var iter = textBuffer.EndIter;
            var s = string.Format(format, args);
            textBuffer.InsertWithTagsByName(ref iter, s, tag);
        }

        public static void Add(this TextBuffer textBuffer, string format, params object[] args)
        {
            var iter = textBuffer.EndIter;
            var s = string.Format(format, args);
            textBuffer.Insert(ref iter, s);
        }
    }
}

