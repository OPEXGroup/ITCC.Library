// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ITCC.UI.Commands
{
    public static class AsyncCommandFactory
    {
        public static AsyncCommand Create(Func<Task> command, Func<bool> canExecuteCondition = null)
            => new AsyncCommand(async (param, _) =>
            {
                await command();
                return null;
            }, canExecuteCondition);
        public static AsyncCommand Create(Func<object, Task> command, Func<bool> canExecuteCondition = null)
            => new AsyncCommand(async (param, _) =>
            {
                await command(param);
                return null;
            }, canExecuteCondition);
        public static AsyncCommand<TResult> Create<TResult>(Func<Task<TResult>> command, Func<bool> canExecuteCondition = null)
            => new AsyncCommand<TResult>((param, _) => command(), canExecuteCondition);
        public static AsyncCommand<TResult> Create<TResult>(Func<object, Task<TResult>> command, Func<bool> canExecuteCondition = null)
            => new AsyncCommand<TResult>((param, _) => command(param), canExecuteCondition);
        public static AsyncCommand Create(Func<CancellationToken, Task> command, Func<bool> canExecuteCondition = null)
            => new AsyncCommand(async (param, token) =>
            {
                await command(token);
                return null;
            }, canExecuteCondition);
        public static AsyncCommand Create(Func<object, CancellationToken, Task> command, Func<bool> canExecuteCondition = null)
            => new AsyncCommand(async (param, token) =>
            {
                await command(param, token);
                return null;
            }, canExecuteCondition);
        public static AsyncCommand<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> command, Func<bool> canExecuteCondition = null)
            => new AsyncCommand<TResult>(async (param, token) => await command(token), canExecuteCondition);
        public static AsyncCommand<TResult> Create<TResult>(Func<object, CancellationToken, Task<TResult>> command, Func<bool> canExecuteCondition = null)
            => new AsyncCommand<TResult>(async (param, token) => await command(param, token), canExecuteCondition);
    }
}
