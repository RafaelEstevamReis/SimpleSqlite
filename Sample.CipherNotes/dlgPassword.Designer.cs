namespace Sample.CipherNotes;

using System.Drawing;
using System.Windows.Forms;

partial class dlgPassword
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
        txtPassword = new TextBox();
        btnUnlock = new Button();
        SuspendLayout();
        // 
        // txtPassword
        // 
        txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtPassword.Location = new Point(12, 12);
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new Size(308, 23);
        txtPassword.TabIndex = 0;
        txtPassword.KeyPress += txtPassword_KeyPress;
        // 
        // btnUnlock
        // 
        btnUnlock.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnUnlock.DialogResult = DialogResult.OK;
        btnUnlock.Location = new Point(245, 45);
        btnUnlock.Name = "btnUnlock";
        btnUnlock.Size = new Size(75, 23);
        btnUnlock.TabIndex = 2;
        btnUnlock.Text = "Unlock";
        btnUnlock.UseVisualStyleBackColor = true;
        // 
        // dlgPassword
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(329, 75);
        Controls.Add(btnUnlock);
        Controls.Add(txtPassword);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Name = "dlgPassword";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Database Password";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TextBox txtPassword;
    private Button btnUnlock;
}