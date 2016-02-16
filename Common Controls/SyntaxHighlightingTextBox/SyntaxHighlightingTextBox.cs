using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using DamirM.CommonLibrary;

namespace DamirM.Controls
{
	/// <summary>
	/// A textbox the does syntax highlighting.
	/// </summary>
	public class SyntaxHighlightingTextBox :	System.Windows.Forms.RichTextBox 
	{
		#region Members

		//Members exposed via properties
		private SeperaratorCollection mSeperators = new SeperaratorCollection();  
		private HighLightDescriptorCollection mHighlightDescriptors = new HighLightDescriptorCollection();
		private bool mCaseSesitive = false;
        private DescriptorProccessModes descriptorProccessMode = DescriptorProccessModes.ProccessBySeperarators;

		//Internal use members
		private bool mParsing = false;

		//Undo/Redo members
		private ArrayList mUndoList = new ArrayList();
		private Stack mRedoStack = new Stack();
		private bool mIsUndo = false;
		private UndoRedoInfo mLastInfo = new UndoRedoInfo("", new Win32.POINT(), 0);
		private int mMaxUndoRedoSteps = 50;

		#endregion

        public enum DescriptorProccessModes
        {
            /// <summary>
            /// Proccess by all Seperarators in collection
            /// </summary>
            ProccessBySeperarators,
            /// <summary>
            /// Proccess line for all Descriptor
            /// </summary>
            ProcsessLineForDescriptor
        }


		#region Properties
		/// <summary>
		/// Determines if token recognition is case sensitive.
		/// </summary>
		[Category("Behavior")]
		public bool CaseSensitive 
		{ 
			get 
			{ 
				return mCaseSesitive; 
			}
			set 
			{ 
				mCaseSesitive = value;
			}
		}

		/// <summary>
		/// Set the maximum amount of Undo/Redo steps.
		/// </summary>
		[Category("Behavior")]
		public int MaxUndoRedoSteps 
		{
			get 
			{
				return mMaxUndoRedoSteps;
			}
			set
			{
				mMaxUndoRedoSteps = value;
			}
		}

		/// <summary>
		/// A collection of charecters. a token is every string between two seperators.
		/// </summary>
		public SeperaratorCollection Seperators 
		{
			get 
			{
				return mSeperators;
			}
		}
		
		/// <summary>
		/// The collection of highlight descriptors.
		/// </summary>
		public HighLightDescriptorCollection HighlightDescriptors 
		{
			get 
			{
				return mHighlightDescriptors;
			}
		}

        /// <summary>
        /// Determines if token recognition is case sensitive.
        /// </summary>
        [Category("Behavior")]
        public DescriptorProccessModes DescriptorProccessMode
        {
            get
            {
                return descriptorProccessMode;
            }
            set
            {
                descriptorProccessMode = value;
            }
        }
		#endregion

		#region Overriden methods

		/// <summary>
		/// The on text changed overrided. Here we parse the text into RTF for the highlighting.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnTextChanged(EventArgs e)
		{
			if (mParsing) return;
			mParsing = true;
			Win32.LockWindowUpdate(Handle);
			base.OnTextChanged(e);

            try
            {
                if (!mIsUndo)
                {
                    mRedoStack.Clear();
                    mUndoList.Insert(0, mLastInfo);
                    this.LimitUndo();
                    mLastInfo = new UndoRedoInfo(Text, GetScrollPos(), SelectionStart);
                }

                //Save scroll bar an cursor position, changeing the RTF moves the cursor and scrollbars to top positin
                Win32.POINT scrollPos = GetScrollPos();
                int cursorLoc = SelectionStart;

                //Created with an estimate of how big the stringbuilder has to be...
                StringBuilder sb = new StringBuilder((int)(Text.Length * 1.5 + 150));

                //Adding RTF header
                sb.Append(@"{\rtf1\fbidis\ansi\ansicpg1255\deff0\deflang1037{\fonttbl{");

                //Font table creation
                int fontCounter = 0;
                Hashtable fonts = new Hashtable();
                AddFontToTable(sb, Font, ref fontCounter, fonts);
                foreach (HighlightDescriptor hd in mHighlightDescriptors)
                {
                    if ((hd.Font != null) && !fonts.ContainsKey(hd.Font.Name))
                    {
                        AddFontToTable(sb, hd.Font, ref fontCounter, fonts);
                    }
                }
                sb.Append("}\n");

                //ColorTable

                sb.Append(@"{\colortbl ;");
                Hashtable colors = new Hashtable();
                int colorCounter = 1;
                AddColorToTable(sb, ForeColor, ref colorCounter, colors);
                AddColorToTable(sb, BackColor, ref colorCounter, colors);

                foreach (HighlightDescriptor hd in mHighlightDescriptors)
                {
                    if (!colors.ContainsKey(hd.Color))
                    {
                        AddColorToTable(sb, hd.Color, ref colorCounter, colors);
                    }
                }

                //Parsing text

                sb.Append("}\n").Append(@"\viewkind4\uc1\pard\ltrpar");
                SetDefaultSettings(sb, colors, fonts);

                char[] sperators = mSeperators.GetAsCharArray();

                //Replacing "\" to "\\" for RTF...
                //string[] lines = Text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Split('\n');

                int firstCharIndexFromLine = this.GetFirstCharIndexOfCurrentLine();
                //int lastCharindexFromLine = this.Rtf.IndexOf('\n', firstCharIndexFromLine);

                //lastCharindexFromLine = lastCharindexFromLine != -1 ? lastCharindexFromLine : this.Rtf.Length;
                
                    //string line = this.Rtf.Substring(firstCharIndexFromLine, lastCharindexFromLine != -1 ? lastCharindexFromLine - firstCharIndexFromLine : this.Rtf.Length - firstCharIndexFromLine);
                    //Log.Write(line, this, "OnTextChanged - Line", Log.LogType.DEBUG);

                int lineNumber = this.GetLineFromCharIndex(this.GetFirstCharIndexOfCurrentLine());
                
                string line = this.Text.Length != 0 ? this.Lines[lineNumber] : "";

                // Select line in control
                this.SelectionStart = firstCharIndexFromLine;
                this.SelectionLength = line.Length; // lastCharindexFromLine - firstCharIndexFromLine;

                line = line.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");                //Log.Write(firstCharIndexFromLine + ", " + lastCharindexFromLine, this, "OnTextChanged - IndexOf", Log.LogType.DEBUG);

                string[] tokens = mCaseSesitive ? line.Split(sperators) : line.ToUpper().Split(sperators);
                if (tokens.Length == 0)
                {
                    sb.Append(line);
                    AddNewLine(sb);
                    //continue;
                }

                //Log.Write(line, this, "OntextChange", Log.LogType.DEBUG);

                int tokenCounter = 0;
                for (int i = 0; i < line.Length; )
                {
                    char curChar = line[i];
                    if (mSeperators.Contains(curChar))
                    {
                        sb.Append(curChar);
                        i++;
                    }
                    else
                    {
                        string curToken = tokens[tokenCounter++];
                        //curToken = curToken.Replace("\\}", "}").Replace("\\{", "{");
                        if (descriptorProccessMode == DescriptorProccessModes.ProccessBySeperarators)
                        {
                            ///Log.Write("Ulazim u obradu...DescriptorRecognitionJob");
                            //DescriptorRecognitionJob(sb, fonts, colors, sperators, lines, ref lineCounter, ref line, ref tokens, ref tokenCounter, ref i, curToken, bAddToken);
                            ProccessFormatOnAllLines();
                        }
                        else if (descriptorProccessMode == DescriptorProccessModes.ProcsessLineForDescriptor)
                        {
                            ///Log.Write("Ulazim u obradu...DescriptorRecognitionJob2");
                            line = DescriptorRecognitionJob2(fonts, colors, line);
                            i = line.Length;
                        }
                    }
                }


                    ///Log.Write(string.Concat(this.Rtf.Substring(0, firstCharIndexFromLine), line, this.Rtf.Substring(lastCharindexFromLine)), this, "OnTextChanged", Log.LogType.DEBUG);
                    //Rtf = string.Concat(this.Rtf.Substring(0, firstCharIndexFromLine), line, this.Rtf.Substring(lastCharindexFromLine));

                // Replace edited line with new
                
                //Log.Write(sb.ToString(), this, "OnTextChanged - sb", Log.LogType.DEBUG);
                //Log.Write(this.Rtf, this, "OnTextChanged - rtf", Log.LogType.DEBUG);
                this.SelectedRtf = string.Concat(sb.ToString(), line,"}");
                
                //Restore cursor and scrollbars location.
                SelectionStart = cursorLoc;

                mParsing = false;

                SetScrollPos(scrollPos);
            }
            catch (Exception ex)
            {
                Log.Write(ex, this, "OntextChange", Log.LogType.ERROR);
            }

			Win32.LockWindowUpdate((IntPtr)0);
			Invalidate();

		}
        public void ProccessFormatOnAllLines()
        {
            if (mParsing) return;
            mParsing = true;
            Win32.LockWindowUpdate(Handle);
            base.OnTextChanged(new EventArgs());

            try
            {
                if (!mIsUndo)
                {
                    mRedoStack.Clear();
                    mUndoList.Insert(0, mLastInfo);
                    this.LimitUndo();
                    mLastInfo = new UndoRedoInfo(Text, GetScrollPos(), SelectionStart);
                }

                //Save scroll bar an cursor position, changeing the RTF moves the cursor and scrollbars to top positin
                Win32.POINT scrollPos = GetScrollPos();
                int cursorLoc = SelectionStart;

                //Created with an estimate of how big the stringbuilder has to be...
                StringBuilder sb = new StringBuilder((int)(Text.Length * 1.5 + 150));

                //Adding RTF header
                sb.Append(@"{\rtf1\fbidis\ansi\ansicpg1255\deff0\deflang1037{\fonttbl{");

                //Font table creation
                int fontCounter = 0;
                Hashtable fonts = new Hashtable();
                AddFontToTable(sb, Font, ref fontCounter, fonts);
                foreach (HighlightDescriptor hd in mHighlightDescriptors)
                {
                    if ((hd.Font != null) && !fonts.ContainsKey(hd.Font.Name))
                    {
                        AddFontToTable(sb, hd.Font, ref fontCounter, fonts);
                    }
                }
                sb.Append("}\n");

                //ColorTable

                sb.Append(@"{\colortbl ;");
                Hashtable colors = new Hashtable();
                int colorCounter = 1;
                AddColorToTable(sb, ForeColor, ref colorCounter, colors);
                AddColorToTable(sb, BackColor, ref colorCounter, colors);

                foreach (HighlightDescriptor hd in mHighlightDescriptors)
                {
                    if (!colors.ContainsKey(hd.Color))
                    {
                        AddColorToTable(sb, hd.Color, ref colorCounter, colors);
                    }
                }

                //Parsing text

                sb.Append("}\n").Append(@"\viewkind4\uc1\pard\ltrpar");
                SetDefaultSettings(sb, colors, fonts);

                char[] sperators = mSeperators.GetAsCharArray();

                //Replacing "\" to "\\" for RTF...
                string[] lines = Text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Split('\n');
                for (int lineCounter = 0; lineCounter < lines.Length; lineCounter++)
                {
                    if (lineCounter != 0)
                    {
                        AddNewLine(sb);
                    }
                    string line = lines[lineCounter];

                    string[] tokens = mCaseSesitive ? line.Split(sperators) : line.ToUpper().Split(sperators);
                    if (tokens.Length == 0)
                    {
                        sb.Append(line);
                        AddNewLine(sb);
                        continue;
                    }

                    //Log.Write(tokens, this, "OntextChange", Log.LogType.DEBUG);

                    int tokenCounter = 0;
                    for (int i = 0; i < line.Length; )
                    {
                        char curChar = line[i];
                        if (mSeperators.Contains(curChar))
                        {
                            sb.Append(curChar);
                            i++;
                        }
                        else
                        {
                            string curToken = tokens[tokenCounter++];
                            //curToken = curToken.Replace("\\}", "}").Replace("\\{", "{");
                            bool bAddToken = true;
                            if (descriptorProccessMode == DescriptorProccessModes.ProccessBySeperarators)
                            {
                                ///Log.Write("Ulazim u obradu...DescriptorRecognitionJob");
                                DescriptorRecognitionJob(sb, fonts, colors, sperators, lines, ref lineCounter, ref line, ref tokens, ref tokenCounter, ref i, curToken, bAddToken);
                            }
                            else if (descriptorProccessMode == DescriptorProccessModes.ProcsessLineForDescriptor)
                            {
                                ///Log.Write("Ulazim u obradu...DescriptorRecognitionJob2");
                                sb.Append(DescriptorRecognitionJob2(fonts, colors, line));
                                i = line.Length;
                            }
                        }
                    }
                }

                //			System.Diagnostics.Debug.WriteLine(sb.ToString());
                Rtf = sb.ToString();

                //Restore cursor and scrollbars location.
                SelectionStart = cursorLoc;

                mParsing = false;

                SetScrollPos(scrollPos);
            }
            catch (Exception ex)
            {
                Log.Write(ex, this, "OntextChange", Log.LogType.ERROR);
            }

            Win32.LockWindowUpdate((IntPtr)0);
            Invalidate();

        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Control == true && e.KeyValue == 86)
            {
                ProccessFormatOnAllLines();
                //Log.Write("", this, "OnKeyDown - control + V", Log.LogType.DEBUG);
            }
        }

        private void DescriptorRecognitionJob(StringBuilder sb, Hashtable fonts, Hashtable colors, char[] sperators, string[] lines, ref int lineCounter, ref string line, ref string[] tokens, ref int tokenCounter, ref int i, string curToken, bool bAddToken)
        {
            foreach (HighlightDescriptor hd in mHighlightDescriptors)
            {
                string compareStr = mCaseSesitive ? hd.Token : hd.Token.ToUpper();
                bool match = false;

                //Check if the highlight descriptor matches the current toker according to the DescriptoRecognision property.
                switch (hd.DescriptorRecognition)
                {
                    case DescriptorRecognition.WholeWord:
                        if (curToken == compareStr)
                        {
                            match = true;
                            Log.Write(new string[] { "Type: is WholeWord", "Token: " + curToken, "compareStr: " + compareStr, "Result: true" }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                        }
                        else
                        {
                            Log.Write(new string[] { "Type: is WholeWord", "Token: " + curToken, "compareStr: " + compareStr, "Result: false" }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                        }
                        break;
                    case DescriptorRecognition.StartsWith:
                        if (curToken.StartsWith(compareStr))
                        {
                            match = true;
                            Log.Write(new string[] { "Type: is StartsWith", "Token: " + curToken, "compareStr: " + compareStr, "Result: true" }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                        }
                        else
                        {
                            Log.Write(new string[] { "Type: is StartsWith", "Token: " + curToken, "compareStr: " + compareStr, "Result: false" }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                        }
                        break;
                    case DescriptorRecognition.Contains:
                        if (curToken.IndexOf(compareStr) != -1)
                        {
                            match = true;
                            Log.Write(new string[] { "Type: is Contains", "Token: " + curToken, "compareStr: " + compareStr, "Result: true" }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                        }
                        else
                        {
                            Log.Write(new string[] { "Type: is Contains", "Token: " + curToken, "compareStr: " + compareStr, "Result: false" }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                        }
                        break;
                    case DescriptorRecognition.RegEx:
                        Regex regEx = new Regex(compareStr);
                        if (regEx.IsMatch(curToken))
                        {
                            match = true;
                            //Log.Write(new string[] { "Type: is regex", "Token: " + curToken, "compareStr" + compareStr, "Result: true" }, this, "OntextChange", Log.LogType.DEBUG);
                        }
                        else
                        {
                            //Log.Write(new string[] { "Type: is regex", "Token: " + curToken, "compareStr" + compareStr, "Result: false" }, this, "OntextChange", Log.LogType.DEBUG);
                        }
                        break;
                }
                if (!match)
                {
                    //If this token doesn't match chech the next one.
                    continue;
                }

                //printing this token will be handled by the inner code, don't apply default settings...
                bAddToken = false;

                //Set colors to current descriptor settings.
                SetDescriptorSettings(sb, hd, colors, fonts);

                //Print text affected by this descriptor.
                switch (hd.DescriptorType)
                {
                    case DescriptorType.Word:
                        sb.Append(line.Substring(i, curToken.Length));
                        SetDefaultSettings(sb, colors, fonts);
                        i += curToken.Length;
                        break;
                    case DescriptorType.WordMatch:

                        int indexOfToken = line.IndexOf(compareStr, StringComparison.InvariantCultureIgnoreCase);
                        // if no match then break
                        if (indexOfToken == -1) break;

                        SetDefaultSettings(sb, colors, fonts);
                        sb.Append(line.Substring(0, indexOfToken));

                        SetDescriptorSettings(sb, hd, colors, fonts);
                        sb.Append(line.Substring(indexOfToken, compareStr.Length));

                        Log.Write(new string[] { "Line: " + line, "Token: " + curToken, "CompareStr: " + compareStr }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);

                        //Log.Write(line.Substring(0, indexOfToken), this, "<-------Start", Log.LogType.DEBUG);
                        //Log.Write(line.Substring(indexOfToken, compareStr.Length), this, "<------Color", Log.LogType.DEBUG);

                        //sb.Append(line.Substring(indexOfToken + compareStr.Length));

                        SetDefaultSettings(sb, colors, fonts);
                        if (indexOfToken + compareStr.Length > 0)
                        {
                            // line is resto of text now
                            line = line.Substring(indexOfToken + compareStr.Length);
                            curToken = mCaseSesitive ? line : line.ToUpper();
                            i = 0;

                            Log.Write(new string[] { "WordMatch: recurese", "line and curToken: " + line }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                            //Log.Write(new string[] { "Start at: " + indexOfToken + compareStr.Length, "lenght: " + (line.Length - (indexOfToken + compareStr.Length - 2)) }, this, "OntextChange", Log.LogType.DEBUG);
                            //i = indexOfToken + compareStr.Length;
                            //SetDefaultSettings(sb, colors, fonts);

                            DescriptorRecognitionJob(sb, fonts, colors, sperators, lines, ref lineCounter, ref line, ref tokens, ref tokenCounter, ref i, curToken, true);
                        }
                        else
                        {
                            i += curToken.Length;
                            Log.Write(new string[] { "WordMatch: exit", "start index: " + indexOfToken + compareStr.Length }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                        }
                        break;
                    case DescriptorType.ToEOL:
                        sb.Append(line.Remove(0, i));
                        i = line.Length;
                        SetDefaultSettings(sb, colors, fonts);
                        break;
                    case DescriptorType.ToCloseToken:
                        while ((line.IndexOf(hd.CloseToken, i) == -1) && (lineCounter < lines.Length))
                        {
                            sb.Append(line.Remove(0, i));
                            lineCounter++;
                            if (lineCounter < lines.Length)
                            {
                                AddNewLine(sb);
                                line = lines[lineCounter];
                                i = 0;
                            }
                            else
                            {
                                i = line.Length;
                            }
                        }
                        if (line.IndexOf(hd.CloseToken, i) != -1)
                        {
                            sb.Append(line.Substring(i, line.IndexOf(hd.CloseToken, i) + hd.CloseToken.Length - i));
                            line = line.Remove(0, line.IndexOf(hd.CloseToken, i) + hd.CloseToken.Length);
                            tokenCounter = 0;
                            tokens = mCaseSesitive ? line.Split(sperators) : line.ToUpper().Split(sperators);
                            SetDefaultSettings(sb, colors, fonts);
                            i = 0;
                        }
                        break;
                }
                break;
            }
            if (bAddToken)
            {
                //Print text with default settings...
                sb.Append(line.Substring(i, curToken.Length));
                Log.Write(new string[] { "pokusaj rezanja ostatka tekst", "Text:" + line.Substring(i, curToken.Length) }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
                i += curToken.Length;
            }
            else
            {
                Log.Write(new string[] { "izaso peder", "line:" + line }, this, "DescriptorRecognitionJob", Log.LogType.DEBUG);
            }
        }
        private string DescriptorRecognitionJob2(Hashtable fonts, Hashtable colors, string line )
        {
            StringBuilder sb = new StringBuilder();
            Regex regEx;
            Match match;
            int startMain;
            int startMainTemp;
            bool NextDescriptor ;
            string defaultColors;
            string stringLeft = "";
            string stringMatch = "";
            string stringRight = "";

            string lastColorTag = "";
            // get default colors
            SetDefaultSettings(sb, colors, fonts);
            defaultColors = sb.ToString();

            // temp string za logiranje
            string line_backup = line;

            //line = line.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");

            foreach (HighlightDescriptor hd in mHighlightDescriptors)
            {
                startMain = 0;
                NextDescriptor = false;
                do
                {
                    // clear all
                    sb.Remove(0, sb.Length);
                    string compareStr = hd.Token;

                    regEx = new Regex(compareStr, mCaseSesitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                    if (regEx.IsMatch(line, startMain))
                    {
                        // main match, search token
                        match = regEx.Match(line, startMain);
                        // cut left from match string
                        if (compareStr.StartsWith("\\"))
                        {
                            // if compare string start with escape char then go - 1 from index
                            stringLeft = line.Substring(0, (match.Index < 1 ? 0 : match.Index - 1));
                        }
                        else
                        {
                            stringLeft = line.Substring(0, (match.Index < 1 ? 0 : match.Index));
                        }
                        //Log.Write("stringLeft -> " + stringLeft + ",match.Index -> " + match.Index, this, "DescriptorRecognitionJob2", Log.LogType.DEBUG);
                        // save match and index
                        //regEx = new Regex("\\"

                        ///stringMatch = match.Value.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");
                        stringMatch = Escape.EscapeAll(match.Value, new char[] { '{', '}' }, '\\');

                        startMainTemp = match.Index;
                        // cut ring from match string
                        stringRight = line.Substring(match.Index + match.Value.Length);
                        // define new regex, to get last used color, hm reg ex zatvara space ili \
                        regEx = new Regex(@"\\cf[0-9]*( |\\)", RegexOptions.RightToLeft);
                        lastColorTag = regEx.Match(stringLeft).Value;
                        lastColorTag = lastColorTag.EndsWith("\\") ? lastColorTag.Substring(0, lastColorTag.Length - 1) + " " : lastColorTag;

                        // define new regex, set end line
                        regEx = new Regex(hd.TokenLeftCondition + "$");
                        // if left match is true then main string is match to
                        if (regEx.IsMatch(stringLeft))
                        {
                            // define new regex, set end line
                            regEx = new Regex("^" + hd.TokenRightCondition);
                            // if right match is true then main string is match to
                            if (regEx.IsMatch(stringRight))
                            {
                                SetDescriptorSettings(sb, hd, colors, fonts);
                                // new color is not same as last color then
                                if (!lastColorTag.Equals(sb.ToString()))
                                {
                                    // if no color is found
                                    if (lastColorTag == "")
                                    {
                                        line = string.Concat(stringLeft, sb.ToString(), stringMatch, defaultColors, stringRight);
                                    }
                                    else
                                    {
                                        // insert new color but continue with last color tag
                                        line = string.Concat(stringLeft, sb.ToString(), stringMatch, lastColorTag, stringRight);
                                    }
                                    startMain = startMainTemp + sb.ToString().Length;
                                }
                                else
                                {
                                    // ignore insert
                                }
                                startMain = startMainTemp + lastColorTag.Length;
                                ///Log.Write(new string[] { "Line: " + line_backup, "CompareStr: " + hd.TokenLeftCondition + compareStr, "Left: " + stringLeft, "Mid: " + stringMatch, "Right: " + stringRight, "Output: " + line }, this, "DescriptorRecognitionJob2", Log.LogType.DEBUG);
                            }
                            else
                            {
                                startMain = startMainTemp + 1;
                                ///Log.Write(new string[] { "Line: " + line_backup, "CompareStr: " + hd.TokenLeftCondition + compareStr, "Left: " + stringLeft, "Mid: " + stringMatch, "Right: " + stringRight, "RightCompareStr: " + hd.TokenRightCondition }, this, "DescriptorRecognitionJob2", Log.LogType.DEBUG);
                            }
                        }
                        else
                        {
                            //NextDescriptor = true;
                            startMain = startMainTemp + 1;
                            ///Log.Write(new string[] { "Line: " + line_backup, "CompareStr: " + hd.TokenLeftCondition + compareStr, "Left: " + stringLeft, "Mid: " + stringMatch, "Right: " + stringRight, "LeftCompareStr: " + hd.TokenLeftCondition }, this, "DescriptorRecognitionJob2", Log.LogType.DEBUG);
                        }
                    }
                    else
                    {
                        //continue;
                        // no descriptor in token select new descriptor
                        NextDescriptor = true;
                    }

                } while (!NextDescriptor);
            }
            return line; //.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");
        }


		protected override void OnVScroll(EventArgs e)
		{
			if (mParsing) return;
			base.OnVScroll (e);
		}

		/// <summary>
		/// Taking care of Keyboard events
		/// </summary>
		/// <param name="m"></param>
		/// <remarks>
		/// Since even when overriding the OnKeyDown methoed and not calling the base function 
		/// you don't have full control of the input, I've decided to catch windows messages to handle them.
		/// </remarks>
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case Win32.WM_PAINT:
				{
					//Don't draw the control while parsing to avoid flicker.
					if (mParsing)
					{
						return;
					}
					break;
				}
				case Win32.WM_KEYDOWN:
				{
                    if (((Keys)(int)m.WParam == Keys.Z) && 
						((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0))
					{
						Undo();
						return;
					}
					else if (((Keys)(int)m.WParam == Keys.Y) && 
						((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0))
					{
						Redo();
						return;
					}
                    break;
				}
				case Win32.WM_CHAR:
				{
					switch ((Keys)(int)m.WParam)
					{
						case Keys.Space:
							if ((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN )!= 0)
							{
								return;
							}
							break;
						//case Keys.Enter:
//							if (mAutoCompleteShown) return;
							//break;
					}
				}
				break;

			}
			base.WndProc (ref m);
		}


		#endregion

		#region Undo/Redo Code
		public new bool CanUndo 
		{
			get 
			{
				return mUndoList.Count > 0;
			}
		}
		public new bool CanRedo
		{
			get 
			{
				return mRedoStack.Count > 0;
			}
		}

		private void LimitUndo()
		{
			while (mUndoList.Count > mMaxUndoRedoSteps)
			{
				mUndoList.RemoveAt(mMaxUndoRedoSteps);
			}
		}

		public new void Undo()
		{
			if (!CanUndo)
				return;
			mIsUndo = true;
			mRedoStack.Push(new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
			UndoRedoInfo info = (UndoRedoInfo)mUndoList[0];
			mUndoList.RemoveAt(0);
			Text = info.Text;
			SelectionStart = info.CursorLocation;
			SetScrollPos(info.ScrollPos);
			mLastInfo = info;
			mIsUndo = false;
		}
		public new void Redo()
		{
			if (!CanRedo)
				return;
			mIsUndo = true;
			mUndoList.Insert(0,new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
			LimitUndo();
			UndoRedoInfo info = (UndoRedoInfo)mRedoStack.Pop();
			Text = info.Text;
			SelectionStart = info.CursorLocation;
			SetScrollPos(info.ScrollPos);
			mIsUndo = false;
		}

		private class UndoRedoInfo
		{
			public UndoRedoInfo(string text, Win32.POINT scrollPos, int cursorLoc)
			{
				Text = text;
				ScrollPos = scrollPos;
				CursorLocation = cursorLoc;
			}
			public readonly Win32.POINT ScrollPos;
			public readonly int CursorLocation;
			public readonly string Text;
		}
		#endregion

		#region Rtf building helper functions

		/// <summary>
		/// Set color and font to default control settings.
		/// </summary>
		/// <param name="sb">the string builder building the RTF</param>
		/// <param name="colors">colors hashtable</param>
		/// <param name="fonts">fonts hashtable</param>
		private void SetDefaultSettings(StringBuilder sb, Hashtable colors, Hashtable fonts)
		{
			SetColor(sb, ForeColor, colors);
			SetFont(sb, Font, fonts);
			SetFontSize(sb, (int)Font.Size);
			EndTags(sb);
		}

		/// <summary>
		/// Set Color and font to a highlight descriptor settings.
		/// </summary>
		/// <param name="sb">the string builder building the RTF</param>
		/// <param name="hd">the HighlightDescriptor with the font and color settings to apply.</param>
		/// <param name="colors">colors hashtable</param>
		/// <param name="fonts">fonts hashtable</param>
		private void SetDescriptorSettings(StringBuilder sb, HighlightDescriptor hd, Hashtable colors, Hashtable fonts)
		{
			SetColor(sb, hd.Color, colors);
			if (hd.Font != null)
			{
				SetFont(sb, hd.Font, fonts);
				SetFontSize(sb, (int)hd.Font.Size);
			}
			EndTags(sb);

		}
		/// <summary>
		/// Sets the color to the specified color
		/// </summary>
		private void SetColor(StringBuilder sb, Color color, Hashtable colors)
		{
			sb.Append(@"\cf").Append(colors[color]);
		}
		/// <summary>
		/// Sets the backgroung color to the specified color.
		/// </summary>
		private void SetBackColor(StringBuilder sb, Color color, Hashtable colors)
		{
			sb.Append(@"\cb").Append(colors[color]);
		}
		/// <summary>
		/// Sets the font to the specified font.
		/// </summary>
		private void SetFont(StringBuilder sb, Font font, Hashtable fonts)
		{
			if (font == null) return;
			sb.Append(@"\f").Append(fonts[font.Name]);
		}
		/// <summary>
		/// Sets the font size to the specified font size.
		/// </summary>
		private void SetFontSize(StringBuilder sb, int size)
		{
			sb.Append(@"\fs").Append(size*2);
		}
		/// <summary>
		/// Adds a newLine mark to the RTF.
		/// </summary>
		private void AddNewLine(StringBuilder sb)
		{
			sb.Append("\\par\n");
		}

		/// <summary>
		/// Ends a RTF tags section.
		/// </summary>
		private void EndTags(StringBuilder sb)
		{
			sb.Append(' ');
		}

		/// <summary>
		/// Adds a font to the RTF's font table and to the fonts hashtable.
		/// </summary>
		/// <param name="sb">The RTF's string builder</param>
		/// <param name="font">the Font to add</param>
		/// <param name="counter">a counter, containing the amount of fonts in the table</param>
		/// <param name="fonts">an hashtable. the key is the font's name. the value is it's index in the table</param>
		private void AddFontToTable(StringBuilder sb, Font font, ref int counter, Hashtable fonts)
		{
	
			sb.Append(@"\f").Append(counter).Append(@"\fnil\fcharset0").Append(font.Name).Append(";}");
			fonts.Add(font.Name, counter++);
		}

		/// <summary>
		/// Adds a color to the RTF's color table and to the colors hashtable.
		/// </summary>
		/// <param name="sb">The RTF's string builder</param>
		/// <param name="color">the color to add</param>
		/// <param name="counter">a counter, containing the amount of colors in the table</param>
		/// <param name="colors">an hashtable. the key is the color. the value is it's index in the table</param>
		private void AddColorToTable(StringBuilder sb, Color color, ref int counter, Hashtable colors)
		{
	
			sb.Append(@"\red").Append(color.R).Append(@"\green").Append(color.G).Append(@"\blue")
				.Append(color.B).Append(";");
			colors.Add(color, counter++);
		}

		#endregion

		#region Scrollbar positions functions
		/// <summary>
		/// Sends a win32 message to get the scrollbars' position.
		/// </summary>
		/// <returns>a POINT structore containing horizontal and vertical scrollbar position.</returns>
		private unsafe Win32.POINT GetScrollPos()
		{
			Win32.POINT res = new Win32.POINT();
			IntPtr ptr = new IntPtr(&res);
			Win32.SendMessage(Handle, Win32.EM_GETSCROLLPOS, 0, ptr);
			return res;

		}

		/// <summary>
		/// Sends a win32 message to set scrollbars position.
		/// </summary>
		/// <param name="point">a POINT conatining H/Vscrollbar scrollpos.</param>
		private unsafe void SetScrollPos(Win32.POINT point)
		{
			IntPtr ptr = new IntPtr(&point);
			Win32.SendMessage(Handle, Win32.EM_SETSCROLLPOS, 0, ptr);

		}
		#endregion
	}

}