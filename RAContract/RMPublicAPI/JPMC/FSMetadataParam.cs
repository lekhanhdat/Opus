using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public class FSMetadataParam
    {
        public string FullPath { get; set; }
    }

    public class FSMetadataByCategoryParam
    {
        public string FullPath { get; set; }

        public FSMetadataCategory Category { get; set; }

        public string ClassCode { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
    }

    public enum FSMetadataCategory
    {
        None,
        Created = 1,
        Modified = 2,
        Accessed = 3,
        ClassCode = 4,
        Destroyed = 5,
        All = 6,
    }

    public class FSMetadata
    {
        public long TotalSizeActive { get; set; }
        public long FolderActiveCount { get; set; }
        public long FileActiveCount { get; set; }
        public long FolderDestroyedCount { get; set; }
        public long FileDestroyedCount { get; set; }
        public long FolderAllCount { get; set; }
        public long FileAllCount { get; set; }
    }

    public class FSFileCount
    {
        public long FileCount { get; set; }
    }
}
