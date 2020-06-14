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

        public double IconWidth
        {
            get => (double)GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }

        public double IconHeight
        {
            get => (double)GetValue(IconHeightProperty);
            set => SetValue(IconHeightProperty, value);
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

        public static readonly DependencyProperty IconWidthProperty
            = DependencyProperty.Register(
                nameof(IconWidth),
                typeof(double),
                typeof(TabHeaderIconText),
                new PropertyMetadata(
                    22.0,
                    OnIconWidthChanged
                    )
                );

        public static readonly DependencyProperty IconHeightProperty
            = DependencyProperty.Register(
                nameof(IconHeight),
                typeof(double),
                typeof(TabHeaderIconText),
                new PropertyMetadata(
                    22.0,
                    OnIconHeightChanged
                    )
                );

        private static void OnIconWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabHeaderIconText control)
            {
                control.packIcon.Width = (double)e.NewValue;
            }
        }

        private static void OnIconHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabHeaderIconText control)
            {
                control.packIcon.Height = (double)e.NewValue;
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabHeaderIconText control)
            {
                control.textBlock.Text = (string)e.NewValue;
            }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabHeaderIconText control)
            {
                control.packIcon.Kind = (PackIconKind)e.NewValue;
            }
        }
    }
}
