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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Common.Extension;
using AvePoint.Wrapper.Common;
namespace AvePoint.Wrapper.Restore.Extension
{
    public static class WrapperRestoreExtension
    {
        //public static void RestoreProjectPolicy(this AvePoint.Wrapper.Restore.AveSPWeb mAveSPWeb)
        //{
        //    if (mAveSPWeb.ParentSite.ObjectModelFactory as AveObjectModelFactoryExtension != null)
        //    {
        //        AveProjectPolicyInfo policyInfo = new AveProjectPolicyInfo();
        //        if (null != mAveSPWeb.WebSettingInfo.IsSiteClosed && mAveSPWeb.WebSettingInfo.IsSiteClosed.IsAvailable)
        //        {
        //            policyInfo.IsSiteClosed = mAveSPWeb.WebSettingInfo.IsSiteClosed.Value;
        //        }
        //        if (null != mAveSPWeb.WebSettingInfo.ProjectPolicyContentTypeId && mAveSPWeb.WebSettingInfo.ProjectPolicyContentTypeId.IsAvailable)
        //        {
        //            policyInfo.ProjectPolicyContentType = mAveSPWeb.WebSettingInfo.ProjectPolicyContentTypeId.Value;
        //        }
        //        if (null != mAveSPWeb.WebSettingInfo.SiteClosedTime && mAveSPWeb.WebSettingInfo.SiteClosedTime.IsAvailable)
        //        {
        //            policyInfo.SiteClosedTime = mAveSPWeb.WebSettingInfo.SiteClosedTime.Value;
        //        }
        //        if (null != mAveSPWeb.WebSettingInfo.ProjectExpirationDate && mAveSPWeb.WebSettingInfo.ProjectExpirationDate.IsAvailable)
        //        {
        //            policyInfo.projectExpirationDate = mAveSPWeb.WebSettingInfo.ProjectExpirationDate.Value;
        //        }
        //        if (null != mAveSPWeb.WebSettingInfo.ProjectPolicyName && mAveSPWeb.WebSettingInfo.ProjectPolicyName.IsAvailable)
        //        {
        //            policyInfo.ProjectPolicyName = mAveSPWeb.WebSettingInfo.ProjectPolicyName.Value;
        //        }

        //        AvePoint.Wrapper.Restore.AveSPSite mAveSPSite = mAveSPWeb.ParentSite;
        //        if (mAveSPSite.SPContextKind == AveContextKind.Server13ObjectModel && AvePoint.Common.AveEnv.IsMoss == true)
        //        {
        //            IAveProjectPolicyItemListUtility utility = ((AveObjectModelFactoryExtension)mAveSPSite.ObjectModelFactory).CreatePolicyItemListUtility();
        //            utility.SetObjectData(mAveSPSite.SPSite.ID, mAveSPWeb.SPWeb.ID, policyInfo);
        //        }
        //    }
        //}
       
    }
}
