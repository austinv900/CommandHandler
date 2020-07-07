using CommandHandler.Abstractions;
using System;
using System.Reflection;

namespace CommandHandler
{
    public static class CommandExtension
    {
        public static void Bind<TCommandInterface>(this ICommandHandler handler)
        {
            Type t = typeof(TCommandInterface);
            MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                handler.Bind(m, null);
            }
        }

        public static void Bind<TCommandInterface>(this ICommandHandler handler, TCommandInterface instance)
        {
            Type t = instance.GetType();
            MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                handler.Bind(m, instance);
            }
        }
    }
}
