# libvideo

libvideo is a .NET library that provides a fast, succinct way to download YouTube videos. It has **NO** external dependencies, and full support for both async and synchronous callers.

libvideo runs about 25% to 75% faster than the main alternative, [YoutubeExtractor](https://github.com/flagbug/YoutubeExtractor). This is mainly due to its substitution of optimized string code in place of YoutubeExtractor's use of full-fledged JSON and query parsers; libvideo also avoids regex for simple string operations.

## On NuGet
To install libvideo, you can run the following command in the Package Manager Console:

    Install-Package VideoLibrary

## Usage

See the [docs](docs/README.md) for instructions.
