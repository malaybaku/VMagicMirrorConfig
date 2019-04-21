using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    using static LineParseUtils;

    public class GamepadSettingViewModel : SettingViewModelBase
    {

        internal GamepadSettingViewModel(IMessageSender sender, StartupSettingViewModel startup) : base(sender, startup)
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

        private int _height = 100;
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

        private bool _visibility = true;
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

        protected override void ResetToDefault()
        {
            GamepadEnabled = true;

            GamepadHeight = 100;
            GamepadHorizontalScale = 100;
            GamepadVisibility = true;

            GamepadLeanNone = false;
            GamepadLeanLeftButtons = false;
            GamepadLeanLeftStick = true;
            GamepadLeanRightStick = false;

            GamepadLeanReverseHorizontal = false;
            GamepadLeanReverseVertical = false;
        }

        protected override void SaveSetting()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Save Gamepad Setting File",
                Filter = "VMagicMirror Gamepad File(*.vmm_gamepad)|*.vmm_gamepad",
                DefaultExt = ".vmm_gamepad",
                AddExtension = true,
            };
            if (dialog.ShowDialog() == true)
            {
                SaveSetting(dialog.FileName);
            }
        }

        protected override void LoadSetting()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Open Gamepad Setting File",
                Filter = "VMagicMirror Gamepad File(*.vmm_gamepad)|*.vmm_gamepad",
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                LoadSetting(dialog.FileName);
            }
        }

        internal override void SaveSetting(string path)
        {
            File.WriteAllLines(path, GetLinesToSave().ToArray());
        }

        internal override void LoadSetting(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                var lines = File.ReadAllLines(path);
                ParseLines(lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load gamepad setting: " + ex.Message);
            }

        }

        internal IEnumerable<string> GetLinesToSave()
        {
            return new string[]
            {
                $"{nameof(GamepadEnabled)}:{GamepadEnabled}",

                $"{nameof(GamepadHeight)}:{GamepadHeight}",
                $"{nameof(GamepadHorizontalScale)}:{GamepadHorizontalScale}",
                $"{nameof(GamepadVisibility)}:{GamepadVisibility}",

                $"{nameof(GamepadLeanNone)}:{GamepadLeanNone}",
                $"{nameof(GamepadLeanLeftButtons)}:{GamepadLeanLeftButtons}",
                $"{nameof(GamepadLeanLeftStick)}:{GamepadLeanLeftStick}",
                $"{nameof(GamepadLeanRightStick)}:{GamepadLeanRightStick}",

                $"{nameof(GamepadLeanReverseHorizontal)}:{GamepadLeanReverseHorizontal}",
                $"{nameof(GamepadLeanReverseVertical)}:{GamepadLeanReverseVertical}",
            };
        }

        internal void ParseLines(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                var _ =
                    TryReadBoolParam(line, nameof(GamepadEnabled), v => GamepadEnabled = v) ||

                    TryReadIntParam(line, nameof(GamepadHeight), v => GamepadHeight = v) ||
                    TryReadIntParam(line, nameof(GamepadHorizontalScale), v => GamepadHorizontalScale = v) ||
                    TryReadBoolParam(line, nameof(GamepadVisibility), v => GamepadVisibility = v) ||

                    TryReadBoolParam(line, nameof(GamepadLeanNone), v => GamepadLeanNone = v) ||
                    TryReadBoolParam(line, nameof(GamepadLeanLeftButtons), v => GamepadLeanLeftButtons = v) ||
                    TryReadBoolParam(line, nameof(GamepadLeanLeftStick), v => GamepadLeanLeftStick = v) ||
                    TryReadBoolParam(line, nameof(GamepadLeanRightStick), v => GamepadLeanRightStick = v) ||

                    TryReadBoolParam(line, nameof(GamepadLeanReverseHorizontal), v => GamepadLeanReverseHorizontal = v) ||
                    TryReadBoolParam(line, nameof(GamepadLeanReverseVertical), v => GamepadLeanReverseVertical = v);
            }
        }
    }
}
