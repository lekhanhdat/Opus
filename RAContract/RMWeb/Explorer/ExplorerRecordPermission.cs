using AvePoint.RA.Contract.Explorer;
using System;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    public class ExplorerRecordPermission
    {
        public Guid RecordId { get; set; }
        public SourceFlag ContentSource { get; set; }
    }
}
