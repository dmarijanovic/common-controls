using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using DamirM.CommonLibrary;

namespace DamirM.CommonControls
{
    public class TagsLoader
    {
        string path;

        public TagsLoader(string path)
        {
            if (File.Exists(path))
            {
                this.path = path;
            }
            else
            {
                throw new FileNotFoundException("No file definition found at " + path);
            }
        }
        //public TagsLoader()
        //{
        //    this.path = Common.BuildPath(GenericTemplate.moduleParams.DataPath, GenericTemplate.constModuleDataFolder) + GenericTemplate.constTagsXMLFileName;
        //}
        public TagsStorage LoadTags()
        {
            TagsStorage rootNode = null;
            XmlDocument xmlDoc;
            if (File.Exists(this.path))
            {
                xmlDoc = new XmlDocument();
                try
                {
                    rootNode = new TagsStorage("{=", TagsStorage.TagsStorageType.Object);
                    xmlDoc.Load(path);
                    LoadXMLChild(rootNode, xmlDoc.SelectSingleNode("tags"));
                }
                catch (XmlException ex)
                {
                    Log.Write(ex, this, "LoadTags", Log.LogType.ERROR);
                }
                catch (Exception ex)
                {
                    Log.Write(ex, this, "LoadTags", Log.LogType.ERROR);
                }
            }
            return rootNode;
        }

        private void LoadXMLChild(TagsStorage tagsStorage, XmlNode xmlNode)
        {
            TagsStorage newTagsStorage;
            TagsStorage.TagsStorageType tagsStorageType;
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.Name == "tag")
                    {
                        tagsStorageType = TagsStorage.GetTypeFromString(node.Attributes["type"].Value);

                        if (node.Attributes["type"] != null)
                        {
                            // check if node is type of object

                            // ovo staviti pod isti i, jer je isto code
                            if (node.Attributes["type"].Value == "object")
                            {
                                if (tagsStorage != null)
                                {
                                    // if TagsStorage is not null then build nodes
                                    newTagsStorage = new TagsStorage(node.Attributes["name"].Value, tagsStorageType);
                                    tagsStorage.Add(newTagsStorage);
                                    if (node.HasChildNodes)
                                    {
                                        LoadXMLChild(newTagsStorage, node);
                                    }
                                }
                                else
                                {
                                    Log.Write("Bad xml format \r\n" + node.InnerXml, this, "LoadXMLChild", Log.LogType.WARNING);
                                }
                            }
                            else
                            {
                                if (tagsStorage != null)
                                {
                                    // if TagsStorage is not null then build nodes
                                    newTagsStorage = new TagsStorage(node.Attributes["name"].Value, tagsStorageType);
                                    tagsStorage.Add(newTagsStorage);
                                    if (node.HasChildNodes)
                                    {
                                        LoadXMLChild(newTagsStorage, node);
                                    }
                                }
                                else
                                {
                                    Log.Write("Bad xml format \r\n" + node.InnerXml, this, "LoadXMLChild", Log.LogType.WARNING);
                                }
                            }
                        }
                        else
                        {
                            Log.Write("Bad xml format \r\n" + node.InnerXml, this, "LoadXMLChild", Log.LogType.WARNING);
                        }
                    }
                    else if (node.Name == "syntax")
                    {
                        tagsStorage.Syntax = node.InnerText;
                    }
                    else if (node.Name == "result")
                    {
                        //lblResult.Text = "Result: " + node.InnerText;
                    }
                    else if (node.Name == "example")
                    {
                        //lblExample.Text = "Example: " + node.InnerText;
                    }
                    else if (node.Name == "desctiption")
                    {
                        tagsStorage.Desctiption = node.InnerText;
                    }
                    else
                    {
                        if (node.HasChildNodes)
                        {
                            LoadXMLChild(tagsStorage, node);
                        }
                    }
                }
            }
        }

    }

}
