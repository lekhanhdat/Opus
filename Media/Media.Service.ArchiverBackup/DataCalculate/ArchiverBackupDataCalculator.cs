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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region directives
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.GCommon;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using global::Media.Common.ClassicStorageApi;
    #endregion

    public class ArchiverBackupDataCalculator
        : DataCalculatorBase<ArchiverCalculateInfo, ArchiverCalculateResult>
         , IDataCalculator
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem physicalDevice;

        public override void Open(ArchiverCalculateInfo calculateInfo)
        {
            this.physicalDevice = XFactoryCommon.InstanceSystem(calculateInfo.PhysicalDevice.BuildXRI());
            this.physicalDevice.Open();
        }

        public override ArchiverCalculateResult Calculate(ArchiverCalculateInfo calculateInfo)
        {
            var dataInfos = new List<ArchiverDataInfo>();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupDataCalculatorCalculateBegin);
            foreach (ArchiverDataInfo dataInfo in calculateInfo.ArchiverDataInfos)
            {
                StorageInfo info = XConvert.FromNames(dataInfo.DataVolume, String.Empty);
                if (this.physicalDevice.DirectoryExists(info))
                {
                    dataInfo.DataSize = GetDirectorySize(info, dataInfo.DataVolume, physicalDevice);
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupDataCalculatorCalculateSize, this.physicalDevice.SystemID, dataInfo.DataSize);
                }
                dataInfos.Add(dataInfo);
            }
            return new ArchiverCalculateResult
            {
                PhysicalDevice = calculateInfo.PhysicalDevice,
                ArchiverDataInfos = dataInfos,
            };
        }

        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            this.logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupDataCalculatorProcessExceptionError, this.physicalDevice.SystemID, e.ToString());
        }

        public override void Dispose()
        {
            if (this.physicalDevice != null)
            {
                this.physicalDevice.Close();
            }
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupDataCalculatorDisposeCalculateFinish);
        }

        private Int64 GetDirectorySize(StorageInfo info, String path, IXSystem physicalDevice)
        {
            long dataSize = 0;
            List<XFileInfo> fileList = physicalDevice.ListFiles(info);
            foreach (XFileInfo fileInfo in fileList)
            {
                dataSize = dataSize + fileInfo.FileSize;
            }
            return dataSize;
        }
    }
}