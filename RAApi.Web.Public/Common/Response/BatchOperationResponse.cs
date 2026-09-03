using AvePoint.RA.Contract.Object;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Api.Web.Public.Common.Response
{
    public class BatchOperationResponse
    {
        public int TotalCount { get; set; }

        public int SucceededCount { get; set; }

        public int FailedCount { get; set; }

        public List<BatchOperationItemResponse> Items { get; set; }
    }

    public class BatchOperationItemResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string Id { get; set; }
    }

    public static class BatchOperationResponseFactory
    {
        public static BatchOperationResponse Create(IEnumerable<RAReturnMessage> results, string successMessage = "Operation completed successfully.", string failureMessage = "Operation failed.")
        {
            var resultList = results?.ToList() ?? [];
            return new BatchOperationResponse
            {
                TotalCount = resultList.Count,
                SucceededCount = resultList.Count(result => result?.MessageType == RAMessageType.Successful),
                FailedCount = resultList.Count(result => result?.MessageType != RAMessageType.Successful),
                Items = resultList.Select(result => new BatchOperationItemResponse
                {
                    Success = result?.MessageType == RAMessageType.Successful,
                    Message = result?.MessageType == RAMessageType.Successful
                        ? successMessage
                        : string.IsNullOrWhiteSpace(result?.ErrorMessage) ? failureMessage : result.ErrorMessage,
                    Id = result?.MessageType == RAMessageType.Successful ? result.Extension : null
                }).ToList()
            };
        }
    }
}

