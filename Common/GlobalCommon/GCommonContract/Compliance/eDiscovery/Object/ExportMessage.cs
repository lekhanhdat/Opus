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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object.HoldManager;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ExportMessage : EDBaseMessage
    {

        [DataMember]
        public string FarmId { get; set; }
        [DataMember]
        public string SubJodId { get; set; }

        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public PlanCategory PlanCategory { get; set; }
        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public ExportFormat ExportFormat { get; set; }



        [DataMember]
        public ExportDataSource ExportDataSource { get; set; }

        #region offline 不用的属性

        [DataMember]
        public OperationType OperationType { get; set; }

        [DataMember]
        public List<ExportDataInfo> ExportDatas { get; set; }

        [DataMember]
        public CplDBSettingsDto ComplicanceDbSettingInfo { get; set; }

        #endregion


        [DataMember]
        public PhysicalDeviceDto PhysicalDeviceDto { get; set; }


        [DataMember]
        public PhysicalDeviceDto SearchResultLocation { get; set; }

        [DataMember]
        public string SearchJobId { get; set; }


    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportDataInfo
    {
        [DataMember]
        public string FileId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public int UiVersion { get; set; }

        [DataMember]
        public FileType FileType { get; set; }

        [DataMember]
        public string Location { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportFormat
    {
        [EnumMember]
        Concordance = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportDataSource
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SharePointData = 1,
        [EnumMember]
        HeldData = 2,
        [EnumMember]
        ArchivedData = 3,
        [EnumMember]
        OffLineExport = 4
    }

}
