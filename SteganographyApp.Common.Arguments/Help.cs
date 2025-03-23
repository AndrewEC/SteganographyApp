namespace SteganographyApp.Common.Arguments;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

using SteganographyApp.Common.Arguments.Matching;

/// <summary>
/// A static utility to print help related information about the program being
/// executed and its attributes.
/// </summary>
public interface IHelp
{
    /// <summary>
    /// Returns the HelpText property of the ProgramDescriptor attribute on the input
    /// type. If there is no ProgramDescriptor attribute on the input type then this
    /// will return an empty string.
    /// </summary>
    /// <param name="commandType">The type from which the ProgramDescriptor attribute
    /// will be pulled.</param>
    /// <returns>The HelpText property of the ProgramDescriptor attribute or null if
    /// no attribute is available.</returns>
    public string GetCommandDescription(Type commandType);

    /// <summary>
    /// Logs all help related information for the input type to the console.
    /// </summary>
    /// <param name="instanceType">The type of the class containing the arugments to
    /// be parsed.</param>
    /// <param name="instance">An instance of the the argument class. The type of
    /// this objects needs to match the type specified
    /// in the instanceType parameter.</param>
    /// <param name="registeredArguments">The list of all the arguments specified
    /// within the instanceType that have been previously
    /// identified.</param>
    public void PrintHelp(Type instanceType, object instance, ImmutableArray<RegisteredArgument> registeredArguments);
}

/// <summary>
/// A static utility to print help related information about the program being
/// executed and its attributes.
/// </summary>
public sealed class Help : IHelp
{
    private const int MaxDescriptionWidth = 80;
    private const char Space = ' ';

    /// <inheritdoc/>
    public string GetCommandDescription(Type commandType)
    {
        string helpText = GetDescriptorAttribute(commandType)?.HelpText ?? string.Empty;
        return SplitHelpText(helpText);
    }

    /// <inheritdoc/>
    public void PrintHelp(Type instanceType, object instance, ImmutableArray<RegisteredArgument> registeredArguments)
    {
        var positionalArguments = registeredArguments.Where(argument => argument.Attribute.Position > 0)
            .OrderBy(argument => argument.Attribute.Position)
            .ToImmutableArray();

        var nonPositionalRequiredArguments = registeredArguments
            .Where(argument => TypeHelper.IsArgumentRequired(argument.Attribute, argument.Member))
            .OrderBy(argument => argument.Attribute.Name)
            .ToImmutableArray();

        var optionalArguments = registeredArguments.Where(argument => !TypeHelper.IsArgumentRequired(argument.Attribute, argument.Member))
            .OrderBy(argument => argument.Attribute.Name)
            .ToImmutableArray();

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
            builder.Append(SplitHelpText(descriptor.HelpText)).Append('\n');
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

    private static string SplitHelpText(string description)
    {
        if (description.Length < MaxDescriptionWidth)
        {
            return description;
        }

        string[] words = description.Split(' ');

        var currentLine = new StringBuilder();
        var entireDescription = new StringBuilder();
        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            currentLine.Append(word).Append(Space);
            if (currentLine.Length > MaxDescriptionWidth)
            {
                if (i < words.Length - 1)
                {
                    currentLine.Append("\n\t");
                }

                entireDescription.Append(currentLine);
                currentLine.Clear();
            }
        }

        if (currentLine.Length > 0)
        {
            entireDescription.Append(currentLine);
        }

        return entireDescription.ToString();
    }

    private static ProgramDescriptorAttribute? GetDescriptorAttribute(Type instanceType) => instanceType
        .GetCustomAttribute(typeof(ProgramDescriptorAttribute)) as ProgramDescriptorAttribute;

    private static string FormArgumentHelpText(ArgumentAttribute argument, MemberInfo member, object instance)
    {
        var builder = new StringBuilder();

        if (TypeHelper.IsArgumentRequired(argument, member))
        {
            builder.Append("[Required] ");
        }

        builder.Append(argument.Name);

        if (argument.ShortName != null)
        {
            builder.Append(" | ").Append(argument.ShortName);
        }

        builder.Append("\n\t").Append(SplitHelpText(argument.HelpText));

        if (argument.Position > 0)
        {
            builder.Append("\n\t[Position]: ").Append(argument.Position);
        }

        Type memberType = TypeHelper.GetDeclaredType(member);
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

        return builder.Append('\n').ToString();
    }

    private static string? DefaultValueOf(object instance, MemberInfo member, ArgumentAttribute argument)
    {
        // Since required parameters must be provided by the user there is no point in having a default value.
        if (TypeHelper.IsArgumentRequired(argument, member))
        {
            return null;
        }

        if (IsStruct(TypeHelper.GetDeclaredType(member)))
        {
            return null;
        }

        return TypeHelper.GetValue(instance, member)?.ToString() ?? null;
    }

    private static bool IsStruct(Type memberType)
        => memberType.IsValueType && !memberType.IsEnum && !memberType.IsPrimitive;
}