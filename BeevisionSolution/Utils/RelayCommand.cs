using System;
using System.Windows.Input;

namespace BeevisionSolution.Utils
{
    class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(param => execute(), canExecute != null ? param => canExecute() : (Func<object, bool>)null)
        { }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (_ => true);
        }

        public bool CanExecute(object parameter) => _canExecute(parameter);

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute(parameter);
            }
        }
    }
}
