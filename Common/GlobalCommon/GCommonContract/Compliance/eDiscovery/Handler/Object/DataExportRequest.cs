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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataExportRequest : EDiscoveryRequest
    {
        [DataMember]
        public ExportAction ExportAction { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public List<ExportDataInfo> ExportDataList { get; set; }

        /// <summary>
        /// Archive数据在Search Results中Export需要采用此属性.
        /// </summary>
        [DataMember]
        public List<HeldFileDto> EDHeldFiles { get; set; }

        [DataMember]
        public string ExportLocationId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportAction
    {
        [EnumMember]
        SharePointDataExport = 1,

        [EnumMember]
        ArchivedDataExport = 2,

        [EnumMember]
        HeldDataExport = 3
    }
}
