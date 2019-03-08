using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    public class MainWindowViewModel : ViewModelBase, IWindowViewModel
    {
        internal ModelInitializer Initializer { get; } = new ModelInitializer();
        internal UdpSender UdpSender => Initializer.UdpSender;
        internal InputChecker InputChecker => Initializer.InputChecker;

        //とりあえず設定ファイルとか無視して常にGBでスタートしましょう
        private ColorSelectViewModel _colorSelection { get; } = new ColorSelectViewModel(0, 255, 0);

        private ActionCommand _loadVrmCommand;
        public ActionCommand LoadVrmCommand
            => _loadVrmCommand ?? (_loadVrmCommand = new ActionCommand(LoadVrm));

        private ActionCommand _selectBGColorCommand;
        public ActionCommand SelectBGColorCommand
            => _selectBGColorCommand ?? (_selectBGColorCommand = new ActionCommand(SelectBGColor));

        private ActionCommand _openAboutWindowCommand;
        public ActionCommand OpenAboutWindowCommand
            => _openAboutWindowCommand ?? (_openAboutWindowCommand = new ActionCommand(OpenAboutWindow));


        private void LoadVrm()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Open VRM file",
                Filter = "VRM files (*.vrm)|*.vrm|All files (*.*)|*.*",
                Multiselect = false,
            };

            if (!(
                dialog.ShowDialog() == true && 
                File.Exists(dialog.FileName)
                ))
            {
                return;
            }

            UdpSender.SendMessage(UdpMessageFactory.Instance.OpenVrmPreview(dialog.FileName));

            var res = MessageBox.Show(
                "ビューアー画面のライセンスを確認してください。読み込みますか？", 
                "VRMの読み込み", 
                MessageBoxButton.OKCancel
                );

            if (res == MessageBoxResult.OK)
            {
                UdpSender.SendMessage(UdpMessageFactory.Instance.OpenVrm(dialog.FileName));
            }
        }

        private void SelectBGColor()
        {
            var context = new ColorSelectViewModel(_colorSelection.R, _colorSelection.G, _colorSelection.B);

            if (new ColorPickerWindow() { DataContext = context }.ShowDialog() != true)
            {
                return;
            }

            _colorSelection.R = context.R;
            _colorSelection.G = context.G;
            _colorSelection.B = context.B;
            UdpSender.SendMessage(
                UdpMessageFactory.Instance.UpdateChromakey(255, _colorSelection.R, _colorSelection.G, _colorSelection.B)
                );
        }

        private void OpenAboutWindow()
            => new AboutWindow().ShowDialog();

        public void Initialize()
        {
            if (Application.Current.MainWindow != null &&
                !DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
            {
                Initializer.Initialize();
            }
        }

        public void Dispose()
        {
            Initializer.Dispose();
        }
    }

    public interface IWindowViewModel : IDisposable
    {
        void Initialize();
    }
}
