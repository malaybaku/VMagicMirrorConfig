﻿namespace Baku.VMagicMirrorConfig
{
    public class MotionSetting : SettingEntityBase
    {
        public const int KeyboardMouseMotionNone = -1;
        public const int KeyboardMouseMotionDefault = 0;
        public const int KeyboardMouseMotionPresentation = 1;
        public const int KeyboardMouseMotionPenTablet = 2;


        /// <summary>
        /// NOTE: 規約としてこの値は書き換えません。
        /// デフォルト値を参照したい人が、プロパティ読み込みのみの為だけに使います。
        /// </summary>
        public static MotionSetting Default { get; } = new MotionSetting();

        #region Full Body 

        public bool EnableNoHandTrackMode { get; set; } = false;

        #endregion

        #region Face

        public bool EnableFaceTracking { get; set; } = true;

        public bool AutoBlinkDuringFaceTracking { get; set; } = true;

        public bool EnableBodyLeanZ { get; set; } = false;

        public bool EnableBlinkAdjust { get; set; } = true;

        public bool EnableVoiceBasedMotion { get; set; } = true;

        public bool DisableFaceTrackingHorizontalFlip { get; set; } = false;

        public bool EnableWebCamHighPowerMode { get; set; } = false;
        public bool EnableImageBasedHandTracking { get; set; } = false;
        public bool ShowEffectDuringHandTracking { get; set; } = false;
        public bool DisableHandTrackingHorizontalFlip { get; set; } = false;

        public string CameraDeviceName { get; set; } = "";

        /// <summary>
        /// NOTE: この値はUIに出す必要はないが、起動時に空でなければ送り、Unityからデータが来たら受け取り、終了時にはセーブする。
        /// </summary>
        public string CalibrateFaceData { get; set; } = "";

        public int FaceDefaultFun { get; set; } = 0;

        public string FaceNeutralClip { get; set; } = "";
        public string FaceOffsetClip { get; set; } = "";

        #endregion

        #region Eye

        public bool UseLookAtPointNone { get; set; } = false;
        public bool UseLookAtPointMousePointer { get; set; } = true;
        public bool UseLookAtPointMainCamera { get; set; } = false;

        public int EyeBoneRotationScale { get; set; } = 100;

        #endregion

        #region Mouth

        public bool EnableLipSync { get; set; } = true;

        public string LipSyncMicrophoneDeviceName { get; set; } = "";

        //NOTE: dB単位なので0がデフォルト。対数ベースのほうがレンジ取りやすい
        public int MicrophoneSensitivity { get; set; } = 0;

        #endregion

        #region Arm

        public int KeyboardAndMouseMotionMode { get; set; } = 0;
        public int GamepadMotionMode { get; set; } = 0;

        public bool EnableHidRandomTyping { get; set; } = false;
        public bool EnableShoulderMotionModify { get; set; } = true;
        public bool EnableHandDownTimeout { get; set; } = true;

        public int WaistWidth { get; set; } = 30;
        public int ElbowCloseStrength { get; set; } = 30;
        public bool EnableFpsAssumedRightHand { get; set; } = false;

        public bool ShowPresentationPointer { get; set; } = false;
        public int PresentationArmRadiusMin { get; set; } = 20;

        #endregion

        #region Hand

        /// <summary> Unit: [cm] </summary>
        public int LengthFromWristToTip { get; set; } = 12;
        public int HandYOffsetBasic { get; set; } = 3;
        public int HandYOffsetAfterKeyDown { get; set; } = 2;

        #endregion

        #region Wait

        public bool EnableWaitMotion { get; set; } = true;
        public int WaitMotionScale { get; set; } = 125;
        public int WaitMotionPeriod { get; set; } = 10;

        #endregion

        #region Reset API

        public void ResetFaceBasicSetting()
        {
            EnableFaceTracking = true;
            CameraDeviceName = "";
            AutoBlinkDuringFaceTracking = true;
            EnableBodyLeanZ = false;

            EnableVoiceBasedMotion = true;
            DisableFaceTrackingHorizontalFlip = false;
            EnableImageBasedHandTracking = false;

            EnableLipSync = true;
            LipSyncMicrophoneDeviceName = "";
            MicrophoneSensitivity = 0;
        }

        public void ResetFaceEyeSetting()
        {
            EnableBlinkAdjust = true;
            UseLookAtPointNone = false;
            UseLookAtPointMousePointer = true;
            UseLookAtPointMainCamera = false;
            EyeBoneRotationScale = 100;
        }

        public void ResetFaceBlendShapeSetting()
        {
            FaceDefaultFun = 0;
            FaceNeutralClip = "";
            FaceOffsetClip = "";
        }

        public void ResetArmSetting()
        {
            KeyboardAndMouseMotionMode = 0;
            GamepadMotionMode = 0;

            EnableHidRandomTyping = false;
            EnableShoulderMotionModify = true;
            EnableHandDownTimeout = true;
            WaistWidth = 30;
            ElbowCloseStrength = 30;
            EnableFpsAssumedRightHand = false;
            ShowPresentationPointer = false;
            PresentationArmRadiusMin = 20;
        }

        public void ResetHandSetting()
        {
            LengthFromWristToTip = 12;
            HandYOffsetBasic = 3;
            HandYOffsetAfterKeyDown = 2;
        }

        public void ResetWaitMotionSetting()
        {
            EnableWaitMotion = true;
            WaitMotionScale = 125;
            WaitMotionPeriod = 10;
        }

        public void ResetToDefault()
        {
            EnableNoHandTrackMode = false;
            ResetFaceBasicSetting();
            ResetFaceEyeSetting();
            ResetFaceBlendShapeSetting();
            ResetArmSetting();
            ResetHandSetting();
            ResetWaitMotionSetting();
        }

        #endregion
    }
}
