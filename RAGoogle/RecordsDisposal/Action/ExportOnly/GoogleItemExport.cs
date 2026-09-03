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

using AvePoint.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using RAGoogle.Models;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using System.Text;

namespace RAGoogle
{
    internal class GoogleItemExport : GoogleObjectBackup
    {
        internal void VaultExport(DownloadedFileInfo entity)
        {
            try
            {
                GoogleExportInfo exportInfo = null;
                var googleItemPathInfo = new GoogleItemPathGeneratorInfo()
                {
                    JobId = this.Configuration.JobId,
                    GoogleItem = entity,
                    ParentFolderName = entity.ParentId,
                    NodeLevel = Configuration.SelectedNode.Level
                };
                using (var performance = new PerformanceScope("GoogleItemExport.GenerateGoogleItemExportInfo", "", true))
                {
                    exportInfo = this.GoogleExportBeforeArcInfo.GoogleExportPathGenerator.GenerateGoogleItemExportInfo(googleItemPathInfo);
                }
                using (var performance = new PerformanceScope("GoogleItemExport.ExportGoogleItem", "", true))
                {
                    exportInfo.DisposalClassString = Configuration.CurrentRule.DisposalClass;
                    var recordsEncryptionKey = Configuration.CurrentRule.GoogleDriveRule.ExportDataEncryptionKey;
                    var recordsEncryptionIV = Configuration.CurrentRule.GoogleDriveRule.ExportDataEncryptionIV;
                    if (!string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                    {
                        _logger.Info("Export data encryption is enabled.");
                        exportInfo.EncryptionKey = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                        exportInfo.EncryptionIV = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                    }
                    this.GoogleExportBeforeArcInfo.GoogleExport.ExportGoogleItem(exportInfo);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
    internal class GoogleFolderExport : GoogleObjectBackup
    {
        internal void VaultExport(GoogleItemData item)
        {
            try
            {
                using (var performance = new PerformanceScope("GoogleItemExport.ExportGoogleFolder", "", true))
                {
                    this.GoogleExportBeforeArcInfo.GoogleExport.ExportGoogleFolder(item);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Export GoogleItem:" + ex.ToString());
                throw;
            }
        }
    }
}
