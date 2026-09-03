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
using AvePoint.RA.Contract.Global.Object;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Common.Global.Utils
{
    public class RMSPSettingUtil
    {
        private static List<RMSharePointOnPremiseSetting> mAllSettings;
        public static void Init(List<RMSharePointOnPremiseSetting> settings)
        {
            mAllSettings = settings;
        }
        public static RMSharePointOnPremiseSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId)
        {
            return mAllSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ScopeId == scopeId).FirstOrDefault();
        }
        public static RMSharePointOnPremiseSetting LoadSharePointSetting(Guid id, Guid siteId, bool includeOnlySetPhysicalNode = false)
        {
            //using (var context = GetNewContext())
            {
                RMSharePointOnPremiseSetting spSetting = null;
                if (siteId != Guid.Empty)
                {
                    spSetting = mAllSettings.Where(s => s.ScopeId.Equals(id) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                    //当TermId为空时，代表该节点只设置了“Mark the Physical Library”，并没有设置Custom Setting所以返回null.
                    if (!includeOnlySetPhysicalNode
                        && spSetting != null
                        && spSetting.TermId == Guid.Empty && spSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        spSetting = null;
                    }
                }
                if (spSetting == null)
                {
                    //add this for RA 3.1 old data.
                    spSetting = mAllSettings.Where(s => s.ScopeId.Equals(id) && s.SiteId.Equals(Guid.Empty) && !s.IsRemoved).FirstOrDefault();
                }
                return spSetting;
            }
        }

        public static List<RMSharePointOnPremiseSetting> GetFolderSettingUnderList(Guid listId, Guid siteId)
        {
            return mAllSettings.Where(s => s.SiteId == siteId && s.ListId == listId && s.ScopeId == s.FolderId && !s.IsRemoved).ToList();
        }
    }
}
