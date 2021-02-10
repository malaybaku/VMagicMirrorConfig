using System.ComponentModel;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// デバイスのフリーレイアウトが有効になると色々スイッチが入る、という処理をやるグルーコード
    /// </summary>
    internal class DeviceFreeLayoutHelper
    {
        public DeviceFreeLayoutHelper(LayoutSettingViewModel layout, WindowSettingViewModel window)
        {
            _layout = layout;
            _window = window;
        }

        private readonly LayoutSettingViewModel _layout;
        private readonly WindowSettingViewModel _window;

        private bool _freeCameraWhenStartFreeLayout = false;
        private bool _transparentWhenStartFreeLayout = false;

        /// <summary>
        /// プロパティの監視を開始します。初期ロードの完了後に呼び出します。
        /// </summary>
        public void StartObserve() => _layout.EnableDeviceFreeLayout.PropertyChanged += OnLayoutPropertyChanged;

        /// <summary>
        /// プロパティの監視を終了します。
        /// 処理を一時中断する可能性があるときは呼び出すことができますが、
        /// 呼び出さなくてもいいです。
        /// </summary>
        public void EndObserve() => _layout.PropertyChanged -= OnLayoutPropertyChanged;

        private void OnLayoutPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //やること: 
            //フリーレイアウトが始まったとき、
            // - フリーカメラが無効なら有効にする(無効だとしんどいので)
            // - 背景透過はオフにする(透過のままだと操作不可なので)
            //フリーレイアウトを終了するとき、
            // - もともとフリーカメラが無効だった場合、明示的に無効化を呼ぶ
            // - もともと背景が透過されていた場合、透過する

            //NOTE: 
            //  フリーレイアウトにしたあとでカメラとか透過のUIをガチャガチャいじると
            //  動作がちょっと不自然になる可能性があるが、これは諦めます

            if (_layout.EnableDeviceFreeLayout.Value)
            {
                _freeCameraWhenStartFreeLayout = _layout.EnableFreeCameraMode;
                _transparentWhenStartFreeLayout = _window.IsTransparent.Value;

                _layout.EnableFreeCameraMode = true;
                _window.IsTransparent.Value = false;
            }
            else
            {
                if (!_freeCameraWhenStartFreeLayout)
                {
                    _layout.EnableFreeCameraMode = false;
                }

                if (_transparentWhenStartFreeLayout)
                {
                    _window.IsTransparent.Value = true;
                }
            }
        }
    }
}
