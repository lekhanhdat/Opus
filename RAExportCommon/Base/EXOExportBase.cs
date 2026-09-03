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
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using LOGRESOURCE = Merged18NResources.Export;
using LOGRESOURCEInternationalization = Merged18NResources.ExportForInternationalization;
using AvePoint.GCommon.Contract.CodeReview;
using System.Globalization;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;

namespace RAExportCommon
{

    internal class EXOExportBase : IDisposable
    {
        protected ExportMode CurrentExportMode;
        protected IEXOExportUtil RealVaultExport = null;
        protected Dictionary<string, IEXOExportUtil> MultileVaultExport = null;
        protected string JobId = string.Empty;    

        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 用于PhysicalDeviceDto不变的情况，适用于导出路径单一的情况。
        /// </summary>
        /// <param name="deviceDto"></param>
        /// <param name="jobId"></param>
        /// <param name="format"></param>
        private void Init(PhysicalDeviceDto deviceDto, string jobId, VaultExportFormat format, byte[] encryptionKey, byte[] encryptionIV)
        {
            JobId = jobId;
            CurrentExportMode = ExportMode.single;
            ExportFormat exportFormat = ExportFormat.Native;
            switch (format)
            {
                case VaultExportFormat.VEO:
                case VaultExportFormat.EXOVEO:
                    exportFormat = ExportFormat.Autonomy;
                    break;
                case VaultExportFormat.NAA:
                case VaultExportFormat.EXONAA:
                    exportFormat = ExportFormat.NAA;
                    break;
                case VaultExportFormat.NARA:
                case VaultExportFormat.EXONARA:
                    exportFormat = ExportFormat.NARA;
                    break;

                default:
                    throw new Exception(string.Format("Do not support the format: {0}", format.ToString()));
            }
            RealVaultExport = new EXOExportUtil(deviceDto, jobId, exportFormat, encryptionKey, encryptionIV);
        }
        
        private void Init(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, VaultExportFormat format, byte[] encryptionKey, byte[] encryptionIV)
        {
            JobId = jobId;
            CurrentExportMode = ExportMode.single;
            ExportFormat exportFormat = ExportFormat.Native;
            switch (format)
            {
                case VaultExportFormat.VEO:
                case VaultExportFormat.EXOVEO:
                    exportFormat = ExportFormat.Autonomy;
                    break;
                case VaultExportFormat.NAA:
                case VaultExportFormat.EXONAA:
                    exportFormat = ExportFormat.NAA;
                    break;
                case VaultExportFormat.EXONARA:
                case VaultExportFormat.NARA:
                    exportFormat = ExportFormat.NARA;
                    break;
                default:
                    throw new Exception(string.Format("Do not support the format: {0}", format.ToString()));
            }
            RealVaultExport = new EXOExportUtil(spoDto, user, siteUrl, jobId, exportFormat, encryptionKey, encryptionIV);
        }

        /// <summary>
        /// 用于多个不同PhysicalDeviceDto导出的情况。
        /// </summary>
        /// <param name="deviceDtos"></param>
        /// <param name="jobId"></param>
        /// <param name="format"></param>
        private void Init(List<PhysicalDeviceDto> deviceDtos, string jobId, VaultExportFormat format, byte[] encryptionKey, byte[] encryptionIV)
        {
            JobId = jobId;
            CurrentExportMode = ExportMode.Multile;
            MultileVaultExport = new Dictionary<string, IEXOExportUtil>();
            foreach (PhysicalDeviceDto Dev in deviceDtos)
            {
                ExportFormat exportFormt = ExportFormat.Native;
                exportFormt = (ExportFormat)Enum.Parse(typeof(ExportFormat), format.ToString());
                IEXOExportUtil VaultExport = new EXOExportUtil(Dev, jobId, exportFormt, encryptionKey, encryptionIV);
                MultileVaultExport.Add(Dev.Id, VaultExport);
            }
        }

        public EXOExportBase(PhysicalDeviceDto deviceDto, string jobId, VaultExportFormat format, byte[] encryptionKey, byte[] encryptionIV)
        {
            Init(deviceDto, jobId, format, encryptionKey, encryptionIV);
        }

        public EXOExportBase(List<PhysicalDeviceDto> deviceDtoWithRule, string jobId, VaultExportFormat format, byte[] encryptionKey, byte[] encryptionIV)
        {
            Init(deviceDtoWithRule, jobId, format, encryptionKey, encryptionIV);
        }
        
        public EXOExportBase(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, VaultExportFormat format, byte[] encryptionKey, byte[] encryptionIV)
        {
            Init(spoDto,user, siteUrl, jobId, format, encryptionKey, encryptionIV);
        }

        protected enum ExportMode
        {
            single,
            Multile,
        }

        public void Dispose()
        {
            try
            {
                if (RealVaultExport != null)
                {
                    RealVaultExport.Dispose();
                    RealVaultExport = null;
                }
                if (MultileVaultExport != null)
                {
                    foreach (string key in MultileVaultExport.Keys)
                    {
                        MultileVaultExport[key].Dispose();
                    }
                    MultileVaultExport = null;
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while Dispose Vault Export Base. Error is :{0}", e.ToString());
            }
        }
    }
}
