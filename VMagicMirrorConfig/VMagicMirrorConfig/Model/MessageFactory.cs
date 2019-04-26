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
        public Message CancelLoadVrm() => NoArg();

        #endregion

        #region ウィンドウ

        public Message Chromakey(int a, int r, int g, int b) => WithArg($"{a},{r},{g},{b}");

        public Message WindowFrameVisibility(bool v) => WithArg($"{v}");
        public Message IgnoreMouse(bool v) => WithArg($"{v}");
        public Message TopMost(bool v) => WithArg($"{v}");
        public Message WindowDraggable(bool v) => WithArg($"{v}");

        public Message MoveWindow(int x, int y) => WithArg($"{x},{y}");

        #endregion

        #region キャラの動き方

        public Message LengthFromWristToTip(int lengthCentimeter) => WithArg($"{lengthCentimeter}");
        public Message LengthFromWristToPalm(int lengthCentimeter) => WithArg($"{lengthCentimeter}");

        public Message HandYOffsetBasic(int offsetCentimeter) => WithArg($"{offsetCentimeter}");
        public Message HandYOffsetAfterKeyDown(int offsetCentimeter) => WithArg($"{offsetCentimeter}");
        public Message EnablePresenterMotion(bool enable) => WithArg($"{enable}");
        public Message PresentationArmMotionScale(int percent) => WithArg($"{percent}");

        public Message EnableWaitMotion(bool enable) => WithArg($"{enable}");
        public Message WaitMotionScale(int scalePercent) => WithArg($"{scalePercent}");
        public Message WaitMotionPeriod(int periodSec) => WithArg($"{periodSec}");

        public Message EnableTouchTyping(bool enable) => WithArg($"{enable}");
        public Message EnableLipSync(bool enable) => WithArg($"{enable}");

        public Message SetMicrophoneDeviceName(string deviceName) => WithArg(deviceName);
        /// <summary>
        /// Query.
        /// </summary>
        /// <returns></returns>
        public Message MicrophoneDeviceNames() => NoArg();

        #endregion

        #region カメラの配置

        public Message CameraHeight(int heightCentimeter) => WithArg($"{heightCentimeter}");
        public Message CameraDistance(int distanceCentimeter) => WithArg($"{distanceCentimeter}");
        public Message CameraVerticalAngle(int angleDegree) => WithArg($"{angleDegree}");

        public Message EnableCustomCameraPosition(bool enable) => WithArg($"{enable}");
        public Message SetCustomCameraPosition(string posData) => WithArg($"{posData}");
        public Message EnableFreeCameraMode(bool enable) => WithArg($"{enable}");

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

        public Message BloomColor(int r, int g, int b) => WithArg($"{r},{g},{b}");
        public Message BloomIntensity(int intensityPercent) => WithArg($"{intensityPercent}");
        public Message BloomThreshold(int thresholdPercent) => WithArg($"{thresholdPercent}");

        #endregion

    }
}