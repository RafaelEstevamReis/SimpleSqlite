namespace Sample.CipherNotes
{
    partial class frmNote
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
            components = new System.ComponentModel.Container();
            rtfText = new System.Windows.Forms.RichTextBox();
            tmrSalvar = new System.Windows.Forms.Timer(components);
            lblSaving = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // rtfText
            // 
            rtfText.Dock = System.Windows.Forms.DockStyle.Fill;
            rtfText.Location = new System.Drawing.Point(0, 0);
            rtfText.Name = "rtfText";
            rtfText.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            rtfText.ShortcutsEnabled = false;
            rtfText.Size = new System.Drawing.Size(234, 136);
            rtfText.TabIndex = 0;
            rtfText.Text = "";
            rtfText.TextChanged += rtfText_TextChanged;
            rtfText.KeyUp += rtfText_KeyUp;
            // 
            // tmrSalvar
            // 
            tmrSalvar.Interval = 2000;
            tmrSalvar.Tick += tmrSalvar_Tick;
            // 
            // lblSaving
            // 
            lblSaving.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            lblSaving.AutoSize = true;
            lblSaving.Location = new System.Drawing.Point(181, 121);
            lblSaving.Name = "lblSaving";
            lblSaving.Size = new System.Drawing.Size(51, 15);
            lblSaving.TabIndex = 1;
            lblSaving.Text = "Saving...";
            lblSaving.Visible = false;
            // 
            // frmNote
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(234, 136);
            Controls.Add(lblSaving);
            Controls.Add(rtfText);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(200, 100);
            Name = "frmNote";
            Text = "frmNote";
            FormClosing += frmNote_FormClosing;
            Load += frmNote_Load;
            Resize += frmNote_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RichTextBox rtfText;
        private System.Windows.Forms.Timer tmrSalvar;
        private System.Windows.Forms.Label lblSaving;
    }
}