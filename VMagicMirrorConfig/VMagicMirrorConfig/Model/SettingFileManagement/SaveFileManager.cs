﻿using System;
using System.IO;

namespace Baku.VMagicMirrorConfig
{
    /// <summary> 設定ファイルをスロット的に管理するクラス </summary>
    class SaveFileManager
    {
        /// <summary>保存できるファイルの数。1始まりで管理し、状況次第で0はオートセーブ扱いする。</summary>
        public const int FileCount = 15;

        public SaveFileManager(SettingFileIo fileIo, RootSettingSync setting, IMessageSender sender)
        {
            SettingFileIo = fileIo;
            _setting = setting;
            _sender = sender;
        }
        public SettingFileIo SettingFileIo { get; }
        private readonly RootSettingSync _setting;
        private readonly IMessageSender _sender;

        public event Action<string>? VRoidModelLoadRequested;

        /// <summary> 
        /// ソフト起動以降で<see cref="LoadSetting(int, bool, bool, bool)"/>を最後に呼び出して成功したときのindexを取得します。
        /// 一度も呼び出してなければ0を取得します。
        /// 「未保存の変更」みたいなのをお知らせするために使えます。
        /// </summary>
        public int LatestLoadedFileIndex { get; private set; } = 0;

        /// <summary>
        /// 指定した番号のファイルを保存します。
        /// </summary>
        /// <param name="index"></param>
        public void SaveCurrentSetting(int index)
        {
            if (index <= 0 || index > FileCount)
            {
                return;
            }

            SettingFileIo.SaveSetting(SpecialFilePath.GetSaveFilePath(index), SettingFileReadWriteModes.Internal);
        }

        /// <summary>
        /// インデックスと、各情報(キャラとそれ以外)をロードするかどうかを指定してファイルをロードします。
        /// キャラをロードする場合、必要なら実際のロードメッセージまで実行します。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="loadCharacter"></param>
        /// <param name="loadNonCharacter"></param>
        /// <param name="fromAutomation"></param>
        public void LoadSetting(int index, bool loadCharacter, bool loadNonCharacter, bool fromAutomation)
        {
            if (!CheckFileExist(index))
            {
                LogOutput.Instance.Write($"Tried to load setting, but file does not exist for {index}");
                return;
            }

            if (!loadCharacter && !loadNonCharacter)
            {
                //意味がないので何もしない
                return;
            }

            var prevVrmPath = _setting.LastVrmLoadFilePath;
            var prevVRoidModelId = _setting.LastLoadedVRoidModelId;

            var content =
                (loadCharacter && loadNonCharacter) ? SettingFileReadContent.All :
                loadCharacter ? SettingFileReadContent.Character :
                SettingFileReadContent.NonCharacter;

            SettingFileIo.LoadSetting(SpecialFilePath.GetSaveFilePath(index), SettingFileReadWriteModes.Internal, content, fromAutomation);
            var loadedVrmPath = _setting.LastVrmLoadFilePath;
            var loadedVRoidModelId = _setting.LastLoadedVRoidModelId;

            //NOTE: この時点ではキャラを切り替えたわけではないので、実態に合わすため元に戻してから続ける。
            _setting.LastVrmLoadFilePath = prevVrmPath;
            _setting.LastLoadedVRoidModelId = prevVRoidModelId;

            if (content != SettingFileReadContent.NonCharacter &&
                loadedVrmPath != prevVrmPath &&
                File.Exists(loadedVrmPath)
                )
            {
                LogOutput.Instance.Write($"Load Local VRM, setting no={index}, automation={fromAutomation}, path={loadedVrmPath}");
                _sender.SendMessage(MessageFactory.Instance.OpenVrm(loadedVrmPath));
                _setting.OnLocalModelLoaded(loadedVrmPath);
            }
            else if(content != SettingFileReadContent.NonCharacter && 
                loadedVRoidModelId != prevVRoidModelId && 
                !string.IsNullOrEmpty(loadedVRoidModelId) && 
                !fromAutomation
                )
            {
                LogOutput.Instance.Write($"Load VRoid, setting no={index}, automation={fromAutomation},  id={loadedVRoidModelId}");
                VRoidModelLoadRequested?.Invoke(loadedVRoidModelId);
            }

            //NOTE: キャラだけ切り替えるのはファイルロード扱いせず、前のファイルがロードされているように見なす。
            if (loadNonCharacter)
            {
                LatestLoadedFileIndex = index;
            }
        }

        /// <summary>
        /// 指定した番号のファイルがすでにセーブされているか確認します。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CheckFileExist(int index) =>
            index >= 0 &&
            index <= FileCount &&
            File.Exists(SpecialFilePath.GetSaveFilePath(index));
    }
}