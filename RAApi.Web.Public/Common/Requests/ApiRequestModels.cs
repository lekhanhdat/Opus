using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Enums;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Api.Web.Public.Common.Requests
{
    public sealed class CreateAgentRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CollectLog { get; set; }
        public string DataCentreName { get; set; }

        public AgentCreateParam ToContract() => new()
        {
            Name = Name,
            Description = Description,
            CollectLog = CollectLog,
            DCInternalName = DataCentreName
        };
    }

    public sealed class UpdateAgentRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CollectLog { get; set; }
        public string DataCentreName { get; set; }

        public AgentUpdateParam ToContract(Guid id) => new()
        {
            Id = id,
            Name = Name,
            Description = Description,
            CollectLog = CollectLog,
            DCInternalName = DataCentreName
        };
    }

    public sealed class UpdateAgentJobLimitRequest
    {
        public int Count { get; set; }

        public AgentJobLimitParam ToContract() => new() { JobLimit = Count };
    }

    public sealed class ConnectionGroupRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AccessConnectionType AccessConnectionType { get; set; }
        public List<Guid> ConnectionIds { get; set; }
        public List<Guid> ConnectionIdsToRemove { get; set; }
        public List<Guid> AssignedAgentIds { get; set; }
        public DataCenterType DataCenterType { get; set; }
        public string DataCenterName { get; set; }

        public ConnectionGroupPublic ToContract(Guid? id = null) => new()
        {
            Id = id ?? Id,
            Name = Name,
            Description = Description,
            AccessConnectionType = AccessConnectionType,
            ConnectionIds = ConnectionIds,
            ConnectionIdsToRemove = ConnectionIdsToRemove,
            AssignedAgentIds = AssignedAgentIds,
            DataCenterType = DataCenterType,
            DCInternalName = DataCenterName
        };
    }

    public sealed class ConnectionRequest
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UncPath { get; set; }
        public string AdditionalConnectionId { get; set; }
        public List<ToUserInfo> RecordOwners { get; set; }
        public List<ToUserInfo> InformationOwners { get; set; }
        public int? Monitor { get; set; }

        public ConnectionDto ToContract() => new()
        {
            Id = Id,
            GroupId = GroupId,
            Name = Name,
            Description = Description,
            UNCPath = UncPath,
            AgentId = string.Empty,
            JPMCConnectionId = AdditionalConnectionId,
            RecordOwners = RecordOwners,
            InformationOwners = InformationOwners,
            Monitor = Monitor
        };
    }

    public sealed class FileSystemJobNodeRequest
    {
        public Guid NodeId { get; set; }
        public Guid ConnectionGroupId { get; set; }
        public int Level { get; set; }
        public string FullPath { get; set; }

        public FSJobNodeParam ToContract() => new()
        {
            NodeId = NodeId,
            ConnectionGroupId = ConnectionGroupId,
            Level = Level,
            FullPath = FullPath
        };
    }

    public sealed class FileSystemDashboardSyncRequest
    {
        public Guid GroupId { get; set; }
        public Guid NodeId { get; set; }
        public string FullPath { get; set; }
        public string PartitionKeyId { get; set; }
        public int Level { get; set; }

        public FileSystemMyhubSelectedNodeDto ToContract() => new()
        {
            GroupId = GroupId,
            NodeId = NodeId,
            FullPath = FullPath,
            PartitionKeyId = PartitionKeyId,
            Level = Level
        };
    }

    public sealed class DisposalProcessRequest
    {
        public List<string> NodeIds { get; set; }
        public int IsPause { get; set; }

        public PauseOrResumeReq ToContract() => new() { NodeIds = NodeIds, IsPause = IsPause };
    }

    public sealed class DisposalByClassCodeRequest
    {
        public FileSystemJobNodeRequest JobNodeParam { get; set; }
        public List<Guid> Terms { get; set; }

        public FSDisposalClassCodeParam ToContract() => new()
        {
            JobNodeParam = JobNodeParam?.ToContract(),
            Terms = Terms
        };
    }

    public sealed class RccReportDownloadRequest
    {
        public List<RA.Contract.JPMC.RCCNode> Nodes { get; set; }
        public Guid ConnGroupId { get; set; }
        public Guid ConnectionId { get; set; }
        public string JpmcId { get; set; }
        public int Level { get; set; }
        public RCCReportTimeRangePublic TimeRange { get; set; }
        public bool IsMyHub { get; set; }

        public RCCReportRequestPublic ToContract() => new()
        {
            Nodes = Nodes,
            ConnGroupId = ConnGroupId,
            ConnectionId = ConnectionId,
            JPMCId = JpmcId,
            Level = Level,
            TimeRange = TimeRange,
            IsMyHub = IsMyHub
        };
    }

    public sealed class ApplyClassCodeRequest
    {
        public string ClassCode { get; set; }
        public string CountryCode { get; set; }
        public int RetentionType { get; set; }
        public long StartDate { get; set; }
        public bool ApplyToExistingDoc { get; set; }
        public string FullPath { get; set; }
        public string TermId { get; set; }

        public ApplyClassCodeParam ToContract() => new()
        {
            ClassCode = ClassCode,
            CountryCode = CountryCode,
            RetentionType = RetentionType,
            StartDate = StartDate,
            ApplyToExistingDoc = ApplyToExistingDoc,
            FullPath = FullPath,
            TermId = TermId
        };
    }

    public sealed class ManualApprovalActionRequest
    {
        public List<Guid> NeedActionIds { get; set; }
        public string ApprovalComment { get; set; }
        public string QuickReason { get; set; }
        public ManualApprovalExtendType ExtendType { get; set; }
        public int ExtendNumber { get; set; }
        public DateTime CustomeExtendDate { get; set; }
        public ManualApprovalTab ManualFromTab { get; set; }
        public bool IsEnableFolderView { get; set; }
        public SOApproveDBStatus ActionType { get; set; }
        public bool FromGControl { get; set; }

        public ManualApprovalActionParams ToContract() => new()
        {
            NeedActionIds = NeedActionIds,
            ApprovalComment = ApprovalComment,
            QuickReason = QuickReason,
            ExtendType = ExtendType,
            ExtendNumber = ExtendNumber,
            CustomeExtendDate = CustomeExtendDate,
            ManualFromTab = ManualFromTab,
            IsEnableFolderView = IsEnableFolderView,
            ActionType = ActionType,
            FromGControl = FromGControl,
            PartitionKeyId = string.Empty
        };
    }

    public sealed class ApprovalHistoryExportRequest
    {
        public int ExportType { get; set; }
        public long StartDateTime { get; set; }
        public long EndDateTime { get; set; }

        public ManualApprovalHistory ToContract() => new()
        {
            ExportType = ExportType,
            StartDateTime = StartDateTime,
            EndDateTime = EndDateTime
        };
    }
}
