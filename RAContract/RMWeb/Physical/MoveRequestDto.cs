using AvePoint.RA.Contract.Object.RealTime;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.Physical
{
    public class MoveRequestDto
    {
        [DataMember(EmitDefaultValue = false)]
        public List<RequestFileDto> Items { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PhysicalMoveOption MoveDto { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Comment { get; set; }
    }
}
