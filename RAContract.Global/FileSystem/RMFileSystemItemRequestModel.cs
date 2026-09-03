using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.Contract.FileSystem
{
    public class RMFileSystemItemRequestModel
    {
        public string ConnectionId { get; set; }

        public List<Guid> ItemIds { get; set; }
    }

    public class RMFileSystemAdsQueryModel
    {
        public string ConnectionId { get; set; }

        public List<string> AdsIds { get; set; }
    }

    public class RMFileSystemFailedItemPaginationQueryModel
    {
        public string ScopeId { get; set; }

        public string ContinuationToken { get; set; }

        public int PageSize { get; set; } = 1_000;
    }

    public class RMFileSystemFailedItemDeleteModel
    {
        public string ScopeId { get; set; }

        public List<Guid> ItemIds { get; set; }
    }
}
