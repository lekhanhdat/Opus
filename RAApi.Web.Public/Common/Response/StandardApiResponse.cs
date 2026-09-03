namespace AvePoint.RA.Api.Web.Public.Common.Response
{
    public class StandardApiResponse
    {
        public int statusCode { get; set; }

        public string message { get; set; }

        public object data { get; set; }

        public static StandardApiResponse Success(object data, string message = "OK")
        {
            return new StandardApiResponse
            {
                statusCode = (int)ApiResponseStatusCode.OK,
                message = message,
                data = data
            };
        }

        public static StandardApiResponse Error(ApiResponseStatusCode statusCode, string message, object data = null)
        {
            return new StandardApiResponse
            {
                statusCode = (int)statusCode,
                message = message,
                data = data
            };
        }

        public static StandardApiResponse Error(int statusCode, string message, object data = null)
        {
            return new StandardApiResponse
            {
                statusCode = statusCode,
                message = message,
                data = data
            };
        }
    }
}

