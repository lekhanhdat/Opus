using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views
{
    public class RMMyHubFolderDashboard
    {
        public Guid NodeId { set; get; }
        public string FullPath { get; set; }

        public DateRange DateRange { get; set; }
        public string PartitionKeyId { get; set; }
        public string TimeZoneId { get; set; }
        public bool IsDaylight {  get; set; }
    }

    public enum DateRange
    {
        Last_7_Days = 0,
        Last_30_Days = 1,
        Three_Month = 2,
        Six_Month = 3,
        Custom = 4,
    }
}
