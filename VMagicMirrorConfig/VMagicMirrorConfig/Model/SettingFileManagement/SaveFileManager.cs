using System;
using System.IO;

namespace Baku.VMagicMirrorConfig
{
    /// <summary> 設定ファイルをスロット的に管理するクラス </summary>
    class SaveFileManager
    {
        /// <summary>保存できるファイルの数。1始まりで管理するため、indexとしては1,2,3のみを許可する</summary>
        public const int FileCount = 3;

        public SaveFileManager(SettingFileIo fileIo, RootSettingSync setting, IMessageSender sender)
        {
            _fileIo = fileIo;
            _setting = setting;
            _sender = sender;
        }
        private readonly SettingFileIo _fileIo;
        private readonly RootSettingSync _setting;
        private readonly IMessageSender _sender;

        public event Action<string>? VRoidModelLoadRequested;

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

            _fileIo.SaveSetting(SpecialFilePath.GetSaveFilePath(index), SettingFileReadWriteModes.Internal);
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

            var currentVrmPath = _setting.LastVrmLoadFilePath;
            var currentVRoidModelId = _setting.LastLoadedVRoidModelId;

            var content =
                (loadCharacter && loadNonCharacter) ? SettingFileReadContent.All :
                loadCharacter ? SettingFileReadContent.Character :
                SettingFileReadContent.NonCharacter;

            _fileIo.LoadSetting(SpecialFilePath.GetSaveFilePath(index), SettingFileReadWriteModes.Internal, content, fromAutomation);

            if (content != SettingFileReadContent.NonCharacter &&
                _setting.LastVrmLoadFilePath != currentVrmPath &&
                File.Exists(_setting.LastVrmLoadFilePath)
                )
            {
                _sender.SendMessage(MessageFactory.Instance.OpenVrm(_setting.LastVrmLoadFilePath));
            }
            else if(content != SettingFileReadContent.NonCharacter && 
                _setting.LastLoadedVRoidModelId != currentVRoidModelId && 
                !fromAutomation
                )
            {
                VRoidModelLoadRequested?.Invoke(_setting.LastLoadedVRoidModelId);
            }
        }

        /// <summary>
        /// 指定した番号のファイルがすでにセーブされているか確認します。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CheckFileExist(int index) =>
            index > 0 &&
            index <= FileCount &&
            File.Exists(SpecialFilePath.GetSaveFilePath(index));
    }
}
