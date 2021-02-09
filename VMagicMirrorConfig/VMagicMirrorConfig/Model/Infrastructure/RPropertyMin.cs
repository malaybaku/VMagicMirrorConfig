using System;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// ReactivePropertyの機能めちゃくちゃ削減したようなもの。モデル層の簡略化を主目的とします。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class RPropertyMin<T> : NotifiableBase
        where T : IEquatable<T>
    {
        /// <summary>
        /// 初期値と、および値が変化したときのPropertyChanged呼び出し以外の処理を指定してインスタンスを初期化します。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onChanged"></param>
        public RPropertyMin(T value, Action<T> onChanged)
        {
            _onChanged = onChanged;
            _value = value;
        }

        /// <summary>
        /// 初期値のみを指定してインスタンスを初期化します。
        /// </summary>
        /// <param name="value"></param>
        public RPropertyMin(T value) : this(value, _ => { })
        {
        }


        private readonly Action<T> _onChanged;
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (SetValue(ref _value, value))
                {
                    _onChanged(value);
                }
            }
        }

        /// <summary>
        /// 値は変更しますが、イベントやコールバックは呼びません。
        /// Unity側から値を受信し、その値がUI上で表示不要であるような、ごく一部のケースでのみ使います。
        /// </summary>
        /// <param name="value"></param>
        public void SilentSet(T value) { _value = value; }
    }
}
