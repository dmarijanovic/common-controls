using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DamirM.CommonLibrary;

namespace DamirM.CommonControls
{
    public partial class AutoComplete : Form
    {
        private bool isVisible = false;
        private TextBoxBase activeControl;
        private TagsStorage tagsStorage;

        public AutoComplete(Form owner, TagsStorage tagsStorage)
        {
            InitializeComponent();

            this.Owner = owner;
            this.Owner.Click += new EventHandler(Generic_LostFocus);
            this.tagsStorage = tagsStorage;
        }

        #region ControlMethodsAndEvents
        public void AddControl(TextBoxBase control)
        {
            control.KeyUp += new KeyEventHandler(control_KeyUp);
            //control.LostFocus += new EventHandler(control_LostFocus);
            control.MouseClick += new MouseEventHandler(control_MouseClick);
            control.GotFocus += new EventHandler(control_GotFocus);
            control.KeyDown += new KeyEventHandler(control_KeyDown);
        }
        private void control_KeyDown(object sender, KeyEventArgs e)
        {
            if (isVisible == true)
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (tvList.SelectedNode != null)
                    {
                        if (tvList.SelectedNode.Index != 0)
                        {
                            if (tvList.SelectedNode.Index != -1)
                            {
                                if (tvList.SelectedNode.Index > 0)
                                {
                                    tvList.SelectedNode = tvList.Nodes[tvList.SelectedNode.Index - 1];
                                }
                            }
                            else
                            {
                                tvList.SelectedNode = tvList.Nodes[0];
                            }
                        }
                    }
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (tvList.SelectedNode != null)
                    {
                        if (tvList.SelectedNode.Index != tvList.Nodes.Count)
                        {
                            if (tvList.SelectedNode.Index != -1)
                            {
                                if (tvList.SelectedNode.Index < tvList.Nodes.Count - 1)
                                {
                                    tvList.SelectedNode = tvList.Nodes[tvList.SelectedNode.Index + 1];
                                }
                            }
                            else
                            {
                                tvList.SelectedNode = tvList.Nodes[0];
                            }
                        }
                    }
                    else
                    {
                        if (tvList.Nodes.Count > 0)
                        {
                            tvList.SelectedNode = tvList.Nodes[0];
                        }
                    }
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.HideMe();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    SelectIthem();
                    e.SuppressKeyPress = true;
                }
            }
            else
            {
                if (e.Control == true && e.KeyCode == Keys.Space)
                {
                    AutoComplete_Refresh();
                    e.SuppressKeyPress = true;
                }
            }
        }
        private void control_MouseClick(object sender, MouseEventArgs e)
        {
            HideMe();
        }
        private void control_GotFocus(object sender, EventArgs e)
        {
            this.activeControl = (TextBoxBase)sender;
        }
        private void control_KeyUp(object sender, KeyEventArgs e)
        {
            TextBoxBase control = (TextBoxBase)sender;


            if (isVisible == false)
            {
                if (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && e.KeyCode != Keys.Back && e.KeyCode != Keys.Escape)
                {
                    if (control.SelectionStart > 1)
                    {
                        if ((control.Text[control.SelectionStart - 2] == '{' && control.Text[control.SelectionStart - 1] == '=') || (control.Text[control.SelectionStart - 2] != '\\' && control.Text[control.SelectionStart - 1] == '.'))
                        {
                            AutoComplete_Refresh();
                        }
                    }
                }
            }
            else
            {
                if (e.KeyCode != Keys.Up && e.KeyCode != Keys.Down)
                {
                    AutoComplete_Refresh();
                }
            }
        } 
        #endregion

        #region FormMethods
        private void HideMe()
        {
            this.Visible = false;
            this.isVisible = false;
            ttToolTip.Hide(this);
            ttToolTip.RemoveAll();
        }
        private void ShowMe(TextBoxBase control)
        {
            this.SetAutoCompleteLocation(control, true);
            this.Visible = true;
            this.isVisible = true;
            activeControl.Focus();
        }
        private void Generic_LostFocus(object sender, EventArgs e)
        {
            HideMe();
        }
        private void tvList_DoubleClick(object sender, EventArgs e)
        {
            SelectIthem();
        }
        private void tvList_GotFocus(object sender, EventArgs e)
        {
            this.activeControl.Focus();
        }
        private void tvList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TagsStorage tagsStorage;
            tagsStorage = (TagsStorage)e.Node.Tag;
            ttToolTip.Show(tagsStorage.Desctiption, this, this.Width, this.Height / 3);
        }
        #endregion


        #region OtherMethods
        private void AutoComplete_Refresh()
        {
            string result;

            try
            {
                result = GetTagBlock(activeControl.Text, activeControl.SelectionStart);

                Log.Write(result, this, "AutoComplete_Refresh", Log.LogType.DEBUG);
                // Show window
                if (isVisible == false)
                {
                    if (result != "")
                    {
                        UpdateHoveringWindowList(result);
                        this.ShowMe(activeControl);
                    }
                }
                else
                {
                    if (result != "")
                    {
                        UpdateHoveringWindowList(result);
                        this.ShowMe(activeControl);
                    }
                    else
                    {
                        HideMe();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, this, "AutoComplete_Refresh", Log.LogType.ERROR);
            }
        }
        private void UpdateHoveringWindowList(string text)
        {
            TagsStorage activeTagStorage = null;
            TreeNode treeNode;

            if (text.Length > 2)
            {
                text = text.Substring(2, text.Length - 2);
                //Tag2 tag = new Tag2("Root", text);
                Tag2 tag = new Tag2(text);
                activeTagStorage = tagsStorage.GetTagsStorage(tag);
            }
            else
            {
                activeTagStorage = tagsStorage;
            }
            if (activeTagStorage != null)
            {
                tvList.Nodes.Clear();
                foreach (TagsStorage tags in activeTagStorage)
                {
                    treeNode = new TreeNode(tags.DisplayText);
                    treeNode.ImageKey = tags.Type.ToString();
                    treeNode.SelectedImageKey = tags.Type.ToString();
                    //treeNode.Tag = tags.Value;
                    treeNode.Tag = tags;
                    tvList.Nodes.Add(treeNode);
                }
            }
            else
            {
                tvList.Nodes.Clear();
                HideMe();
            }
            // ovo je tmp rjesenje jer " . " bug, raditi preko tagova orginalno
            int lastIndexOfDot = text.LastIndexOf('.');
            string lastText;
            if (lastIndexOfDot != -1)
            {
                lastText = text.Substring(lastIndexOfDot + 1, (text.Length - lastIndexOfDot) - 1);
            }
            else
            {
                lastText = text;
            }

            // select active node base on user input
            foreach (TreeNode node in tvList.Nodes)
            {
                if (node.Text.StartsWith(lastText))
                {
                    tvList.SelectedNode = node;
                    break;
                }
            }
        }
        private string GetTagBlock(string text, int cursorPos)
        {
            string result = "";
            int endTagCount = 0;

            cursorPos--;

            for (int i = cursorPos; i >= 0; i--)
            {
                if (text[i] == '=')
                {
                    if ((i - 1) >= 0)
                    {
                        if (text[i - 1] == '{')
                        {
                            if (endTagCount == 0)
                            {
                                result = text.Substring(i - 1, (cursorPos - i) + 2);
                                break;
                            }
                            else
                            {
                                endTagCount--;
                            }
                        }
                    }
                }
                else if (text[i] == '}')
                {
                    if ((i - 1) >= 0)
                    {
                        if (text[i - 1] != '\\')
                        {
                            endTagCount++;
                        }
                    }
                }
            }

            return result;
        }
        private void SetAutoCompleteLocation(TextBoxBase control, bool moveHorizontly)
        {
            Point cursorLocation = control.GetPositionFromCharIndex(control.SelectionStart);
            Screen screen = Screen.FromPoint(cursorLocation);
            Point optimalLocation = new Point(control.PointToScreen(cursorLocation).X - 15, (int)(control.PointToScreen(cursorLocation).Y + Font.Size * 2 + 2));
            Rectangle desiredPlace = new Rectangle(optimalLocation, this.Size);
            desiredPlace.Width = 152;

            if (desiredPlace.Left < screen.Bounds.Left)
            {
                desiredPlace.X = screen.Bounds.Left;
            }
            if (desiredPlace.Right > screen.Bounds.Right)
            {
                desiredPlace.X -= (desiredPlace.Right - screen.Bounds.Right);
            }
            if (desiredPlace.Bottom > screen.Bounds.Bottom)
            {
                desiredPlace.Y = cursorLocation.Y - 2 - desiredPlace.Height;
            }
            if (!moveHorizontly)
            {
                desiredPlace.X = this.Left;
            }

            this.Bounds = desiredPlace;
        }
        private void SelectIthem()
        {
            TagsStorage tagsStorage;
            int oldSelectionStart;
            string tagBlock;
            int lastIndexOf;

            if (tvList.SelectedNode != null)
            {
                tagsStorage = (TagsStorage)tvList.SelectedNode.Tag;
                oldSelectionStart = activeControl.SelectionStart;
                tagBlock = GetTagBlock(activeControl.Text, activeControl.SelectionStart);

                if (tagBlock.Length > 2)
                {
                    lastIndexOf = activeControl.Text.LastIndexOf('.', activeControl.SelectionStart - 1, tagBlock.Length);
                    if (lastIndexOf != -1)
                    {
                        activeControl.SelectionStart = lastIndexOf + 1;
                    }
                    else
                    {
                        activeControl.SelectionStart = oldSelectionStart - tagBlock.Length + 2;
                    }
                    activeControl.SelectionLength = oldSelectionStart - activeControl.SelectionStart;

                }

                activeControl.SelectedText = tagsStorage.Value;
                if (tagsStorage.Type == TagsStorage.TagsStorageType.String)
                {
                    activeControl.SelectionStart = activeControl.SelectionStart - 1;
                }
                HideMe();
            }
        }

        #endregion

        private void AutoComplete_Enter(object sender, EventArgs e)
        {
            Log.Write("AutoComplete_Enter");
            this.activeControl.Focus();
        }
    }

  
}
