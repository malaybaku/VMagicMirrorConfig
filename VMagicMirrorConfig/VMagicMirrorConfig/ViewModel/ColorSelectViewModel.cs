using System.Windows.Media;

namespace Baku.VMagicMirrorConfig
{
    public class ColorSelectViewModel : ViewModelBase
    {
        public ColorSelectViewModel(int r, int g, int b)
        {
            _r = r;
            _g = g;
            _b = b;
            UpdateColor();
        }

        private int _r;
        public int R
        {
            get => _r;
            set
            {
                if (SetValue(ref _r, value))
                {
                    UpdateColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        private int _g;
        public int G
        {
            get => _g;
            set
            {
                if (SetValue(ref _g, value))
                {
                    UpdateColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        private int _b;
        public int B
        {
            get => _b;
            set
            {
                if (SetValue(ref _b, value))
                {
                    UpdateColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        public Color Color { get; private set; } 
   
        private void UpdateColor()
        {
            Color = Color.FromArgb(255, (byte)R, (byte)G, (byte)B);
        }
    }
}
