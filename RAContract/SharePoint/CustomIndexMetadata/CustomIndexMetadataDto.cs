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
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.SharePoint.CustomIndexMetadata
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CustomIndexMetadataDto
    {
        [DataMember]
        public int Id { set; get; }

        [DataMember]
        public Guid UniqueId { set; get; }

        [DataMember]
        public string SourceColumnName { get; set; }

        [DataMember]
        public string TargetColumnName { get; set; }

        [DataMember]
        public Guid TargetColumnId { get; set; }

        [DataMember]
        public CustomColumnType ColumnType { get; set; }

        [DataMember]
        public SourceFlag ContentSource { get; set; }

        [DataMember]
        public long ModifiedTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CustomIndexMetadataInfo
    {
        [DataMember]
        public bool IsEnableCustomIndexMetadata { get; set; } = false;

        [DataMember]
        public List<CustomIndexMetadataDto> CustomIndexMetadataDtos { get; set; } = new();
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CustomMetadataColumnInfo
    {

        [DataMember]
        public Guid UniqueId { get; set; }

        [DataMember]
        public string ColumnName { get; set; }

        [DataMember]
        public CustomColumnType ColumnType { get; set; }

        [DataMember]
        public bool EnableSort { get; set; }
    }
}
