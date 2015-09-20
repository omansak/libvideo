# Documentation

Here you'll find a more in-depth explanation of our API.

To get information about a video:

```csharp
string uri = "https://www.youtube.com/watch?v=vPto6XpRq-U"; // URL we want to use
var youTube = YouTube.Default;
var video = youTube.GetVideo(uri); // here we "parse" a YouTubeVideo object from the given URL

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
IEnumerable<YouTubeVideo> videos = youTube.GetAllVideos(uri);
```

We also have full support for async:

```csharp
var video = await youTube.GetVideoAsync(uri);
var videos = await youTube.GetAllVideosAsync(uri);
var contents = await video.GetBytesAsync();
```

In addition, you should be aware that for every time you visit YouTube a new `HttpClient` is created and disposed. To avoid this, use the `Client` class:

```csharp
using (var cli = Client.For(new YouTube())) // put this in a using block to not leak resources
{
    cli.GetVideo(uri);
    cli.GetVideo("[some other video]"); // the service's HttpClient is reused here, saving memory and reducing GC pressure
}
```

Likewise, if you'd like to reuse `HttpClients` when downloading a video, use `VideoClient`.

```csharp
using (var cli = new VideoClient())
{
    cli.GetBytes(video);
    await cli.StreamAsync(video);
}
```

### Custom HTTP Configurations

If you need to custom-configure the `HttpClient` for some reason- maybe you need to increase the timeout length, or add credentials, or use [a different message handler](https://github.com/paulcbetts/ModernHttpClient)- fear not. Simply derive your class from `YouTube` and configure as necessary:

```csharp
class MyYouTube : YouTube
{
    protected override HttpMessageHandler MakeHandler()
    {
        return new BlahBlahMessageHandler();
    }
    
    protected override HttpClient MakeClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(12345);
        };
    }
}
```

Use like so:

```csharp
var youTube = new MyYouTube();
youTube.GetVideo("foo");

// --- OR ---

using (var cli = Client.For(new MyYouTube()))
{
    // ...
}
```

Note that this does not change the HTTP behavior when downloading the video itself. To do that, inherit from `VideoClient`:

```csharp
class MyVideoClient : VideoClient
{
    protected override HttpMessageHandler MakeHandler() { ... }
    protected override HttpClient MakeClient(HttpMessageHandler handler) { ... }
}
```

And to use it:

```csharp
using (var cli = new MyVideoClient())
{
    byte[] contents = cli.GetBytes(video);
}
```

---

That's it, enjoy! If you're looking for more features, feel free to raise an issue and we can discuss it with you.
