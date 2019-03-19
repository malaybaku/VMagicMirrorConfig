using System;

namespace Baku.VMagicMirrorConfig
{
    class Message
    {
        //NOTE: コマンドにはコロン(":")を入れない事！(例外スローの方が健全かも)
        public Message(string command, string content)
        {
            Command = command?.Replace(":", "") ?? "";
            Content = content ?? "";
        }

        //パラメータが無いものはコレで十分
        public Message(string command) : this(command, "")
        {
        }

        public string Command { get; }
        public string Content { get; }
    }

    //シングルトンにしているのはstaticと書く回数を減らすため
    //TODO: そろそろシステマチック感を増してもいい頃合い
    class MessageFactory
    {
        private static MessageFactory _instance;

        public static MessageFactory Instance
            => _instance ?? (_instance = new MessageFactory());
        private MessageFactory() { }

        public Message Language(string langName)
            => new Message(nameof(Language), langName);

        #region HID Input

        public Message Key(System.Windows.Forms.Keys key)
            => new Message(nameof(Key), key.ToString());

        public Message KeyDown(string keyName)
            => new Message(nameof(KeyDown), keyName);

        public Message MouseButton(string info)
            => new Message(nameof(MouseButton), info);

        public Message MouseMoved(int x, int y)
            => new Message(nameof(MouseMoved), $"{x},{y}");

        #endregion

        #region VRM Load

        public Message OpenVrmPreview(string filePath)
            => new Message(nameof(OpenVrmPreview), filePath);

        public Message OpenVrm(string filePath)
            => new Message(nameof(OpenVrm), filePath);

        public Message CancelLoadVrm()
            => new Message(nameof(CancelLoadVrm));

        #endregion

        #region Window Setting

        public Message Chromakey(int a, int r, int g, int b)
            => new Message(nameof(Chromakey), $"{a},{r},{g},{b}");

        public Message WindowFrameVisibility(bool v)
            => new Message(nameof(WindowFrameVisibility), v.ToString());

        public Message IgnoreMouse(bool v)
            => new Message(nameof(IgnoreMouse), v.ToString());

        public Message TopMost(bool v)
            => new Message(nameof(TopMost), v.ToString());

        public Message WindowDraggable(bool windowDraggable)
            => new Message(nameof(WindowDraggable), windowDraggable.ToString());

        #endregion

        #region Layout Setting

        #region キャラの動き方

        public Message LengthFromWristToTip(int lengthFromWristToTip)
            => new Message(nameof(LengthFromWristToTip), lengthFromWristToTip.ToString());

        public Message LengthFromWristToPalm(int lengthFromWristToPalm)
            => new Message(nameof(LengthFromWristToPalm), lengthFromWristToPalm.ToString());

        public Message HandYOffsetBasic(int handYOffsetBasic)
            => new Message(nameof(HandYOffsetBasic), handYOffsetBasic.ToString());

        public Message HandYOffsetAfterKeyDown(int handYOffsetAfterKeyDown)
            => new Message(nameof(handYOffsetAfterKeyDown), handYOffsetAfterKeyDown.ToString());

        public Message EnableWaitMotion(bool enable)
            => new Message(nameof(EnableWaitMotion), enable.ToString());

        public Message WaitMotionScale(int scale)
            => new Message(nameof(WaitMotionScale), scale.ToString());

        public Message WaitMotionPeriod(int periodSec)
            => new Message(nameof(WaitMotionPeriod), periodSec.ToString());

        public Message EnableTouchTyping(bool enableTouchTyping)
            => new Message(nameof(EnableTouchTyping), enableTouchTyping.ToString());

        public Message EnableLipSync(bool enableLipSync)
            => new Message(nameof(EnableLipSync), enableLipSync.ToString());

        #endregion

        #region カメラの配置

        public Message CameraHeight(int height)
            => new Message(nameof(CameraHeight), height.ToString());

        public Message CameraDistance(int distance)
            => new Message(nameof(CameraDistance), distance.ToString());

        public Message CameraVerticalAngle(int angle)
            => new Message(nameof(CameraVerticalAngle), angle.ToString());

        #endregion

        #region キーボードとマウスパッドの配置

        public Message HidHeight(int height)
            => new Message(nameof(HidHeight), height.ToString());

        public Message HidHorizontalScale(int scale)
            => new Message(nameof(HidHorizontalScale), scale.ToString());

        public Message HidVisibility(bool visible)
            => new Message(nameof(HidVisibility), visible.ToString());

        #endregion

        #endregion

        #region Light Setting

        public Message LightColor(int r, int g, int b)
            => new Message(nameof(LightColor), $"{r},{g},{b}");

        public Message LightIntensity(int lightIntensity)
            => new Message(nameof(LightIntensity), lightIntensity.ToString());

        public Message BloomColor(int r, int g, int b)
            => new Message(nameof(BloomColor), $"{r},{g},{b}");

        public Message BloomIntensity(int intensity)
            => new Message(nameof(BloomIntensity), intensity.ToString());

        public Message BloomThreshold(int threshold)
            => new Message(nameof(BloomThreshold), threshold.ToString());

        public Message MoveWindow(int x, int y)
            => new Message(nameof(MoveWindow), $"{x},{y}");

        #endregion

    }

}
