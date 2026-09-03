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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveMetadataServiceCache
    {
        internal static AveLogger logger = AveLogger.GetInstance(typeof(AveMetadataServiceCache));
        internal static bool EnableCache = true;
        internal static Dictionary<Guid, AveMetadataServiceCacheInfo> TermStoreInfoCache = new Dictionary<Guid, AveMetadataServiceCacheInfo>();
    }

    internal class AveMetadataServiceCacheInfo
    {
        internal DateTime LastModifiedTime { get; set; }

        internal DateTime LastAccessTime { get; set; }

        internal AveTermStoreInfo TermStoreInfo { get; set; }

        public AveMetadataServiceCacheInfo()
        {
            LastModifiedTime = DateTime.MinValue;
            LastAccessTime = DateTime.MinValue;
            TermStoreInfo = null;
        }
    }

    public class AveMetadataService
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory objectModelFactory;
        private IAveSite mAveSite = null;

        public AveMetadataService(AveObjectModelFactory objectModelFactory)
        {
            this.objectModelFactory = objectModelFactory;
        }

        public AveMetadataService(IAveSite mAveSite)
        {
            this.mAveSite = mAveSite;
        }

        /// <summary>
        /// Export Term信息时是否忽略Global的Term Group，默认为不忽略。
        /// </summary>
        public bool SkipGlobalTermGroup
        {
            get
            {
                if (mAveSite == null)
                {
                    throw new ArgumentNullException();
                }
                return mAveSite.MetaDataServiceSerializer.SkipGlobalTermGroup;
            }
            set
            {
                if (mAveSite == null)
                {
                    throw new ArgumentNullException();
                }
                mAveSite.MetaDataServiceSerializer.SkipGlobalTermGroup = value;
            }
        }

        public bool IsTeamsLevelJob
        {
            get
            {
                if (mAveSite == null)
                {
                    throw new ArgumentNullException();
                }
                return GetSerializerTeamsLevelJob();
            }
            set
            {
                if (mAveSite == null)
                {
                    throw new ArgumentNullException();
                }
                SetSerializerTeamsLevelJob(value);
            }
        }

        private bool GetSerializerTeamsLevelJob()
        {
            PropertyInfo propertyInfo = mAveSite.MetaDataServiceSerializer.GetType().GetProperty("IsTeamsLevelJob");
            if (propertyInfo == null)
            {
                return false;
            }

            object value = propertyInfo.GetValue(mAveSite.MetaDataServiceSerializer, null);
            return value is bool boolValue && boolValue;
        }

        private void SetSerializerTeamsLevelJob(bool value)
        {
            PropertyInfo propertyInfo = mAveSite.MetaDataServiceSerializer.GetType().GetProperty("IsTeamsLevelJob");
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(mAveSite.MetaDataServiceSerializer, value, null);
            }
        }

        public void Export(IAveBackupStream output, Guid serviceApplicationId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveMetadataService.Export"))
            {
                IAveMetaDataServiceSerializer serilizer = this.objectModelFactory.CreateMetadataServiceSerilizer(serviceApplicationId);
                //等wrapper中serilizer.GetObjectData()方法修改之后再运行这个逻辑
                //AveManagedMetadataServiceApplicationInfo metadataServiceApplicationInfo = serilizer.GetObjectData() as AveManagedMetadataServiceApplicationInfo;
                //output.WriteMetadata(AveMetadataType.MetadataService, metadataServiceApplicationInfo);
                output.WriteMetadata(AveMetadataType.MetadataService, serilizer.GetObjectData());
            }
        }

        public void Export(IAveBackupStream output)
        {
            Export(output, false);
        }

        public void Export(IAveBackupStream output, bool enbaleCache)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.MetadataService"))
            {
                try
                {
                    if (enbaleCache)
                    {
                        mAveSite.MetaDataServiceSerializer.EnableCache = true;
                    }
                    //log
                    List<AveTermStoreInfo> mMetadataInfo = mAveSite.MetaDataServiceSerializer.GetObjectData() as List<AveTermStoreInfo>;
                    //Log
                    output.WriteMetadata(AveMetadataType.MetadataService, mMetadataInfo);
                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.BackupMetadataServiceFailedEventMessage(ex));
                }
            }
        }
    }
}