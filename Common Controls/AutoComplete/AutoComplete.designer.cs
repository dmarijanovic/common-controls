namespace DamirM.CommonControls
{
    partial class AutoComplete
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Loading...");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoComplete));
            this.tvList = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ttToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // tvList
            // 
            this.tvList.BackColor = System.Drawing.SystemColors.Window;
            this.tvList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tvList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvList.FullRowSelect = true;
            this.tvList.HideSelection = false;
            this.tvList.ImageIndex = 0;
            this.tvList.ImageList = this.imageList1;
            this.tvList.Indent = 15;
            this.tvList.ItemHeight = 16;
            this.tvList.Location = new System.Drawing.Point(0, 2);
            this.tvList.Name = "tvList";
            treeNode1.ImageIndex = 0;
            treeNode1.Name = "Node0";
            treeNode1.Text = "Loading...";
            this.tvList.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.tvList.SelectedImageIndex = 0;
            this.tvList.ShowLines = false;
            this.tvList.ShowRootLines = false;
            this.tvList.Size = new System.Drawing.Size(180, 146);
            this.tvList.TabIndex = 2;
            this.tvList.DoubleClick += new System.EventHandler(this.tvList_DoubleClick);
            this.tvList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvList_AfterSelect);
            this.tvList.GotFocus += new System.EventHandler(this.tvList_GotFocus);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "object");
            this.imageList1.Images.SetKeyName(1, "string");
            this.imageList1.Images.SetKeyName(2, "integer");
            this.imageList1.Images.SetKeyName(3, "unknown");
            // 
            // AutoComplete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(182, 148);
            this.ControlBox = false;
            this.Controls.Add(this.tvList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AutoComplete";
            this.Padding = new System.Windows.Forms.Padding(0, 2, 2, 0);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "AutoComplet";
            this.GotFocus += new System.EventHandler(this.AutoComplete_Enter);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvList;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolTip ttToolTip;
    }
}