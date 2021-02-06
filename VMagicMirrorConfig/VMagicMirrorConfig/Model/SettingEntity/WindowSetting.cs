namespace Baku.VMagicMirrorConfig
{
    public class WindowSetting
    {
        public int R { get; set; } = 0;
        public int G { get; set; } = 255;
        public int B { get; set; } = 0;

        public bool IsTransparent { get; set; } = false;
        public bool WindowDraggable { get; set; } = true;
        public bool TopMost { get; set; } = true;

        public int WholeWindowTransparencyLevel { get; set; } = 2;
        public int AlphaValueOnTransparent { get; set; } = 128;


        #region Reset API

        private void ResetBackgroundColor()
        {
            R = 0;
            G = 255;
            B = 0;
        }

        private void ResetOpacity()
        {
            WholeWindowTransparencyLevel = 2;
            AlphaValueOnTransparent = 128;
        }


        public void ResetToDefault()
        {
            ResetBackgroundColor();

            IsTransparent = false;
            WindowDraggable = true;
            TopMost = true;

            ResetOpacity();
            //NOTE: この関数を呼ぶときはUnity側のウィンドウリセットも必要なことに注意
        }

        #endregion 
    }
}
