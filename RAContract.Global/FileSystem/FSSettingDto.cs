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
using AvePoint.RA.Contract.Common;
using System;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.FileSystem
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FSSettingDto
    {
        //[DataMember(EmitDefaultValue = false)]
        //public int Id { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Guid ScopeId { set; get; }

        //[DataMember(EmitDefaultValue = false)]
        //public Guid ConnectionGroupId { set; get; }

        //[DataMember(EmitDefaultValue = false)]
        //public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TermSetId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TermId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Guid DefaultTermId { set; get; }

        //[DataMember(EmitDefaultValue = false)]
        //public string TermSetName { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public string TermName { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public string DefaultTermName { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public string DescriptionOfContainer { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public string TermNameOfContainer { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public Guid TermIdOfContainer { set; get; }

        //[DataMember(EmitDefaultValue = false)]
        //public bool IsEnableContainerLevelClassification { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public bool HaveConfigSetting { get; set; }//to do lock this setting for get job node

        //[DataMember(EmitDefaultValue = false)]
        //public long SettingTime { get; set; }//update the datetime

        //[DataMember(EmitDefaultValue = false)]
        //public string NodeInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool NeedCheckDefaultValue { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public string IdPath { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public bool EMailToRecordOwner { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public bool EnableRelatedRecords { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int ApplyExistType { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public bool IsNewEdited { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public string FSSettingJobId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsActive { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string AutoClassificationRules { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int DeployTermMethod { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int AutoJobOption { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool RunAutoFullJob { get; set; }
    }
}
