SteganographyApp
=====

Steganography is the art of hiding data in plain view by hiding it inside of other seemingly untouched files such as images, audio and video.

The apptly named SteganographyApp is a command line utility that hides data inside of one or more images by modifying the least significant bit (LSB) of the RGB values in each individual pixel

Current Features
---
* .Net Core for cross platform support
* Hiding one file in one or more images
* Encrypting the file's contents before hiding it
* Minifying the file's contents using standard GZip compression
* Supplying random seeds allows to write content to random positions in the image
* Clean function to clear out any saved data on one or more images that currently contain data

SteganographyAppCalculator
---
* Calculate the storage space provided by one or more images
* Calculate the storage space required to store an encypted and/or compressed file

Important Arguments
---
```
Arguments must be specified with and = sign and no spaces.

Example arguments for encoding a file to a set of images: 
    dotnet .\\SteganographApp =-action=encode --images=001.png,002.png --input=FileToEncode.zip --password=Pass1234 --randomSeed=monkey --compress=true

Example arguments for decoding data from a set of images to an output file.
    dotnet .\\SteganpgraphyApp --action=decode --images=001.png,002.png --output=DecodedOutputFile.zip --password=Pass1234 --randomSeed=monkey --compress=true

--action or -a :: Specifies whether to 'encode' a file to a set of images or 'decode' a set of images to a file.
    Value must be either 'encode', 'decode', or 'clean'.
    Clean specifies that all LSBs in the set of images will be overwritten with garbage values.

    When used with the Calculator this value can be 'calculate-encrypted-size' or 'calculate-storage-space'.

--input or -i :: The path to the file to encode if 'encode' was specified in the action argument.

--output or -o :: The path to the output file when 'decode' was specified in the action argument.

--images or -im :: A comma delimited list of paths to images to be either encoded or decoded
    The order of the images affects the encoding and decoding results.

--passsword or -p :: The password to encrypt the input file when 'encode' was specified in the action argument.

--validate or -v :: Specifies whether or not to validate the proper arguments have been given.
    Value must be either 'true' or 'false'

--printStack or -ps :: Specifies whether or not to print the full stack trace if an error occurs.
    Value must either be 'true' or 'false'

--randomSeed or -rs :: Specifies whether or not to randomly read/write the bits to the images.
    If the argument was specified when writing, then the same random argument value must be provided when reading.
    The accuracy and repeatability is not guaranteed between different systems.

--compress :: -c :: Specifies whether or not to compress/decompress the encoded/decoded content.
    Value must be either 'true' or 'false'.
```

3rd Party Libraries
---

This app takes advantage of the following third party nuget packages:

* [Rijindael256](https://github.com/2Toad/Rijndael256)
* [ImageSharp](https://github.com/JimBobSquarePants/ImageSharp)
* [System.ValueTuple](https://www.nuget.org/packages/System.ValueTuple/)

Software Used
---

Visual Studio 2017 Community
 was used to code, build, and test.

Gimp was used to verify changes in the image after encoding and cleaning data.