using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    public class WordToMotionItemViewModel : ViewModelBase
    {
        //NOTE: エディタ用
        public WordToMotionItemViewModel() : this(new WordToMotionSettingViewModel(), null) { }

        public WordToMotionItemViewModel(WordToMotionSettingViewModel parent, MotionRequest? model)
        {
            _parent = parent;
            MotionRequest = model;
            InitializeBuiltInClipNames();
            InitializeBlendShapeItems();
            AvailableBuiltInClipNames = 
                new ReadOnlyObservableCollection<string>(_availableBuiltInClipNames);
            BlendShapeItems = 
                new ReadOnlyObservableCollection<BlendShapeItemViewModel>(_blendShapeItems);

            LoadFromModel(model);
        }

        private readonly WordToMotionSettingViewModel _parent;

        /// <summary>ファイルI/Oや通信のベースになるデータを取得します。</summary>
        public MotionRequest? MotionRequest { get; }

        //NOTE: ビューは同時に1つまでのItemしか表示しないので、コレだけで
        public bool EnablePreview
        {
            get => _parent.EnablePreview;
            set
            {
                if (_parent.EnablePreview != value)
                {
                    _parent.EnablePreview = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private string _word = "";
        public string Word
        {
            get => _word;
            set => SetValue(ref _word, value);
        }

        //NOTE: この値はシリアライズ時に使うだけなのでgetter onlyでよく、変更通知も不要
        public int MotionType
        {
            get
            {
                return IsMotionTypeNone ? MotionRequest.MotionTypeNone :
                    IsMotionTypeBuiltInClip ? MotionRequest.MotionTypeBuiltInClip :
                    IsMotionTypeBvhFile ? MotionRequest.MotionTypeBvhFile :
                    MotionRequest.MotionTypeNone;
            }
        }

        private bool _isMotionTypeNone = true;
        public bool IsMotionTypeNone
        {
            get => _isMotionTypeNone;
            set
            {
                if (_isMotionTypeNone == value) { return; }

                if (value)
                {
                    IsMotionTypeBuiltInClip = false;
                    IsMotionTypeBvhFile = false;
                }
                _isMotionTypeNone = value;
                RaisePropertyChanged();
            }
        }

        private bool _isMotionTypeBuiltInClip = false;
        public bool IsMotionTypeBuiltInClip
        {
            get => _isMotionTypeBuiltInClip;
            set
            {
                if (_isMotionTypeBuiltInClip == value) { return; }

                if (value)
                {
                    IsMotionTypeNone = false;
                    IsMotionTypeBvhFile = false;
                }
                _isMotionTypeBuiltInClip = value;
                RaisePropertyChanged();
            }
        }

        private bool _isMotionTypeBvhFile = false;
        public bool IsMotionTypeBvhFile
        {
            get => _isMotionTypeBvhFile;
            set
            {
                if (_isMotionTypeBvhFile == value) { return; }
                if (value)
                {
                    IsMotionTypeNone = false;
                    IsMotionTypeBuiltInClip = false;
                }
                _isMotionTypeBvhFile = value;
                RaisePropertyChanged();
            }
        }

        private string _builtInClipName = "";
        public string BuiltInClipName
        {
            get => _builtInClipName;
            set => SetValue(ref _builtInClipName, value);
        }

        private string _bvhFilePath = "";
        public string BvhFilePath
        {
            get => _bvhFilePath;
            set => SetValue(ref _bvhFilePath, value);
        }

        private bool _useBlendShape = false;
        /// <summary>このアイテムがブレンドシェイプの変更要求を含んでいるかどうかを取得、設定します。</summary>
        public bool UseBlendShape
        {
            get => _useBlendShape;
            set => SetValue(ref _useBlendShape, value);
        }

        private bool _holdBlendShape = false;
        public bool HoldBlendShape
        {
            get => _holdBlendShape;
            set => SetValue(ref _holdBlendShape, value);
        }


        private float _durationWhenOnlyBlendShape = 3.0f;
        public float DurationWhenOnlyBlendShape
        {
            get => _durationWhenOnlyBlendShape;
            set => SetValue(ref _durationWhenOnlyBlendShape, value);
        }

        public ReadOnlyObservableCollection<BlendShapeItemViewModel> BlendShapeItems { get; }
        private ObservableCollection<BlendShapeItemViewModel> _blendShapeItems
            = new ObservableCollection<BlendShapeItemViewModel>();

        private ActionCommand? _selectBvhFileCommand;
        public ActionCommand SelectBvhFileCommand
            => _selectBvhFileCommand ??= new ActionCommand(SelectBvhFile);
        private void SelectBvhFile()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Select Motion File",
                Filter = "BVH file(*.bvh)|*.bvh",
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                BvhFilePath = System.IO.Path.GetFullPath(dialog.FileName);
            }
        }

        private ActionCommand? _moveUpCommand;
        public ActionCommand MoveUpCommand
            => _moveUpCommand ??= new ActionCommand(() => _parent.MoveUpItem(this));

        private ActionCommand? _moveDownCommand;
        public ActionCommand MoveDownCommand
            => _moveDownCommand ??= new ActionCommand(() => _parent.MoveDownItem(this));

        private ActionCommand? _editCommand;
        public ActionCommand EditCommand
            => _editCommand ??= new ActionCommand(() => _parent.EditItemByDialog(this));

        private ActionCommand? _playCommand;
        public ActionCommand PlayCommand
            => _playCommand ??= new ActionCommand(() => _parent.Play(this));

        private ActionCommand? _deleteCommand;
        public ActionCommand DeleteCommand
            => _deleteCommand ??= new ActionCommand(() => _parent.DeleteItem(this));

        private ObservableCollection<string> _availableBuiltInClipNames
            = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> AvailableBuiltInClipNames { get; }

        /// <summary>ViewModelの変更を破棄してモデルの値を取得し直します。</summary>
        public void ResetChanges() => LoadFromModel(MotionRequest);

        /// <summary>変更内容を確定し、モデルクラスにデータを書き込みます。</summary>
        public void SaveChanges() => WriteToModel(MotionRequest);

        public void WriteToModel(MotionRequest? model)
        {
            if (model == null) { return; }
            model.Word = Word;
            model.MotionType = MotionType;

            model.BuiltInAnimationClipName = BuiltInClipName;
            model.ExternalBvhFilePath = BvhFilePath;
            model.UseBlendShape = UseBlendShape;
            model.HoldBlendShape = HoldBlendShape;

            model.DurationWhenOnlyBlendShape = DurationWhenOnlyBlendShape;

            

            foreach (var item in BlendShapeItems)
            {
                model.BlendShapeValues[item.BlendShapeName] = item.ValuePercentage;
            }
        }

        private void LoadFromModel(MotionRequest? model)
        {
            if (model == null) { return; }
            Word = model.Word;

            switch (model.MotionType)
            {
                case MotionRequest.MotionTypeNone:
                    IsMotionTypeNone = true;
                    break;
                case MotionRequest.MotionTypeBuiltInClip:
                    IsMotionTypeBuiltInClip = true;
                    break;
                case MotionRequest.MotionTypeBvhFile:
                    IsMotionTypeBvhFile = true;
                    break;
                default:
                    IsMotionTypeNone = true;
                    break;
            }

            BuiltInClipName = model.BuiltInAnimationClipName;
            BvhFilePath = model.ExternalBvhFilePath;

            UseBlendShape = model.UseBlendShape;
            HoldBlendShape = model.HoldBlendShape;
            DurationWhenOnlyBlendShape = model.DurationWhenOnlyBlendShape;
            foreach (var blendShapeItem in model.BlendShapeValues)
            {
                var item = BlendShapeItems.FirstOrDefault(i => i.BlendShapeName == blendShapeItem.Key);
                if (item != null)
                {
                    item.ValuePercentage = blendShapeItem.Value;
                }
            }
        }

        private void InitializeBuiltInClipNames()
        {
            //NOTE: 数が少ないのでハードコーディングで済ます
            _availableBuiltInClipNames.Add("Wave");
            _availableBuiltInClipNames.Add("Rokuro");
            _availableBuiltInClipNames.Add("Good");
        }

        private void InitializeBlendShapeItems()
        {
            _blendShapeItems.Add(new BlendShapeItemViewModel("Joy", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("Angry", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("Sorrow", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("Fun", 0));

            _blendShapeItems.Add(new BlendShapeItemViewModel("A", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("I", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("U", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("E", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("O", 0));

            _blendShapeItems.Add(new BlendShapeItemViewModel("Neutral", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("Blink", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("Blink_L", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("Blink_R", 0));

            _blendShapeItems.Add(new BlendShapeItemViewModel("LookUp", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("LookDown", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("LookLeft", 0));
            _blendShapeItems.Add(new BlendShapeItemViewModel("LookRight", 0));
        }
    }

    public class BlendShapeItemViewModel : ViewModelBase
    {
        public BlendShapeItemViewModel(string name, int value)
        {
            BlendShapeName = name;
            ValuePercentage = value;
        }

        public string BlendShapeName { get; }

        private int _valuePercentage = 0;
        public int ValuePercentage
        {
            get => _valuePercentage;
            set => SetValue(ref _valuePercentage, value);
        }
    }
}
