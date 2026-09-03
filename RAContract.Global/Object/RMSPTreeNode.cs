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
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Global.Object
{
    [DataContract(IsReference = true)]
    public class RMSPTreeNode : IDisposable
    {
        #region == basetreenode属性 老数据需要 ==    
        [DataMember(EmitDefaultValue = false)]
        public string Id { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int Level { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string Name { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string DisplayName { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string Title { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string FullPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int NodeType { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool Hidden { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int ChildrenCount { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool? Loaded { get; set; }

        /// <summary>
        /// IncludeNew为-1代表当前节点没有Include New的逻辑，为0代表不是Include New，为1代表是Include New
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int IncludeNew { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool Expanded { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ParentId { set; get; }

        /// <summary>
        /// CheckNumber为1代表当前节点是Checked状态，为0代表UnChecked状态
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int CheckNumber { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<string> ChildrenIds { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public IconStatus IconStatus { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int PageIndex { set; get; }
        #endregion
        [DataMember(EmitDefaultValue = false)]
        public string SPObjectId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string FarmId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string FarmName { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int SPType { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int SPVersion { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int TemplateId { set; get; }
        
        private string teamName;
        [DataMember(EmitDefaultValue = false)]
        public string TeamName
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    teamName = value.Split('@').Length > 1 ? value.Split('@')[0] : value;
                }
            }
            get { return teamName; }

        }
        [DataMember(EmitDefaultValue = false)]
        public BposInfo BposInfo { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public RMSPTreeNode Parent { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public List<RMSPTreeNode> Children { set; get; }

        #region for sharepoint column settings
        [DataMember(EmitDefaultValue = false)]
        public string ColumnName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid TermStoreId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid TermSetId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid TermId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid DefaultTermId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DefaultTermName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DefaultTermNameFullPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TermSetName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TermName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TermNameFullPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool InitDefaultValue { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsTermRemoved { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsDefaultTermRemoved { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsTermDeprecated { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsDefaultTermDeprecated { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string DescriptionOfContainer { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid TermIdOfContainer { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TermNameOfContainer { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool isEnableClassification { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int EnableRecordManagement { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool isFailedConfigClassification { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool isFailedConfigMetaDataColumn { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsClassificationTermDeprecated { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsEnableHoldPhyical { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid WebId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid ListId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid FolderId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid SiteGroupId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool NeedCheckDefaultValue { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsEnableUniqueIDSetting { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string ExistColumnName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsUsingExistColumnName { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public Guid SettingScopeId { get; set; }
        #endregion
        //[DataMember(EmitDefaultValue = false)]
        //public List<ToUserInfo> RecordOwner { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool EMailToRecordOwner { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool NeedLoadSchedule { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public ScheduleInfo ScheduleInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsContainScheduleForOwnAndChildNodes { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool HasCustomSetting { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int ApplyExistType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool EnableRelatedRecords { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsCustomSetting { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsDisplyaTermPath { set; get; }
        /// <summary>
        /// ！！！该属性要使用GroupLevel中的
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsShowUniqueId { set; get; }

        //[DataMember(EmitDefaultValue = false)]
        //public ScheduleInfo DisposeScheduleInfo { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public ScheduleInfo CollectionScheduleInfo { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public string ProfileId { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public bool UseAutoClassification { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public List<ClassificationRule> AutoClassificationRules { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public DeployTermMethod DeployTermMethod { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool ColumnRequired { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool RunAutoFullJob { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public AutoJobOption AutoJobOption { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IncludeDeclaredRecords { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsSyncData { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool SetDocLevelTermForExistColumn { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool SkipRemoveContentAndDestroyAction { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TermScopeFullPath { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string DefaultTermFullPath { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string ContainerTermFullPath { set; get; }

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
                //出现异常概率极低
            }
        }
        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        public RMSPTreeNode Clone()
        {
            return this.MemberwiseClone() as RMSPTreeNode;
        }
    }
    public enum EnableRecordManagementSetting
    {
        None = 0,
        Enable = 1,
        Disable = 2,
        ParentDisable = 3,
    }

    public enum AutoJobOption
    {
        None = 0,
        SkipAndKeep = 1,
        Override = 2,
        Append = 3,
    }
    public enum ApplyExistingTermType
    {
        None = 0,
        OverWrite = 1,
        SkipAndKeep = 2
    }   
}
