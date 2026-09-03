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






using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace AvePoint.RA.Contract.Global.Object
{
    [DataContract(IsReference = true)]
    public class FSTreeNodeDto
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Id { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string FarmID { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Guid ConnGroupId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string FullPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int Level { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int NodeType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int ChildrenCount { get; set; }

        /// <summary>
        /// CheckNumber为1代表当前节点是Checked状态，为0代表UnChecked状态
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int CheckNumber { get; set; }

        /// <summary>
        /// IncludeNew为-1代表当前节点没有Include New的逻辑，为0代表不是Include New，为1代表是Include New
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int IncludeNew { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool Expanded { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public FSTreeNodeDto Parent { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string ParentId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public List<string> ChildrenIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<FSTreeNodeDto> Children { set; get; }

        #region for fs column settings

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
        public bool IsClassificationTermRemoved { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsClassificationTermDeprecated { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool NeedCheckDefaultValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
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
        public bool NeedLoadSchedule { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public ScheduleInfo ScheduleInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int PageIndex { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsCustomSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool HasCustomSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ApplyExistType { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public List<ToUserInfo> RecordOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool EMailToRecordOwner { set; get; }
        //[DataMember(EmitDefaultValue = false)]
        //public ScheduleInfo DisposeScheduleInfo { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public ScheduleInfo CollectionScheduleInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ProfileId { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public IconStatus IconStatus { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsActive { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string Domain { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string EncryptedPassword { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Username { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<ClassificationRule> AutoClassificationRules { set; get; }

        //[DataMember(EmitDefaultValue = false)]
        //public DeployTermMethod DeployTermMethod { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool RunAutoFullJob { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public AutoJobOption AutoJobOption { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AgentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long TimeStamp { get; set; }
        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        //public RMFSTreeNode Clone()
        //{
        //    return this.MemberwiseClone() as RMFSTreeNode;
        //}
    }
    
    [DataContract]
    public enum PathType
    {
        [EnumMember]
        Local,
        [EnumMember]
        NetShare
    }
}




