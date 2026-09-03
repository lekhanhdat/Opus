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
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Object
{
    [DataContract(IsReference = true)]
    [JsonObject]
    public class RMFSTreeNode : IDisposable
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid Id { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FarmID { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid ConnGroupId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FullPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Name { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int Level { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int NodeType { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int PathType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ChildrenCount { get; set; }

        /// <summary>
        /// CheckNumber为1代表当前节点是Checked状态，为0代表UnChecked状态
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int CheckNumber { get; set; }

        /// <summary>
        /// IncludeNew为-1代表当前节点没有Include New的逻辑，为0代表不是Include New，为1代表是Include New
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int IncludeNew { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool Expanded { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public RMFSTreeNode Parent { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ParentId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<string> ChildrenIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMFSTreeNode> Children { set; get; }

        #region for fs column settings

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TermSetId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid DefaultTermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DefaultTermName { get; set; }

        [Obsolete("DefaultTermNameFullPath")]
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DefaultTermNameFullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DefaultTermFullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermSetName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermName { get; set; }

       [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermNameFullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool InitDefaultValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsTermRemoved { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsDefaultTermRemoved { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsTermDeprecated { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsDefaultTermDeprecated { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DescriptionOfContainer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TermIdOfContainer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermNameOfContainer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool isEnableClassification { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsClassificationTermRemoved { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsClassificationTermDeprecated { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool NeedCheckDefaultValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool EnableRelatedRecords { get; set; }
        public void Dispose()
        {
            try
            {
                foreach (var child in this.Children)
                {
                    using (child as IDisposable)
                    { }
                }
                this.Children = null;
            }
            catch
            { 
                //Noncompliant
            }
        }

        //[DataMember(EmitDefaultValue = false)]
        //public Guid SettingScopeId { get; set; }
        #endregion

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool NeedLoadSchedule { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo ScheduleInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int PageIndex { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int PageSize { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsCustomSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool HasCustomSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ApplyExistType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<ToUserInfo> RecordOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool EMailToRecordOwner { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo DisposeScheduleInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo CollectionScheduleInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ProfileId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public IconStatus IconStatus { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsActive { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Domain { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string EncryptedPassword { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Username { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<ClassificationRule> AutoClassificationRules { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public DeployTermMethod DeployTermMethod { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool RunAutoFullJob { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public AutoJobOption AutoJobOption { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string AgentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ApprovalType { get; set; }

       // [DataMember(EmitDefaultValue = false)]
        public string WorkflowReferenceId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string WorkflowReferenceName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermScopeFullPath { get; set; }
        public bool IsProcessApprovalDatasOnly { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int EnableRecordManagement { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsAllowUserDownloadRCCReport { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsSearch { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string SearchKey { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsDeletedFromLocal { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string NodeSize { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FolderCreationDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FolderLastModifiedDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string AgentName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ConnectionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public FSClassCodeDto ClassCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool ApplyExistDocument { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int EffectScope { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int IsPause { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public GCommon.Contract.Tree.Object.NodeLevel ClassificationLevel { set; get; }
        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        public RMFSTreeNode Clone()
        {
            return this.MemberwiseClone() as RMFSTreeNode;
        }

        public enum EnableRecordManagementSetting
        {
            Enable = 1,
            Disable = 2,
            ParentDisable = 3,
        }
    }
}
