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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public static class SharepointUtil
    {
        private static RALogger mLog = RALogger.GetInstance(typeof(SharepointUtil));

        private static ITenantService mTenantService;
        private static ITenantService TenantService
        {
            get
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
            }
        }

        private static List<string> designLists = null;

        private static List<string> DesignLists
        {
            get
            {
                if (designLists == null)
                {
                    designLists = WebUtil.GetDesignLists(TenantService.IsCSDTenant());
                }
                return designLists;
            }
        }

        public static bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"An error has occurred when CheckIsDesignList, message:{e.Message}");
            }
            return isDesignList;
        }

        public static bool CheckIsDesignList(IAveList discoverList)
        {
            return CheckIsDesignList(CombineListUrlAndTemplate(discoverList));
        }

        private static string CombineListUrlAndTemplate(IAveList discoverList)
        {
            string combineUrlTemplate = string.Empty;
            string listUrl = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(discoverList.RootFolder.ServerRelativeUrl))
                {
                    int listUrlIndex = discoverList.RootFolder.ServerRelativeUrl.LastIndexOf("/");
                    if (listUrlIndex > 0)
                    {
                        listUrl = discoverList.RootFolder.ServerRelativeUrl.Substring(listUrlIndex + 1);
                    }
                    else
                    {
                        listUrl = discoverList.Title;
                    }
                    combineUrlTemplate = listUrl + (int)discoverList.BaseTemplate;
                    mLog.Info($"CombineListUrlAndTemplate combineUrlTemplate is {combineUrlTemplate}.");
                }
                else
                {
                    mLog.Info("CombineListUrlAndTemplate discoverList.RootFolderUrl is IsNullOrEmpty.");
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"CombineListUrlAndTemplate error: ({ex})");
                combineUrlTemplate = string.Empty;
            }
            return combineUrlTemplate;
        }

    }
}
