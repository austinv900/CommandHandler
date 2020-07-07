using CommandHandler.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace CommandHandler
{
    public sealed class CommandHandler : ICommandHandler
    {
        private readonly List<Command> commands;
        public IEnumerable<ICommand> Commands => commands;

        private CommandHandler()
        {
            commands = new List<Command>();
        }

        public static ICommandHandler Create() => new CommandHandler();

        public void Bind(MethodBase method, object instance = null)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (!method.IsStatic && instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            CommandAttribute[] attributes = GetCommandInfo(method);

            if (attributes == null || attributes.Length == 0)
            {
                return;
            }

            for (int i = 0; i < attributes.Length; i++)
            {
                CommandAttribute attribute = attributes[i];

                if (string.IsNullOrWhiteSpace(attribute.Name))
                {
                    attribute.Name = method.Name;
                }

                if (string.IsNullOrWhiteSpace(attribute.Group))
                {
                    attribute.Group = "global";
                }

                Command cmd = new Command(this, method, instance, attribute);

                if (!commands.Contains(cmd))
                {
                    commands.Add(cmd);
                }
            }
        }

        public object Execute(string command, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return DBNull.Value;
            }

            IEnumerable<Command> cmds;

            if (command.Contains('.'))
            {
                cmds = commands.Where(c => c.FullName.Equals(command, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                cmds = commands.Where(c => c.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase));
            }

            if (cmds.Count() > 1)
            {
                cmds = cmds.Where(c => c.Group.Equals("global", StringComparison.InvariantCultureIgnoreCase));
            }

            if (cmds.Count() > 1)
            {
                return DBNull.Value;
            }

            Command cmd = cmds.FirstOrDefault();

            if (cmd == null)
            {
                return DBNull.Value;
            }

            try
            {
                return cmd.Execute(args);
            }
            catch (TargetInvocationException e)
            {
                return e;
            }
        }

        public void Unbind(MethodBase method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            commands.RemoveAll(c => c.Method == method);
        }

        private CommandAttribute[] GetCommandInfo(MethodBase method)
        {
            CommandAttribute[] commands = method.GetCustomAttributes<CommandAttribute>(true).ToArray();

            for (int i = 0; i < commands.Length; i++)
            {
                CommandAttribute c = commands[i];

                if (string.IsNullOrWhiteSpace(c.Name))
                {
                    c.Name = method.Name;
                }

                if (string.IsNullOrWhiteSpace(c.Group))
                {
                    c.Group = "global";
                }
            }

            return commands;
        }

        #region Command Class

        private sealed class Command : ICommand
        {
            public string FullName { get; }
            public string Group { get; }
            public ICommandHandler Handler { get; }
            public string HelpText { get; }
            public object Instance { get; }
            public MethodBase Method { get; }
            public string Name { get; }

            public Command(ICommandHandler handler, MethodBase method, object instance, CommandAttribute attribute)
            {
                Handler = handler;
                Name = attribute.Name.ToLowerInvariant();
                Group = attribute.Group.ToLowerInvariant();
                HelpText = attribute.HelpText;
                FullName = $"{Group}.{Name}";
                Method = method;
                Instance = instance;
            }

            public int CompareTo([AllowNull] ICommand other)
            {
                if (other == null)
                {
                    return 1;
                }

                return FullName.CompareTo(other.FullName);
            }

            public bool Equals([AllowNull] ICommand other)
            {
                if (other == null)
                {
                    return false;
                }

                return other.FullName.Equals(FullName, StringComparison.OrdinalIgnoreCase);
            }

            public override bool Equals(object obj) => obj != null && obj is ICommand cmd && Equals(cmd);

            public override int GetHashCode() => FullName.GetHashCode();

            public object Execute(params object[] args)
            {
                ParameterInfo[] parameters = Method.GetParameters();

                if (args == null)
                {
                    args = Array.Empty<object>();
                }

                if (parameters.Length != args.Length)
                {
                    return null;
                }

                object[] values = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo p = parameters[i];
                    values[p.Position] = ConvertTo(args[p.Position], p.ParameterType);
                }

                if (Method.IsStatic)
                {
                    return Method.Invoke(null, values);
                }
                else
                {
                    return Method.Invoke(Instance, values);
                }
            }

            private object ConvertTo(object obj, Type type)
            {
                if (obj == null)
                {
                    return null;
                }

                if (type.IsInstanceOfType(obj))
                {
                    return obj;
                }

                TypeConverter converter = TypeDescriptor.GetConverter(obj);

                if (converter.CanConvertTo(type))
                {
                    return converter.ConvertTo(obj, type);
                }

                converter = TypeDescriptor.GetConverter(type);

                if (converter.CanConvertFrom(obj.GetType()))
                {
                    return converter.ConvertFrom(obj);
                }

                return obj;
            }
        }

        #endregion
    }
}
