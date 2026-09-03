/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.ControlPlus;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    [DataContract]
    public class ExplorerQueryDto
    {
        [DataMember]
        public bool IsArchived { get; set; }
        [DataMember]
        public ExplorerFilterOption FilterOption { get; set; }
        [DataMember]
        public ExplorerPagingInfo PagingInfo { get; set; }
        /// <summary>
        /// current only physical support it.
        /// </summary>
        [DataMember]
        public List<int> PermissionIds { get; set; }
        [DataMember]
        public bool IsForGlobalSearchJob { get; set; }

    }
    [DataContract]
    public class ExplorerFilterOption
    {
        [DataMember]
        public ExplorerSearchOption SearchOption { get; set; }
        [DataMember]
        public List<SourceFlag> SourceFlags { get; set; }
        [DataMember]
        public bool? HoldStatus { get; set; }
        [DataMember]
        public List<Guid?> TermIds { get; set; }
        [DataMember]
        public bool? WithOutTerms { get; set; }
        #region old
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public Guid? TermId { get; set; }
        [DataMember]
        public SourceFlag SourceFlag { get; set; }
        #endregion
        [DataMember]
        public List<string> Owners { get; set; }
        [DataMember]
        public List<string> CreatedBy { get; set; }
        [DataMember]
        public List<Guid?> RuleIds { get; set; }
        [DataMember]
        public bool? DeclaredRecord { get; set; }
        [DataMember]
        public List<string> FileExtensions { get; set; }
        //public SizeInfo SizeInfo { get; set; }
        [DataMember]
        public DateInfo DateInfo { get; set; }
        [DataMember]
        public List<string> ModifiedBy { get; set; }
        [DataMember]
        public DateInfo ModifiedDateInfo { get; set; }
        [DataMember]
        public List<int> PersmissionScopes { get; set; }
        [DataMember]
        public string OrderColumn { set; get; }
        [DataMember]
        public bool OrderAsc { set; get; }
        [DataMember]
        public List<SPFilterNode> SPNodes { set; get; }
    }

    public class DueDateUtil
    {
        public const long NextJob = -1;
        public const long Pending = -2;
        public static long None = DateTime.MinValue.Ticks;

        public static long ConvertStringDueDate2Long(string dueDateStr)
        {
            switch (dueDateStr)
            {
                case null:
                case "":
                    return DueDateUtil.None;
                case "RM_JS_JM_EndTimePending":
                case "Pending":
                    return DueDateUtil.Pending;
                case "RDM_RecordsExporer_Status_NextJob":
                case "Next Job":
                    return DueDateUtil.NextJob;
                default:
                    long dueDateLong;
                    if (long.TryParse(dueDateStr, out dueDateLong))
                    {
                        DateTime dt = new DateTime(dueDateLong);
                        return dueDateLong;
                    }
                    else
                    {
                        throw new Exception("DueDate can not convert to long...");
                    }
            }

        }
        public static string ConvertLongDueDate2String(long dueDate)
        {
            switch (dueDate)
            {
                case 0:
                    return string.Empty;
                case DueDateUtil.Pending:
                    return "RM_JS_JM_EndTimePending";
                case DueDateUtil.NextJob:
                    return "RDM_RecordsExporer_Status_NextJob";
                default:
                    return dueDate.ToString();
            }
        }

        public static string ConvertLongDueDate2I18NString(long dueDate)
        {
            return dueDate switch
            {
                Pending => I18NEntity.GetString("RM_JS_JM_EndTimePending"),
                NextJob => I18NEntity.GetString("RDM_RecordsExporer_Status_NextJob"),
                _ => string.Empty,
            };
        }
    }
    [DataContract]
    public class DateInfo
    {
        [DataMember]
        public DateCondition Condition { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public string Value1 { get; set; }
        [DataMember]
        public string Value2 { get; set; }
        [DataMember]
        public bool IsDayLight { get; set; }
    }
    [DataContract]
    public enum DateCondition
    {
        [EnumMember]
        BeforeNow = -3,
        [EnumMember]
        Pending = -2,
        [EnumMember]
        NextJob = -1,
        [EnumMember]
        None = 0,
        [EnumMember]
        Before = 1,
        [EnumMember]
        After = 2,
        [EnumMember]
        FromTo = 3, 
        [EnumMember]
        All = 4,
        //用于查询DisposalDueDate到期数据1.DueDate为NextJob 2.DueDate早于当前时间
        [EnumMember]
        NextJobOrOverDue = 5
    }

    public class SizeInfo
    {
        public int Value { get; set; }
        public SizeUnit SizeUnit { get; set; }
        public SizeCondition Condition { get; set; }
    }

    public enum SizeCondition
    {
        None = 0,
        GreaterOrEqualThan = 1,
        LessOrEqualThan = 2
    }

    public enum SizeUnit
    {
        None = 0,
        KB = 1,
        MB = 2,
        GB = 3
    }
    [DataContract]
    public class ExplorerPagingInfo
    {
        [DataMember]
        public string PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int Total { get; set; }
        [DataMember]
        public bool HasNextPage { get; set; }
    }
    [DataContract]
    public class PhysicalExplorerPagingInfo
    {
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int Total { get; set; }
        [DataMember]
        public bool HasNextPage { get; set; }
        [DataMember]
        public string currentBrowserState { get; set; }
    }
    [DataContract]
    public class ExplorerSearchOption
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public int SearchType { get; set; }
    }

    public class ExplorerResultInfo
    {
        public List<BaseRecordDto> Datas { get; set; }
        public ExplorerPagingInfo PagingInfo { get; set; }
    }

    public class ExplorerResultInfoV3 : ExplorerResultInfo
    {
        /// <summary>
        /// check if the search query can be converted to basic search criteria.
        /// </summary>
        public bool CanConvert2BasicSearch { get; set; }

        /// <summary>
        /// check if can do action from GUI after searching
        /// </summary>
        public bool CanDoGlobalAction { get; set; }
        public bool CanDoPhysicalBulkUpdate { get; set; }
    }

    public class PhysicalResultInfo
    {
        public List<PhysicalObjectDto> Datas { get; set; }
        public PhysicalExplorerPagingInfo PagingInfo { get; set; }
    }

    public class DeleteResultInfo
    {
        public bool HasError { get; set; }
        public List<Guid> ErrorDatas { get; set; }
    }

    public enum SearchType
    {
        RecordsId = 1,
        FileName = 2
    }
    [DataContract]
    public class ExplorerSetHoldDto
    {
        [DataMember]
        public string holdId { get; set; }
        [DataMember]
        public ExplorerPagingInfo PagingInfo { get; set; }
    }
    [DataContract]
    public class UpdateRecordsDto
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public List<Guid> ReletedIds { get; set; }
        [DataMember]
        public List<Guid> DeleteReletedIds { get; set; }
        [DataMember]
        public Dictionary<Guid, string> IdNameDict { get; set; }
    }
    [DataContract]
    public class ChangeTermDto
    {
        [DataMember]
        public List<Guid> RecordIds { get; set; }
        [DataMember]
        public List<Guid> EXORecordIds { get; set; }
        [DataMember]
        public List<Guid> PhyRecordIds { get; set; }
        [DataMember]
        public List<Guid> FSRecordIds { get; set; }
        [DataMember]
        public List<Guid> SPOnPremRecordIds { get; set; }
        [DataMember]
        public List<Guid> OneDriveRecordIds { get; set; }
        [DataMember]
        public List<Guid> AzureFileShareRecordIds { get; set; }
        [DataMember]
        public List<Guid> BoxRecordIds { get; set; }
        [DataMember]
        public List<Guid> CustomizeConnectorRecordIds { get; set; }
        [DataMember]
        public List<Guid> GoogleDriveRecordIds { get; set; }
        [DataMember]
        public List<Guid> TeamsRecordIds { get; set; }
        [DataMember]
        public TargetTermInfo TermInfo { get; set; }
        [DataMember]
        public bool OverWriteSubFiles { get; set; }
        [DataMember]
        public bool ReclassifySubFiles { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public List<ManualApprovalFilterDefinition> QueryDefintion { get; set; }
        [DataMember]
        public string NodeId {  get; set; }
        [DataMember]
        public RequesterTypeEnum RequesterType { get; set; }
        [DataMember]
        public bool CanReclassifyAllTerm { get; set; }
        [DataMember]
        public bool IsManualData { get; set; }
        [DataMember]
        public ChangeTermOrigin ChangeTermOrigin { get; set; }
    }

    public enum ChangeTermPage
    {
        None,
        Search,
        RecordForReview,
        MyHub
    }

    [DataContract]
    public class ChangeLabelDto
    {
        [DataMember]
        public List<Guid> GoogleDriveRecordIds { get; set; }
        [DataMember]
        public TargetLabelInfo LabelInfo { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public bool OverWriteSubFiles { get; set; }
        [DataMember]
        public bool ReclassifySubFiles { get; set; }
    }

    [DataContract]
    public class TargetLabelInfo
    {
        [DataMember]
        public int LabelId
        {
            get; set;
        }
        [DataMember]
        public string UniqueLabelId
        {
            get; set;
        }
        [DataMember]
        public string LabelName
        {
            get; set;
        }
    }

    [DataContract]
    public class TargetTermInfo
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Guid UniqueId { get; set; }
    }
    [DataContract]
    public class DetailQueryDto
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public ExplorerDetailTab tab { get; set; }
    }
    [DataContract]
    public class AddPageSearchRecordsDto
    {
        [DataMember]
        public string PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public Guid CurrentId { get; set; }
        [DataMember]
        public List<Guid> RelatedsCache { get; set; }
    }

    [DataContract]
    public class TryAddRecordDto
    {
        [DataMember]
        public SharePointOnPremRecordInputDto Input { get; set; }

        [DataMember]
        public bool PersistAfterConvert { get; set; } = true;
    }

    [DataContract]
    public class SharePointOnPremRecordInputDto
    {
        [DataMember]
        public Guid ScopeId { get; set; }

        [DataMember]
        public Guid NodeId { get; set; }

        [DataMember]
        public int NodeType { get; set; } = (int)RMNodeLevel.Item;

        [DataMember]
        public Guid AveSiteId { get; set; }

        [DataMember]
        public Guid WebId { get; set; }

        [DataMember]
        public Guid ListId { get; set; }

        [DataMember]
        public Guid ItemId { get; set; }

        [DataMember]
        public Guid FolderId { get; set; }

        [DataMember]
        public string LeafName { get; set; }

        [DataMember]
        public string FullPath { get; set; }

        [DataMember]
        public string DirPath { get; set; }

        [DataMember]
        public string RecordsId { get; set; }

        [DataMember]
        public long TimeCreated { get; set; }

        [DataMember]
        public long TimeLastModified { get; set; }

        [DataMember]
        public int CreateDate { get; set; }

        [DataMember]
        public string TermName { get; set; }

        [DataMember]
        public Guid TermId { get; set; }

        [DataMember]
        public string DisposalDueDate { get; set; }

        [DataMember]
        public Guid RuleId { get; set; }

        [DataMember]
        public int RuleLevel { get; set; }

        [DataMember]
        public bool DeclareAsRecord { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public string ModifiedBy { get; set; }

        [DataMember]
        public string ExtensionForFile { get; set; }

        [DataMember]
        public string MetaInfo { get; set; }

        [DataMember]
        public string RelatedRecords { get; set; }

        [DataMember]
        public int RelatedRecordsCount { get; set; }

        [DataMember]
        public int ItemRowId { get; set; }

        [DataMember]
        public string ApproveUsers { get; set; }
    }

    [DataContract]
    public class TryAddRecordResultDto
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public bool Converted { get; set; }

        [DataMember]
        public bool Persisted { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public RecordDto Record { get; set; }
    }

    public class GlobalSearchDto
    {
        public string PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Value { get; set; }
    }

    public enum FSTreeType
    {
        None = 1,
        Root = 2201,
        ConnGroup = 2202,
        Folder = 2100,
        File = 2200
    }
    [DataContract]
    public class FolderTreePage
    {
        [DataMember]
        public int? PageIndex { get; set; }
        [DataMember]
        public int? PageSize { get; set; }
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public int NodeType { get; set; }
    }
    [DataContract]
    public class MoveToDto
    {
        [DataMember]
        public List<BaseRecordDto> SourceRecords { get; set; }
        [DataMember]
        public DestMode DestMode { get; set; }
        [DataMember]
        public bool IsSpecifyLocation { get; set; }
        [DataMember]
        public string LocationPath { get; set; }
        [DataMember]
        public RMAccountProfileDto SPAccount { get; set; }
        [DataMember]
        public RMFSTreeNode FSTree { get; set; }
        [DataMember]
        public RMSPTreeNode SPTree { get; set; } //Object Type RM or DA
        [DataMember]
        public RMGoogleTreeNode GoogleTree { get; set; }
        [DataMember]
        public RMPhysicalExplorerNode PhysicalTreeNode { get; set; }
        [DataMember]
        public FolderNameConflictOption FolderNameConflictOption { get; set; }
        [DataMember]
        public FileNameConflictOption FolderFilesNameConflictOption { get; set; }
        [DataMember]
        public bool FolderInherit { get; set; }
        [DataMember]
        public bool FolderUnderInherit { get; set; }
        [DataMember]
        public FileNameConflictOption FileNameConflictOption { get; set; }
        [DataMember]
        public bool FileInherit { get; set; }
        [DataMember]
        public string SPTreeStr { get; set; }
        [DataMember]
        public string GoogleTreeStr { get; set; }
        [DataMember]
        public string FSTreeStr { get; set; }
        [DataMember]
        public string PhysicalTreeStr { get; set; }
        [DataMember]
        public bool NotDeclareMovedData { get; set; }
        [DataMember]
        public bool IsDeleteSourceItem { get; set; }
        [DataMember]
        public bool isKeepClassification { get; set; }
        [DataMember]
        public bool IsKeepFolderStructure { get; set; }
        [DataMember]
        public bool IsMoveAllVersions { get; set; }
        [DataMember]
        public CheckLocationObject CheckLocationObject { get; set; }
        [DataMember]
        public MoveHoldConflictOption MoveHoldConflictOption { get; set; }
        [DataMember]
        public bool IsMoveToSP { set; get; }
        [DataMember]
        public List<MoveMetadataInfo> MoveToSPDataList { set; get; }
    }

    public class ArchivedContentResultInfo
    {
        public List<ArchivedContentDto> Datas { get; set; }
        public ExplorerPagingInfo PagingInfo { get; set; }
    }
    [DataContract]
    public class ArchivedContentSearchInfo
    {
        [DataMember]
        public string SearchKey { get; set; }
        [DataMember]
        public ExplorerPagingInfo PagingInfo { get; set; }
    }
    public enum FolderNameConflictOption
    {
        Merge = 1, Skip = 2, Rename = 3
    }

    public enum FileNameConflictOption
    {
        Skip = 1, Overwrite = 2, Rename = 3
    }

    public enum MoveHoldConflictOption
    {
        Current = 1, Compare = 2
    }

    public enum DestMode
    {
        SharePoint = 1, FileSystem = 2
    }

    public class RMAccountProfileDto
    {
        public string UserName { get; set; }
        public string Id { get; set; }
        public SPAccountType AccountType { get; set; }

    }
    public enum SPAccountType
    {
        Local = 1, O365 = 2
    }

    public class CheckLocationObject
    {
        public string DestRootPath { get; set; }
        public Guid AveSiteId { get; set; }
        public string UserInfoKey { get; set; }
        public string UserInfoName { get; set; }
        /// <summary>
        /// ContainerId(Web Application Node/Group Node)
        /// </summary>
        public string ContainerId { get; set; }
    }
    [DataContract]
    public class PhysicalMoveDto
    {
        [DataMember]
        public List<Guid> SourcePhyRecordIds { get; set; }
        [DataMember]
        public string LocationId { get; set; }
        [DataMember]
        public string BoxId { get; set; }
        [DataMember]
        public string FolderId { get; set; }
        [DataMember]
        public PhysicalNameConflictOption NameConflictOption { get; set; }
        [DataMember]
        public PhysicalMoveHoldConflictOption HoldConflictOption { get; set; }
        [DataMember]
        public int FromModule { get; set; }
    }

    public enum PhysicalNameConflictOption
    {
        Skip = 1, Overwrite = 2, Rename = 3
    }
    public enum PhysicalMoveHoldConflictOption
    {
        None = 0, UseDest = 1, UseLongest = 2
    }
    [DataContract]
    public class ExportBarcodeDto
    {
        [DataMember]
        public ExportType ExportType { get; set; }
        [DataMember]
        public Guid NodeId { get; set; }
        [DataMember]
        public RMNodeType NodeType { get; set; }
        [DataMember]
        public String FullPath { get; set; }
        [DataMember]
        public String ExportLocationId { get; set; }
        [DataMember]
        public String ExportLocationName { get; set; }
        [DataMember]
        public Guid SuiteId { get; set; }
        
    }

    public class ExportResultDto
    { 
        public byte[] FileContent { get; set; }
        public string FileName { get; set; }
    }

    public class ExportBarcodeDataModel
    {
        public string ColumnB { set; get; }
        public string ColumnC { set; get; }
        public Dictionary<string, string> ColumnDValue { set; get; }
        public string ColumnE { set; get; }
        public string ColumnF { set; get; }
       
        public Byte[] Image { set; get; }
        public Byte[] BarcodeByte { set; get; }

        public string UniqueId { set; get; }

        public RMNodeType NodeType { get; set; }
        public int ImageWidth { set; get; }

        public int ImageHeight { set; get; }

        public string Barcode { set; get; }
    }
    [DataContract]
    public class SPFilterNode
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public int Level { set; get; }
        [DataMember]
        public string DriveId { set; get; }
    }

        public enum ExportType
    {
        None = 0,
        Download = 1,
        ExportToFS = 2,
    }

    public enum ChangeTermOrigin
    {
        Search = 0,
        Manual = 1,
        MyHub = 2,
    }
}
