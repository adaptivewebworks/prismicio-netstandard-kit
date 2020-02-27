# Prismic .NET Standard Release Notes

## v1.4.0
* Automatically add current reference in when HttpContext is accessible.
    Check in the following order Preview => Experiment => Master
* Cache Api for the life of an HttpContext if the HttpContextAccessor has an HttpContext.
* Support boolean fragments
* Fix caching bug


