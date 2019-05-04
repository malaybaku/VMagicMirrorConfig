namespace Baku.VMagicMirrorConfig
{
    public class GamepadSettingViewModel : SettingViewModelBase
    {
        public GamepadSettingViewModel() : base() { }

        internal GamepadSettingViewModel(IMessageSender sender) : base(sender)
        {
        }

        #region Properties

        private bool _gamepadEnabled = true;
        public bool GamepadEnabled
        {
            get => _gamepadEnabled;
            set
            {
                if (SetValue(ref _gamepadEnabled, value))
                {
                    SendMessage(MessageFactory.Instance.EnableGamepad(GamepadEnabled));
                    if (!value)
                    {
                        GamepadVisibility = false;
                    }
                }
            }
        }

        private int _height = 90;
        /// <summary> Unit: [cm] </summary>
        public int GamepadHeight
        {
            get => _height;
            set
            {
                if (SetValue(ref _height, value))
                {
                    SendMessage(MessageFactory.Instance.GamepadHeight(GamepadHeight));
                }
            }
        }

        private int _horizontalScale = 100;
        /// <summary> Unit: [%] </summary>
        public int GamepadHorizontalScale
        {
            get => _horizontalScale;
            set
            {
                if (SetValue(ref _horizontalScale, value))
                {
                    SendMessage(MessageFactory.Instance.GamepadHorizontalScale(GamepadHorizontalScale));
                }
            }
        }

        private bool _visibility = false;
        public bool GamepadVisibility
        {
            get => _visibility;
            set
            {
                if (SetValue(ref _visibility, value))
                {
                    SendMessage(MessageFactory.Instance.GamepadVisibility(GamepadVisibility));
                }
            }
        }

        //NOTE: 以下、本来ならEnum値1つで管理する方がよいが、TwoWayバインディングが簡便になるのでbool4つで代用。

        private bool _gamepadLeanNone = false;
        public bool GamepadLeanNone
        {
            get => _gamepadLeanNone;
            set
            {
                if (SetValue(ref _gamepadLeanNone, value) && value)
                {
                    SendMessage(MessageFactory.Instance.GamepadLeanMode(nameof(GamepadLeanNone)));
                    GamepadLeanLeftButtons = false;
                    GamepadLeanLeftStick = false;
                    GamepadLeanRightStick = false;
                }
            }
        }

        private bool _gamepadLeanLeftButtons = false;
        public bool GamepadLeanLeftButtons
        {
            get => _gamepadLeanLeftButtons;
            set
            {
                if (SetValue(ref _gamepadLeanLeftButtons, value) && value)
                {
                    SendMessage(MessageFactory.Instance.GamepadLeanMode(nameof(GamepadLeanLeftButtons)));
                    GamepadLeanNone = false;
                    GamepadLeanLeftStick = false;
                    GamepadLeanRightStick = false;
                }
            }
        }

        private bool _gamepadLeanLeftStick = true;
        public bool GamepadLeanLeftStick
        {
            get => _gamepadLeanLeftStick;
            set
            {
                if (SetValue(ref _gamepadLeanLeftStick, value) && value)
                {
                    SendMessage(MessageFactory.Instance.GamepadLeanMode(nameof(GamepadLeanLeftStick)));
                    GamepadLeanNone = false;
                    GamepadLeanLeftButtons = false;
                    GamepadLeanRightStick = false;
                }
            }
        }

        private bool _gamepadLeanRightStick = false;
        public bool GamepadLeanRightStick
        {
            get => _gamepadLeanRightStick;
            set
            {
                if (SetValue(ref _gamepadLeanRightStick, value) && value)
                {
                    SendMessage(MessageFactory.Instance.GamepadLeanMode(nameof(GamepadLeanRightStick)));
                    GamepadLeanNone = false;
                    GamepadLeanLeftButtons = false;
                    GamepadLeanLeftStick = false;
                }
            }
        }

        private bool _gamepadLeanReverseHorizontal;
        public bool GamepadLeanReverseHorizontal
        {
            get => _gamepadLeanReverseHorizontal;
            set
            {
                if (SetValue(ref _gamepadLeanReverseHorizontal, value))
                {
                    SendMessage(MessageFactory.Instance.GamepadLeanReverseHorizontal(GamepadLeanReverseHorizontal));
                }
            }
        }

        private bool _gamepadLeanReverseVertical;
        public bool GamepadLeanReverseVertical
        {
            get => _gamepadLeanReverseVertical;
            set
            {
                if (SetValue(ref _gamepadLeanReverseVertical, value))
                {
                    SendMessage(MessageFactory.Instance.GamepadLeanReverseVertical(GamepadLeanReverseVertical));
                }
            }
        }

        #endregion

        public override void ResetToDefault()
        {
            GamepadEnabled = true;

            GamepadHeight = 90;
            GamepadHorizontalScale = 100;
            GamepadVisibility = false;

            GamepadLeanNone = false;
            GamepadLeanLeftButtons = false;
            GamepadLeanLeftStick = true;
            GamepadLeanRightStick = false;

            GamepadLeanReverseHorizontal = false;
            GamepadLeanReverseVertical = false;
        }
    }
}
