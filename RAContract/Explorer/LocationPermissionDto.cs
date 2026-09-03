using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract]
    public class LocationPermissionDto
    {
        [DataMember]
        public string LocationId { get; set; }
        [DataMember]
        public bool IsPhysicalAdmin { get; set; }
        [DataMember]
        public bool IsPhysicalEndUser { get; set; }
        [DataMember]
        public bool IsHoldManager { get; set; }
    }
}
