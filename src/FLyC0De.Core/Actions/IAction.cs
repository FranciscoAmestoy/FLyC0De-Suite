using System;
using System.Threading.Tasks;
using FLyC0De.Core.Interception;

namespace FLyC0De.Core.Actions
{
    /// <summary>
    /// Interface for all macro actions.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Unique identifier for this action type.
        /// </summary>
        string TypeId { get; }

        /// <summary>
        /// Display name for UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Execute the action. Called on a thread pool thread, not the input thread.
        /// </summary>
        Task ExecuteAsync(KeyboardEventArgs keyEvent);

        /// <summary>
        /// Validates the action configuration.
        /// </summary>
        bool Validate(out string error);
    }

    /// <summary>
    /// Base class for actions with common functionality.
    /// </summary>
    public abstract class ActionBase : IAction
    {
        public abstract string TypeId { get; }
        public abstract string DisplayName { get; }

        public abstract Task ExecuteAsync(KeyboardEventArgs keyEvent);

        public virtual bool Validate(out string error)
        {
            error = null;
            return true;
        }
    }
}
