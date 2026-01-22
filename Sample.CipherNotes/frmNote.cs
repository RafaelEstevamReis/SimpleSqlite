namespace Sample.CipherNotes;

using Simple.Sqlite;
using System;
using System.Drawing;
using System.Windows.Forms;

public partial class frmNote : Form
{
    internal ConnectionFactory db;
    public int NoteId { get; set; }

    public frmNote()
    {
        InitializeComponent();
    }

    private void frmNote_Load(object sender, EventArgs e)
    {
        tbNotes? note;
        using (var cnn = db.GetConnection())
        {
            note = cnn.Get<tbNotes>(NoteId);
            if (note == null)
            {
                Close();
                return;
            }
        }
        Text = note.Caption;
        rtfText.Rtf = note.Text;

        if (note.NoteSize_Width > 0) Width = note.NoteSize_Width;
        if (note.NoteSize_Height > 0) Height = note.NoteSize_Height;
    }

    private void frmNote_FormClosing(object sender, FormClosingEventArgs e)
    {
        save();
    }

    private void frmNote_Resize(object sender, EventArgs e)
    {
        tmrSalvar.Enabled = false;
        tmrSalvar.Enabled = true;
    }
    private void rtfText_TextChanged(object sender, EventArgs e)
    {

        tmrSalvar.Enabled = false;
        tmrSalvar.Enabled = true;
    }

    private void tmrSalvar_Tick(object sender, EventArgs e)
    {
        tmrSalvar.Enabled = false;
        save();
    }

    private void save()
    {
        lblSaving.Visible = true;
        tmrSalvar.Enabled = false;

        using var cnn = db.GetConnection();
        cnn.Insert(new tbNotes()
        {
            Id = NoteId,
            Caption = Text,
            Text = rtfText.Rtf ?? "",
            NoteSize_Width = Width,
            NoteSize_Height = Height,
        }, OnConflict.Replace);
        lblSaving.Visible = false;
    }

    private void rtfText_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Modifiers != Keys.Control) return;

        bool handled = true;
        switch (e.KeyCode)
        {
            case Keys.S:
                save();
                break;

            case Keys.B:
                //setStyle_bold();
                setFont(f => f.Bold, FontStyle.Bold);
                break;
            case Keys.U:
                setFont(f => f.Underline, FontStyle.Underline);
                break;
            case Keys.I:
                setFont(f => f.Italic, FontStyle.Italic);
                break;

            default:
                handled = false;
                break;
        }

        if (handled)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

    }

    private void setFont(Func<Font, bool> check, FontStyle newStyle)
        => setFont(check, newStyle,  FontStyle.Regular);
    private void setFont(Func<Font, bool> check, FontStyle newStyle, FontStyle reset)
    {
        var sFont = rtfText.SelectionFont ?? Font;

        FontStyle style = check(sFont) ? reset : newStyle;

        rtfText.SelectionFont = new Font( sFont, style);
    }
}
