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

        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _act?.Invoke();

    }
}
