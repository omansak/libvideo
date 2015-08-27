# Documentation

Here you'll find a more in-depth explanation of the libvideo APIs.

The entry point for most of the API is in `YouTubeService`:

```csharp
var service = YouTubeService.Default;
```

To dowload a video:

```csharp
string uri = "https://www.youtube.com/watch?v=vPto6XpRq-U";
Video video = service.GetVideo(uri);
```

To get information about it:

```csharp
string title = video.Title;
string fileExtension = video.FileExtension;
string fullName = video.FullName; // essentially the same thing as title + fileExtension, provided for convenience
byte[] contents = video.GetBytes(); // gets binary contents of video
Stream stream = video.Stream(); // stream the video
// We also support more advanced info like audio bitrate, resolution, etc.
```

To save it to disk:

```csharp
File.WriteAllBytes(@"C:\" + fullName, contents);
```

---

## Advanced

YouTube actually exposes multiple videos for each URL- e.g. when you change the resolution of a video, you're watching a *different video*. libvideo supports downloading more than one of them:

```csharp
IEnumerable<Video> videos = service.GetAllVideos(uri);
```

We also have full support for async code:

```csharp
Video video = await service.GetVideoAsync(uri);
IEnumerable<Video> videos = await service.GetAllVideosAsync(uri);
byte[] contents = await video.GetBytesAsync();
```

Lastly, you should be aware that for every time you download a video a new `HttpClient` is created and disposed. To avoid this, use `SingleClientService`:

```csharp
using (var service = new SingleClientService(YouTubeService.Default)) // be sure to put this in a using block to not leak memory
{
    service.Download(uri);
    service.Download("[some other video]"); // the service's HttpClient is reused here, saving memory and reducing GC pressure
}
```

---

That's it. We hope you enjoy libvideo! If you're looking for more features, feel free to raise an issue and we'll discuss it with you.
