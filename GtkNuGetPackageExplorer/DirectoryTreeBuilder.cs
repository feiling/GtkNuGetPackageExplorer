using System;
using System.Collections.Generic;
using NuGet;
using System.Linq;

namespace GtkNuGetPackageExplorer
{
    public class DirectoryTreeBuilder
    {
        Dictionary<string, TreeNode> _nodes;

        public DirectoryTreeBuilder()
        {
        }

        public TreeNode Create(IPackage package)
		{
			_nodes = new Dictionary<string, TreeNode>();
			foreach (var file in package.GetFiles())
			{
				var parent = GetParentNode(file.Path);
				parent.Children.Add(new TreeNode(file.Path));
			}

			var root = _nodes [""];

			// sort children of each node
			var q = new Queue<TreeNode>();
			q.Enqueue(root);
			while (!q.IsEmpty())
			{
				var n = q.Peek();
				q.Dequeue();

				n.SortChildren();
				foreach (var c in n.Children)
				{
					q.Enqueue(c);
				}
			}

			return root;
		}       

        private TreeNode GetParentNode(string path)
        {
            var d = System.IO.Path.GetDirectoryName(path);
            TreeNode n;
            if (_nodes.TryGetValue(d, out n))
            {
                return n;
            }
            else
            {
                // Create new node
                if (d == "")
                {
                    // create root
                    n = new TreeNode("");
                }
                else
                {
                    TreeNode parent = GetParentNode(d);
                    n = new TreeNode(d);
                    parent.Children.Add(n);
                }

                _nodes[d] = n;
                return n;
            }
        }
    }

    public class TreeNode
    {
        private List<TreeNode> _children;

        public string Name { get; set; }
        public IList<TreeNode> Children { get { return _children; } }

        public TreeNode(string name)
        {
            Name = name;
            _children = new List<TreeNode>();
        }

        public void SortChildren()
        {
            _children.Sort((a, b) => { 
                if (a.Children.Count > 0 && b.Children.Count == 0)
                {
                    return -1;
                }
                if (a.Children.Count == 0 && b.Children.Count > 0)
                {
                    return 1;
                }

                return string.Compare(a.Name, b.Name);
            });
        }
    }
}

