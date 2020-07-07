using System;

namespace CommandHandler
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class CommandAttribute : Attribute
    {
        public string Group { get; set; }
        public string HelpText { get; set; }
        public string Name { get; set; }
    }
}
