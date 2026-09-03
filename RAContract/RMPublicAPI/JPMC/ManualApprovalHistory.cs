using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public class ManualApprovalHistory
    {
        public int ExportType { get; set; }
        public long StartDateTime { get; set; }
        public long EndDateTime { get; set; }
    }
}
