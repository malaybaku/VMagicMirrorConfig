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
        public SettingFileIo(RootSettingSync model, IMessageSender sender)
        {
            _model = model;
            _sender = sender;
        }

        private readonly RootSettingSync _model;
        private readonly IMessageSender _sender;

        public void SaveSetting(string path, bool isInternalFile)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var sw = new StreamWriter(path))
            {
                var autoLoadEnabled = _model.AutoLoadLastLoadedVrm.Value;

                var saveData = new EntityBasedSaveData()
                {
                    IsInternalSaveFile = isInternalFile,
                    LastLoadedVrmFilePath = 
                        (isInternalFile && autoLoadEnabled) ? _model.LastVrmLoadFilePath : "",
                    LastLoadedVRoidModelId = 
                        (isInternalFile && autoLoadEnabled) ? _model.LastLoadedVRoidModelId : "",
                    AutoLoadLastLoadedVrm = isInternalFile ? autoLoadEnabled : false,
                    PreferredLanguageName = isInternalFile ? _model.LanguageName.Value : "",
                    WindowSetting = _model.Window.Save(),
                    MotionSetting = _model.Motion.Save(),
                    LayoutSetting = _model.Layout.Save(),
                    LightSetting = _model.Light.Save(),
                    WordToMotionSetting = _model.WordToMotion.Save(),
                    ExternalTrackerSetting = _model.ExternalTracker.Save(),
                };

                //ここだけ互換性の都合で入れ子になってることに注意
                saveData.LayoutSetting.Gamepad = _model.Gamepad.Save();

                new XmlSerializer(typeof(EntityBasedSaveData)).Serialize(sw, saveData);
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
                    _model.LastVrmLoadFilePath = saveData.LastLoadedVrmFilePath ?? "";
                    _model.LastLoadedVRoidModelId = saveData.LastLoadedVRoidModelId ?? "";
                    _model.AutoLoadLastLoadedVrm.Value = saveData.AutoLoadLastLoadedVrm;
                    _model.LanguageName.Value =
                        _model.AvailableLanguageNames.Contains(saveData.PreferredLanguageName ?? "") ?
                        (saveData.PreferredLanguageName ?? "") :
                        "";
                }

                _model.Window.Load(saveData.WindowSetting);
                _model.Motion.Load(saveData.MotionSetting);
                _model.Layout.Load(saveData.LayoutSetting);
                _model.Gamepad.Load(saveData.LayoutSetting?.Gamepad);
                _model.Light.Load(saveData.LightSetting);
                _model.WordToMotion.Load(saveData.WordToMotionSetting);
                _model.ExternalTracker.Load(saveData.ExternalTrackerSetting);
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
                //NOTE: ファイルロードではメッセージが凄い量になるので、
                //コンポジットして「1つの大きいメッセージ」として書き込むためにこうしてます
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
