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

                var leafNode = new LeafTreeNode(System.IO.Path.GetFileName(file.Path), file.Path);
                parent.Children.Add(leafNode);
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
            // the key of the node is the directory part of the path
            var nodeKey = System.IO.Path.GetDirectoryName(path);           
            var nodeName = System.IO.Path.GetFileName(nodeKey);

            TreeNode n;
            if (_nodes.TryGetValue(nodeKey, out n))
            {
                return n;
            }
            else
            {
                // Create the parent node
                if (nodeKey == "")
                {
                    // create root
                    n = new TreeNode("");
                }
                else
                {
                    TreeNode parent = GetParentNode(nodeKey);
                    n = new TreeNode(nodeName);
                    parent.Children.Add(n);
                }

                _nodes[nodeKey] = n;
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

        public virtual string FilePath 
        { 
            get { return null; } 
            set {} 
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

    public class LeafTreeNode : TreeNode
    {
        public override string FilePath { get; set; }

        public LeafTreeNode(string name, string filePath) : base(name)
        {
            FilePath = filePath;
        }
    }
}

