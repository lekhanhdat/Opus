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
namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    #region == using namespace ==

    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Monitor.Object;

    #endregion

    public struct DownloadParameter
    {
        public DownloadType DownloadType { get; set; }

        public List<DownloadParameterItem> QueryParams { get; set; }

        public string FileName { get; set; }

        public MonitorSelectionType MonitorSelectionType { get; set; }

        public string Url { get; set; }
    }

    public struct DownloadParameterItem
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DownloadType
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        ExportSCJobResult = 1,

        [EnumMember]
        JobMonitorDownloadView = 2,

        [EnumMember]
        JobMonitorReport = 3,

        [EnumMember]
        CAExportProfileReport = 6,

        [EnumMember]
        ReportCenterExport = 4,

        [EnumMember]
        LogManagerDownload = 5,

        [EnumMember]
        DownloadSCJobResult = 7,

        [EnumMember]
        DownloadViewMappingsReport = 8,

        [EnumMember]
        PRWFEInfoDownload = 9,

        [EnumMember]
        LicenseManagerDownloadFile = 10,

        [EnumMember]
        ReplicatorPlanInfo = 11,

        [EnumMember]
        CentralAdminEditGroupDownloadFile = 12,

        [EnumMember]
        AuditorExport = 13,

        [EnumMember]
        ReplicatorDownload = 16,

        [EnumMember]
        CeipFileDownload = 18,

        [EnumMember]
        DeploymentManagerDownload = 19,

        [EnumMember]
        SP07To10ProfileManager = 20,

        [EnumMember]
        SecurityProfileExport = 21,

        [EnumMember]
        PublicFolderProfileManager = 22,

        [EnumMember]
        CentralAdminAdminSearchDownloadFile = 23,

        [EnumMember]
        GroupMapping = 30,

        [EnumMember]
        MigrationDownload = 41,

        [EnumMember]
        LivelinkMigrationProfileDownload = 42,

        [EnumMember]
        PublicFolderMigrationProfileDownload = 43,

        [EnumMember]
        ArchiverIndexChangeInfoExport = 50,

        [EnumMember]
        IndexManagerDownLoadExcel = 60,

        [EnumMember]
        Office365ImportTemplate = 70,

        [EnumMember]
        PhysicalDeviceWebService = 80,

        [EnumMember]
        SkyDriveProImportTemplate = 90,

        [EnumMember]
        MappingDownload = 31,

        [EnumMember]
        ReplicatorDetail = 91,

        [EnumMember]
        Office365ImportReport = 92,

        [EnumMember]
        OneDriverImportReport = 93,

        [EnumMember]
        CAAExportResult = 94,

        [EnumMember]
        RCPowerBIReport = 95,
    }
}
