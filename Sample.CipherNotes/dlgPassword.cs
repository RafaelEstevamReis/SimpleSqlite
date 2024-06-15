namespace Sample.CipherNotes;

using System.Windows.Forms;


public partial class dlgPassword : Form
{
    public dlgPassword()
    {
        InitializeComponent();
    }

    private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == 13)
        {
            e.Handled = true;
            DialogResult = DialogResult.OK;
        }
    }

    public static DialogResult ShowDialog(out string password)
    {
        password = string.Empty;

        using var dlg = new dlgPassword();
        var result = dlg.ShowDialog();
        password = dlg.txtPassword.Text;

        return result;
    }

}
