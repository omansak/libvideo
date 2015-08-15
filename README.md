# libvideo

![icon](icons/icon_200.png)

libvideo is a .NET library that lets you download YouTube videos in a fast, clean way.

## How do I get started?

See our [docs](docs/README.md) for instructions.

## Do you have a NuGet package?

[Yes, we do.](https://www.nuget.org/packages/VideoLibrary) You can install it with this command:

    Install-Package VideoLibrary

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
IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls("https://www.youtube.com/watch?v=vPto6XpRq-U");
VideoInfo video = videoInfos.First();

if (video.RequiresDecryption)
{
    DownloadUrlResolver.DecryptDownloadUrl(video);
}

var videoDownloader = new VideoDownloader(video, Path.Combine("D:/Downloads", video.Title + video.VideoExtension));
videoDownloader.Execute();
```

### Is libvideo *that* much faster than YoutubeExtractor?

Yes, I've even made some [speed tests](tests/SpeedTest/SpeedTest/Program.cs) to make sure of this. libvideo runs around 50% faster than YoutubeExtractor, since it does not use any JSON parsers, query tokenizers, or regex. As a result, it's much more performant and has zero dependencies.

## Do you accept donations?

libvideo does not accept donations, but please [help out](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=daume%2edennis%40gmail%2ecom&lc=US&item_name=YoutubeExtractor&no_note=0&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHostedGuest) the creator of the original library, YoutubeExtractor. His project was an essential part in creating libvideo, and I don't want to cut off his funds.
