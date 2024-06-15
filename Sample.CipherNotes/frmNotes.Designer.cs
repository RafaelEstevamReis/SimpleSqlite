namespace Sample.CipherNotes
{
    partial class frmNotes
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNotes));
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            txtCaption = new System.Windows.Forms.ToolStripTextBox();
            btnAdd = new System.Windows.Forms.ToolStripButton();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { txtCaption, btnAdd });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(800, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // txtCaption
            // 
            txtCaption.Name = "txtCaption";
            txtCaption.Size = new System.Drawing.Size(100, 25);
            // 
            // btnAdd
            // 
            btnAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnAdd.Image = (System.Drawing.Image)resources.GetObject("btnAdd.Image");
            btnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(23, 22);
            btnAdd.Text = "toolStripButton1";
            btnAdd.Click += btnAdd_Click;
            // 
            // frmNotes
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(toolStrip1);
            IsMdiContainer = true;
            Name = "frmNotes";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Cipher Notes";
            Load += frmNotes_Load;
            Shown += frmNotes_Shown;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripTextBox txtCaption;
        private System.Windows.Forms.ToolStripButton btnAdd;
    }
}
