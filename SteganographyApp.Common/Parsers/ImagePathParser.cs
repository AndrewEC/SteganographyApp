namespace SteganographyApp.Common.Parsers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using Microsoft.Extensions.FileSystemGlobbing;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Static utility class to help parse out a list of paths to the images provided in the
/// users arguments.
/// </summary>
public static class ImagePathParser
{
    /// <summary>
    /// Parses a list of image paths from the original input string.
    /// </summary>
    /// <param name="value">The string to be parsed in the list of image paths. Can be a comma
    /// delimited list of globs.</param>
    /// <returns>A list of the files identified by the matching glob patterns. This method
    /// minimally ensures that each file exists and is a file and not a directory.</returns>
    public static ImmutableArray<string> ParseImages(string value)
    {
        var images = new List<string>(RetrievePathsByGlobs(value));
        images.AddRange(GetAbsolutePaths(value));
        var imagePaths = images.ToImmutableArray();
        ValidateImagePaths(imagePaths);
        return imagePaths;
    }

    private static string[] RetrievePathsByGlobs(string value)
    {
        var globs = value.Split(",").Where(path => !Path.IsPathFullyQualified(path)).ToArray();
        if (globs.Length == 0)
        {
            return [];
        }

        var matcher = new Matcher();
        matcher.AddIncludePatterns(globs);
        return matcher.GetResultsInFullPath(Directory.GetCurrentDirectory()).ToArray();
    }

    private static string[] GetAbsolutePaths(string value) => value.Split(",").Where(Path.IsPathFullyQualified).ToArray();

    private static void ValidateImagePaths(ImmutableArray<string> imagePaths)
    {
        if (imagePaths.Length == 0)
        {
            throw new ArgumentValueException($"No images could be found.");
        }

        VerifyAllImagesExist(imagePaths);

        VerifyAllImagesAreUnique(imagePaths);
    }

    private static void VerifyAllImagesExist(ImmutableArray<string> imagePaths)
    {
        var fileProxy = ServiceContainer.GetService<IFileIOProxy>();

        string? missingImagePath = imagePaths.Where(path => !fileProxy.IsExistingFile(path))
            .FirstOrDefault();

        if (missingImagePath != null)
        {
            throw new ArgumentValueException($"The file specified could not be read: [{missingImagePath}]");
        }
    }

    private static void VerifyAllImagesAreUnique(ImmutableArray<string> imagePaths)
    {
        var uniqueImages = new HashSet<string>();
        foreach (string imagePath in imagePaths)
        {
            if (!uniqueImages.Add(imagePath))
            {
                throw new ArgumentValueException($"Two or more paths point to the same image of: [{imagePath}]");
            }
        }
    }
}