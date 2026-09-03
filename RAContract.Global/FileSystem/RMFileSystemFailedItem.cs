using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.Contract.FileSystem
{
    public class RMFileSystemFailedItemPagination
    {
        public string ContinuationToken { get; set; }

        public List<RMFileSystemFailedItem> FailedItems { get; set; }
    }

    public class RMFileSystemFailedItem
    {
        public Guid ItemId { get; set; }

        public Guid FailedRerunId { get; set; }

        public string FullPath { get; set; }

        public string ScopeId { get; set; }
        
        public string JobId { get; set; }

        public override bool Equals(object obj)
        {
            if(obj == null || !(obj is RMFileSystemFailedItem item)) return false;
            return this.FailedRerunId == item.FailedRerunId;
        }

        public override int GetHashCode()
        {
            return this.FailedRerunId.GetHashCode();
        }
    }
}
