﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AsyncAwaitBestPractices.MVVM
{
    /// <summary>
    /// An implmentation of IAsyncCommand. Allows Commands to safely be used asynchronously with Task.
    /// </summary>
    public sealed class AsyncCommand<T> : IAsyncCommand
    {
        #region Constant Fields
        readonly Func<T, Task> _execute;
        readonly Func<object, bool> _canExecute;
        readonly Action<Exception> _onException;
        readonly bool _continueOnCapturedContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:TaskExtensions.MVVM.AsyncCommand`1"/> class.
        /// </summary>
        /// <param name="execute">The Function executed when Execute or ExecuteAysnc is called. This does not check canExecute before executing and will execute even if canExecute is false</param>
        /// <param name="continueOnCapturedContext">If set to <c>true</c> continue on captured context; this will ensure that the Synchronization Context returns to the calling thread. If set to <c>false</c> continue on a different context; this will allow the Synchronization Context to continue on a different thread</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        /// <param name="canExecute">The Function that verifies whether or not AsyncCommand should execute.</param>
        public AsyncCommand(Func<T, Task> execute,
                            bool continueOnCapturedContext = true,
                            Action<Exception> onException = null,
                            Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _continueOnCapturedContext = continueOnCapturedContext;
            _onException = (onException is null) ? (ex => throw ex) : onException;
            _canExecute = (canExecute is null) ? _ => true : canExecute;
        }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute
        /// </summary>
        public event EventHandler CanExecuteChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether the command can execute in its current state
        /// </summary>
        /// <returns><c>true</c>, if this command can be executed; otherwise, <c>false</c>.</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Executes the Command as a Task
        /// </summary>
        /// <returns>The executed Task</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public Task ExecuteAsync(T parameter) => _execute?.Invoke(parameter);


        /// <summary>
        /// Executes the Command as a Task
        /// </summary>
        /// <returns>The executed Task</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public Task ExecuteAsync(object parameter)
        {
            if (parameter is T validParameter)
                return ExecuteAsync(validParameter);
            else if (parameter is null && !typeof(T).IsValueType)
                return ExecuteAsync((T)parameter);

            throw new InvalidCommandParameterException(typeof(T), parameter.GetType());
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is T validParameter)
                _execute?.Invoke(validParameter)?.SafeFireAndForget(_continueOnCapturedContext, _onException);
            else if (parameter is null && !typeof(T).IsValueType)
                _execute?.Invoke((T)parameter)?.SafeFireAndForget(_continueOnCapturedContext, _onException);
            else
                throw new InvalidCommandParameterException(typeof(T), parameter.GetType());
        }
        #endregion
    }

    /// <summary>
    /// An implmentation of IAsyncCommand. Allows Commands to safely be used asynchronously with Task.
    /// </summary>
    public sealed class AsyncCommand : IAsyncCommand
    {
        #region Constant Fields
        readonly Func<Task> _execute;
        readonly Func<object, bool> _canExecute;
        readonly Action<Exception> _onException;
        readonly bool _continueOnCapturedContext;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:TaskExtensions.MVVM.AsyncCommand`1"/> class.
        /// </summary>
        /// <param name="execute">The Function executed when Execute or ExecuteAysnc is called. This does not check canExecute before executing and will execute even if canExecute is false</param>
        /// <param name="continueOnCapturedContext">If set to <c>true</c> continue on captured context; this will ensure that the Synchronization Context returns to the calling thread. If set to <c>false</c> continue on a different context; this will allow the Synchronization Context to continue on a different thread</param>
        /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
        /// <param name="canExecute">The Function that verifies whether or not AsyncCommand should execute.</param>
        public AsyncCommand(Func<Task> execute,
                            bool continueOnCapturedContext = true,
                            Action<Exception> onException = null,
                            Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _continueOnCapturedContext = continueOnCapturedContext;
            _onException = (onException is null) ? (ex => throw ex) : onException;
            _canExecute = (canExecute is null) ? _ => true : canExecute;
        }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute
        /// </summary>
        public event EventHandler CanExecuteChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether the command can execute in its current state
        /// </summary>
        /// <returns><c>true</c>, if this command can be executed; otherwise, <c>false</c>.</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Executes the Command as a Task
        /// </summary>
        /// <returns>The executed Task</returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>

        public Task ExecuteAsync(object parameter) => _execute?.Invoke();

        void ICommand.Execute(object parameter) => _execute?.Invoke()?.SafeFireAndForget(_continueOnCapturedContext, _onException);
        #endregion
    }
}
