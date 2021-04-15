# libvideo

![icon](icons/icon_200.png)

[![NuGet](https://img.shields.io/nuget/dt/VideoLibrary.svg)](https://www.nuget.org/packages/VideoLibrary)
[![NuGet](https://img.shields.io/nuget/v/VideoLibrary.svg)](https://www.nuget.org/packages/VideoLibrary)
[![license](https://img.shields.io/github/license/i3arnon/libvideo.svg)](LICENSE)
[![Join the chat at https://discord.gg/SERVhPp](https://user-images.githubusercontent.com/7288322/34429152-141689f8-ecb9-11e7-8003-b5a10a5fcb29.png)](https://discord.gg/SERVhPp)

libvideo (aka VideoLibrary) is a modern .NET library for downloading YouTube videos. It is portable to most platforms and is very lightweight.

## Installation

You can grab a copy of the library [on NuGet](https://www.nuget.org/packages/VideoLibrary) by running:

    Install-Package VideoLibrary

Alternatively, you can try building the repo if you like your assemblies extra-fresh.

## Supported Platforms
- .NET Core Web/Console/ClassLibrary
- .NET Framework 4.5+
- Windows 10 Universal apps
- Portable Class Libraries
- Xamarin.iOS
- Xamarin.Android
- Mono (Mac/Linux)
- Windows 8.1 and 8.0
- Windows Phone 8.1

## Getting Started

Here's a small sample to help you get familiar with libvideo:

```csharp
using VideoLibrary;

void SaveVideoToDisk(string link)
{
    var youTube = YouTube.Default; // starting point for YouTube actions
    var video = youTube.GetVideo(link); // gets a Video object with info about the video
    File.WriteAllBytes(@"C:\" + video.FullName, video.GetBytes());
}
```

Or, if you use Visual Basic:

```vbnet
Imports VideoLibrary

Sub SaveVideoToDisk(ByVal link As String)
     Dim video = YouTube.Default.GetVideo(link)
     File.WriteAllBytes("C:\" & video.FullName, video.GetBytes())
End Sub
```

If you'd like to check out some more of our features, take a look at our [docs](docs/README.md). You can also refer to our [example application](samples/Valks/Valks/Program.cs) (named Valks, yes, I know, it's a silly name) if you're looking for a more comprehensive sample.

## License

libvideo is licensed under the [BSD 2-clause license](LICENSE).
