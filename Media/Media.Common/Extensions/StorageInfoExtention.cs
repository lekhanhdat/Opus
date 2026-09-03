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
using Storage;
using Storage.Cloud.Azure;
using Storage.Cloud.Common;
using Storage.Cloud.Google;

namespace AvePoint.GCommon.Utility
{
    public static class StorageInfoExtention
    {
        /// <summary>
        /// Converts the specified Azure file tier type to the corresponding Google Cloud Storage class.
        /// </summary>
        /// <param name="azureInfo"></param>
        /// <returns>A GoogleStorageClass value that corresponds to the Azure AccessTierType.</returns>
        public static GoogleStorageClass ToGoogleStorageClass(this AccessTierType tierType)
        {
            return tierType switch
            {
                AccessTierType.Hot => GoogleStorageClass.Standard,
                AccessTierType.Cool => GoogleStorageClass.Nearline,
                AccessTierType.Cold => GoogleStorageClass.Coldline,
                AccessTierType.Archive => GoogleStorageClass.Archive,
                _ => GoogleStorageClass.Standard
            };
        }

        /// <summary>
        /// Converts the specified Google Cloud Storage class to the corresponding Azure file tier type.
        /// </summary>
        /// <param name="storageClass"></param>
        /// <returns>A Azure AccessTierType value that corresponds to the GoogleStorageClass</returns>
        public static AccessTierType ToAzureTierType(this GoogleStorageClass storageClass)
        {
            return storageClass switch
            {
                GoogleStorageClass.Standard => AccessTierType.Hot,
                GoogleStorageClass.Nearline => AccessTierType.Cool,
                GoogleStorageClass.Coldline => AccessTierType.Cold,
                GoogleStorageClass.Archive => AccessTierType.Archive,
                _ => AccessTierType.Hot
            };
        }

        /// <summary>
        /// Converts the given StorageInfo to the correct type based on the storage system's storage type.
        /// </summary>
        /// <param name="storageInfo"></param>
        /// <param name="storageSystem"></param>
        /// <returns></returns>
        public static StorageInfo ToCorrectTypeStorageInfo(this StorageInfo storageInfo, IXSystem storageSystem)
        {
            StorageInfo? result = null;
            PathConverter converter = new PathConverter();
            string objectName = SecurityUtils.SafeCombinePath(storageInfo.HighName, storageInfo.LowName);
            switch (storageSystem.StorageType)
            {
                case XStorageType.Azure:
                    result = converter.ToStorageInfo<AzureCloudInfo>(objectName);
                    break;
                case XStorageType.GoogleCloud:
                    result = converter.ToStorageInfo<GoogleCloudInfo>(objectName);
                    break;
                default:
                    result = XConvert.FromNames(storageInfo.HighName, storageInfo.Name);
                    break;
            }
            return result ?? storageInfo;
        }
    }
}
