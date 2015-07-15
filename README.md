# libvideo
libvideo is a .NET library that provides a fast, concise way to download YouTube videos. It has **NO** external dependencies, and full support for both async and synchronous callers.

libvideo runs about 25% faster than the main alternative, [YoutubeExtractor](https://github.com/flagbug/YoutubeExtractor). This is mainly due to its substitution of optimized string code in place of YoutubeExtractor's use of full-fledged JSON and query parsers; libvideo also avoids regex where possible.

## Usage

libvideo has a very simple API. First, make sure you have the appropriate statement at the top of your file.

    using VideoLibrary;

Great! Now let's get started.

The entry point for most of the API is in `YouTubeService`:

    var service = new YouTubeService();

To download a video or get a URI for download, simply type this:

    byte[] bytes = service.Download(videoUri);
    string downloadUri = service.GetUri(videoUri);

YouTube exposes multiple URIs for each video, which vary in quality and size. To download all of them:

    IEnumerable<byte[]> arrays = service.DownloadMany(videoUri);
    IEnumerable<string> downloadUris = service.GetAllUris(videoUri);

## Advanced

If you need more information about the video, such as bitrate or resolution, you can use the following methods:

    Video video = service.GetVideo(videoUri);
    IEnumerable<Video> videos = service.GetAllVideos(videoUri);

The `Video` class enscapulates more detailed information about the video, and includes a `GetBytes()` method for convenience.

---

libvideo has full support for async callers; if you need the asynchronous version of a method, just append `Async` to the name. For example:

    byte[] bytes = service.DownloadAsync(videoUri);
    string downloadUri = service.GetUriAsync(videoUri);

---

If you already have an `HttpClient`, `WebClient`, or `HttpWebRequest` in use and you don't want libvideo to create a new one every time it visits YouTube, don't worry! Just pass in a delegate describing how to download the page:

    using (var client = new WebClient())
    {
        // Do some work with WebClient...
        service.Download(videoUri, uri => client.DownloadString(uri));
    }

This works for asynchronous clients as well:

    using (var client = new HttpClient())
    {
        // Do some work with HttpClient...
        await service.DownloadAsync(videoUri, uri => client.GetStringAsync(uri));
    }

---

If you want to download a video from another site, like **Vimeo** (more are coming), libvideo supports that as well:

    var service = new VimeoService();


