using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace DamirM.CommonControls
{
    public class TagsStorage
    {
        private const string const_TagNameFormat = "{0} [{1}]"; // Name, TagType
        private ArrayList tagsList;
        private TagsStorageType tagType;
        private TagsStorage parent;
        string name;
        string syntax;
        string description;

        /// <summary>
        /// Declare types of tags
        /// </summary>
        public enum TagsStorageType
        {
            Unknown, Object, Integer, String, ArrayObject
        }

        /// <summary>
        /// Make new tag
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tagType"></param>
        public TagsStorage(string name, TagsStorageType tagType)
        {
            this.tagType = tagType;
            // Set tag name
            this.name = name;
        }

        /// <summary>
        /// Add new object
        /// </summary>
        /// <param name="tagsStorage"></param>
        public void Add(TagsStorage tagsStorage)
        {
            tagsStorage.parent = this;
            if (tagsList == null)
            {
                tagsList = new ArrayList();
            }
            tagsList.Add(tagsStorage);
        }

        /// <summary>
        /// Get object that matches input tag or ...
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public TagsStorage GetTagsStorage(Tag2 tag)
        {
            TagsStorage result = null;
            foreach (TagsStorage tagsStorage in this)
            {
                if (tagsStorage.name == tag.Name || (tagsStorage.tagType != TagsStorageType.Object))
                {
                    if (tag.Child == null)
                    {
                        result = tagsStorage.parent;
                        break;
                    }
                    else
                    {
                        result = tagsStorage.GetTagsStorage(tag.Child);
                        break;
                    }
                }
            }
            if (result == null)
            {
                //if (tag.Tags != null)
                //{
                result = this;
                //}
            }

            return result;
        }

        /// <summary>
        /// Returns tag storage type depending on input text
        /// </summary>
        /// <param name="tagsStorageTypeString">String representing tag storage type</param>
        /// <returns></returns>
        public static TagsStorageType GetTypeFromString(string typeString)
        {
            TagsStorageType tagsStorageType;
            if (typeString.Equals("Object", StringComparison.OrdinalIgnoreCase))
            {
                tagsStorageType = TagsStorage.TagsStorageType.Object;
            }
            else if (typeString.Equals("String", StringComparison.OrdinalIgnoreCase))
            {
                tagsStorageType = TagsStorage.TagsStorageType.String;
            }
            else if (typeString.Equals("Integer", StringComparison.OrdinalIgnoreCase))
            {
                tagsStorageType = TagsStorage.TagsStorageType.Integer;
            }
            else if (typeString.Equals("ArrayObject", StringComparison.OrdinalIgnoreCase))
            {
                tagsStorageType = TagsStorage.TagsStorageType.ArrayObject;
            }
            else
            {
                tagsStorageType = TagsStorage.TagsStorageType.Unknown;
            }
            return tagsStorageType;
        }

        /// <summary>
        /// Go through a collection of TagsStorage
        /// </summary>
        /// <returns>TagsStorage</returns>
        public System.Collections.Generic.IEnumerator<TagsStorage> GetEnumerator()
        {
            if (tagsList != null)
            {
                for (int i = 0; i < tagsList.Count; i++)
                {
                    yield return (TagsStorage)tagsList[i];
                }
            }
        }

        /// <summary>
        /// Display text that will appear to user
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (this.tagType == TagsStorageType.Object)
                {
                    // for object type only set name
                    return name;
                }
                else
                {
                    // for others types set additionally information, other types are params
                    return string.Format(TagsStorage.const_TagNameFormat, name, tagType.ToString());
                }
            }
        }
        /// <summary>
        /// Returnt value depending on TagsStorage type
        /// </summary>
        public string Value
        {
            get
            {
                if (this.tagType == TagsStorageType.Object)
                {
                    return name;
                }
                else if (this.tagType == TagsStorageType.Integer)
                {
                    return "";
                }
                else if (this.tagType == TagsStorageType.String)
                {
                    return "\"\"";
                }
                else
                {
                    return "\"\"";
                }
            }
        }
        // Get name of tag
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        public string Syntax
        {
            get
            {
                return this.syntax;
            }
            set
            {
                this.syntax = value;
            }
        }
        public TagsStorage Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }
        public bool HasChilds
        {
            get
            {
                if (tagsList != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string Desctiption
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public TagsStorageType Type
        {
            get
            {
                return this.tagType;
            }
        }
        public override string ToString()
        {
            return name;
        }

    }

}
