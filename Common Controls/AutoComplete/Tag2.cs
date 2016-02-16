using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using DamirM.CommonLibrary;

namespace DamirM.CommonControls
{
    public class Tag2
    {
        private Tag2 child;
        private Tag2 parent;

        public const string const_startTag = "{=";
        public const string const_startTag_regex = @"\{=";  // if is in regex then this is escape
        public const string const_endTag = "}";
        public const string const_endTag_regex = @"\}";
        public const string const_escape = "\"";
        private string name = "";
        private string orginalInputText;

        public Tag2(string inputText)
            : this(inputText, null)
        { }

        public Tag2(string inputText, Tag2 parent)
        {
            this.parent = parent;

            if (inputText.StartsWith(Tag2.const_startTag))
            {
                inputText = inputText.Substring(Tag2.const_startTag.Length, inputText.Length - Tag2.const_startTag.Length);
            }
            if (inputText.EndsWith(Tag2.const_endTag))
            {
                inputText = inputText.Substring(0, inputText.Length - Tag2.const_endTag.Length);
            }

            if (parent == null)
            {
                // save orginal input text, for debug
                this.orginalInputText = Tag2.const_startTag + inputText + Tag2.const_endTag;
            }

            if (!inputText.Equals(""))
            {
                // set tag name and create child tag
                this.child = ParseValue(inputText);
            }
        }


        private Tag2 ParseValue(string value)
        {
            //string[] valueList = value.Split('.');
            //if (valueList.Length >= 2)
            //{
            //    //0123456.89
            //    // 10 - 7
            //    return new Tag(valueList[0], value.Substring(valueList[0].Length + 1, (value.Length - valueList[0].Length) - 1));
            //}

            // {=string.indexof." . ".test text}
            // 
            int indexofFirstDot = 0;
            int indexofFirstQuotes = 0;
            int indexofSecondQuotes = 0;

            indexofFirstDot = value.IndexOf('.');
            indexofFirstQuotes = IndexOf("\"", 0, value, '\\');
            indexofSecondQuotes = IndexOf("\"", indexofFirstQuotes + 1, value, '\\');

            if (indexofFirstDot > indexofFirstQuotes && indexofFirstQuotes != -1)
            {
                if (indexofSecondQuotes != -1)
                {
                    indexofFirstDot = value.IndexOf('.', indexofSecondQuotes);

                    // check syntax
                    if (indexofFirstDot == -1 && indexofSecondQuotes < value.Length - 1)
                    {
                        this.name = "Syntax error";
                        Log.Write("Syntax error in tag block" + Environment.NewLine + this.TopParent.InputText, this, "ParseValue", Log.LogType.ERROR);
                        return null;
                    }
                }
                else
                {
                    indexofFirstDot = -1;
                }
            }


            // if dot is in midle of quotes then ingnore it
            if (indexofFirstDot > indexofFirstQuotes && indexofFirstDot < indexofSecondQuotes)
            {
                indexofFirstDot = -1;
            }



            // if frist dot is before first quotes
            if (indexofFirstDot == -1)
            {
                if (indexofFirstDot == indexofSecondQuotes)
                {
                    // all indexof is -1 so just return all text as name
                    this.name = value;
                }
                else
                {
                    // - -- - - - - - - -  - - - BUG !!!!!!!!!!!!!!  - - - -- - - -  - - - -  -
                    // TODO: neki bug je ovdje sa escapinim znakovima "\"" , 1 ,2 vrati samo \
                    //
                    int start = indexofFirstQuotes >= indexofSecondQuotes ? 0 : indexofFirstQuotes + 1;
                    int lenght = indexofSecondQuotes > indexofFirstQuotes + 1 ? indexofSecondQuotes - 1 : 0;
                    //this.Name = value.Substring(indexofFirstQuotes >= indexofSecondQuotes ? 0 : indexofFirstQuotes + 1, indexofSecondQuotes > indexofFirstQuotes + 1 ? indexofSecondQuotes - 1 : 0);
                    this.Name = value.Substring(start, lenght);
                }
                //this.Name = value.Substring(indexofFirstQuotes == -1? 0 : indexofFirstQuotes, indexofSecondQuotes > indexofFirstQuotes + 1 ? indexofSecondQuotes - 1 : 0);
            }
            else if (indexofFirstQuotes == -1 || (indexofFirstDot < indexofFirstQuotes))
            {
                // Set tag name
                this.Name = value.Substring(0, indexofFirstDot != -1 ? indexofFirstDot : 0);
                return new Tag2(value.Substring(indexofFirstDot != -1 ? indexofFirstDot + 1 : 0, indexofFirstDot != -1 ? ((value.Length - indexofFirstDot) - 1) : 0), this);
            }
            else
            {
                // if first dot is before secend quotes
                if (indexofFirstDot < indexofSecondQuotes)
                {
                    indexofFirstDot = value.IndexOf('.', indexofSecondQuotes + 1);
                    if (indexofFirstDot > 0)
                    {
                        // Set tag name
                        this.Name = value.Substring(0, indexofFirstDot);
                        return new Tag2(value.Substring(indexofFirstDot + 1, (value.Length - indexofFirstDot) - 1), this);
                    }
                }
                else
                {
                    // Set tag name
                    this.Name = value.Substring(0, indexofFirstDot);
                    return new Tag2(value.Substring(indexofFirstDot + 1, (value.Length - indexofFirstDot) - 1), this);
                }
            }


            return null;
        }

        public static string UnEscape(string text, bool skipTagEscape)
        {
            string result = text;
            Regex regex;

            //result = text.Replace("<*D*>", ".");
            //if (text.StartsWith("\"") && text.EndsWith("\"") && text.Length > 1)
            //{
            //    result = (text.Substring(1, text.Length - 1)).Substring(0, text.Length - 2);
            //}
            //result = result.Replace("\\\\", "\\");

            regex = new Regex(@"\\\\");
            result = regex.Replace(result, @"\");

            regex = new Regex("\\\\\"");
            result = regex.Replace(result, "\"");

            if (!skipTagEscape)
            {
                // start tag
                regex = new Regex(@"\\" + Tag2.const_startTag_regex);
                result = regex.Replace(result, Tag2.const_startTag);

                // end tag
                regex = new Regex(@"\\" + Tag2.const_endTag_regex);
                result = regex.Replace(result, Tag2.const_endTag);
            }

            return result;
        }

        public static string UnEscape(string text)
        {
            return UnEscape(text, false);
        }


        public static string Excape(string text)
        {
            // TODO: metoda mnora raditi excape i svih tagova, a ne samo \ znaka
            Regex regex;
            string result = text;

            //result = result.Replace(@"\\", @"\\\\");

            regex = new Regex(@"(?<!\\)\\");
            result = regex.Replace(result, @"\\");

            regex = new Regex("\"");
            result = regex.Replace(result, "\\\"");

            // start tag
            regex = new Regex(Tag2.const_startTag_regex);
            result = regex.Replace(result, "\\" + Tag2.const_startTag);

            // end tag
            regex = new Regex(Tag2.const_endTag_regex);
            result = regex.Replace(result, "\\" + Tag2.const_endTag);

            return result;
        }

        /// <summary>
        /// Same like String.IndexOf method but this method will ignore search text if chast is escaped
        /// </summary>
        /// <param name="search"></param>
        /// <param name="start"></param>
        /// <param name="text"></param>
        /// <param name="escapeChar"></param>
        /// <returns></returns>
        private int IndexOf(string search, int start, string text, char escapeChar)
        {
            int indexOf = 0;

            do
            {
                // reguler indexof search
                indexOf = text.IndexOf(search, start);

                // if char is found
                if (indexOf != -1)
                {
                    // check if indexof is greater of 1 so if we can check if search text is escaped
                    if (indexOf > 0)
                    {
                        // if search text is not escaped
                        if (text[indexOf - 1] != escapeChar)
                        {
                            // index OK, exit
                            break;
                        }
                        else
                        {
                            // char is escaped but now see if he ie escaped to
                            if (indexOf > 1)
                            {
                                if (text[indexOf - 2] == escapeChar)
                                {
                                    // index OK, exit
                                    break;
                                }
                            }

                        }
                    }
                    else
                    {
                        // indexof is zero so it can not be escaped
                        break;
                    }
                    start = indexOf + 1;
                }
            } while (indexOf != -1);

            return indexOf;
        }

        public Tag2 Child
        {
            get { return this.child; }
        }
        public Tag2 Parent
        {
            get { return this.parent; }
        }

        public Tag2 TopParent
        {
            get
            {
                Tag2 topParent = this;
                do
                {
                    if(topParent.parent == null)
                    {
                        break;
                    }
                    else
                    {
                        topParent = topParent.parent;
                    }
                } while (true);
                return topParent;
            }
        }

        public string Name
        {
            get 
            {
                return UnEscape(this.name, true); 
                //return this.name; 
            }
            set
            {
                this.name = value;
                if (this.name.StartsWith(Tag2.const_escape))
                {
                    this.name = this.name.Substring(Tag2.const_escape.Length, this.name.Length - Tag2.const_escape.Length);
                }
                if (this.name.EndsWith(Tag2.const_escape))
                {
                    this.name = this.name.Substring(0, this.name.Length - Tag2.const_escape.Length);
                }
            }
        }

        public string InputText
        {
            get
            {
                return this.orginalInputText;
            }
        }

        public override string ToString()
        {
            return this.orginalInputText;
        }

        /// <summary>
        /// ovo je stara metoda korisiti onu  iz commonlibery escape.issecape
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsExcaped(string text, int index)
        {
            int excapeCount = -1;
            for (int i = index - 1; i >= 0; i--)
            {
                if (text[i] == '\\')
                {
                    excapeCount++;
                }
                else
                {
                    break;
                }
            }

            if (excapeCount % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



    }
}
