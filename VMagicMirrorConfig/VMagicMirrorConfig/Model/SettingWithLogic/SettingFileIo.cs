using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    /// <summary> 設定ファイルの読み書きを行う、わりと権力のあるクラス。 </summary>
    internal class SettingFileIo
    {
        /// <summary>
        /// ファイルI/Oを行うときに操作対象とするモデルを指定してインスタンスを初期化します。
        /// </summary>
        /// <param name="model"></param>
        public SettingFileIo(SettingModel model, IMessageSender sender)
        {
            _model = model;
            _sender = sender;
        }

        private readonly SettingModel _model;
        private readonly IMessageSender _sender;


        //TODO: この辺の値はロード/セーブ時に加えてキャラロードとか設定改変でも変化するぞ！
        //というかSettingModelに書いた方が良い気がする
        private string _lastVrmLoadFilePath = "";
        private string _lastLoadedVRoidModelId = "";
        private bool AutoLoadLastLoadedVrm = false;
        private string LanguageName = "Japanese";


        public void SaveSetting(string path, bool isInternalFile)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var sw = new StreamWriter(path))
            {
                var saveData = new EntityBasedSaveData()
                {
                    IsInternalSaveFile = isInternalFile,
                    LastLoadedVrmFilePath = isInternalFile ? _lastVrmLoadFilePath : "",
                    LastLoadedVRoidModelId = isInternalFile ? _lastLoadedVRoidModelId : "",
                    AutoLoadLastLoadedVrm = isInternalFile ? AutoLoadLastLoadedVrm : false,
                    PreferredLanguageName = isInternalFile ? LanguageName : "",
                    WindowSetting = _model.WindowSetting.Save(),
                    MotionSetting = _model.MotionSetting.Save(),
                    LayoutSetting = _model.LayoutSetting.Save(),
                    LightSetting = _model.LightSetting.Save(),
                    WordToMotionSetting = _model.WordToMotionSetting.Save(),
                    ExternalTrackerSetting = _model.ExternalTrackerSetting.Save(),
                };

                //ここだけ互換性の都合で入れ子になってることに注意
                saveData.LayoutSetting.Gamepad = _model.GamepadSetting.Save();

                new XmlSerializer(typeof(SaveData)).Serialize(sw, saveData);
            }
        }

        private void LoadSettingSub(string path, bool isInternalFile)
        {
            using (var sr = new StreamReader(path))
            {
                var serializer = new XmlSerializer(typeof(EntityBasedSaveData));
                var saveData = (EntityBasedSaveData?)serializer.Deserialize(sr);
                if (saveData == null)
                {
                    LogOutput.Instance.Write("Setting file loaded, but result was not EntityBasedSaveData.");
                    return;
                }

                if (isInternalFile && saveData.IsInternalSaveFile)
                {
                    _lastVrmLoadFilePath = saveData.LastLoadedVrmFilePath ?? "";
                    _lastLoadedVRoidModelId = saveData.LastLoadedVRoidModelId ?? "";
                    AutoLoadLastLoadedVrm = saveData.AutoLoadLastLoadedVrm;
                    LanguageName =
                        _model.AvailableLanguageNames.Contains(saveData.PreferredLanguageName ?? "") ?
                        (saveData.PreferredLanguageName ?? "") :
                        "";
                }

                _model.WindowSetting.Load(saveData.WindowSetting);
                _model.MotionSetting.Load(saveData.MotionSetting);
                _model.LayoutSetting.Load(saveData.LayoutSetting);
                _model.GamepadSetting.Load(saveData.LayoutSetting?.Gamepad);
                _model.LightSetting.Load(saveData.LightSetting);
                _model.WordToMotionSetting.Load(saveData.WordToMotionSetting);
                _model.ExternalTrackerSetting.Load(saveData.ExternalTrackerSetting);
            }
        }

        public void LoadSetting(string path, bool isInternalFile)
        {
            if (!File.Exists(path))
            {
                LogOutput.Instance.Write($"Setting file load requested (internalFile={isInternalFile}, but file does not exist at: {path}");
                return;
            }

            try
            {
                //NOTE: ファイルロードではメッセージが凄い量になるので、コンポジットして「1つの大きいメッセージ」扱いし、トータルでスピードを取る
                _sender.StartCommandComposite();
                LoadSettingSub(path, isInternalFile);
                _sender.EndCommandComposite();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load setting file {path} : {ex.Message}");
            }
        }
    }
}
