using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views
{
    public class ReadAllClassCodeNameReq
    {
        public List<string> PartitionKeyIds { get; set; }
    }
}
