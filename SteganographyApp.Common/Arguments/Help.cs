namespace SteganographyApp.Common.Arguments;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// A static utility class to print help related information about the program being executed and its attributes.
/// </summary>
public static class Help
{
    /// <summary>
    /// Logs all the help related information to the console.
    /// </summary>
    /// <param name="instanceType">The type of the class containing the arugments to be parsed.</param>
    /// <param name="instance">An instance of the the argument class. The type of this objects needs to match the type specified
    /// in the instanceType parameter.</param>
    /// <param name="registeredArguments">The list of all the arguments specified within the instanceType that have been previously
    /// identified.</param>
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

    private static string FormUsageString(
        Type instanceType,
        ImmutableArray<RegisteredArgument> positionArguments,
        ImmutableArray<RegisteredArgument> requiredArguments,
        ImmutableArray<RegisteredArgument> optionalArguments)
    {
        var builder = new StringBuilder();
        ProgramDescriptorAttribute? descriptor = GetDescriptorAttribute(instanceType);
        if (descriptor != null)
        {
            builder.Append(descriptor.Description).Append('\n');
        }
        builder.Append("Usage: ")
            .AppendJoin(" ", positionArguments.Select(argument => argument.Attribute.Name))
            .Append(' ')
            .AppendJoin(" ", requiredArguments.Select(argument => argument.Attribute.Name))
            .Append(' ')
            .AppendJoin(" ", optionalArguments.Select(argument => $"[{argument.Attribute.Name}]"))
            .Append('\n');
        if (descriptor != null && descriptor.Example != null)
        {
            builder.Append("Example Usage: ").Append(descriptor.Example).Append('\n');
        }
        return builder.ToString();
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

        builder.Append("\n\t").Append(argument.HelpText);

        if (argument.Position > 0)
        {
            builder.Append("\n\t[Position]: ").Append(argument.Position);
        }

        Type memberType = TypeHelper.DeclaredType(member);
        if (memberType.IsEnum)
        {
            var possibleValues = string.Join(", ", Enum.GetNames(memberType));
            builder.Append("\n\t[Allowed Values]: ").Append($"[{possibleValues}]");
        }

        if (argument.Position == -1)
        {
            string? value = DefaultValueOf(instance, member, argument);
            if (value != null)
            {
                builder.Append("\n\t[Default]: ").Append(value);
            }
        }

        if (argument.Example != null)
        {
            builder.Append("\n\t[Example]: ").Append(argument.Example);
        }

        return builder.ToString();
    }

    private static string? DefaultValueOf(object instance, MemberInfo member, ArgumentAttribute argument)
    {
        // Somce required parameters must be provided by the user there is no point in having a default value.
        if (argument.Required)
        {
            return null;
        }

        object? value = TypeHelper.GetValue(instance, member);
        if (value == null)
        {
            return null;
        }

        Type memberType = TypeHelper.DeclaredType(member);
        if (IsStruct(memberType))
        {
            return null;
        }

        return value.ToString();
    }

    private static bool IsStruct(Type memberType) => memberType.IsValueType && !memberType.IsEnum && !memberType.IsPrimitive;
}