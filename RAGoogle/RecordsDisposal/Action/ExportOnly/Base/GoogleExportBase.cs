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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Media.StorageService;

namespace RAGoogle
{
    internal class GoogleExportBase : IDisposable
    {
        protected ExportMode CurrentExportMode;
        protected IGoogleExportUtil RealVaultExport = null;
        protected string JobId = string.Empty;
        private static AveLogger _logger = AveLogger.GetInstance(typeof(GoogleExportBase));
        public GoogleExportBase(PhysicalDeviceDto deviceDto, string jobId)
        {
            Init(deviceDto, jobId);
        }
        private void Init(PhysicalDeviceDto deviceDto, string jobId)
        {
            JobId = jobId;
            CurrentExportMode = ExportMode.Single;
            RealVaultExport = new GoogleExportUtil(deviceDto, jobId, ExportFormat.NARA);
        }
        protected enum ExportMode
        {
            Single,
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
            }
            catch (Exception e)
            {
                _logger.Warn("An error occurred while Dispose Vault Export Base. Error is :{0}", e.ToString());
            }
        }
    }
}
