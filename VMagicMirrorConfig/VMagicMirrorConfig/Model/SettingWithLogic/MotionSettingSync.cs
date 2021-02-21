using Newtonsoft.Json;
using System;

namespace Baku.VMagicMirrorConfig
{
    class MotionSettingSync : SettingSyncBase<MotionSetting>
    {
        static class LookAtStyles
        {
            public const string UseLookAtPointNone = nameof(UseLookAtPointNone);
            public const string UseLookAtPointMousePointer = nameof(UseLookAtPointMousePointer);
            public const string UseLookAtpointMainCamera = nameof(UseLookAtpointMainCamera);
        }

        public MotionSettingSync(IMessageSender sender) : base(sender)
        {
            var factory = MessageFactory.Instance;
            var setting = MotionSetting.Default;

            //NOTE: 長大になってるのはプロパティの初期化仕様によるもの。半手動でテキスト変換して作ってます

            EnableNoHandTrackMode = new RPropertyMin<bool>(setting.EnableNoHandTrackMode, v => SendMessage(factory.EnableNoHandTrackMode(v)));

            EnableFaceTracking = new RPropertyMin<bool>(setting.EnableFaceTracking, v => SendMessage(factory.EnableFaceTracking(v)));
            AutoBlinkDuringFaceTracking = new RPropertyMin<bool>(setting.AutoBlinkDuringFaceTracking, v => SendMessage(factory.AutoBlinkDuringFaceTracking(v)));
            EnableBodyLeanZ = new RPropertyMin<bool>(setting.EnableBodyLeanZ, v => SendMessage(factory.EnableBodyLeanZ(v)));
            EnableBlinkAdjust = new RPropertyMin<bool>(setting.EnableBlinkAdjust, v =>
            {
                SendMessage(factory.EnableHeadRotationBasedBlinkAdjust(v));
                SendMessage(factory.EnableLipSyncBasedBlinkAdjust(v));
            });
            EnableVoiceBasedMotion = new RPropertyMin<bool>(setting.EnableVoiceBasedMotion, v => SendMessage(factory.EnableVoiceBasedMotion(v)));
            DisableFaceTrackingHorizontalFlip = new RPropertyMin<bool>(setting.DisableFaceTrackingHorizontalFlip, v => SendMessage(factory.DisableFaceTrackingHorizontalFlip(v)));

            EnableWebCamHighPowerMode = new RPropertyMin<bool>(setting.EnableWebCamHighPowerMode, v => SendMessage(factory.EnableWebCamHighPowerMode(v)));
            EnableImageBasedHandTracking = new RPropertyMin<bool>(setting.EnableImageBasedHandTracking, v => SendMessage(factory.EnableImageBasedHandTracking(v)));
            CameraDeviceName = new RPropertyMin<string>(setting.CameraDeviceName, v => SendMessage(factory.SetCameraDeviceName(v)));
            CalibrateFaceData = new RPropertyMin<string>(setting.CalibrateFaceData, v => SendMessage(factory.SetCalibrateFaceData(v)));

            FaceDefaultFun = new RPropertyMin<int>(setting.FaceDefaultFun, v => SendMessage(factory.FaceDefaultFun(v)));
            FaceNeutralClip = new RPropertyMin<string>(setting.FaceNeutralClip, v => SendMessage(factory.FaceNeutralClip(v)));
            FaceOffsetClip = new RPropertyMin<string>(setting.FaceOffsetClip, v => SendMessage(factory.FaceOffsetClip(v)));

            //TODO: 排他のタイミング次第でRadioButtonが使えなくなってしまうので要検証
            UseLookAtPointNone = new RPropertyMin<bool>(setting.UseLookAtPointNone, v =>
            {
                if (v)
                {
                    SendMessage(factory.LookAtStyle(LookAtStyles.UseLookAtPointNone));
                    UseLookAtPointMousePointer?.Set(false);
                    UseLookAtPointMainCamera?.Set(false);
                }
            });

            UseLookAtPointMousePointer = new RPropertyMin<bool>(setting.UseLookAtPointMousePointer, v =>
            {
                if (v)
                {
                    SendMessage(factory.LookAtStyle(LookAtStyles.UseLookAtPointMousePointer));
                    UseLookAtPointNone.Value = false;
                    UseLookAtPointMainCamera?.Set(false);
                }
            });

            UseLookAtPointMainCamera = new RPropertyMin<bool>(setting.UseLookAtPointMainCamera, v =>
            {
                if (v)
                {
                    SendMessage(factory.LookAtStyle(LookAtStyles.UseLookAtpointMainCamera));
                    UseLookAtPointNone.Value = false;
                    UseLookAtPointMousePointer.Value = false;
                }
            });

            EyeBoneRotationScale = new RPropertyMin<int>(setting.EyeBoneRotationScale, v => SendMessage(factory.SetEyeBoneRotationScale(v)));

            EnableLipSync = new RPropertyMin<bool>(setting.EnableLipSync, v => SendMessage(factory.EnableLipSync(v)));
            LipSyncMicrophoneDeviceName = new RPropertyMin<string>(setting.LipSyncMicrophoneDeviceName, v => SendMessage(factory.SetMicrophoneDeviceName(v)));
            MicrophoneSensitivity = new RPropertyMin<int>(setting.MicrophoneSensitivity, v => SendMessage(factory.SetMicrophoneSensitivity(v)));

            EnableHidArmMotion = new RPropertyMin<bool>(setting.EnableHidArmMotion, v => SendMessage(factory.EnableHidArmMotion(v)));
            EnableHidRandomTyping = new RPropertyMin<bool>(setting.EnableHidRandomTyping, v => SendMessage(factory.EnableHidRandomTyping(v)));
            EnableShoulderMotionModify = new RPropertyMin<bool>(setting.EnableShoulderMotionModify, v => SendMessage(factory.EnableShoulderMotionModify(v)));
            EnableHandDownTimeout = new RPropertyMin<bool>(setting.EnableHandDownTimeout, v => SendMessage(factory.EnableTypingHandDownTimeout(v)));
            WaistWidth = new RPropertyMin<int>(setting.WaistWidth, v => SendMessage(factory.SetWaistWidth(v)));
            ElbowCloseStrength = new RPropertyMin<int>(setting.ElbowCloseStrength, v => SendMessage(factory.SetElbowCloseStrength(v)));

            EnableFpsAssumedRightHand = new RPropertyMin<bool>(setting.EnableFpsAssumedRightHand, v => SendMessage(factory.EnableFpsAssumedRightHand(v)));

            EnablePresenterMotion = new RPropertyMin<bool>(setting.EnablePresenterMotion, v =>
            {
                SendMessage(factory.EnablePresenterMotion(v));
                UpdatePointerVisibility();
            });
            ShowPresentationPointer = new RPropertyMin<bool>(setting.ShowPresentationPointer, v => UpdatePointerVisibility());            
            PresentationArmRadiusMin = new RPropertyMin<int>(setting.PresentationArmRadiusMin, v => SendMessage(factory.PresentationArmRadiusMin(v)));

            LengthFromWristToTip = new RPropertyMin<int>(setting.LengthFromWristToTip, v => SendMessage(factory.LengthFromWristToTip(v)));
            HandYOffsetBasic = new RPropertyMin<int>(setting.HandYOffsetBasic, v => SendMessage(factory.HandYOffsetBasic(v)));
            HandYOffsetAfterKeyDown = new RPropertyMin<int>(setting.HandYOffsetAfterKeyDown, v => SendMessage(factory.HandYOffsetAfterKeyDown(v)));

            EnableWaitMotion = new RPropertyMin<bool>(setting.EnableWaitMotion, v => SendMessage(factory.EnableWaitMotion(v)));
            WaitMotionScale = new RPropertyMin<int>(setting.WaitMotionScale, v => SendMessage(factory.WaitMotionScale(v)));
            WaitMotionPeriod = new RPropertyMin<int>(setting.WaitMotionPeriod, v => SendMessage(factory.WaitMotionPeriod(v)));
        }

        #region Full Body 

        public RPropertyMin<bool> EnableNoHandTrackMode { get; }

        #endregion

        #region Face

        public RPropertyMin<bool> EnableFaceTracking { get; }

        public RPropertyMin<bool> AutoBlinkDuringFaceTracking { get; }

        public RPropertyMin<bool> EnableBodyLeanZ { get; }

        public RPropertyMin<bool> EnableBlinkAdjust { get; }

        public RPropertyMin<bool> EnableVoiceBasedMotion { get; }

        public RPropertyMin<bool> DisableFaceTrackingHorizontalFlip { get; }

        public RPropertyMin<bool> EnableWebCamHighPowerMode { get; }
        public RPropertyMin<bool> EnableImageBasedHandTracking { get; }

        public RPropertyMin<string> CameraDeviceName { get; }

        /// <summary>
        /// NOTE: この値はUIに出す必要はないが、起動時に空でなければ送り、Unityからデータが来たら受け取り、終了時にはセーブする。
        /// </summary>
        public RPropertyMin<string> CalibrateFaceData { get; }

        public RPropertyMin<int> FaceDefaultFun { get; }

        public RPropertyMin<string> FaceNeutralClip { get; }
        public RPropertyMin<string> FaceOffsetClip { get; }

        #endregion

        #region Eye

        public RPropertyMin<bool> UseLookAtPointNone { get; }
        public RPropertyMin<bool> UseLookAtPointMousePointer { get; }
        public RPropertyMin<bool> UseLookAtPointMainCamera { get; }

        public RPropertyMin<int> EyeBoneRotationScale { get; }

        #endregion

        #region Mouth

        public RPropertyMin<bool> EnableLipSync { get; }

        public RPropertyMin<string> LipSyncMicrophoneDeviceName { get; } 

        //NOTE: dB単位なので0がデフォルト。対数ベースのほうがレンジ取りやすい
        public RPropertyMin<int> MicrophoneSensitivity { get; }

        #endregion

        #region Arm

        public RPropertyMin<bool> EnableHidArmMotion { get; }
        public RPropertyMin<bool> EnableHidRandomTyping { get; }
        public RPropertyMin<bool> EnableShoulderMotionModify { get; }
        public RPropertyMin<bool> EnableHandDownTimeout { get; }

        public RPropertyMin<int> WaistWidth { get; }
        public RPropertyMin<int> ElbowCloseStrength { get; }
        public RPropertyMin<bool> EnableFpsAssumedRightHand { get; }
        public RPropertyMin<bool> EnablePresenterMotion { get; }

        public RPropertyMin<bool> ShowPresentationPointer { get; }
        public RPropertyMin<int> PresentationArmRadiusMin { get; }

        #endregion

        #region Hand

        /// <summary> Unit: [cm] </summary>
        public RPropertyMin<int> LengthFromWristToTip { get; }
        public RPropertyMin<int> HandYOffsetBasic { get; }
        public RPropertyMin<int> HandYOffsetAfterKeyDown { get; }

        #endregion

        #region Wait

        public RPropertyMin<bool> EnableWaitMotion { get; }
        public RPropertyMin<int> WaitMotionScale { get; }
        public RPropertyMin<int> WaitMotionPeriod { get; }

        #endregion

        #region Reset API

        public void ResetFaceBasicSetting()
        {
            var setting = MotionSetting.Default;
            EnableFaceTracking.Value = setting.EnableFaceTracking;
            CameraDeviceName.Value = setting.CameraDeviceName;
            AutoBlinkDuringFaceTracking.Value = setting.AutoBlinkDuringFaceTracking;
            EnableBodyLeanZ.Value = setting.EnableBodyLeanZ;

            EnableVoiceBasedMotion.Value = setting.EnableVoiceBasedMotion;
            DisableFaceTrackingHorizontalFlip.Value = setting.DisableFaceTrackingHorizontalFlip;
            EnableImageBasedHandTracking.Value = setting.EnableImageBasedHandTracking;

            EnableLipSync.Value = setting.EnableLipSync;
            LipSyncMicrophoneDeviceName.Value = setting.LipSyncMicrophoneDeviceName;
            MicrophoneSensitivity.Value = setting.MicrophoneSensitivity;
        }

        public void ResetFaceEyeSetting()
        {
            var setting = MotionSetting.Default;
            EnableBlinkAdjust.Value = setting.EnableBlinkAdjust;
            UseLookAtPointNone.Value = setting.UseLookAtPointNone;
            UseLookAtPointMousePointer.Value = setting.UseLookAtPointMousePointer;
            UseLookAtPointMainCamera.Value = setting.UseLookAtPointMainCamera;
            EyeBoneRotationScale.Value = setting.EyeBoneRotationScale;
        }

        public void ResetFaceBlendShapeSetting()
        {
            var setting = MotionSetting.Default;
            FaceDefaultFun.Value = setting.FaceDefaultFun;
            FaceNeutralClip.Value = setting.FaceNeutralClip;
            FaceOffsetClip.Value = setting.FaceOffsetClip;
        }

        public void ResetArmSetting()
        {
            var setting = MotionSetting.Default;
            EnableHidArmMotion.Value = setting.EnableHidArmMotion;
            EnableHidRandomTyping.Value = setting.EnableHidRandomTyping;
            EnableShoulderMotionModify.Value = setting.EnableShoulderMotionModify;
            EnableHandDownTimeout.Value = setting.EnableHandDownTimeout;
            WaistWidth.Value = setting.WaistWidth;
            ElbowCloseStrength.Value = setting.ElbowCloseStrength;
            EnableFpsAssumedRightHand.Value = setting.EnableFpsAssumedRightHand;
            EnablePresenterMotion.Value = setting.EnablePresenterMotion;
            ShowPresentationPointer.Value = setting.ShowPresentationPointer;
            PresentationArmRadiusMin.Value = setting.PresentationArmRadiusMin;
        }

        public void ResetHandSetting()
        {
            var setting = MotionSetting.Default;
            LengthFromWristToTip.Value = setting.LengthFromWristToTip;
            HandYOffsetBasic.Value = setting.HandYOffsetBasic;
            HandYOffsetAfterKeyDown.Value = setting.HandYOffsetAfterKeyDown;
        }

        public void ResetWaitMotionSetting()
        {
            var setting = MotionSetting.Default;
            EnableWaitMotion.Value = setting.EnableWaitMotion;
            WaitMotionScale.Value = setting.WaitMotionScale;
            WaitMotionPeriod.Value = setting.WaitMotionPeriod;
        }

        public override void ResetToDefault()
        {
            var setting = MotionSetting.Default;
            EnableNoHandTrackMode.Value = setting.EnableNoHandTrackMode;
            ResetFaceBasicSetting();
            ResetFaceEyeSetting();
            ResetFaceBlendShapeSetting();
            ResetArmSetting();
            ResetHandSetting();
            ResetWaitMotionSetting();
        }

        #endregion

        /// <summary>
        /// AutoAdjustParametersがシリアライズされた文字列を渡すことで、自動調整パラメータのうち
        /// モーションに関係のある値を適用します。
        /// </summary>
        /// <param name="data"></param>
        /// <remarks>
        /// ここで適用した値はUnityに対してメッセージ送信されません
        /// (そもそもUnity側から来る値だから)
        /// </remarks>
        public void SetAutoAdjustResults(string data)
        {
            try
            {
                var parameters = JsonConvert.DeserializeObject<AutoAdjustParameters>(data);
                LengthFromWristToTip.SilentSet(parameters.LengthFromWristToTip);
            }
            catch (Exception)
            {
                //何もしない: データ形式が悪いので諦める
            }
        }

        protected override void AfterLoad(MotionSetting entity)
        {
            //ファイルに有効なキャリブレーション情報があれば送る。
            //NOTE: これ以外のタイミングではキャリブレーション情報は基本送らないでよい
            //(Unity側がすでにキャリブの値を知ってる状態でメッセージを投げてくるため)
            if (!string.IsNullOrEmpty(CalibrateFaceData.Value))
            {
                SendMessage(MessageFactory.Instance.SetCalibrateFaceData(CalibrateFaceData.Value));
            }
        }

        private LargePointerController _largePointerController => LargePointerController.Instance;

        private void UpdatePointerVisibility()
        {
            _largePointerController.UpdateVisibility(
                EnablePresenterMotion.Value && ShowPresentationPointer.Value
                );
        }
    }
}
