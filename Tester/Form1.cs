using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using DamirM.Controls;

namespace Tester
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{	
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(336, 125);
			this.Name = "Form1";
			this.Text = "Syntax Highlighting Tester";
			this.Load += new System.EventHandler(this.Form1_Load);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			SyntaxHighlightingTextBox shtb = new SyntaxHighlightingTextBox();
            shtb.Location = new Point(0, 0);
            shtb.Dock = DockStyle.Fill;
            shtb.Seperators.Add(' ');
            shtb.Seperators.Add('\r');
            shtb.Seperators.Add('\n');
            shtb.Seperators.Add(',');
            shtb.Seperators.Add('.');
            shtb.Seperators.Add('-');
            shtb.Seperators.Add('+');
            //shtb.Seperators.Add('*');
            //shtb.Seperators.Add('/');
            Controls.Add(shtb);
            shtb.WordWrap = false;
            shtb.ScrollBars = RichTextBoxScrollBars.Both;// & RichTextBoxScrollBars.ForcedVertical;

            /*shtb.HighlightDescriptors.Add(new HighlightDescriptor("<", Color.Gray, null, DescriptorType.Word, DescriptorRecognition.WholeWord, true));

            shtb.HighlightDescriptors.Add(new HighlightDescriptor("<<", ">>", Color.DarkGreen, null, DescriptorType.ToCloseToken, DescriptorRecognition.StartsWith, false));
*/
            shtb.HighlightDescriptors.Add(new HighlightDescriptor("Hello", Color.Red, null, DescriptorType.Word, DescriptorRecognition.Contains));
            shtb.HighlightDescriptors.Add(new HighlightDescriptor("Hellofatime", Color.Green, null, DescriptorType.Word, DescriptorRecognition.WholeWord));
            shtb.HighlightDescriptors.Add(new HighlightDescriptor("Helsinky", Color.Maroon, null, DescriptorType.Word, DescriptorRecognition.WholeWord));
            shtb.HighlightDescriptors.Add(new HighlightDescriptor("World", Color.Blue, null, DescriptorType.Word, DescriptorRecognition.WholeWord));
            shtb.HighlightDescriptors.Add(new HighlightDescriptor("/*", "*/", Color.Magenta, null, DescriptorType.ToCloseToken, DescriptorRecognition.StartsWith));
		}

		private void syntaxHighlightingTextBox1_TextChanged(object sender, System.EventArgs e)
		{
		
		}
	}
}
