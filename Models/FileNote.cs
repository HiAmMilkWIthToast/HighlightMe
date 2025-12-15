using System;

namespace HighlightMe.Models
{
    public class FileNote
    {
        public string FilePath { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
        public string NoteColor { get; set; } = "#FFEB3B"; // Yellow sticky note
    }
}
