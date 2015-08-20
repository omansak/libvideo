# libvideo

![icon](icons/icon_200.png)

[![Join the chat at https://gitter.im/James-Ko/libvideo](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/James-Ko/libvideo?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

libvideo is a .NET library that lets you download YouTube videos in a fast, clean way.

## Where can I install libvideo?

You can grab a copy of the library [on NuGet](https://www.nuget.org/packages/VideoLibrary) by running:

    Install-Package VideoLibrary

Alternatively, you can try building the repo if you like your assemblies extra-fresh.

## How do I get started?

Here's a small sample to help you get familiar with libvideo:

```csharp
using VideoLibrary;

void SaveVideoToDisk(string link)
{
    var service = new YouTubeService(); // this is the starting point for all of our download actions
    var video = service.GetVideo("https://www.youtube.com/watch?v=vPto6XpRq-U"); // gets a Video object containing information about the video
    string fileExtension = video.Extension; // e.g. ".mp4", ".webm"
    File.WriteAllBytes(@"C:\myvideo" + fileExtension, video.GetBytes());
}
```

If you'd like to check out some more of our features, take a look at our [docs](docs/README.md). You can also refer to our [example application](samples/Valks/Valks/Program.cs) (named Valks, yes, I know, it's a sily name) if you're looking for a more comprehensive sample.

## What platforms do you support?

libvideo supports the following platforms:

- .NET Framework 4.5+
- Windows 10 Universal apps
- Portable Class Libraries
- Windows 8.1 and 8.0
- Windows Phone 8.1 (WinRT)

## What's the difference between libvideo and YoutubeExtractor?

A lot. libvideo:

- can be used in Portable Class Libraries
- supports Windows Runtime projects (Windows 10, Windows 8.1, Windows Phone 8.1, etc.)
- is easier to use (see below)
- is much much faster (see below)
- does not support Silverlight
- does not support FLV audio extraction

### Is libvideo *that* much easier to use?

Yes. Here's how you download a video with libvideo:

```csharp
byte[] bytes = new YouTubeService().Download("https://www.youtube.com/watch?v=vPto6XpRq-U");
File.WriteAllBytes(@"C:\video.mp4", bytes);
```

Here's how you do it with YoutubeExtractor (copied from [here](https://github.com/flagbug/YoutubeExtractor)):

```csharp
string link = "https://www.youtube.com/watch?v=vPto6XpRq-U";
IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
VideoInfo video = videoInfos.First();

if (video.RequiresDecryption)
{
    DownloadUrlResolver.DecryptDownloadUrl(video);
}

var videoDownloader = new VideoDownloader(video, Path.Combine("D:/Downloads", video.Title + video.VideoExtension));
videoDownloader.Execute();
```

### Is libvideo *that* much faster than YoutubeExtractor?

Yes, I've even made some [speed tests](tests/SpeedTest/SpeedTest/Program.cs) to make sure of this. libvideo runs around [4.5x](http://imgur.com/VJAOoj5) faster than YoutubeExtractor, since it does not use any JSON parsers, query tokenizers, or regex. As a result, it's much more performant and has zero dependencies.

## Do you accept donations?

libvideo does not accept donations, but please [help out](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=daume%2edennis%40gmail%2ecom&lc=US&item_name=YoutubeExtractor&no_note=0&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHostedGuest) the creator of YoutubeExtractor. Although my library is (objectively) better than his, I'm not a prick who likes cutting off other people's funds.
