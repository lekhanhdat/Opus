using AvePoint.RA.Contract.Explorer;
using System;

namespace AvePoint.RA.Contract.FileSystem
{
    public class RMFileSystemAudit
    {
        public string ConnectionGroupId { get; set; }

        public string ConnectionId { get; set; }

        public Guid ItemId { get; set; }

        public FSJPMCAuditLevel Level { get; set; }

        public string OriginPath { get; set; }

        public string TargetPath { get; set; }
    }
}
