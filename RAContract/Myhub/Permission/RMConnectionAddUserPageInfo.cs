using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Permission
{
    public class RMConnectionAddUserPageInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public List<AOSUserDto> Users { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string StatusMsg { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool Success { get; set; }
    }
}
