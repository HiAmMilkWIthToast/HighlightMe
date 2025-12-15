using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HighlightMe.Models;

namespace HighlightMe.Services
{
    public class FileNotesService
    {
        private readonly string _notesFilePath;
        private Dictionary<string, FileNote> _notes;

        public FileNotesService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "HighlightMe");
            Directory.CreateDirectory(appFolder);
            _notesFilePath = Path.Combine(appFolder, "file_notes.json");
            _notes = new Dictionary<string, FileNote>(StringComparer.OrdinalIgnoreCase);

            LoadNotes();
        }

        public void LoadNotes()
        {
            try
            {
                if (File.Exists(_notesFilePath))
                {
                    var json = File.ReadAllText(_notesFilePath);
                    var notesList = JsonSerializer.Deserialize<List<FileNote>>(json);
                    if (notesList != null)
                    {
                        _notes = notesList.ToDictionary(n => n.FilePath, n => n, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
                _notes = new Dictionary<string, FileNote>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public void SaveNotes()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_notes.Values.ToList(), options);
                File.WriteAllText(_notesFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        public FileNote? GetNote(string filePath)
        {
            return _notes.TryGetValue(filePath, out var note) ? note : null;
        }

        public bool HasNote(string filePath)
        {
            return _notes.ContainsKey(filePath) && !string.IsNullOrWhiteSpace(_notes[filePath].Note);
        }

        public void SetNote(string filePath, string noteText, string? color = null)
        {
            if (string.IsNullOrWhiteSpace(noteText))
            {
                // Remove note if empty
                RemoveNote(filePath);
                return;
            }

            if (_notes.TryGetValue(filePath, out var existing))
            {
                existing.Note = noteText;
                existing.ModifiedAt = DateTime.Now;
                if (color != null)
                    existing.NoteColor = color;
            }
            else
            {
                _notes[filePath] = new FileNote
                {
                    FilePath = filePath,
                    Note = noteText,
                    NoteColor = color ?? "#FFEB3B",
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                };
            }

            SaveNotes();
        }

        public void RemoveNote(string filePath)
        {
            if (_notes.Remove(filePath))
            {
                SaveNotes();
            }
        }

        public IEnumerable<FileNote> GetAllNotes()
        {
            return _notes.Values;
        }

        public int NoteCount => _notes.Count;
    }
}
