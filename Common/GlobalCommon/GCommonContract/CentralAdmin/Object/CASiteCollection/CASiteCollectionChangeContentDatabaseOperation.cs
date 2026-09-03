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
    public class CASiteCollectionChangeContentDatabaseOperation : CAOperation
    {
        [DataMember]
        public List<SiteCollectionChangeDbInfo> SiteCollectionChangeDbInfos { get; set; }

        [DataMember]
        public List<SiteCollectionContentDatabase> ContentDatabases { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteCollectionContentDatabase
    {
        [DataMember]
        public string DatabaseName { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Size { get; set; }

        [DataMember]
        public string MDFSpace { get; set; }

        [DataMember]
        public string FreeDiskSpace { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteCollectionChangeDbInfo
    {
        [DataMember]
        public string SiteCollectionTitle { get; set; }
        [DataMember]
        public string SiteCollectionURL { get; set; }
        [DataMember]
        public string OriginalDatabaseId { get; set; }
        [DataMember]
        public string SelectedContentDatabaseId { get; set; }
        [DataMember]
        public bool TooLarge { get; set; }

        [DataMember]
        public bool isSP1 { get; set; }
        [DataMember]
        public string ProviderName { get; set; }

        [DataMember]
        public ResultStatus Status { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public CAStringFormatMessage FormatComment { get; set; }
    }
}
