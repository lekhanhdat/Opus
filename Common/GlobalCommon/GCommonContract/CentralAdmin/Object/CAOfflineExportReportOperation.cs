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


namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
    using System.Collections.Generic;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAOfflineExportReportOperation : CAOperation
    {
        [DataMember]
        public CAOfflineExportType ExportType { get; set; }

        [DataMember]
        public ReportFileType ReportFileType { get; set; }

        [DataMember]
        public string ReportJobId { get; set; }

        [DataMember]
        public string ExportLocationId { get; set; }

        [DataMember]
        public string ExportLocationName { get; set; }

        //Export Group Result的临时文件名
        [DataMember]
        public string ExportGroupTempFileName { get; set; }

        [DataMember]
        public string LogonUserId { get; set; }
        [DataMember]
        public string LogonGroupId { get; set; }

        [DataMember]
        public List<TreeNodeCollection> Nodes { get; set; }

        [DataMember]
        public string PlanName { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CAOfflineExportType
    {
        [EnumMember]
        DownloadAdminSearchResult,
        [EnumMember]
        DownloadSecuritySearchResult,
        [EnumMember]
        ExportForEditing,
        [EnumMember]
        ExportGroupForEditing,
        [EnumMember]
        ChangeColumnMetadata,
        [EnumMember]
        ExportProfileReport,
    }
}
