SteganographyApp
=====

Steganography is the art of hiding data in plain view by hiding it inside of other seemingly untouched files such as image, audio, and video files.

The apptly named SteganographyApp is a command line utility that hides data inside of one or more images by modifying the least significant bit (LSB) of the RGB values in each individual pixel.

Important
---
When you are hiding data inside in image it is important that the image uses a lossless format. This tool has support for PNG images and WEBP lossless images. Using a lossy format, like JPG, will cause data loss that can't be recovered by the program.

Using the Tools
---
This project provides a Powershell script called `create_testbed.ps1` that will build the project and copy the resulting binaries and some image
files to the testbed directory.

From the testbed directory you will be able to find the main `SteganographyApp.dll`. You can run the help command, `dotnet SteganographyApp.dll -h`,
to get a list of sub-commands, arguments, and example usage strings.

Use the `-h` or `--help` flags to get detailed information on the arguments available for each of the tools provided.

Running Tests
---
Install the required global tools using the dotnet cli:

> dotnet tool restore

This will install the following test dependencies:
* `coverlet` - For measuring and checking unit test coverage
* `dotnet-stryker` - For executing mutation tests
* `dotnet-reportgenerator-globaltool` - For generating an Html report from the stryker mutation testing results.

After installing the global tools you can run the unit tests and generate the coverage reports by executing the script `run_tests.ps1`.

To run and view the mutation test results execute the script `run_mutations.ps1`.

For some performance benchmark execute `run_benchmarks.ps1`.

3rd Party Libraries
---

This app takes advantage of the following third party nuget packages:

* [ImageSharp](https://github.com/JimBobSquarePants/ImageSharp) - Loading, manipulating, saving, and converting images
* [Coverlet](https://github.com/tonerdo/coverlet) - Measure, and subsequently generate reports for, unit test coverage
* [StrykerNet](https://github.com/stryker-mutator/stryker-net) - Mutation testing framework
* [ReportGenerator](https://github.com/danielpalme/ReportGenerator) - Generates an Html report from the mutation test results
* [NUnit](https://github.com/nunit/nunit) - Unit testing framework
* [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) - Benchmarking framework
* [Mircosoft.Extensions.FileSystemGlobbing](https://docs.microsoft.com/en-us/dotnet/core/extensions/file-globbing) - File glob pattern matching
