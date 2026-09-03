using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Multi_Geo.Model
{
    [DataContract]
    public class SyncCommonDataUserInfo
    {
        [DataMember]
        public List<AADAccount> UsersInfo { get; set; }
        [DataMember]
        public string TenantId { get; set; }
    }
}
