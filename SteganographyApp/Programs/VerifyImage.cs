// <auto-generated/>
#nullable enable
namespace SteganographyApp;

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Immutable;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.Logging;
using System.Text;

[ProgramDescriptor(
    "Verify the provided cover images can be safely used for encoding and decoding information."
        + " This works by creating a copy of the image, writing some random data to the image,"
        + " saving those changes, reading data from the image, then confirming the data read matches"
        + " the original data written."
)]
internal sealed class VerifyImagesArguments : IArgumentConverter
{
    [Argument(
        "CoverImages",
        position: 1,
        helpText: "The list of images to verify."
            + " This parameter can be a comma delimited list of globs with the current directory as the root directory from which files will be matched.",
        example: "*.png,*.webp"
    )]
    public ImmutableArray<string> CoverImages = new ImmutableArray<string>();

    [Argument("--logLevel", "-l", helpText: "The log level to determine which logs will feed into the log file.")]
    public LogLevel LogLevel = LogLevel.None;

    public IInputArguments ToCommonArguments()
    {
        RootLogger.Instance.EnableLoggingAtLevel(LogLevel);
        return new CommonArguments
        {
            CoverImages = CoverImages,
        };
    }
}

internal sealed class TempFileBackup : AbstractDisposable
{
    private readonly ILogger logger = new LazyLogger<TempFileBackup>();

    public TempFileBackup(string sourcePath)
    {
        backupPath = DetermineCopyPath(sourcePath);
        logger.Debug("Creating backup copy of file [{0}] at [{1}]", sourcePath, backupPath);
        File.Copy(sourcePath, backupPath);
        this.originalFilePath = sourcePath;
    }

    private readonly string backupPath;

    private readonly string originalFilePath;

    private string DetermineCopyPath(string sourcePath)
    {
        string absolutePath = Path.GetFullPath(sourcePath);
        string extension = Path.GetExtension(absolutePath);
        return Path.ChangeExtension(absolutePath, ".temp" + extension);
    }

    protected override void DoDispose() => RunIfNotDisposed(() => {
        logger.Debug("Deleting modified file: [{0}]", originalFilePath);
        File.Delete(originalFilePath);
        logger.Debug("Restoring file [{0}] from [{1}]", originalFilePath, backupPath);
        File.Move(backupPath, originalFilePath);
    });
}

internal sealed class VerifyImagesCommand : Command<VerifyImagesArguments>
{
    private ILogger logger = new LazyLogger<VerifyImagesCommand>();

    public override string GetName() => "verify";

    public override void Execute(VerifyImagesArguments args)
    {
        var arguments = args.ToCommonArguments();
        Console.WriteLine($"Identified [{args.CoverImages.Length}] images to verify.");
        var tracker = ProgressTracker.CreateAndDisplay(arguments.CoverImages.Length, "Verifying image.", "Finished verifying all images.");
        var failedValidation = new List<string>();
        foreach (string path in args.CoverImages)
        {
            Console.WriteLine($"Verifying image: [{path}]");
            if (!IsImageValid(path, arguments))
            {
                failedValidation.Add(path);
            }
            tracker.UpdateAndDisplayProgress();
        }
        PrintFailed(failedValidation);
    }

    private bool IsImageValid(string path, IInputArguments arguments)
    {
        try
        {
            string writtenBinary = GenerateBinaryString(path);
            using (var copy = new TempFileBackup(path))
            {
                WriteToImage(writtenBinary, arguments);
                string readBinary = ReadFromImage(writtenBinary, arguments);
                if (writtenBinary != readBinary)
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception e)
        {
            logger.Error("Failed to validate image [{0}]. Cause: [{1}]", path, e.Message);
            return false;
        }
    }

    private void PrintFailed(List<string> failedValidation)
    {
        if (failedValidation.Count == 0)
        {
            Console.WriteLine("Successfully validated all images.");
            return;
        }
        Console.WriteLine("The following images failed validation:");
        foreach (var failed in failedValidation)
        {
            Console.WriteLine($"\t[{failed}]");
        }
    }

    private string GenerateBinaryString(string path)
    {
        using (var image = Injector.Provide<IImageProxy>().LoadImage(path))
        {
            long bitCount = Calculator.CalculateStorageSpaceOfImage(image.Width, image.Height, 1) / 2;
            logger.Trace("Generating binary string with a length of: [{0}]", bitCount);

            var random = new Random();
            var builder = new StringBuilder();
            for (long i = 0; i < bitCount; i++)
            {
                builder.Append(random.Next(10) % 2 == 0 ? '0' : '1');
            }
            return builder.ToString();
        }
    }

    private void WriteToImage(string binaryData, IInputArguments arguments)
    {
        using (var stream = new ImageStore(arguments).OpenStream(StreamMode.Write))
        {
            stream.WriteContentChunkToImage(binaryData);
        }
    }

    private string ReadFromImage(string expectedData, IInputArguments arguments)
    {
        using (var stream = new ImageStore(arguments).OpenStream(StreamMode.Read))
        {
            return stream.ReadContentChunkFromImage(expectedData.Length);
        }
    }
}