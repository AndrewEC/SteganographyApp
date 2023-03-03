namespace SteganographyApp.Common.Tests
{
    using System;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Arguments.Commands;

    [TestFixture]
    public class CliProgramTests
    {
        private CliProgram program;

        [SetUp]
        public void SetUp()
        {
#pragma warning disable SA1009
            program = CliProgram.Create(
                Commands.Group(
                    Commands.From<GenericArguments>("function", (args) => LastRun.Instance.Name = LastRunName.Function),
                    Commands.Lazy<SimpleCommand>(),
                    Commands.Alias("aliased", Commands.From<GenericArguments>("no-alias", (args) => LastRun.Instance.Name = LastRunName.Aliased)),
                    Commands.Group(
                        "duplicate",
                        Commands.From<GenericArguments>("function", (args) => LastRun.Instance.Name = LastRunName.Function),
                        Commands.From<GenericArguments>("function", (args) => LastRun.Instance.Name = LastRunName.Function)
                    )
                )
            );
#pragma warning restore SA1009
        }

        [TearDown]
        public void TearDown()
        {
            LastRun.Instance.Name = LastRunName.None;
        }

        [Test]
        public void TestFunctionBasedCommand()
        {
            var arguments = new string[] { "function", "--number", "10" };
            program.Execute(arguments);
            Assert.AreEqual(LastRunName.Function, LastRun.Instance.Name);
        }

        [Test]
        public void TestBaseCommand()
        {
            var arguments = new string[] { "base", "--number", "10" };
            program.Execute(arguments);
            Assert.AreEqual(LastRunName.BaseCommand, LastRun.Instance.Name);
        }

        [Test]
        public void TestAliasedCommand()
        {
            var arguments = new string[] { "aliased", "--number", "10" };
            program.Execute(arguments);
            Assert.AreEqual(LastRunName.Aliased, LastRun.Instance.Name);
        }

        [Test]
        public void TryExecuteWithDuplicateNames()
        {
            var arguments = new string[] { "duplicate", "function", "--number", "10" };
            Exception error = program.TryExecute(arguments);
            Assert.NotNull(error);
        }

        [Test]
        public void TryExecuteWithInvalidSubCommandName()
        {
            var arguments = new string[] { "invalid", "--number", "10" };
            Exception error = program.TryExecute(arguments);
            Assert.NotNull(error);
        }
    }

    internal sealed class LastRun
    {
        public static readonly LastRun Instance = new LastRun();

        private LastRun() { }

        public LastRunName Name { get; set; }
    }

    internal enum LastRunName
    {
        None,
        Function,
        BaseCommand,
        Aliased,
    }

    internal sealed class GenericArguments
    {
        [Argument("--number")]
        public int Number;
    }

    internal sealed class SimpleCommand : Command<GenericArguments>
    {
        public override void Execute(GenericArguments args)
        {
            LastRun.Instance.Name = LastRunName.BaseCommand;
        }

        public override string GetName() => "base";
    }
}