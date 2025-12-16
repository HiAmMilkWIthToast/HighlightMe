using System.Windows;

namespace HighlightMe.Views
{
    public partial class HelpGuideWindow : Window
    {
        public HelpGuideWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
