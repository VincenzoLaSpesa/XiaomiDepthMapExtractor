# XiaomiDepthMapExtractor
Extracts depth information from photos taken with modern Xiaomi phones

This is still a work in progress!

## Usage

The software runs from commandline with `.\DepthMapExtractor [options] inputfile` where options are:

    -v, --verbose               (Default: true) Write output to stdout.
    -l, --log                   Write to a logfile too.
    -c, --confidence_map        Extract the confidence map
    -C, --confidence_map_raw    Extract the raw confidence map
    -d, --depthmap              (Default: true) Extracts the depthmap as a png
    -D, --depthmap_raw          Extract the raw depthmap
    -s, --sub_images            Extracts all the subimages instead of just the first one
    -i, --input                 Required. Input file
    -o, --output                Required. Output file
    --overwrite                 (Default: false) Never overwrite existing files
    --help                      Display this help screen.
    --version                   Display version information.

A typical call is:

-  `.\DepthMapExtractor Image.jpg` 
-  `.\DepthMapExtractor -i Image.jpg -o Image` 

## Currently tested with

It works with: 

- Xiaomi POCO F1
- Redmi Note 9 Pro

## About Kaitai

This software uses Kaitai for parsing the binary structure of the file.
The kaitai grammar can be found in `xiaomi_depthmap.ksy.yaml` and will be also part of 
https://github.com/kaitai-io/kaitai_struct_formats once finished.

## How to build

From the root folder, it will build in the folder "dist"

`dotnet publish -c Release -o ./dist`

For building with the runtime embedded:

`dotnet publish -r {runtime} -c Release -o ./dist`

Where *runtime* can be:

- win-x64
- linux-x64
- osx-x64
- One of the others listed here https://docs.microsoft.com/en-us/dotnet/core/rid-catalog


## TODO

- At least with one of the lens the depthmap has ha totally different format I still can't parse
- Check compatibility with other models of phones
- Improved the padding, current just mirrors from the available depth image
- Covert the images to other standard format ( like the Google's one)
- Understand the structure of the confidence map
- Documentation of the software
- Documentation of the format in a wiki

## License
This software is released under MIT license 

        MIT License

        Copyright (c) 2020 Vincenzo La Spesa

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.