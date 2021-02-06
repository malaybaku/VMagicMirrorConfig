using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    public class GamepadSetting
    {
        public bool GamepadEnabled { get; set; } = true;
        public bool PreferDirectInputGamepad { get; set; } = false;
        public bool GamepadVisibility { get; set; } = false;

        //NOTE: 本来ならEnum値1つで管理する方がよいが、TwoWayバインディングが簡便になるのでbool4つで代用…していた経緯のためこういう持ち方。
        public bool GamepadLeanNone { get; set; } = false;
        public bool GamepadLeanLeftButtons { get; set; } = false;
        public bool GamepadLeanLeftStick { get; set; } = true;
        public bool GamepadLeanRightStick { get; set; } = false;

        public bool GamepadLeanReverseHorizontal { get; set; } = false;
        public bool GamepadLeanReverseVertical { get; set; } = false;

        public void ResetToDefault()
        {
            GamepadEnabled = true;
            PreferDirectInputGamepad = false;
            //NOTE: Visibilityは別のとこでいじるため、ここでは不要

            GamepadLeanNone = false;
            GamepadLeanLeftButtons = false;
            GamepadLeanLeftStick = true;
            GamepadLeanRightStick = false;

            GamepadLeanReverseHorizontal = false;
            GamepadLeanReverseVertical = false;
        }
    }
}
