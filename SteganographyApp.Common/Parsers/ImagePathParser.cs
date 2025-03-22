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
        string[] globs = [.. value.Split(",").Where(path => !Path.IsPathFullyQualified(path))];
        if (globs.Length == 0)
        {
            return [];
        }

        var matcher = new Matcher();
        matcher.AddIncludePatterns(globs);
        return [.. matcher.GetResultsInFullPath(Directory.GetCurrentDirectory())];
    }

    private static string[] GetAbsolutePaths(string value)
        => [.. value.Split(",").Where(Path.IsPathFullyQualified)];

    private static void ValidateImagePaths(ImmutableArray<string> imagePaths)
    {
        if (imagePaths.Length == 0)
        {
            throw new ArgumentValueException($"No images could be found.");
        }

        var fileProxy = ServiceContainer.GetService<IFileIOProxy>();

        HashSet<string> existingPaths = [];
        foreach (string imagePath in imagePaths)
        {
            if (!fileProxy.IsExistingFile(imagePath))
            {
            throw new ArgumentValueException($"The file specified could not be read: [{imagePath}]");
            }

            if (!existingPaths.Add(imagePath))
            {
                throw new ArgumentValueException($"Two or more paths point to the same image of: [{imagePath}]");
            }
        }
    }
}