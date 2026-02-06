using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FLyC0De.Core.Actions;
using FLyC0De.Core.Interception;

namespace FLyC0De.Core.Services
{
    /// <summary>
    /// Manages action execution for key bindings.
    /// Actions are executed on thread pool threads to not block input handling.
    /// </summary>
    public class ActionEngine
    {
        private readonly ConcurrentDictionary<string, Func<IAction>> _actionFactories;

        public ActionEngine()
        {
            _actionFactories = new ConcurrentDictionary<string, Func<IAction>>();
            RegisterBuiltInActions();
        }

        private void RegisterBuiltInActions()
        {
            Register<RunApplicationAction>();
            Register<SendKeystrokesAction>();
            Register<HttpRequestAction>();
            Register<PlaySoundAction>();
        }

        /// <summary>
        /// Registers an action type.
        /// </summary>
        public void Register<T>() where T : IAction, new()
        {
            var sample = new T();
            _actionFactories[sample.TypeId] = () => new T();
        }

        /// <summary>
        /// Creates a new action instance of the specified type.
        /// </summary>
        public IAction CreateAction(string typeId)
        {
            if (_actionFactories.TryGetValue(typeId, out var factory))
            {
                return factory();
            }
            return null;
        }

        /// <summary>
        /// Gets all registered action type IDs.
        /// </summary>
        public string[] GetActionTypes()
        {
            var types = new string[_actionFactories.Count];
            _actionFactories.Keys.CopyTo(types, 0);
            return types;
        }

        /// <summary>
        /// Executes an action asynchronously on a thread pool thread.
        /// </summary>
        public void ExecuteAction(IAction action, KeyboardEventArgs keyEvent)
        {
            if (action == null) return;

            Task.Run(async () =>
            {
                try
                {
                    await action.ExecuteAsync(keyEvent);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Action execution failed: {ex.Message}");
                }
            });
        }
    }
}
