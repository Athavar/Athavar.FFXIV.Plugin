using Dalamud.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Athavar.FFXIV.Plugin
{
    internal partial class YesConfiguration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public bool Enabled = true;

        public TextFolderNode RootFolder { get; private set; } = new TextFolderNode { Name = "/" };

        public bool DesynthDialogEnabled = false;
        public bool DesynthBulkDialogEnabled = false;
        public bool MaterializeDialogEnabled = false;
        public bool ItemInspectionResultEnabled = false;
        public bool RetainerTaskAskEnabled = false;
        public bool RetainerTaskResultEnabled = false;
        public bool GrandCompanySupplyReward = false;
        public bool ShopCardDialog = false;

        public IEnumerable<INode> GetAllNodes() => new INode[] { RootFolder }.Concat(GetAllNodes(RootFolder.Children));

        public IEnumerable<INode> GetAllNodes(IEnumerable<INode> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;
                if (node is TextFolderNode textFolderNode)
                {
                    var children = textFolderNode.Children;
                    foreach (var childNode in GetAllNodes(children))
                    {
                        yield return childNode;
                    }
                }
            }
        }

        public bool TryFindParent(INode node, out TextFolderNode? parent)
        {
            foreach (var candidate in GetAllNodes())
            {
                if (candidate is TextFolderNode folder && folder.Children.Contains(node))
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
