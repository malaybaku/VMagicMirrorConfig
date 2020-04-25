using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    class MessageBoxWrapper
    {
        private MessageBoxWrapper() { }
        private static MessageBoxWrapper? _instance;
        public static MessageBoxWrapper Instance
            => _instance ??= new MessageBoxWrapper();

        private readonly Queue<TaskCompletionSource<bool>> _dialogTasks = new Queue<TaskCompletionSource<bool>>();

        /// <summary>
        /// メインウィンドウ、および表示されている場合は設定ウィンドウに、
        /// 同時にダイアログを表示します。
        /// 片方のダイアログを閉じた時点でもう片方を閉じて結果を返します。
        /// MessageBox.Showの代わりに呼び出して使います。
        /// </summary>
        /// <returns></returns>
        public Task<bool> ShowAsync(string title, string content, MessageBoxStyle style)
        {
            var vm = DialogHelperViewModel.Instance;

            vm.Title = title;
            vm.Content = content;
            vm.CanOk = (style == MessageBoxStyle.OK || style == MessageBoxStyle.OKCancel);
            vm.CanCancel = (style == MessageBoxStyle.Cancel || style == MessageBoxStyle.OKCancel);
            vm.IsOpen = true;

            var result = new TaskCompletionSource<bool>();
            _dialogTasks.Enqueue(result);
            return result.Task;            
        }

        /// <summary>
        /// ダイアログの結果を設定します。UIがDialogHelperViewModelから呼び出すか、
        /// またはプログラム的にダイアログを閉じたいときもここを呼び出します。
        /// </summary>
        /// <param name="result"></param>
        public void SetDialogResult(bool result)
        {
            if (_dialogTasks.Count > 0)
            {
                var tcs = _dialogTasks.Dequeue();
                tcs.SetResult(result);
            }
        }

        public enum MessageBoxStyle
        {
            OK,
            OKCancel,
            Cancel,
        }
    }
}
