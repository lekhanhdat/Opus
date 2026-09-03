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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Server.Common.EmailTemplateSettings.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EmailTemplateDto : IProfileContent
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public EmailType TemplateType { get; set; }
        [DataMember]
        public string Header { get; set; }
        [DataMember]
        public EmailLanguage TemplateLanguage { get; set; }
        [DataMember]
        public string Subject { get; set; }
        [DataMember]
        public string BodyHead { get; set; }
        [DataMember]
        public string BodyContent { get; set; }
        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }
        [DataMember]
        public bool IsNeedCopyRight { get; set; }
    }

    public class DefaultLanguageDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public EmailLanguage Language { get; set; }
    }
    public class templateDto
    {
        public EmailLanguage Language { get; set; }
        public EmailTemplateDto template { get; set; }
    }
    public enum EmailType
    {
        PolicyEnforcer = 0
    }
    public enum EmailLanguage
    {
        English = 0,
        Japanese = 1,
        French = 2,
        German = 3,
        Italian = 4
    }
    public enum EmailTemplateReferences
    {
        PERuleName = 0,
        PERuleDecription = 1,
        PERuleDetails = 2,
        PECustomAction = 3,
        PECustomActionSettings = 4,
        PEOutOfPolicySharePointNode = 5,
        PEDetails = 6
    }
    public enum SaveResultStatus
    {
        NameExisted = 0,
        SuccessFul = 1,
        Failed = 2,
        EmailLanguageExisted = 3,
    }
}
