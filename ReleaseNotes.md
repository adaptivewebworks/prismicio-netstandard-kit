# Prismic .NET Standard Release Notes

## v2.0.0
* Breaking Change
    Remove call to `GetAll` method inside of `Get` to prevent recursive parses of all document fragments.
* Automatically add current reference in when HttpContext is accessible.
    Check in the following order Preview => Experiment => Master
* Cache Api for the life of an HttpContext if the HttpContextAccessor has an HttpContext.
* Support boolean fragments
* Fix caching bug


