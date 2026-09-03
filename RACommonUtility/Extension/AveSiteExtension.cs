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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.Enums;
using AvePoint.Wrapper.Common;

namespace AvePoint.RA.RACommonUtility.Extension;

public static class AveSiteExtension
{
    private static readonly RALogger _logger = RALogger.GetInstance(typeof(AveSiteExtension));
    private const string BLOCK_DELETE_AND_EDIT = "BlockDelete, BlockEdit";
    private const string ROOTWEB_DECLARE_SETTING_PROPERTY = "ecm_siterecordrestrictions";
    internal static Guid HoldRecordStatus
    {
        get
        {
            return new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E");
        }
    }
    public static int GetMaxItemsPerThrottledOperation(this IAveSite aveSite)
    {
        int maxItemsPer = 5000;
        try
        {
            var dataCacheType = aveSite.GetType().GetProperty("DataCache");
            var dataCacheObj = dataCacheType.GetValue(aveSite);
            BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache", InstanceBindFlags);
            var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
            var propertiesDic = (propertiesCacheObj as AveDictionary<string, object>);
            object maxItemsPerObj;
            if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
            {
                maxItemsPer = Convert.ToInt32(maxItemsPerObj);
                _logger.Info($"GetMaxItemsPerThrottledOperation succeed. Count:[{maxItemsPer}]");

                if (maxItemsPer > 2000)
                {
                    _logger.Info("MaxItemsPerThrottledOperation is > 2000, limit it to 2000");
                    maxItemsPer = 2000;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
        }
        return maxItemsPer;
    }

    public static void EnsureRecordFeatureEnabled(this IAveSite spSite, Guid mRecordFeatureId)
    {
        try
        {
            if (spSite.Features[mRecordFeatureId] == null)
            {
                spSite.Features.Add(mRecordFeatureId, true);
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex);
        }
        catch (Exception ex)
        {
            _logger.Warn("Activate In Place Records Management feature error:{0}", ex);
        }
    }

    public static bool CheckDeclarationSettingIsBlockEditAndDelete(this IAveSite site)
    {       
        return site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY)
            && site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY].ToString() == BLOCK_DELETE_AND_EDIT;
    }

    public static bool IsOD4BSite(this IAveSite site)
    {
        bool isOD4B = false;
        try
        {
            var siteInfo = site.SiteSerializer.GetObjectData() as AveSiteInfo;
            if (siteInfo != null && siteInfo.WebTemplate != null)
            {
                isOD4B = siteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (Exception e)
        {
            _logger.Warn("An error occurrred while checking if site is OD4BSite. Url:{0} Error:{1}", site.Url, e.ToString());
        }
        _logger.Info("Site:{0} is OD4BSite:{1}", site.Url, isOD4B);
        return isOD4B;
    }

    public static bool IsBlockEditAndDeleteRecord(IAveListItem item)
    {
        return IsBlockEditAndDeleteRecord(GetHoldAndRecordStatus(item));
    }

    private static bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
    {
        return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
    }

    private static int GetHoldAndRecordStatus(IAveListItem item)
    {
        int result = 0;
        try
        {
            if ((GetBoolIprPropertyCore(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
            {
                try
                {
                    if (item.Fields.Contains(HoldRecordStatus))
                    {
                        object obj2 = item[HoldRecordStatus];
                        if ((obj2 != null) && !int.TryParse(obj2.ToString(), out result))
                        {
                            result = 0;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    result = 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(string.Format("An error occur in get hold and declare status, reason : {0}.", ex.ToString()));
        }
        return result;
    }

    private static bool GetBoolIprPropertyCore(IAveList list, string propName)
    {
        bool? nullable = null;
        if (list != null && list.RootFolder != null && list.RootFolder.Properties != null)
        {
            object obj = list.RootFolder.Properties[propName];
            if (obj != null) nullable = new bool?(obj.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase));
        }
        return (nullable == true);
    }

    private static bool IsHoldOrRecordsEnabled(IAveList list)
    {
        if (list == null || list.Fields == null)
        {
            throw new ArgumentNullException("list");
        }
        if (list.Fields.Contains(new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")))
        {
            return (list.Fields[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")] != null);
        }
        else
        {
            return false;
        }
    }

    public static void EnsureWebDeclarationSetting(this IAveSite site)
    {
        if (site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY)
            && site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY].ToString() == BLOCK_DELETE_AND_EDIT)
        {
            return;
        }

        string orginWebOption = RecordRestrictions.None.ToString();
        if (site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY))
        {
            orginWebOption = (site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY]?.ToString());
        }
        try
        {
            site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY] = BLOCK_DELETE_AND_EDIT;
            site.RootWeb.Update();
        }
        catch (Exception e)
        {
            _logger.Warn($"EnsureWebDeclarationSetting error {e}");
            _logger.Warn($"EnsureWebDeclarationSetting update web property {ROOTWEB_DECLARE_SETTING_PROPERTY} error, reset property to {orginWebOption}");
            site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY] = orginWebOption;
        }
        site.RootWeb.ReloadWeb();
        if (!site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY))
        {
            _logger.Warn("Add web prop ecm_siterecordrestrictions is error, please check site DenyAddAndCustomizePages is disabled.");
            throw new Exception("RM_UI_Failed_EnableCustomScript");
        }
        else
        {
            var webOption = site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY].ToString();
            if (null == webOption || !string.Equals(webOption, BLOCK_DELETE_AND_EDIT, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warn("Update web prop ecm_siterecordrestrictions is error, please check site DenyAddAndCustomizePages is disabled.");
                throw new Exception("RM_UI_Failed_EnableCustomScript");
            }
        }
    }

}