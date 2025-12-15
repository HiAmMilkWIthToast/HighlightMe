using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HighlightMe.Models;
using HighlightMe.Services;

namespace HighlightMe.Views
{
    public partial class NoteEditorWindow : Window
    {
        private readonly FileNotesService _notesService;
        private readonly string _filePath;
        private string _selectedColor = "#FFEB3B";

        public NoteEditorWindow(FileNotesService notesService, string filePath, string fileName)
        {
            InitializeComponent();
            _notesService = notesService;
            _filePath = filePath;
            
            FileNameText.Text = fileName;
            
            // Load existing note if any
            var existingNote = _notesService.GetNote(filePath);
            if (existingNote != null)
            {
                NoteTextBox.Text = existingNote.Note;
                _selectedColor = existingNote.NoteColor;
                UpdateColorSelection();
            }
            else
            {
                DeleteButton.Visibility = Visibility.Collapsed;
            }
            
            NoteTextBox.Focus();
        }

        private void UpdateColorSelection()
        {
            // Reset all buttons
            YellowColor.BorderThickness = new Thickness(0);
            PinkColor.BorderThickness = new Thickness(0);
            GreenColor.BorderThickness = new Thickness(0);
            BlueColor.BorderThickness = new Thickness(0);
            PurpleColor.BorderThickness = new Thickness(0);
            
            // Highlight selected
            var selectedButton = _selectedColor switch
            {
                "#FFEB3B" => YellowColor,
                "#FF6B6B" => PinkColor,
                "#4CAF50" => GreenColor,
                "#2196F3" => BlueColor,
                "#9C27B0" => PurpleColor,
                _ => YellowColor
            };
            selectedButton.BorderThickness = new Thickness(2);
            selectedButton.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string color)
            {
                _selectedColor = color;
                UpdateColorSelection();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var noteText = NoteTextBox.Text.Trim();
            _notesService.SetNote(_filePath, noteText, _selectedColor);
            DialogResult = true;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Delete this note?",
                "Delete Note",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _notesService.RemoveNote(_filePath);
                DialogResult = true;
                Close();
            }
        }
    }
}
