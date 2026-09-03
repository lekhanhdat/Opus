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




namespace AvePoint.Media.Core.Index
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using global::Media.Common.ClassicStorageApi;
    using AvePoint.Media.Storage.Util;
    using AvePoint.Common;
    using AvePoint.RA.Contract.Services;

    #endregion using directives

    public class IndexDeviceValidateManager : IIndexDeviceValidateManager
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// At the beginning of a backup job,validate index logical device is available or not,
        /// if it is available, choose the physical device which has largest free space to locate index.
        /// </summary>
        /// <param name="parameter">index info</param>
        public void Validate(IndexDeviceValidateParameter parameter)
        {
            if (parameter.IndexWorkingSystem.IsDirectSystem)
            {
                this.logger.Debug("Start Valid storage");
                var indexInfo = new StorageInfo(parameter.IndexVolume, parameter.IndexName);
                if (parameter.IndexWorkingSystem.FileExists(indexInfo))
                {
                    EnsureIndexLocatedInBestDeviceByMove(parameter, indexInfo);
                    parameter.IndexWorkingSystem.OpenFile(indexInfo);
                }
                else
                {
                    EnsureIndexHasBestLocationToWrite(parameter);
                }
                this.logger.Debug($"Finish valid storage:{parameter.IndexWorkingSystem.SystemLocation.LogBase64()}");
            }
        }

        private void EnsureIndexHasBestLocationToWrite(IndexDeviceValidateParameter parameter)
        {
            var xLibary = XFactoryCommon.InstanceLibrary(parameter.LogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            if (xLibary.SubSystems.Count == 1) { return; }
            var indexInfo = new StorageInfo(parameter.IndexVolume, parameter.IndexName);
            var maxFreeSizeSystem = GetMaxFreeSizeSystem(parameter, xLibary);
            parameter.IndexWorkingSystem.FindCondition = new Predicate<IXSystem>(xSystem => { return xSystem.SystemLocation.Equals(maxFreeSizeSystem.SystemLocation); });
        }

        private void EnsureIndexLocatedInBestDeviceByMove(IndexDeviceValidateParameter parameter, StorageInfo indexInfo)
        {
            var xLibary = XFactoryCommon.InstanceLibrary(parameter.LogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            if (xLibary.SubSystems.Count == 1) { return; }
            var indexFileSize = parameter.IndexWorkingSystem.OpenFile(indexInfo).FileSize;
            IXSystem indexSystem;
            var maxFreeSizeSystem = GetMaxFreeSizeSystemByIndexSize(parameter, xLibary, indexInfo, indexFileSize, out indexSystem);
            if (!indexSystem.SystemLocation.Equals(maxFreeSizeSystem.SystemLocation))
            {
                MoveTo(indexSystem, maxFreeSizeSystem, indexInfo);
            }
        }

        private IXSystem GetMaxFreeSizeSystem(IndexDeviceValidateParameter parameter, XLibrary xLibary)
        {
            Dictionary<IXSystem, ulong> freeSpaces = new Dictionary<IXSystem, ulong>();
            foreach (var subSystem in xLibary.SubSystems)
            {
                try
                {
                    subSystem.Open();
                    var validateResult = subSystem.Validate();
                    if (validateResult.SystemHealth == XSystemHealth.AvailableAndNotFull)
                    {
                        var freeSpace = subSystem.AvailableSpace;
                        logger.Debug($"get storage free space:{subSystem.SystemLocation.LogBase64()},space:{freeSpace}");
                        freeSpaces.Add(subSystem, freeSpace);
                    }
                }
                finally
                {
                    subSystem.Close();
                }
            }
            freeSpaces = (from entry in freeSpaces
                          orderby entry.Value ascending
                          select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
            return freeSpaces.Last().Key;
        }

        private IXSystem GetMaxFreeSizeSystemByIndexSize(IndexDeviceValidateParameter parameter, XLibrary xLibary, StorageInfo indexInfo, long indexFileSize, out IXSystem indexSystem)
        {
            indexSystem = null;
            Dictionary<IXSystem, ulong> freeSpaces = new Dictionary<IXSystem, ulong>();
            foreach (var subSystem in xLibary.SubSystems)
            {
                try
                {
                    subSystem.Open();
                    var validateResult = subSystem.Validate();
                    {
                        if (subSystem.FileExists(indexInfo))
                        {
                            indexSystem = subSystem;
                            freeSpaces.Add(subSystem, (ulong)indexFileSize + subSystem.AvailableSpace);
                        }
                        else
                        {
                            if (validateResult.SystemHealth == XSystemHealth.AvailableAndNotFull)
                                freeSpaces.Add(subSystem, subSystem.AvailableSpace);
                        }
                    }
                }
                finally
                {
                    subSystem.Close();
                }
            }
            freeSpaces = (from entry in freeSpaces
                          orderby entry.Value ascending
                          select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
            return freeSpaces.Last().Key;
        }

        private void MoveTo(IXSystem sourceDevice, IXSystem destinationDevice, StorageInfo indexInfo)
        {
            try
            {
                sourceDevice.Open();
                destinationDevice.Open();
                byte[] buffer = new byte[64 * 1024];
                using (XStream sourceStream = sourceDevice.OpenStream(indexInfo, FileMode.Open))
                {
                    using (XStream destinationStream = destinationDevice.OpenStream(indexInfo, FileMode.CreateNew))
                    {
                        while (true)
                        {
                            int readLen = sourceStream.Read(buffer, 0, buffer.Length);
                            if (readLen <= 0) break;
                            destinationStream.Write(buffer, 0, readLen);
                        }
                        destinationStream.Commit();
                    }
                }
                sourceDevice.DeleteFile(indexInfo);
            }
            catch (Exception e)
            {
                logger.Error($"valiad move to error:{e}");
                throw;
            }
            finally
            {
                sourceDevice.Close();
                destinationDevice.Close();
            }
        }
    }
}