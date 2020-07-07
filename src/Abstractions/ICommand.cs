using System;

namespace CommandHandler.Abstractions
{
    public interface ICommand : IEquatable<ICommand>, IComparable<ICommand>
    {
        /// <summary>
        /// Full command string
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// The group this command belongs to
        /// </summary>
        string Group { get; }

        /// <summary>
        /// The <see cref="ICommandHandler"/> responible for this command
        /// </summary>
        ICommandHandler Handler { get; }

        /// <summary>
        /// Helpfull documentation
        /// </summary>
        string HelpText { get; }

        /// <summary>
        /// The short name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Executes the command with provided arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        object Execute(params object[] args);
    }
}
