# Changelog

## v1.3.2

- Fix `ArgumentOutOfRangeException` being thrown when deciphering encrypted videos ([#25](https://github.com/i3arnon/libvideo/issues/25))

## v1.3.1

- Fix `ObjectDisposedException` when getting the URL of an encrypted video ([#16](https://github.com/i3arnon/libvideo/issues/16))

## v1.3.0

- Support custom HTTP configuration when downloading videos
 - Fix timeout bug when downloading a large video

## v1.2.0

- Add support for custom HTTP configuration when visiting YouTube

## v1.1.0

- Videos with encrypted signatures are (finally) supported!
- A bug where we would "snip off" parts of the query has been fixed; this should lead to less 403s.
- A libvideo.debug project has been added to give contributors a better debugging experience.
- The API has been rewritten from scratch to become more generic, raising the possibility for other providers (e.g. Vimeo) in the future.
- The XML documentation has been eliminated for reasons of cleanliness; read the docs on GitHub if you want to learn more about the library.
- Breaking changes:
 - `YouTubeService` => `YouTube`
 - `ClientService` => `Client`
 - `Video` => `YouTubeVideo`

## v1.0.0

- First stable release!
- Breaking changes:
 - `SingleClientService` => `ClientService`

## v0.9.0

- Add XML docs to code
- Breaking changes:
 - Unnecessarily visible types (specifically, `IService` and `IAsyncService`) have been made internal.

## v0.8.0

- Support Xamarin.iOS and Xamarin.Android!
- VideoLibrary now matches version numbers with VideoLibrary.Compat

## v0.7.0

- Fix data corruption issues in WinRT
- Breaking changes:
 - A few `ServiceBase` methods have been made internal.

## v0.6.2

- Exclude `libvideo.compat` assembly from VideoLibrary package

## v0.6.1

- Fix installation errors for Windows 10 projects
- Do not pollute packages with NuGet executable

## v0.6.0

- Breaking changes:
 - Remove deprecated methods from public API

## v0.5.0

- Add support for video streaming
- A static field `Default` has been added to `YouTubeService`

## v0.4.2

- Use HTTPS instead of HTTP when visiting YouTube
 - This leads to a 300% boost in performance since we avoid redirects from `HttpClient`

## v0.4.1

- Add support for URL normalization

## v0.4.0

- API and memory allocation improvements
- Breaking changes:
 - Remove support for Vimeo

## v0.3.1

- Initial release, woohoo!
