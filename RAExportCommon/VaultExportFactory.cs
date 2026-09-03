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
using System.Linq;
using System.Text;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;


namespace RAExportCommon
{
    #region CodeReview
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2012/10/26",
    "Liang.Qiao@AvePoint.com",
    "Ning.Liu@AvePoint.com",
    new string[]
    {
        CodeReviewConstants.CHECK_LIST_ID_EH_1,
        CodeReviewConstants.CHECK_LIST_ID_EH_2,
        CodeReviewConstants.CHECK_LIST_ID_FA_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_10,
        CodeReviewConstants.CHECK_LIST_ID_HC_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_2,
    },
    "ADO-40830",
    true
    )]
    #endregion
    public class VautlExportfactory
    {
        public IVaultExport Create(PhysicalDeviceDto deviceDto, string jobId, string disposalClass, VaultExportFormat exportFormat, byte[] naaFile, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IVaultExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat.ToString() + "Export"),
                deviceDto,
                jobId,
                disposalClass,
                exportFormat,
                naaFile,
                encryptionKey,
                encryptionIV
                );
        }
        public IVaultExport Create(SharePointLocationDto spoDto, AveBPOSAccountInfo user, string siteUrl, string jobId, string disposalClass, VaultExportFormat exportFormat, byte[] naaFile, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IVaultExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat.ToString() + "Export"),
                spoDto,
                user,
                siteUrl,
                jobId,
                disposalClass,
                exportFormat,
                naaFile,
                encryptionKey,
                encryptionIV
            );
        }
        public IVaultExport Create(PhysicalDeviceDto deviceDto, string jobId, VaultExportFormat exportFormat, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IVaultExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat.ToString() + "Export"),
                deviceDto,
                jobId,
                exportFormat,
                fileVEO,
                recordVEO,
                manifestVEO,
                encryptionKey,
                encryptionIV
                );
        }
        
        public IVaultExport Create(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, VaultExportFormat exportFormat, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IVaultExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat.ToString() + "Export"),
                spoDto,
                user,
                siteUrl,
                jobId,
                exportFormat,
                fileVEO,
                recordVEO,
                manifestVEO,
                encryptionKey,
                encryptionIV
            );
        }

        // VEOV3Export
        public IVaultExport Create(PhysicalDeviceDto deviceDto, string jobId, string exportFormat , int ruleLevel, ArchiverSetting archiverSetting, byte[] contentVEO, byte[] historyVEO, string encryptKey)
        {
            return (IVaultExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat + "Export"),
                deviceDto,
                jobId,
                ruleLevel,
                archiverSetting,
                contentVEO,
                historyVEO,
                encryptKey
                );
        }
        
        public IVaultExport Create(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, string exportFormat , int ruleLevel, ArchiverSetting archiverSetting, byte[] contentVEO, byte[] historyVEO, string encryptKey)
        {
            return (IVaultExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat + "Export"),
                spoDto,
                user,
                siteUrl,
                jobId,
                ruleLevel,
                archiverSetting,
                contentVEO,
                historyVEO,
                encryptKey
            );
        }
        
        public IVaultExport Create(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, string exportFormat, int ruleLevel, byte[] contentVEO, byte[] historyVEO, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IVaultExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat + "Export"),
                spoDto,
                user,
                siteUrl,
                jobId,
                ruleLevel,
                contentVEO,
                historyVEO,
                encryptionKey,
                encryptionIV
            );
        }

        public IVaultExport Create(List<PhysicalDeviceDto> deviceDto, string jobId, VaultExportFormat exportFormat, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IVaultExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat.ToString() + "Export"),
                deviceDto,
                jobId,
                exportFormat,
                fileVEO,
                recordVEO,
                manifestVEO,
                encryptionKey,
                encryptionIV
                );
        }
    }
}
