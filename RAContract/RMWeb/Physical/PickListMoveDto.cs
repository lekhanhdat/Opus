using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Physical
{
    public class PickListMoveDto
    {
        [DataMember]
        public string ItemName { get; set; }
        [DataMember]
        public string UniqueId { get; set; }
        [DataMember]
        public string ApproveBy { get; set; }
        [DataMember]
        public string HomeLocation { get; set; }
        [DataMember]
        public string DestinationLocation { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public string Comment { get; set; }

    }
    [DataContract]
    public class PickListMoveResultDto
    {
        [DataMember]
        public List<PickListMoveDto> Datas { get; set; }
        [DataMember]
        public int TotalCount { set; get; }
    }
    public class PickListMoveParam
    {
        [DataMember]
        public string SearchText { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public PickMoveFilterOption FilterOptions { get; set; }
    }
    [DataContract]
    public class PickMoveListParam
    {
        [DataMember]
        public bool IsSelectAll { get; set; }
        [DataMember]
        public bool IsContainerLevel { get; set; }
        [DataMember]
        public List<Guid> SelectedItemIds { get; set; }
        [DataMember]
        public string SearchText { get; set; }
        [DataMember]
        public PickMoveFilterOption FilterOptions { get; set; }
    }
    public class PickMoveListJobMessage
    {
        public PickMoveListParam ActionParam { get; set; }
        public string LogonUserId { get; set; }
    }
    public class PickMoveFilterOption
    {
        public List<PickMoveStatusType> Status { get; set; }
    }
    [DataContract]
    public enum PickMoveStatusType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Successfull = 1,
        [EnumMember]
        Fail = 2
    }
}
