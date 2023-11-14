namespace MiniWebServer.Abstractions
{
    public enum HttpResponseCodes
    {
        OK = 200,
        PartialContent = 206,
        TemporaryRedirect = 307,
        PermanentRedirect = 308,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        InternalServerError = 500,
        NotImplemented = 501,
    }
}
