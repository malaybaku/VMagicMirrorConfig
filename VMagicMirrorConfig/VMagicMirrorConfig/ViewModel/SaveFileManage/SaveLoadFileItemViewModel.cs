using System;

namespace Baku.VMagicMirrorConfig
{
    public class SaveLoadFileItemViewModel
    {
        internal SaveLoadFileItemViewModel(bool isLoadMode, bool isCurrent, SettingFileOverview model, SaveLoadDataViewModel parent)
        {
            IsLoadMode = isLoadMode;
            IsCurrent = isCurrent;
            Index = model.Index;
            IsExist = model.Exist;
            ModelName = model.ModelName;
            LastUpdatedDate = model.LastUpdateTime;

            SelectThisCommand = new ActionCommand(async () =>
            {
                if (isLoadMode)
                {
                    await parent.ExecuteLoad(Index);
                }
                else
                {
                    await parent.ExecuteSave(Index);
                }
            });
        }

        //ロードは存在するファイルじゃないとダメ、セーブはオートセーブ以外ならOK。
        public bool CanChooseThisItem =>
            (IsLoadMode && IsExist) ||
            (!IsLoadMode && Index != 0);

        public bool IsLoadMode { get; }
        /// <summary>今ロードしてる設定がこのファイルに由来しているかどうかを取得します。</summary>
        public bool IsCurrent { get; }


        public int Index { get; }
        public string IndexString => Index == 0 ? "No.0 (Auto Save)" : $"No.{Index}";

        /// <summary>
        /// そもそもファイルあるんだっけ、というのを取得します。
        /// </summary>
        public bool IsExist { get; }

        /// <summary>
        /// モデルがロードされている場合、そのモデル名。ロードされてない場合は空文字列
        /// </summary>
        public string ModelName { get; }

        public string ModelNameWithPrefix => string.IsNullOrEmpty(ModelName) ? "VRM File : - " : ModelName;

        public DateTime LastUpdatedDate { get; }
        public string LastUpdatedDateOrDash => IsExist ? $"{LastUpdatedDate:yyyy/MM/dd HH:mm}" : " - ";

        public ActionCommand SelectThisCommand { get; }

    }
}
