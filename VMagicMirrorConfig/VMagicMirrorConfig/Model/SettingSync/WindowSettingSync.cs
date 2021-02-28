﻿using System;

namespace Baku.VMagicMirrorConfig
{
    class WindowSettingSync : SettingSyncBase<WindowSetting>
    {
        public WindowSettingSync(IMessageSender sender) : base(sender)
        {
            var setting = WindowSetting.Default;
            var factory = MessageFactory.Instance;

            Action sendBackgroundColor = () => SendMessage(factory.Chromakey(
                (IsTransparent?.Value == true) ? 0 : 255,
                R?.Value ?? 255,
                G?.Value ?? 255,
                B?.Value ?? 255
                ));

            R = new RProperty<int>(setting.R, _ => sendBackgroundColor());
            G = new RProperty<int>(setting.G, _ => sendBackgroundColor());
            B = new RProperty<int>(setting.B, _ => sendBackgroundColor());

            IsTransparent = new RProperty<bool>(setting.IsTransparent, b =>
            {
                //ここで透明or不透明の背景を送りつけるとUnity側がよろしく背景透過にしてくれる
                sendBackgroundColor();

                //透明 = ウィンドウフレーム不要。逆も然り
                SendMessage(factory.WindowFrameVisibility(!b));

                if (b)
                {
                    //背景透過になった時点でクリックスルーしてほしそうなフラグが立ってる => 実際にやる
                    if (WindowDraggable?.Value == false)
                    {
                        SendMessage(factory.IgnoreMouse(true));
                    }
                }
                else
                {
                    //背景透過でない=クリックスルーできなくする
                    SendMessage(factory.IgnoreMouse(true));
                }
            });

            WindowDraggable = new RProperty<bool>(setting.WindowDraggable, b =>
            {
                SendMessage(factory.WindowDraggable(b));
                //すでにウィンドウが透明ならばクリックスルーもついでにやる。不透明の場合、絶対にクリックスルーにはしない
                if (IsTransparent.Value)
                {
                    //ドラッグできない = クリックスルー、なのでフラグが反転することに注意
                    SendMessage(factory.IgnoreMouse(!b));
                }
            });

            TopMost = new RProperty<bool>(setting.TopMost, b => SendMessage(factory.TopMost(b)));

            WholeWindowTransparencyLevel = new RProperty<int>(
                setting.WholeWindowTransparencyLevel, 
                i => SendMessage(factory.SetWholeWindowTransparencyLevel(i))
                );

            AlphaValueOnTransparent = new RProperty<int>(
                setting.AlphaValueOnTransparent, 
                i => SendMessage(factory.SetAlphaValueOnTransparent(i))
                );
        }

        public RProperty<int> R { get; }
        public RProperty<int> G { get; }
        public RProperty<int> B { get; }

        public RProperty<bool> IsTransparent { get; }
        public RProperty<bool> WindowDraggable { get; }
        public RProperty<bool> TopMost { get; }

        public RProperty<int> WholeWindowTransparencyLevel { get; }
        public RProperty<int> AlphaValueOnTransparent { get; }

        #region Reset API

        public void ResetBackgroundColor()
        {
            var setting = WindowSetting.Default;
            R.Value = setting.R;
            G.Value = setting.G;
            B.Value = setting.B;
        }
        
        public void ResetOpacity()
        {
            var setting = WindowSetting.Default;
            WholeWindowTransparencyLevel.Value = setting.WholeWindowTransparencyLevel;
            AlphaValueOnTransparent.Value = setting.AlphaValueOnTransparent;
        }

        public override void ResetToDefault()
        {
            var setting = WindowSetting.Default;

            ResetBackgroundColor();

            IsTransparent.Value = setting.IsTransparent;
            WindowDraggable.Value = setting.WindowDraggable;
            TopMost.Value = setting.TopMost;

            ResetOpacity();
            ResetWindowPosition();
        }

        #endregion

        public void ResetWindowPosition()
        {
            //NOTE: ウィンドウが被ると困るのを踏まえ、すぐ上ではなく右わきに寄せる点にご注目
            var pos = WindowPositionUtil.GetThisWindowRightTopPosition();
            SendMessage(MessageFactory.Instance.MoveWindow(pos.X, pos.Y));
            SendMessage(MessageFactory.Instance.ResetWindowSize());
        }

    }
}