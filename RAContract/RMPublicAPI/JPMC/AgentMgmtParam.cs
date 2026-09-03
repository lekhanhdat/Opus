using System;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public class AgentQueryParam
    {
        public int PageSize { get; set; } = 15;
        public int PageIndex { get; set; } = 0;
        public string SearchValue { get; set; }
        public string SortBy { get; set; }
        public bool IsAscending { get; set; } = true;
        public string DataCenterName { get; set; }
    }

    public class AgentCreateParam
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Id { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public int SourceType { get; set; } = 1;

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string InstallationCode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string AuthCode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ClientId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid CertificateId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool CollectLog { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DCInternalName { get; set; }
    }

    public class AgentUpdateParam
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool CollectLog { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DCInternalName { get; set; }
    }

    public class AgentActionParam
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Id { get; set; }
    }

    public class AgentJobLimitParam
    {
        [DataMember(EmitDefaultValue = false)]
        public int JobLimit { get; set; }
    }

}