# libvideo

![icon](icons/icon_200.png)

[![Join the chat at https://gitter.im/James-Ko/libvideo](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/James-Ko/libvideo?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

libvideo (aka VideoLibrary) is a modern .NET library for downloading YouTube videos. It is portable to most platforms and is very lightweight.

## Installation

You can grab a copy of the library [on NuGet](https://www.nuget.org/packages/VideoLibrary) by running:

    Install-Package VideoLibrary

Alternatively, you can try [building the repo](docs/building.md) if you like your assemblies extra-fresh.

## Getting Started

Here's a small sample to help you get familiar with libvideo:

```csharp
using VideoLibrary;

void SaveVideoToDisk(string link)
{
    var service = YouTubeService.Default; // this is the starting point for all of our download actions
    var video = service.GetVideo(link); // gets a Video object containing information about the video
    File.WriteAllBytes(@"C:\" + video.FullName, video.GetBytes());
}
```

## Supported Platforms

- .NET Framework 4.5+
- Windows 10 Universal apps
- Portable Class Libraries
- Windows 8.1 and 8.0
- Windows Phone 8.1

If you'd like to check out some more of our features, take a look at our [docs](docs/README.md). You can also refer to our [example application](samples/Valks/Valks/Program.cs) (named Valks, yes, I know, it's a silly name) if you're looking for a more comprehensive sample.

## License

libvideo is licensed under the [BSD 2-clause license](bsd.license).

## FAQ

### What's the difference between libvideo and YoutubeExtractor?

libvideo:

- can be used in Portable Class Libraries
- supports WinRT projects (e.g. Windows 10)
- is roughly 400% faster ([yes, it's true](tests/Speed.Test/Speed.Test/Program.cs))

YoutubeExtractor:

- supports Silverlight
- supports Flash audio extraction

### Can I switch from YoutubeExtractor without having to refactor my code?

Absolutely! Check out our [libvideo.compat](https://www.nuget.org/packages/VideoLibrary.Compat/) package. It has the same API as YoutubeExtractor, but uses libvideo as its backend so your application will get a major speed boost.

### Do you accept donations?

Thanks! I'm flattered, but it's not really necessary. If anything, you should donate to the creator of [YoutubeExtractor](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=daume%2edennis%40gmail%2ecom&lc=US&item_name=YoutubeExtractor&no_note=0&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHostedGuest), because this project wouldn't have happened were it not for him.
