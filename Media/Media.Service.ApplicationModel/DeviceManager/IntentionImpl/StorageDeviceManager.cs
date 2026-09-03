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




namespace AvePoint.Media.Service
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Common;
    using global::Media.Common.ClassicStorageApi;
    using Merged18NResources.MediaServiceApplicationModel;
    using Merged18NResources.MediaServiceGranularBackup;
    using Storage;
    using Storage.Util;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/6/20",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_2 },
    "ADO-34389",
    true)]

    #endregion CodeReview

    public class StorageDeviceManager : IStorageDeviceManager
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IXSystem Open(List<String> deviceConnectionString)
        {
            var device = XFactoryCommon.InstanceLibrary(deviceConnectionString);
            device.Open();
            return device;
        }

        public IXSystem Open(List<String> deviceConnectionString, DeviceAccess deviceAccess)
        {
            var device = XFactoryCommon.InstanceLibrary(deviceConnectionString);
            try
            {
                device.Open();
            }
            catch (Exception e)
            {
                this.logger.Error(MediaServiceGranularBackupResource.StorageDeviceManagerOpenFailed, e.ToString());
                var exceptionResult = device.SystemHealth;
                switch (exceptionResult)
                {
                    case XSystemHealth.AuthenticationFailed:
                        throw new AuthenticationFailedException(String.Format(MediaServiceApplicationModelResource.StorageDeviceManagerOpenAuthenticationFailed));
                    case XSystemHealth.ConnectedFailed:
                    case XSystemHealth.Unaccessable:
                        throw new Exception(String.Format(MediaServiceApplicationModelResource.StorageDeviceManagerOpenUnaccessed));
                    default:
                        throw new Exception(String.Format(MediaServiceApplicationModelResource.StorageDeviceManagerOpenError, e.Message.ToString()));
                }
            }
            var validateResult = device.Validate();
            ThrowIfDeviceisInvalid(validateResult);
            if (validateResult.SystemHealth == XSystemHealth.Available && deviceAccess == DeviceAccess.ReadWrite)
            {
                throw new NotEnoughFreeSpaceException(String.Format(MediaServiceApplicationModelResource.StorageDeviceManagerOpenNotEnough));
            }
            return device;
        }

        public void Close(IXSystem storageDevice)
        {
            if (storageDevice != null)
                storageDevice.Close();
        }

        public IXSystem OpenDataSystemForWrite(LogicalDeviceDto dataLogicalDevice, Boolean useSnapLock = false)
        {
            if (dataLogicalDevice.PhysicalDrives.Count == 0)
            {
                //NetApp: Farm is unable to use.
                throw new Exception(ServiceConstants.FarmCannotBeUsedByPhysicalDevice);
            }
            XLibrary device;
            if (useSnapLock)
                device = XFactoryCommon.InstanceLibrary(dataLogicalDevice.GetSnapLockXRIS());
            else
                device = XFactoryCommon.InstanceLibrary(dataLogicalDevice.GetXRIS(PhysicalDeviceUsage.Data));
            try
            {
                device.Open();
            }
            catch (Exception e)
            {
                var exceptionResult = device.SystemHealth;
                switch (exceptionResult)
                {
                    case XSystemHealth.AuthenticationFailed:
                        throw new AuthenticationFailedException(String.Format(MediaServiceApplicationModelResource.StorageDeviceManagerOpenAuthenticationFailed));
                    case XSystemHealth.ConnectedFailed:
                    case XSystemHealth.Unaccessable:
                        throw new Exception(String.Format(MediaServiceApplicationModelResource.StorageDeviceManagerOpenUnaccessed));
                    default:
                        throw new Exception(String.Format(MediaServiceApplicationModelResource.StorageDeviceManagerOpenError, e.Message));
                }
            }
            var validateResult = device.Validate();
            if (validateResult.SystemHealth == XSystemHealth.Available)
            {
                throw new NotEnoughFreeSpaceException(String.Format(MediaServiceApplicationModelResource.StorageDeviceManagerOpenNotEnough));
            }
            return device;
        }

        internal void ThrowIfDeviceisInvalid(StorageOpenValidResult result)
        {
            logger.Info("Device validate result: {0}", result.SystemHealth);
            switch (result.SystemHealth)
            {
                case XSystemHealth.ConnectedFailed:
                    throw new DeviceNotAvailableException("Cannot connect to the device successfully. Please check if the device is available or if there is any firewall rule block the connection, fix it and try again later.");
                case XSystemHealth.AuthenticationFailed:
                    throw new AuthenticationFailedException("Cannot connect to the device successfully. Please check if the credential of your device has been changed, fix it and try again later.");
                default:
                    return;
            }
        }
    }
}