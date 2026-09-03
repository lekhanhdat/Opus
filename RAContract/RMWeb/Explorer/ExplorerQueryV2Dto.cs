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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    [DataContract]
    public class ExplorerOfflineResultQueryDto
    {
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ProfileId { get; set; }
        [DataMember]
        public string JobId { set; get; }
        [DataMember]
        public ExplorerQueryOrderColumn OrderColumn { set; get; }
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PhysicalExplorerPagingInfo PagingInfo { get; set; }
    }
    [DataContract]
    public class ExplorerQueryV2Dto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public ExplorerQueryOptionV2 QueryOption { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public ExplorerPagingInfo PagingInfo { get; set; }
    }
    
    public class ExplorerQueryOptionV2
    {
        /// <summary>
        /// for search condition
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerSearchOptionV2 SearchOption { get; set; }

        /// <summary>
        /// for filter condition
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerFilterOptionV2 FilterOption { get; set; }
        
        /// <summary>
        /// order by column
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerQueryOrderColumn OrderColumn { set; get; }
    }
    [DataContract]
    public class ExplorerQueryOrderColumn
    {
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerQueryColumn Column { set; get; }
        [DataMember]
        public bool OrderAsc { set; get; }
    }

    public class ExplorerSearchOptionV2
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        /// <summary>
        /// if search key can be splitted into multiple sub keys, this is the search logic for these sub keys.
        /// default is AND logic
        /// </summary>
        public ExplorerSearchKeyOperationLogic OperationLogic { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ExplorerQueryColumn> Columns { get; set; }
    }

    [DataContract]
    public class ExplorerQueryColumn
    {
        /// <summary>
        /// column id
        /// </summary>
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        /// <summary>
        /// column name in cosmos db
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// same name and type, different id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public List<Guid> IdsWithDuplicateName { set; get; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public AvePoint.RA.Contract.TemplateManagement.ColumnType? Type { get; set; }
    }

    public class ExplorerFilterOptionV2
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<SourceFlag> SourceFlags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<RMNodeLevel> NodeTypes { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<AOSUserDto> HoldBy { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> HoldByUsers { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HoldStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? DeclaredRecord { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? LockedByRecordLabel { get; set; }

        /// <summary>
        ///  if true, 取得没有term并且不是Physical Record的记录
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? WithOutTerms { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> TermIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> RuleIds { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public bool? WithOutTerms { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<RMRecordStatus> Status { get; set; }

        /// <summary>
        /// FS node id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NodeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<SPFilterNode> SPNodes { set; get; }

        /// <summary>
        /// record owners
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<AOSUserDto> Owners { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<AOSUserDto> CreatedBy { get; set; } //display name list

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FileExtensions { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<AOSUserDto> ModifiedBy { get; set; } //display name list

        /// <summary>
        /// modified time
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateInfo ModifiedDateInfo { get; set; }

        /// <summary>
        /// created time
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateInfo CreatedDateInfo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateInfo DestryoedDateInfo { get; set; }

        /// <summary>
        /// disposal due date
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateInfo DisposalDateInfo { get; set; }

        /// <summary>
        /// the permission id should be included.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> PersmissionScopes { get; set; }

        /// <summary>
        /// the permission id should be excluded.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> ExcludePersmissionScopes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ContainerIds { get; set; }
                
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ExplorerFilterColumn> CustomColumns { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> ExceptIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> Ids { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> ParentIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<RMNodeLevel> WithoutNodeTypes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> PhysicalLocationIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> PhysicalBoxIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> PhysicalFileIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> PhysicalTemplateds { get; set; }

        /// <summary>
        /// Shallow or deep search
        /// </summary>
        public PhysicalSearchModel? PhycialModel { get; set; }

        /// <summary>
        /// the node level to be query/search
        /// </summary>
        public RMNodeLevel? PhysicalSearchNodeLevel { get; set; }
        /// <summary>
        /// currently used for Physical
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Ancestor { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ListId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> WebIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ScopeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MailboxAddress { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateInfo CollectionDateInfo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? QueryArchivedData { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DirPath { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<DirPathListItem> DirPathListItems { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> ExceptRuleIds { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> ExceptSCIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool FSFolderLevelEnabled { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PickStatusType> LoanPickStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PickStatusType> DestructionStatus { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<MLFileStatus> TrainScopeStatus { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> TrainScopeTermIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TrainingAddType> TrainingAddTypes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> PredictTermIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateInfo PredictDateInfo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> MLApproveStatus { get; set; }
    }

    public class ExplorerFilterColumn
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerQueryColumn Column { get; set; }


        /// <summary>
        /// same name and type, different id
        /// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public List<Guid> IdsWithDuplicateName { set; get; }
        /// <summary>
        /// a serialized json string
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public class ExplorerQueryColumnNumber
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerQueryColumnNumberCondition Condition { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public enum ExplorerQueryColumnNumberCondition
    { 
        Equal = 0,
        GreaterThenOrEqual = 1,
        LessThenOrEqual = 2
    }

    /// <summary>
    /// And/Or logic
    /// </summary>
    [DataContract]
    public enum ExplorerSearchKeyOperationLogic
    {
        [EnumMember]
        AND,
        [EnumMember]
        OR
    }

    /// <summary>
    /// Contains/Equals logic
    /// </summary>
    [DataContract]
    public enum ExplorerSearchColumnOperationLogic
    {
        [EnumMember]
        Contains,
        [EnumMember]
        Equals
    }

    public enum PhysicalSearchModel
    {
        /// <summary>
        /// just query the direct children
        /// </summary>
        Shallow,
        /// <summary>
        /// will query all of the descendants
        /// </summary>
        Deep
    }

    public class ExplorerQueryOrderByColumn
    {
        /// <summary>
        /// indicate if the column be order
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsBuiltInOrDefaultColumn { get; set; }

        /// <summary>
        /// valid if IsBuiltInOrDefaultColumn is true.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// valid if IsBuiltInOrDefaultColumn is false
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerQueryColumn Column { get; set; }
    }

    public class DirPathListItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DirPath { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ListId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AveSiteId { get; set; }
    }
}
