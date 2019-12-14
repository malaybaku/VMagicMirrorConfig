namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// ダイアログを表示してくれるすごいやつだよ
    /// </summary>
    public class DialogHelperViewModel : ViewModelBase
    {
        private DialogHelperViewModel() { }
        private static DialogHelperViewModel? _instance;
        public static DialogHelperViewModel Instance
            => _instance ??= new DialogHelperViewModel();

        private string _title = "";
        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value);
        }

        private string _content = "";
        public string Content
        {
            get => _content;
            set => SetValue(ref _content, value);
        }

        private bool _isOpen = false;
        public bool IsOpen
        {
            get => _isOpen;
            set => SetValue(ref _isOpen, value);
        }

        private bool _canCancel = false;
        public bool CanCancel
        {
            get => _canCancel;
            set => SetValue(ref _canCancel, value);
        }

        private ActionCommand? _okCommand;
        public ActionCommand OKCommand
            => _okCommand ??= new ActionCommand(OkAndCloseDialog);
        private void OkAndCloseDialog()
        {
            MessageBoxWrapper.Instance.SetDialogResult(true);
            IsOpen = false;
        }
 
        private ActionCommand? _cancelCommand;
        public ActionCommand CancelCommand
            => _cancelCommand ??= new ActionCommand(CancelAndCloseDialog);
        private void CancelAndCloseDialog()
        {
            MessageBoxWrapper.Instance.SetDialogResult(false);
            IsOpen = false;
        }
    }
}
