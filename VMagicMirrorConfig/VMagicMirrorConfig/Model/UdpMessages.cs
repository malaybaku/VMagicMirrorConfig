using System;

namespace Baku.VMagicMirrorConfig
{
    class UdpMessage
    {
        //NOTE: コマンドにはコロン(":")を入れない事！(例外スローの方が健全かも)
        public UdpMessage(string command, string content)
        {
            Command = command?.Replace(":", "") ?? "";
            Content = content ?? "";
        }

        //パラメータが無いものはコレで十分
        public UdpMessage(string command) : this(command, "")
        {
        }

        public string Command { get; }
        public string Content { get; }
    }

    //シングルトンにしているのはstaticと書く回数を減らすため
    class UdpMessageFactory
    {
        private static UdpMessageFactory _instance;

        public static UdpMessageFactory Instance
            => _instance ?? (_instance = new UdpMessageFactory());
        private UdpMessageFactory() { }

        #region HID Input

        public UdpMessage Key(System.Windows.Forms.Keys key)
            => new UdpMessage(nameof(Key), key.ToString());

        public UdpMessage KeyDown(string keyName)
            => new UdpMessage(nameof(KeyDown), keyName);

        public UdpMessage MouseButton(string info)
            => new UdpMessage(nameof(MouseButton), info);

        public UdpMessage MouseMoved(int x, int y)
            => new UdpMessage(nameof(MouseMoved), $"{x},{y}");

        #endregion

        #region VRM Load

        public UdpMessage OpenVrmPreview(string filePath)
            => new UdpMessage(nameof(OpenVrmPreview), filePath);

        public UdpMessage OpenVrm(string filePath)
            => new UdpMessage(nameof(OpenVrm), filePath);

        internal UdpMessage CancelLoadVrm()
            => new UdpMessage(nameof(CancelLoadVrm));

        #endregion

        #region BG (Background) Setting

        //NOTE: 汎用形式っぽくしてるが透過処理はほかと比べて特殊な点に注意

        public UdpMessage Chromakey(int a, int r, int g, int b)
            => new UdpMessage(nameof(Chromakey), $"{a},{r},{g},{b}");

        public UdpMessage LightIntensity(int lightIntensity)
            => new UdpMessage(nameof(LightIntensity), lightIntensity.ToString());

        #endregion

        #region Layout Setting

        #region キャラの動き方

        public UdpMessage LengthFromWristToTip(int lengthFromWristToTip)
            => new UdpMessage(nameof(LengthFromWristToTip), lengthFromWristToTip.ToString());

        public UdpMessage LengthFromWristToPalm(int lengthFromWristToPalm)
            => new UdpMessage(nameof(LengthFromWristToPalm), lengthFromWristToPalm.ToString());

        public UdpMessage EnableTouchTyping(bool enableTouchTyping)
            => new UdpMessage(nameof(EnableTouchTyping), enableTouchTyping.ToString());

        #endregion

        #region カメラの配置

        public UdpMessage CameraHeight(int height)
            => new UdpMessage(nameof(CameraHeight), height.ToString());

        public UdpMessage CameraDistance(int distance)
            => new UdpMessage(nameof(CameraDistance), distance.ToString());

        public UdpMessage CameraVerticalAngle(int angle)
            => new UdpMessage(nameof(CameraVerticalAngle), angle.ToString());

        #endregion

        #region キーボードとマウスパッドの配置

        public UdpMessage HidHeight(int height)
            => new UdpMessage(nameof(HidHeight), height.ToString());

        public UdpMessage HidHorizontalScale(int scale)
            => new UdpMessage(nameof(HidHorizontalScale), scale.ToString());

        public UdpMessage HidVisibility(bool visible)
            => new UdpMessage(nameof(HidVisibility), visible.ToString());

        #endregion

        #endregion

    }

}
