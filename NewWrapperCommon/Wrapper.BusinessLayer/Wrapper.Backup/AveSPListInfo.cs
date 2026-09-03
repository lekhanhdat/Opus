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
            AveListInfo listInfo = null;
            if (mAveSPList.SPList == null)//when {System Folder}, the list is null
            {
                listInfo = new AveListInfo();
                listInfo.Title = AveConstants.SYSTEM_FOLDER;
            }
            else
            {
                try
                {
                    listInfo = mAveSPList.SPList.GetListInfo();
                    var nintexFormLibrayId = this.mAveSPList.SPList.ParentWeb.Properties.ContainsKey("nintexformslibraryid")
                            && AveTypeHelper.IsGuid(this.mAveSPList.SPList.ParentWeb.Properties["nintexformslibraryid"]) ?
                            new Guid(this.mAveSPList.SPList.ParentWeb.Properties["nintexformslibraryid"]) : Guid.Empty;
                    if (Guid.Equals(nintexFormLibrayId, listInfo.Id))
                    {
                        listInfo.IsNintexFormLibrary = true;
                    }
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while backup list basic info by API. list id:{0}. list title:{1}\n error message:{2}", mAveSPList.SPList.ID, mAveSPList.SPList.Title, e));
                    throw;
                }
            }
            return listInfo;
        }
    }

    public class AveSPListSettingInfo
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
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
            AveListSettingInfo listSettingInfo = mAveSPList.SPList.GetListSettings();
            SetUserResource(listSettingInfo);
            SetComplianceTag(listSettingInfo);
            if (AvePoint.Common.AveEnv.IsMoss && mAveSPList.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.Server07ObjectModel &&
                this.mAveSPList.ParentSite.SPSite.APIType != AveAPIType.BPOS_S)
            {
                //ADO-26398 backup metadat Keywords Setting for office365
                IAveMetadataListFieldSettings fieldSettings = mAveSPList.ParentSite.ObjectModelFactory.CreateMetadataListFieldSettings(mAveSPList.SPList);
                listSettingInfo.EnableMetaPublish = fieldSettings.EnableMetadataPromotion;
                listSettingInfo.EnterPriseKeyWordsEnable = fieldSettings.EnableKeywordsField;
            }
            return listSettingInfo;
        }
        private void SetComplianceTag(AveListSettingInfo listSettingInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.SetComplianceTag"))
            {
                var spList = mAveSPList.SPList;
                //System Folder
                if (spList != null && mAveSPList.ParentSite.SPSite.IsOnlineSite)
                {
                    listSettingInfo.ComplianceTag = spList.ComplianceTag;
                }
            }
        }
        private void SetUserResource(AveListSettingInfo listSettingInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.SetUserResource"))
            {
                var spList = mAveSPList.SPList;
                //System Folder
                if (spList != null)
                {
                    listSettingInfo.TitleResource = spList.TitleResource.GetUserResourceInfo(spList);
                    listSettingInfo.DescriptionResource = spList.DescriptionResource.GetUserResourceInfo(spList);
                }
            }
        }
    }
}