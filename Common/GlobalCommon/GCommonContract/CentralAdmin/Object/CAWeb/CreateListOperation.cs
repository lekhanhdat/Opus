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




using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CreateListOperation : CAOperation
    {
        [DataMember]
        public string TemplateType { get; set; }
        [DataMember]
        public bool EmailEnabled { get; set; }
        [DataMember]
        public string EmailServerDisplayAddress { get; set; }
        [DataMember]
        public List<CreateListTypeGroup> listTypeGroup { get; set; }
        [DataMember]
        public ListOrLibraryDto ListOrLibrary { get; set; }
        [DataMember]
        public string FullPath { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CreateListTypeGroup
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsCustomList { get; set; }
        [DataMember]
        public int Action { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public int ListType { get; set; }
        [DataMember]
        public List<CreateListType> ListTypes { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CreateListType
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsCustomList { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public int Action { get; set; }
        [DataMember]
        public int ListType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListOrLibraryDto
    {
        [DataMember]
        public string ListType { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool DisplayOnQuickLaunch { get; set; }
        [DataMember]
        public bool HasVersionHistory { get; set; }
        [DataMember]
        public bool HasTemplate { get; set; }
        [DataMember]
        public string TemplateName { get; set; }
        [DataMember]
        public bool IsCustomList { get; set; }
        [DataMember]
        public string SiteName { get; set; }
        [DataMember]
        public List<DocumentTemplate> DocumentTemplates { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DocumentTemplate
    {
        [DataMember]
        public int Index { get; set; }
        [DataMember]
        public int TemplateType { get; set; }
        [DataMember]
        public string TemplateName { get; set; }
        [DataMember]
        public bool IsDefaultTemplate { get; set; }
    }
}
