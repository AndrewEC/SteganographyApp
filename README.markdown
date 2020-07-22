SteganographyApp
=====

Steganography is the art of hiding data in plain view by hiding it inside of other seemingly untouched files such as image, audio, and video files.

The apptly named SteganographyApp is a command line utility that hides data inside of one or more images by modifying the least significant bit (LSB) of the RGB values in each individual pixel.

Important
---
When you are hiding data inside in image it is important that the image uses a lossless format.
Images such as BMP, and PNG, are both lossless and supported by the ImageSharp processing library.
Using other formats, such as JPG, can cause data loss when saving any modifications to the image.

Running Tests
---
Install the required global tools.
1. `dotnet tool install --global coverlet.console`
2. `dotnet tool install -g dotnet-reportgenerator-globaltool`
3. `dotnet tool install -g dotnet-stryker`

After installing the global tools you can run the unit tests and generate the coverage reports by executing the script `run_tests.ps1`.
To run and view the mutation test results execute the script `run_mutations.ps1`.

Running Benchmarks
---
From the root directory execute `dotnet run --project ./SteganographyApp.Common.Benchmarks -c release`

Current Features
---
* .Net Core for cross platform support
* Hiding one file in one or more images
* Encrypting the file's contents using AES-256 before hiding it
* Minifying the file's contents using standard GZip compression
* Clean function to clear out any saved data on one or more images that currently contain data

SteganographyApp.Calculator
---
* Calculate the storage space provided by one or more images
* Calculate the storage space required to store an encypted and/or compressed file

Important Arguments
---
Use the -h or --help flag with the Converter, Encoder, and Calculator utilities to view the full list of arguments relavant to that particular tool.

3rd Party Libraries
---

This app takes advantage of the following third party nuget packages:

* [Rijindael256](https://github.com/2Toad/Rijndael256)
* [ImageSharp](https://github.com/JimBobSquarePants/ImageSharp)
* [System.ValueTuple](https://www.nuget.org/packages/System.ValueTuple/)
* [Coverlet](https://github.com/tonerdo/coverlet)
* [StrykerNet](https://github.com/stryker-mutator/stryker-net)
* [ReportGenerator](https://github.com/danielpalme/ReportGenerator)

Software Used
---

Visual Studio 2017 Community was used to code, build, and test the project.

Gimp was used to verify changes in the image after encoding and cleaning data.
