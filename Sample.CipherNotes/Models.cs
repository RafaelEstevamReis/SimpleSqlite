namespace Sample.CipherNotes;

using Simple.DatabaseWrapper.Attributes;

public record tbNotes
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Caption { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public int NoteSize_Width { get; set; }
    public int NoteSize_Height { get; set; }
}
