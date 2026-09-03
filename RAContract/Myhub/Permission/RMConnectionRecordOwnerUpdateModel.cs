using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Permission
{
    public class RMConnectionRecordOwnerUpdateModel
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid ConnectionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ToUserInfo> RecordOwners { get; set; } = [];
    }
}
