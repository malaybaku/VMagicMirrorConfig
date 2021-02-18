namespace Baku.VMagicMirrorConfig
{
    class GamepadSettingModel : SettingModelBase<GamepadSetting>
    {
        static class LeanModeNames
        {
            public const string GamepadLeanNone = nameof(GamepadLeanNone);
            public const string GamepadLeanLeftStick = nameof(GamepadLeanLeftStick);
            public const string GamepadLeanRightStick = nameof(GamepadLeanRightStick);
            public const string GamepadLeanLeftButtons = nameof(GamepadLeanLeftButtons);
        }

        public GamepadSettingModel(IMessageSender sender) : base(sender)
        {
            var s = GamepadSetting.Default;
            var factory = MessageFactory.Instance;


            GamepadEnabled = new RPropertyMin<bool>(s.GamepadEnabled, b =>
            {
                SendMessage(factory.EnableGamepad(b));
                if (!b && GamepadVisibility != null)
                {
                    //読み込み無効なら表示する価値は無いであろう、と判断
                    GamepadVisibility.Value = false;
                }
            });
            
            PreferDirectInputGamepad = new RPropertyMin<bool>(s.PreferDirectInputGamepad, b => SendMessage(factory.PreferDirectInputGamepad(b)));
            GamepadVisibility = new RPropertyMin<bool>(s.GamepadVisibility, b => SendMessage(factory.GamepadVisibility(b)));

            //排他になるように制御
            //TODO: RadioButtonの要請により、「一瞬たりとてフラグが2つ同時に立つのは許さん」みたいな要件もありうるので試しておくこと。
            GamepadLeanNone = new RPropertyMin<bool>(s.GamepadLeanNone, b =>
            {
                if (b)
                {
                    SendMessage(factory.GamepadLeanMode(LeanModeNames.GamepadLeanNone));
                    GamepadLeanLeftStick.Value = false;
                    GamepadLeanRightStick.Value = false;
                    GamepadLeanLeftButtons.Value = false;
                }
            });
            GamepadLeanLeftStick = new RPropertyMin<bool>(s.GamepadLeanLeftStick, b =>
            {
                if (b)
                {
                    SendMessage(factory.GamepadLeanMode(LeanModeNames.GamepadLeanLeftStick));
                    GamepadLeanNone.Value = false;
                    GamepadLeanRightStick.Value = false;
                    GamepadLeanLeftButtons.Value = false;
                }
            });
            GamepadLeanRightStick = new RPropertyMin<bool>(s.GamepadLeanRightStick, b =>
            {
                if (b)
                {
                    SendMessage(factory.GamepadLeanMode(LeanModeNames.GamepadLeanRightStick));
                    GamepadLeanNone.Value = false;
                    GamepadLeanLeftStick.Value = false;
                    GamepadLeanLeftButtons.Value = false;
                }
            });
            GamepadLeanLeftButtons = new RPropertyMin<bool>(s.GamepadLeanLeftButtons, b =>
            {
                if (b)
                {
                    SendMessage(factory.GamepadLeanMode(LeanModeNames.GamepadLeanLeftButtons));
                    GamepadLeanNone.Value = false;
                    GamepadLeanLeftStick.Value = false;
                    GamepadLeanRightStick.Value = false;
                }
            });

            GamepadLeanReverseHorizontal = new RPropertyMin<bool>(
                s.GamepadLeanReverseHorizontal, b => SendMessage(factory.GamepadLeanReverseHorizontal(b))
                );
            GamepadLeanReverseVertical = new RPropertyMin<bool>(s.GamepadLeanReverseVertical, b => SendMessage(factory.GamepadLeanReverseVertical(b)));
        }

        public RPropertyMin<bool> GamepadEnabled { get; } 
        public RPropertyMin<bool> PreferDirectInputGamepad { get; } 
        public RPropertyMin<bool> GamepadVisibility { get; } 

        //NOTE: 本来ならEnum値1つで管理する方がよいが、TwoWayバインディングが簡便になるのでbool4つで代用していた経緯があり、こういう持ち方。

        //モデル層では「1つの値がtrueになるとき他を全部falseにする」という措置を行わないといけないため、RPropertyを使わずに捌く
        public RPropertyMin<bool> GamepadLeanNone { get; } 
        public RPropertyMin<bool> GamepadLeanLeftButtons { get; } 
        public RPropertyMin<bool> GamepadLeanLeftStick { get; } 
        public RPropertyMin<bool> GamepadLeanRightStick { get; } 

        public RPropertyMin<bool> GamepadLeanReverseHorizontal { get; } 
        public RPropertyMin<bool> GamepadLeanReverseVertical { get; }

        public override void ResetToDefault() => Load(GamepadSetting.Default);
    }
}
