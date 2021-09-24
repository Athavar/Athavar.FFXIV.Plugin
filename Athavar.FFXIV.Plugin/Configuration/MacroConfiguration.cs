using System.Collections.Generic;
using System.Linq;

namespace Athavar.FFXIV.Plugin
{
    internal class MacroConfiguration
    {
        public int Version { get; set; } = 1;

        public FolderNode RootFolder { get; private set; } = new FolderNode { Name = "/" };

        public float CustomFontSize { get; set; } = 15.0f;

        public IEnumerable<INode> GetAllNodes()
        {
            return new INode[] { RootFolder }.Concat(GetAllNodes(RootFolder.Children));
        }

        public IEnumerable<INode> GetAllNodes(IEnumerable<INode> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;
                if (node is FolderNode folderNode)
                {
                    var children = folderNode.Children;
                    foreach (var childNode in GetAllNodes(children))
                    {
                        yield return childNode;
                    }
                }
            }
        }

        public bool TryFindParent(INode node, out FolderNode? parent)
        {
            foreach (var candidate in GetAllNodes())
            {
                if (candidate is FolderNode folder && folder.Children.Contains(node))
                {
                    parent = folder;
                    return true;
                }
            }
            parent = null;
            return false;
        }
    }
}
