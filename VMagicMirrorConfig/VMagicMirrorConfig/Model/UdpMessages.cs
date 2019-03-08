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


        public UdpMessage Key(System.Windows.Forms.Keys key)
            => new UdpMessage(nameof(Key), key.ToString());

        public UdpMessage KeyDown(string keyName)
            => new UdpMessage(nameof(KeyDown), keyName);

        public UdpMessage MouseButton(string info)
            => new UdpMessage(nameof(MouseButton), info);

        public UdpMessage MouseMoved(int x, int y)
            => new UdpMessage(nameof(MouseMoved), $"{x},{y}");

        public UdpMessage OpenVrmPreview(string filePath)
            => new UdpMessage(nameof(OpenVrmPreview), filePath);

        public UdpMessage OpenVrm(string filePath)
            => new UdpMessage(nameof(OpenVrm), filePath);

        //NOTE: 汎用形式っぽくしてるが透過処理はほかと比べて特殊な点に注意
        public UdpMessage UpdateChromakey(int a, int r, int g, int b)
            => new UdpMessage(nameof(UpdateChromakey), $"{a},{r},{g},{b}");

    }
}
