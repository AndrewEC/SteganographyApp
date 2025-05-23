namespace SteganographyApp.Programs;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;

#pragma warning disable SA1600, SA1402

[ProgramDescriptor("Convert a set of images to either a webp or png format.")]
internal sealed class ConvertArguments : ImageFields
{
    [Argument("--imageFormat", "-i", helpText: "The format the images should be converted to.")]
    public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;

    [Argument("--deleteOriginals", "-d", helpText: "Specify whether the original image files should be deleted after conversion.")]
    public bool DeleteOriginals { get; set; } = false;

    public IInputArguments ToCommonArguments()
    {
        RootLogger.Instance.EnableLoggingAtLevel(LogLevel);
        return new CommonArguments
        {
            CoverImages = CoverImages,
            ImageFormat = ImageFormat,
            DeleteAfterConversion = DeleteOriginals,
        };
    }
}

internal sealed class ConvertImagesCommand : Command<ConvertArguments>
{
    public override string GetName() => "convert";

    public override void Execute(ConvertArguments args)
    {
        var arguments = args.ToCommonArguments();

        Console.WriteLine("Converting [{0}] images.", arguments.CoverImages.Length);
        var tracker = ServiceContainer.CreateProgressTracker(
            arguments.CoverImages.Length, "Converting images", "Finished converting all images")
            .Display();

        var failures = new List<string>();

        var encoder = ServiceContainer.GetService<IEncoderProvider>()
            .GetEncoder(arguments.ImageFormat);

        foreach (string sourceImage in arguments.CoverImages)
        {
            string destinationImage = ReplaceFileExtension(sourceImage, arguments);
            try
            {
                ConvertImage(sourceImage, destinationImage, encoder, arguments);
            }
            catch (Exception e)
            {
                failures.Add($"{sourceImage}: {e.Message}");
                tracker.UpdateAndDisplayProgress();
                continue;
            }

            if (arguments.DeleteAfterConversion)
            {
                File.Delete(sourceImage);
                RenameFile(destinationImage);
            }

            tracker.UpdateAndDisplayProgress();
        }

        PrintFailures(failures);
    }

    private static void ConvertImage(string sourceImage, string detinationImage, IImageEncoder encoder, IInputArguments arguments)
    {
        using (var source = Image.Load<Rgba32>(sourceImage))
        {
            using (var destination = new Image<Rgba32>(source.Width, source.Height))
            {
                for (int i = 0; i < source.Width; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        destination[i, j] = source[i, j];
                    }
                }

                destination.Save(detinationImage, encoder);
            }
        }

        Console.WriteLine($"Saved image [{sourceImage}] to [{detinationImage}]");
    }

    private static void PrintFailures(List<string> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }

        Console.WriteLine("One or more of the specified images could not be converted.");
        Console.WriteLine("Failed to convert: ");
        foreach (string failure in failures)
        {
            Console.WriteLine($"\t{failure}");
        }
    }

    private static string ReplaceFileExtension(string image, IInputArguments arguments)
        => Path.ChangeExtension(image, ".converted." + arguments.ImageFormat.ToString().ToLower(CultureInfo.InvariantCulture));

    private static void RenameFile(string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        int index = fileName.LastIndexOf('.');
        string withoutConverted = fileName.Substring(0, index);
        string newPath = withoutConverted + Path.GetExtension(path);
        File.Move(path, newPath);
        Console.WriteLine($"Renamed image from [{path}] to [{newPath}]");
    }
}

#pragma warning disable SA1600, SA1402