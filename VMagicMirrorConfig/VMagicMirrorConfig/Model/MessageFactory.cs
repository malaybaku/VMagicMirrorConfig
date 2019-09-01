using System;
using System.Runtime.CompilerServices;

namespace Baku.VMagicMirrorConfig
{
    class MessageFactory
    {
        private static MessageFactory _instance;
        public static MessageFactory Instance
            => _instance ?? (_instance = new MessageFactory());

        private MessageFactory() { }

        //メッセージのCommandには呼び出した関数の名前が入る: もともとnameof(Hoge)のように関数名を入れていたが、その必要が無くなった
        private static Message NoArg([CallerMemberName]string command = "")
            => new Message(command);

        private static Message WithArg(string content, [CallerMemberName]string command = "")
            => new Message(command, content);

        public Message Language(string langName) => WithArg(langName);

        #region HID Input

        public Message KeyDown(string keyName) => WithArg(keyName);
        public Message MouseButton(string info) => WithArg(info);
        public Message MouseMoved(int x, int y) => WithArg($"{x},{y}");

        #endregion

        #region VRM Load

        public Message OpenVrmPreview(string filePath) => WithArg(filePath);
        public Message OpenVrm(string filePath) => WithArg(filePath);
        public Message AccessToVRoidHub() => NoArg();

        public Message CancelLoadVrm() => NoArg();

        public Message RequestAutoAdjust() => NoArg();
        public Message RequestAutoAdjustEyebrow() => NoArg();

        #endregion

        #region ウィンドウ

        public Message Chromakey(int a, int r, int g, int b) => WithArg($"{a},{r},{g},{b}");

        public Message WindowFrameVisibility(bool v) => WithArg($"{v}");
        public Message IgnoreMouse(bool v) => WithArg($"{v}");
        public Message TopMost(bool v) => WithArg($"{v}");
        public Message WindowDraggable(bool v) => WithArg($"{v}");

        public Message MoveWindow(int x, int y) => WithArg($"{x},{y}");
        public Message ResetWindowSize() => NoArg();

        public Message SetWholeWindowTransparencyLevel(int level) => WithArg($"{level}");

        public Message SetAlphaValueOnTransparent(int alpha) => WithArg($"{alpha}");

        #endregion

        #region モーション

        public Message LengthFromWristToTip(int lengthCentimeter) => WithArg($"{lengthCentimeter}");

        public Message LengthFromWristToPalm(int lengthCentimeter) => WithArg($"{lengthCentimeter}");

        public Message HandYOffsetBasic(int offsetCentimeter) => WithArg($"{offsetCentimeter}");
        public Message HandYOffsetAfterKeyDown(int offsetCentimeter) => WithArg($"{offsetCentimeter}");

        public Message EnableHidArmMotion(bool enable) => WithArg($"{enable}");
        public Message SetWaistWidth(int waistWidthCentimeter) => WithArg($"{waistWidthCentimeter}");
        public Message SetElbowCloseStrength(int strengthPercent) => WithArg($"{strengthPercent}");        

        public Message EnablePresenterMotion(bool enable) => WithArg($"{enable}");
        public Message PresentationArmMotionScale(int scalePercent) => WithArg($"{scalePercent}");
        public Message PresentationArmRadiusMin(int radiusMinCentimeter) => WithArg($"{radiusMinCentimeter}");

        public Message EnableWaitMotion(bool enable) => WithArg($"{enable}");
        public Message WaitMotionScale(int scalePercent) => WithArg($"{scalePercent}");
        public Message WaitMotionPeriod(int periodSec) => WithArg($"{periodSec}");

        public Message CalibrateFace() => NoArg();
        public Message SetCalibrateFaceData(string data) => WithArg(data);

        public Message EnableFaceTracking(bool enable) => WithArg($"{enable}");
        public Message SetCameraDeviceName(string deviceName) => WithArg(deviceName);
        public Message AutoBlinkDuringFaceTracking(bool enable) => WithArg($"{enable}");

        public Message FaceDefaultFun(int percentage) => WithArg($"{percentage}");

        /// <summary>
        /// Query.
        /// </summary>
        /// <returns></returns>
        public Message CameraDeviceNames() => NoArg();

        public Message EnableTouchTyping(bool enable) => WithArg($"{enable}");
        public Message EnableLipSync(bool enable) => WithArg($"{enable}");

        public Message SetMicrophoneDeviceName(string deviceName) => WithArg(deviceName);
        /// <summary>
        /// Query.
        /// </summary>
        /// <returns></returns>
        public Message MicrophoneDeviceNames() => NoArg();

        public Message LookAtStyle(string v) => WithArg(v);

        /// <summary>
        /// Query.
        /// </summary>
        /// <returns></returns>
        public Message GetBlendShapeNames() => NoArg();


        //眉毛関係

        public Message EyebrowLeftUpKey(string key) => WithArg(key);
        public Message EyebrowLeftDownKey(string key) => WithArg(key);
        public Message UseSeparatedKeyForEyebrow(bool separate) => WithArg($"{separate}");
        public Message EyebrowRightUpKey(string key) => WithArg(key);

        public Message EyebrowRightDownKey(string key) => WithArg(key);
        public Message EyebrowUpScale(int percentage) => WithArg($"{percentage}");
        public Message EyebrowDownScale(int percentage) => WithArg($"{percentage}");

        #endregion

        #region カメラの配置

        public Message CameraFov(int cameraFov) => WithArg($"{cameraFov}");
        public Message SetCustomCameraPosition(string posData) => WithArg($"{posData}");

        public Message EnableFreeCameraMode(bool enable) => WithArg($"{enable}");
        public Message ResetCameraPosition() => NoArg();

        /// <summary>
        /// Query.
        /// </summary>
        /// <returns></returns>
        public Message CurrentCameraPosition() => NoArg();
        #endregion

        #region キーボード・マウスパッド

        public Message HidHeight(int heightCentimeter) => WithArg($"{heightCentimeter}");
        public Message HidHorizontalScale(int scalePercent) => WithArg($"{scalePercent}");
        public Message HidVisibility(bool visible) => WithArg($"{visible}");
        public Message SetKeyboardTypingEffectType(int typeIndex) => WithArg($"{typeIndex}");

        #endregion

        #region ゲームパッド

        public Message EnableGamepad(bool enable) => WithArg($"{enable}");

        public Message GamepadHeight(int height) => WithArg($"{height}");
        public Message GamepadHorizontalScale(int scale) => WithArg($"{scale}");

        public Message GamepadVisibility(bool visibility) => WithArg($"{visibility}");

        public Message GamepadLeanMode(string v) => WithArg(v);

        public Message GamepadLeanReverseHorizontal(bool reverse) => WithArg($"{reverse}");
        public Message GamepadLeanReverseVertical(bool reverse) => WithArg($"{reverse}");

        #endregion

        #region Light Setting

        public Message LightColor(int r, int g, int b) => WithArg($"{r},{g},{b}");
        public Message LightIntensity(int intensityPercent) => WithArg($"{intensityPercent}");
        public Message LightYaw(int angleDeg) => WithArg($"{angleDeg}");
        public Message LightPitch(int angleDeg) => WithArg($"{angleDeg}");

        public Message ShadowEnable(bool enable) => WithArg($"{enable}");
        public Message ShadowIntensity(int intensityPercent) => WithArg($"{intensityPercent}");
        public Message ShadowYaw(int angleDeg) => WithArg($"{angleDeg}");
        public Message ShadowPitch(int angleDeg) => WithArg($"{angleDeg}");
        public Message ShadowDepthOffset(int depthCentimeter) => WithArg($"{depthCentimeter}");

        public Message BloomColor(int r, int g, int b) => WithArg($"{r},{g},{b}");
        public Message BloomIntensity(int intensityPercent) => WithArg($"{intensityPercent}");
        public Message BloomThreshold(int thresholdPercent) => WithArg($"{thresholdPercent}");

        #endregion

        #region Word To Motion
        
        public Message EnableWordToMotion(bool enable) => WithArg($"{enable}");
        public Message ReloadMotionRequests(string content) => WithArg(content);

        //NOTE: 以下の3つはユーザーが動作チェックに使う
        public Message PlayWordToMotionItem(string word) => WithArg(word);
        public Message EnableWordToMotionPreview(bool enable) => WithArg($"{enable}");
        public Message SendWordToMotionPreviewInfo(string json) => WithArg(json);

        #endregion

    }
}