using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;

namespace DamirM.CommonControls
{
    public class TagsShow
    {
        TagsStorage tagsStorage;
        bool showOnlyTagsOfObjectType = false;

        public TagsShow(TagsStorage tagsStorage)
        {
            this.tagsStorage = tagsStorage;
        }

        public void Show(TreeView treeView)
        {
            TreeNode rootNode = new TreeNode("Tags");
            ShowTreeView(rootNode, tagsStorage);
            rootNode.Expand();
            treeView.Nodes.Add(rootNode);
        }

        private void ShowTreeView(TreeNode treeNode, TagsStorage tagsStorage)
        {
            TreeNode newTreeNode;

            foreach (TagsStorage tags in tagsStorage)
            {
                // if showOnlyTagsOfObjectType is true and type is object then skip this tagsStorage
                if ( this.showOnlyTagsOfObjectType == false || (tags.Type == TagsStorage.TagsStorageType.Object && this.showOnlyTagsOfObjectType == true))
                {
                    newTreeNode = new TreeNode(tags.Value);
                    // tag object hold referenc to tags storage object 
                    newTreeNode.Tag = tags;
                    newTreeNode.ToolTipText = tags.Desctiption;
                    treeNode.Nodes.Add(newTreeNode);
                    ShowTreeView(newTreeNode, tags);
                }
            }
        }

        public ArrayList GetTagsList()
        {
            ArrayList list = new ArrayList();
            TagStorageToList(list, tagsStorage, ".");
            return list;
        }

        private void TagStorageToList(ArrayList list, TagsStorage tagsStorage, string separator)
        {
            ArrayList parantList = new ArrayList();
            string parentString = "";
            foreach (TagsStorage tags in tagsStorage)
            {
                // if showOnlyTagsOfObjectType is true and type is object then skip this tagsStorage
                if (this.showOnlyTagsOfObjectType == false || (tags.Type == TagsStorage.TagsStorageType.Object && this.showOnlyTagsOfObjectType == true))
                {
                    parentString = "";
                    parantList.Clear();
                    GetParentPath(parantList, tags.Parent, false);
                    for (int i = parantList.Count - 1; i >= 0; i--)
                    {
                        parentString = i == parantList.Count - 1 ? string.Concat(parentString, parantList[i].ToString()) : string.Concat(parentString, separator, parantList[i].ToString());
                    }
                    list.Add(parentString.Equals("") ? tags.Value : string.Concat(parentString, separator, tags.Value));
                    if (tags.HasChilds)
                    {
                        //parentString = parentString.Equals("") ? tags.Value : string.Concat(parentString, separator, tags.Value);
                        TagStorageToList(list, tags, separator);
                    }
                }
            }
        }
        // string.substring


        private void GetParentPath(ArrayList list, TagsStorage tagsStorage, bool includeRoot)
        {
            if (tagsStorage.Parent != null)
            {
                list.Add(tagsStorage.Value);
                GetParentPath(list, tagsStorage.Parent, includeRoot);
            }
            else
            {
                if (includeRoot == true)
                {
                    list.Add(tagsStorage.Value);
                }
            }
        }


        public bool ShowOnlyTagsOfObjectType
        {
            set
            {
                this.showOnlyTagsOfObjectType = value;
            }
        }
    }
}
