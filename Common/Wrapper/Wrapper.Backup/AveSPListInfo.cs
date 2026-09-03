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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPListInfo
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPList mAveSPList = null;

        public AveSPListInfo(AveSPList aveSPList)
        {
            mAveSPList = aveSPList;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ListInfo"))
            {
                output.WriteMetadata(AveMetadataType.ListBasicInfo, GetListInfo());
            }
        }

        public AveListInfo GetListInfo()
        {
            AveListInfo listInfo = new AveListInfo();

            if (mAveSPList.SPList == null)//when {System Folder}, the list is null
            {
                listInfo.Title = AveConstants.SYSTEM_FOLDER;
                return listInfo;
            }
            try
            {
                return mAveSPList.SPList.GetListInfo();
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while backup list basic info by API. list id:{0}. list title:{1}\n error message:{2}", mAveSPList.SPList.ID, mAveSPList.SPList.Title, e));
                throw;
            }
        }
    }

    public class AveSPListSettingInfo
    {
        private AveSPList mAveSPList = null;

        public AveSPListSettingInfo(AveSPList aveSPList)
        {
            mAveSPList = aveSPList;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ListSettingInfo"))
            {
                output.WriteMetadata(AveMetadataType.ListProperty, GetListSettingInfo());
            }
        }

        public AveListSettingInfo GetListSettingInfo()
        {
            return mAveSPList.SPList.GetListSettings();
        }
    }
}