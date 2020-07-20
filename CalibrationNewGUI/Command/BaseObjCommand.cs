using System;
using System.Windows.Input;

namespace J_Project.ViewModel.CommandClass
{
    class BaseObjCommand : ICommand
    {
        Action<object> _execute;
        Func<bool> _canExecute;
        private Action<string> writeTypeSelect;

        public BaseObjCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
