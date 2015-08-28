# Documentation

Here you'll find a more in-depth explanation of the libvideo APIs.

To get information about a video:

```csharp
string uri = "https://www.youtube.com/watch?v=vPto6XpRq-U"; // URL we want to use
Video video = YouTubeService.Default.GetVideo(uri); // here we "parse" a Video object from the given URL

string title = video.Title; // gets the title
string fileExtension = video.FileExtension; // file extension
string fullName = video.FullName; // same thing as title + fileExtension, but provided for convenience
int resolution = video.Resolution; // resolution
// etc.
```

You can download it like this:

```csharp
byte[] bytes = video.GetBytes(); // gets the binary contents of the video
Stream stream = video.Stream(); // you can stream it as well
```

And save it to a file:

```csharp
File.WriteAllBytes(@"C:\" + fullName, bytes);
```

---

## Advanced

YouTube exposes multiple videos for each URL- e.g. when you change the resolution of a video, you're actually watching a different video. libvideo supports downloading multiple of them:

```csharp
IEnumerable<Video> videos = service.GetAllVideos(uri);
```

We also have full support for async:

```csharp
Video video = await service.GetVideoAsync(uri);
IEnumerable<Video> videos = await service.GetAllVideosAsync(uri);
byte[] contents = await video.GetBytesAsync();
```

Lastly, you should be aware that for every time you visit YouTube a new `HttpClient` is created and disposed. To avoid this, use `SingleClientService`:

```csharp
using (var service = new ClientService(new YouTubeService())) // put this in a using block to not leak resources
{
    service.GetVideo(uri);
    service.GetVideo("[some other video]"); // the service's HttpClient is reused here, saving memory and reducing GC pressure
}
```

Likewise, if you'd like to reuse `HttpClients` when downloading a video, use `VideoClient`:

```csharp
using (var client = new VideoClient())
{
    client.GetBytes(video);
    await client.StreamAsync(video);
}
```

---

That's it, enjoy! If you're looking for more features, feel free to raise an issue and we can discuss it with you.
