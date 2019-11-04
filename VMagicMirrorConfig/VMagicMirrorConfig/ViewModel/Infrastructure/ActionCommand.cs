using System;
using System.Windows.Input;

namespace Baku.VMagicMirrorConfig
{
    public class ActionCommand : ICommand
    {
        public ActionCommand(Action act)
        {
            _act = act;
        }

        private readonly Action _act;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _act?.Invoke();

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
    }
}
