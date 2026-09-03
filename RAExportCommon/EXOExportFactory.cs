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
    public class EXOExportFactory
    {
        public IEXOExport Create(PhysicalDeviceDto deviceDto, string jobId, string disposalClass, VaultExportFormat exportFormat, byte[] naaFile, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IEXOExport)Activator.CreateInstance(
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
        
        public IEXOExport Create(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, string disposalClass, VaultExportFormat exportFormat, byte[] naaFile, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IEXOExport)Activator.CreateInstance(
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
        public IEXOExport Create(PhysicalDeviceDto deviceDto, string jobId, VaultExportFormat exportFormat, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IEXOExport)Activator.CreateInstance(
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
        
        public IEXOExport Create(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, VaultExportFormat exportFormat, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IEXOExport)Activator.CreateInstance(
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

        public IEXOExport Create(List<PhysicalDeviceDto> deviceDto, string jobId, VaultExportFormat exportFormat, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
        {
            return (IEXOExport)Activator.CreateInstance(
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

        #region VEO V3
        public IEXOExport Create(PhysicalDeviceDto deviceDto, string jobId, string exportFormat, byte[] veoContent, byte[] veoHistory, ArchiverSetting archiverSetting, string encryptKey)
        {
            return (IEXOExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat + "Export"),
                deviceDto,
                jobId,
                veoContent,
                veoHistory,
                archiverSetting,
                encryptKey
                );
        }
        
        public IEXOExport Create(SharePointLocationDto spoDto, AveBPOSAccountInfo user, string siteUrl, string jobId, string exportFormat, byte[] veoContent, byte[] veoHistory, ArchiverSetting archiverSetting, string encryptKey)
        {
            return (IEXOExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat + "Export"),
                spoDto,
                user,
                siteUrl,
                jobId,
                veoContent,
                veoHistory,
                archiverSetting,
                encryptKey
            );
        }

        public IEXOExport Create(List<PhysicalDeviceDto> deviceDtos, string jobId, string exportFormat, byte[] veoContent, byte[] veoHistory, ArchiverSetting archiverSetting, string encryptKey)
        {
            return (IEXOExport)Activator.CreateInstance(
                Type.GetType(this.GetType().Namespace + "." + exportFormat + "Export"),
                deviceDtos,
                jobId,
                veoContent,
                veoHistory,
                archiverSetting,
                encryptKey
                );
        }
        #endregion


    }
}
