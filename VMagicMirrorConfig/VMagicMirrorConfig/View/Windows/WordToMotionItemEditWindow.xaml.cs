using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    public partial class WordToMotionItemEditWindow : Window
    {
        public WordToMotionItemEditWindow()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
