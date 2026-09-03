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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.FileSystem.Backup
{
    public class ArchiverRestoreFilter
    {
        [JsonProperty]
        public PolicyLevel Level { get; set; }
        [JsonProperty]
        public string FilterName;
        [JsonProperty]
        public string CreateStartTime;
        [JsonProperty]
        public string CreateEndTime;
        [JsonProperty]
        public string ModifiedStartTime;
        [JsonProperty]
        public string ModifiedEndTime;
        [JsonProperty]
        public string ArchivedStartTime;
        [JsonProperty]
        public string ArchivedEndTime;
        [JsonProperty]
        public string FilterContent;
        //[JsonProperty]
        //public FilterDeletedType FilterDeleteType;
        public List<string> PathMD5List;
        public string ModifiedBy;
        public string CreatedBy;
        public string FolderName;
        public int PageSize;
        public int PageIndex;
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleGUIType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ColumnText = 1,
        [EnumMember]
        CustomPropertyText = 2,
        [EnumMember]
        ColumnNumber = 3,
        [EnumMember]
        CustomPropertyNumber = 4,
        [EnumMember]
        ColumnBoolean = 5,
        [EnumMember]
        CustomPropertyBoolean = 6,
        [EnumMember]
        ColumnDateTime = 7,
        [EnumMember]
        CustomPropertyDateTime = 8,
        [EnumMember]
        Workflow = 9,
        [EnumMember]
        AnonymousAccess = 10,
        [EnumMember]
        Attribute = 11,
        [EnumMember]
        Attachment = 12,
        [EnumMember]
        Auditing = 13,
        [EnumMember]
        Category = 14,
        [EnumMember]
        ContentType = 15,
        [EnumMember]
        CreatedBy = 16,
        [EnumMember]
        Created = 17,
        [EnumMember]
        KeepHistoryVersion = 18,
        [EnumMember]
        ListType = 19,
        [EnumMember]
        ModifiedBy = 20,
        [EnumMember]
        Modified = 21,
        [EnumMember]
        NameAndExtention = 22,
        [EnumMember]
        Name = 23,
        [EnumMember]
        Owner = 24,
        [EnumMember]
        SendDate = 25,
        [EnumMember]
        Size = 26,
        [EnumMember]
        Template = 27,
        [EnumMember]
        Title = 28,
        [EnumMember]
        Url = 29,
        [EnumMember]
        Versions = 30,
        [EnumMember]
        Versioning = 31,
        [EnumMember]
        UserAndGroup = 32,
        [EnumMember]
        Inheritance = 33,
        [EnumMember]
        StubCreationTime = 34,
        [EnumMember]
        StubLastAccessTime = 35,
        [EnumMember]
        TemplateId = 36,
        [EnumMember]
        LockStatus = 37,
    }
}
