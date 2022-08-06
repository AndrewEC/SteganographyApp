namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class Help
    {
        internal static void PrintHelp(Type instanceType, object instance, ImmutableArray<RegisteredArgument> registeredArguments)
        {
            var positionalArguments = registeredArguments.Where(argument => argument.Attribute.Position > 0).OrderBy(argument => argument.Attribute.Position).ToImmutableArray();
            var nonPositionalRequiredArguments = registeredArguments.Where(argument => argument.Attribute.Required && argument.Attribute.Position <= 0).ToImmutableArray();
            var optionalArguments = registeredArguments.Where(argument => !argument.Attribute.Required).ToImmutableArray();

            Console.WriteLine(FormUsageString(instanceType, positionalArguments, nonPositionalRequiredArguments, optionalArguments));
            PrintArguments(positionalArguments, instance);
            PrintArguments(nonPositionalRequiredArguments, instance);
            PrintArguments(optionalArguments, instance);
        }

        private static void PrintArguments(ImmutableArray<RegisteredArgument> arguments, object instance)
        {
            foreach (RegisteredArgument argument in arguments)
            {
                Console.WriteLine(FormArgumentHelpText(argument.Attribute, argument.Member, instance));
            }
        }

        private static string FormUsageString(Type instanceType, ImmutableArray<RegisteredArgument> positionArguments,
            ImmutableArray<RegisteredArgument> requiredArguments,
            ImmutableArray<RegisteredArgument> optionalArguments)
        {
            var builder = new StringBuilder();
            ProgramDescriptorAttribute? descriptor = GetDescriptorAttribute(instanceType);
            if (descriptor != null)
            {
                builder.Append(descriptor.Description).Append("\n");
            }
            return builder.Append("Usage: ")
                .AppendJoin(" ", positionArguments.Select(argument => argument.Attribute.Name))
                .Append(" ")
                .AppendJoin(" ", requiredArguments.Select(argument => argument.Attribute.Name))
                .Append(" ")
                .AppendJoin(" ", optionalArguments.Select(argument => $"[{argument.Attribute.Name}]"))
                .Append("\n")
                .ToString();
        }

        private static ProgramDescriptorAttribute? GetDescriptorAttribute(Type instanceType) => instanceType
            .GetCustomAttribute(typeof(ProgramDescriptorAttribute)) as ProgramDescriptorAttribute;

        private static string FormArgumentHelpText(ArgumentAttribute argument, MemberInfo member, object instance)
        {
            var builder = new StringBuilder();

            if (argument.Required)
            {
                builder.Append("[Required] ");
            }

            builder.Append(argument.Name);

            if (argument.ShortName != null)
            {
                builder.Append(" | ").Append(argument.ShortName);
            }

            if (argument.Position > 0)
            {
                builder.Append("\n[Position]: ").Append(argument.Position);
            }
            else
            {
                object? value = TypeHelper.GetValue(instance, member);
                if (value != null)
                {
                    builder.Append("\n[Default]: ").Append($"[{value.ToString()}]");
                }
            }

            return builder.Append("\n\t").Append(argument.HelpText).Append("\n")
                .ToString();
        }
    }
}