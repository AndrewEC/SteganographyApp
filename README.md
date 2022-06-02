SteganographyApp
=====

Steganography is the art of hiding data in plain view by hiding it inside of other seemingly untouched files such as image, audio, and video files.

The apptly named SteganographyApp is a command line utility that hides data inside of one or more images by modifying the least significant bit (LSB) of the RGB values in each individual pixel.

Important
---
When you are hiding data inside in image it is important that the image uses a lossless format.
Images such as BMP, and PNG, are both lossless and supported by the ImageSharp processing library.
Using other formats, such as JPG, will most likely cause data loss when saving any modifications
to the image.

Using the Tools
---
Execute any of the tools using the relevant dotnet command such as `dotnet SteganographyApp.dll -h`.

Use the `-h` or `--help` flags to get detailed information on the arguments available for each of the tools provided.

SteganographyApp
---
* Hide one file in one or more images
* Encrypt the file's contents using AES-256
* Minify the file's contents using standard GZip compression
* Further obfuscate the file's contents by adding in dummy data and randomizing the order in which the data is stored
* Clean function to randomize the LSBs of one or more images

SteganographyApp.Calculator
---
* Calculate the storage space provided by one or more images
* Calculate the storage space required to store an encypted and/or compressed file

SteganographyApp.Converter
---
* Batch convert lossy images such as JPG to the lossless PNG format

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
* [System.ValueTuple](https://www.nuget.org/packages/System.ValueTuple/) - Tuples!
* [Coverlet](https://github.com/tonerdo/coverlet) - Measure, and subsequently generate reports for, unit test coverage
* [StrykerNet](https://github.com/stryker-mutator/stryker-net) - Mutation testing framework
* [ReportGenerator](https://github.com/danielpalme/ReportGenerator) - Generates an Html report from the mutation test results
* [NUnit](https://github.com/nunit/nunit) - Unit testing framework
* [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) - Benchmarking framework
