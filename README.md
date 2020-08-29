# CSharp-PVRTCgen
Tool for generating random [PVRTC textures](https://en.wikipedia.org/wiki/PVRTC) (with or without PVR header)

## Introduction to this project
This project contains few files that can be used to generate PVRTC textures with random data (PVRTC textures are used by PowerVR GPUs). This is .NET Core compatible release, so it should also work with Mono and other C# compatible environments.

## What is PVRTC and PVR
PVRTC is a lossy fixed-rate texture compression format. It is used in iOS world since all iPhones and iPads have hardware decoding support for PVRTC textures. [More info](http://cdn.imgtec.com/sdk-documentation/PVRTC+%26+Texture+Compression.User+Guide.pdf)

PVR is a file format that stores additional header info (resolution, metadata, etc.) and texture data into single file. [More info](http://cdn.imgtec.com/sdk-documentation/PVR+File+Format.Specification.pdf)

## How to use
With .NET Core you can use following command to generate PVRTC textures
```
dotnet run -resolution 512 -format Transparent4bit -file 512_4bit_transparent.pvr
```
by default PVR version 3.0 header will be written for that file.

There is a simple help in case you need some additional info about parameters
```
dotnet run -help
```

## License
Text in this document and source code files are released into the public domain. See [PUBLICDOMAIN](https://github.com/mcraiha/CSharp-PVRTCgen/blob/master/PUBLICDOMAIN.txt) file.

## Is this useful tool?
Not really. Basically you can use PVRTCgen to generate acid style "art" and/or generate PVRTC textures that have very poor compression properties (e.g. zipping the file only gives small decrease in file size).

## Limitations
- Only supports power of two sizes (e.g. 512x256)
- Does not support newer PVRTC2 format
- Supports Little Endian only (PVR header generation would need adjustments)
- Only generates PVR version 3.0 header (no support for older 2.0 header)

## Visual output
Here are two sample files generated with this tool and then converted to PNG files with [PVRTexTool](https://community.imgtec.com/developers/powervr/tools/pvrtextool/)

![512x512 4bit opaque](https://github.com/mcraiha/CSharp-PVRTCgen/blob/master/samples/512_4bit_opaque.png)  
512x512 4bit opaque

![512x512 4bit transparent](https://github.com/mcraiha/CSharp-PVRTCgen/blob/master/samples/512_4bit_transparent.png)  
512x512 4bit transparent
