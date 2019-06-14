using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace Baku.VMagicMirrorConfig
{
    public partial class TabHeaderIconText : UserControl
    {
        public TabHeaderIconText()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public PackIconKind IconKind
        {
            get => (PackIconKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(TabHeaderIconText),
                new PropertyMetadata(
                    "",
                    OnTextChanged
                    )
                );

        public static readonly DependencyProperty IconKindProperty
            = DependencyProperty.Register(
                nameof(IconKind),
                typeof(PackIconKind),
                typeof(TabHeaderIconText),
                new PropertyMetadata(
                    PackIconKind.Abc,
                    OnIconChanged
                    )
                );

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TabHeaderIconText;
            control.textBlock.Text = (string)e.NewValue;
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TabHeaderIconText;
            control.packIcon.Kind = (PackIconKind)e.NewValue;
        }
    }
}
