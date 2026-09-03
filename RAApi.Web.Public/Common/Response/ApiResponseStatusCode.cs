namespace AvePoint.RA.Api.Web.Public.Common.Response
{
    public enum ApiResponseStatusCode
    {
        OK = 200,
        BadRequest = 400,
        NotFound = 404,
        InternalServerError = 500,
        SomeDataOperationFailed = 10000,
    }
}

