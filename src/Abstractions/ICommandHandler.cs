using System.Collections.Generic;
using System.Reflection;

namespace CommandHandler.Abstractions
{
    public interface ICommandHandler
    {
        IEnumerable<ICommand> Commands { get; }

        /// <summary>
        /// Binds a command to a method
        /// </summary>
        /// <param name="method">The method to bind</param>
        /// <param name="instance">The instance to use when executing commands</param>
        void Bind(MethodBase method, object instance = null);

        /// <summary>
        /// Finds and executes a command with given arguments
        /// </summary>
        /// <param name="command">The name of the command</param>
        /// <param name="args">Arguments to supply to the command</param>
        /// <returns>Execution value or DBNull.Value if command not found</returns>
        object Execute(string command, params object[] args);

        /// <summary>
        /// Unbinds a command from a method
        /// </summary>
        /// <param name="method">Method to unbind</param>
        void Unbind(MethodBase method);
    }
}
