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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Archiver
{
    [DataContract]
    public class ExportSettingsInfo
    {
        [DataMember]
        public List<NARAInfo> NARAExportInfos;
        [DataMember]
        public List<NAAInfo> NAAExportInfos;
        [DataMember]
        public List<VEOInfo> VEOExportInfos;
        [DataMember]
        public string DefaultStorageDeviceId;
    }


    [DataContract]
    public class ExportColumnInfo
    {
        [DataMember]
        public int Order;

        [DataMember]
        public string DisplayName;

        [DataMember]
        public string MappedKey;

        [DataMember]
        public bool Additional;

        [DataMember]
        public string DefaultValue;

        [DataMember]
        public string Format;

        [DataMember]
        public string Prefix;
    }
    [DataContract]
    public class BaseExportInfo
    {
        [DataMember]
        public ExportTypeValue ExportType;

        [DataMember]
        public SourceFlag SourceFlag;
    }
    [DataContract]
    public class NARAInfo: BaseExportInfo
    {
        [DataMember]
        public List<ExportColumnInfo> ExportColumnInfoes;
    }
    [DataContract]
    public class NAAInfo: BaseExportInfo
    {
        [DataMember]
        public List<ExportColumnInfo> ExportColumnInfoes;
    }
    [DataContract]
    public class VEOInfo: BaseExportInfo
    {
        [DataMember]
        public List<VEOInfo> ChildTable;

        [DataMember]
        public List<VEOInfo> ChildVEOInfo;

        [DataMember]
        public string TreeNodeName;

        [DataMember]
        public string? MetadataName;

        [DataMember]
        public string? DefaultValue;

        [DataMember]
        public bool? ExchangeMetadataAsSource;

        [DataMember]
        public string? ExchangeMetadata;

        [DataMember]
        public bool? SharePointMetadataAsSource;

        [DataMember]
        public string? SharePointMetadata;
    }
}
