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

        internal GamepadSettingViewModel(UdpSender sender, StartupSettingViewModel startup) : base(sender, startup)
        {
        }

        #region Properties

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

        private bool _leanNone = true;
        public bool GamepadLeanNone
        {
            get => _leanNone;
            set
            {
                if (SetValue(ref _leanNone, value) && value)
                {
                    SendMessage(MessageFactory.Instance.LeanMode(nameof(GamepadLeanNone)));
                    GamepadLeanLeftButtons = false;
                    GamepadLeanLeftStick = false;
                    GamepadLeanRightStick = false;
                }
            }
        }

        private bool _leanLeftButtons = false;
        public bool GamepadLeanLeftButtons
        {
            get => _leanLeftButtons;
            set
            {
                if (SetValue(ref _leanLeftButtons, value) && value)
                {
                    SendMessage(MessageFactory.Instance.LeanMode(nameof(GamepadLeanLeftButtons)));
                    GamepadLeanNone = false;
                    GamepadLeanLeftStick = false;
                    GamepadLeanRightStick = false;
                }
            }
        }

        private bool _leanLeftStick = false;
        public bool GamepadLeanLeftStick
        {
            get => _leanLeftStick;
            set
            {
                if (SetValue(ref _leanLeftStick, value) && value)
                {
                    SendMessage(MessageFactory.Instance.LeanMode(nameof(GamepadLeanLeftStick)));
                    GamepadLeanNone = false;
                    GamepadLeanLeftButtons = false;
                    GamepadLeanRightStick = false;
                }
            }
        }

        private bool _leanRightStick = false;
        public bool GamepadLeanRightStick
        {
            get => _leanRightStick; set
            {
                if (SetValue(ref _leanRightStick, value) && value)
                {
                    SendMessage(MessageFactory.Instance.LeanMode(nameof(GamepadLeanRightStick)));
                    GamepadLeanNone = false;
                    GamepadLeanLeftButtons = false;
                    GamepadLeanLeftStick = false;
                }
            }
        }

        private bool _leanReverseHorizontal;
        public bool GamepadLeanReverseHorizontal
        {
            get => _leanReverseHorizontal;
            set
            {
                if (SetValue(ref _leanReverseHorizontal, value))
                {
                    SendMessage(MessageFactory.Instance.LeanReverseHorizontal(GamepadLeanReverseHorizontal));
                }
            }
        }

        private bool _leanReverseVertical;
        public bool GamepadLeanReverseVertical
        {
            get => _leanReverseVertical;
            set
            {
                if (SetValue(ref _leanReverseVertical, value))
                {
                    SendMessage(MessageFactory.Instance.LeanReverseVertical(GamepadLeanReverseVertical));
                }
            }
        }

        #endregion

        protected override void ResetToDefault()
        {
            GamepadHeight = 100;
            GamepadHorizontalScale = 100;
            GamepadVisibility = true;

            GamepadLeanNone = true;
            GamepadLeanLeftButtons = false;
            GamepadLeanLeftStick = false;
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
