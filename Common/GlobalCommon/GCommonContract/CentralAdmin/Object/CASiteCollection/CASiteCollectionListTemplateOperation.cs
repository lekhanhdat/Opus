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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASiteCollectionListTemplateOperation : CAOperation
    {
        [DataMember]
        public List<ListTemplate> ListTemplates { get; set; }

        [DataMember]
        public List<UploadTemplateInfo> TemplateInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListTemplate
    {
        [DataMember]
        public Guid UniqueId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Modified { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public uint Language { get; set; }

        [DataMember]
        public string LanguageEnglishName { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string FeatrueId { get; set; }

        [DataMember]
        public bool IsHidden { get; set; }
        
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UploadTemplateInfo
    {
        [DataMember]
        public byte[] ListTemplateMetaData { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public bool IsOverwriteExistFile { get; set; }
    }

}
