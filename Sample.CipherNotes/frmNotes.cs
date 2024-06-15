namespace Sample.CipherNotes;

using Microsoft.Data.Sqlite;
using Simple.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

public partial class frmNotes : Form
{
    private ConnectionFactory db;

    public frmNotes()
    {
        InitializeComponent();
    }

    private void frmNotes_Load(object sender, System.EventArgs e)
    {
        if (dlgPassword.ShowDialog(out string pwd) != DialogResult.OK)
        {
            Close();
            return;
        }

        db = ConnectionFactory.FromFile("notes.db", pwd);

        try
        {
            using var cnn = db.GetConnection();
            cnn.CreateTables()
               .Add<tbNotes>()
               .Commit();
        }
        catch (SqliteException ex)
        {
            MessageBox.Show("Unable to Open the database, possibly the passord is incorrect.\nOriginal Message: " + ex.Message);

            Close();
            return;
        }
    }

    private void frmNotes_Shown(object sender, System.EventArgs e)
    {
        tbNotes[] all;
        using (var cnn = db.GetConnection())
        {
            all = cnn.GetAll<tbNotes>().ToArray();
        }

        foreach(var n  in all)
        {
            showNote(n.Id);
        }
    }

    private void btnAdd_Click(object sender, System.EventArgs e)
    {
        string caption = txtCaption.Text;
        txtCaption.Text = "";
        if (string.IsNullOrWhiteSpace(caption))
        {
            txtCaption.Focus();
            return;
        }
        int noteId;
        using (var cnn = db.GetConnection())
        {
            noteId = (int)cnn.Insert(new tbNotes
            {
                Id = 0,
                Caption = caption,
            });
        }
        showNote(noteId);
    }
    void showNote(int noteId)
    {
        var newNote = new frmNote
        {
            db = db,
            NoteId = noteId,
            MdiParent = this
        };
        newNote.Show();
    }

}
