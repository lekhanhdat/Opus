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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Extension;
namespace AvePoint.Wrapper.Restore
{
    public class AveSPPolicy:IDisposable
    {

        private AveSPWeb mAveSPWeb = null;
        private AveSPList mAveSPList = null;

        private AveProjectPolicyInfo mAvePolicy = null;
        private AveListPolicyInfo mAveListPolicy = null;

        private IAveProjectPolicyItemListUtility utility = null;
        public AveSPPolicy( AveSPWeb aveWeb, AveProjectPolicyInfo policy)
        {
            
            mAveSPWeb = aveWeb;
            mAvePolicy = policy;
            
        }

        public AveSPPolicy(AveSPList avelist, AveListPolicyInfo policy)
        {
            mAveSPList = avelist;
            mAveListPolicy = policy;
            if (mAveSPList.ParentSite.SPContextKind.IsServerMode13Upper() && AvePoint.Common.AveEnv.IsMoss)
            {
                utility = ((AveObjectModelFactoryExtension)mAveSPList.ParentSite.ObjectModelFactory).CreatePolicyItemListUtility();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Import()
        {
            if (mAveSPList == null)
            {
                if (mAvePolicy != null && mAveSPWeb.ParentSite.SPContextKind.IsServerMode13Upper())
                {
                    mAveSPWeb.WebSettingInfo.IsSiteClosed = mAvePolicy.IsSiteClosed;
                    mAveSPWeb.WebSettingInfo.SiteClosedTime = mAvePolicy.SiteClosedTime;
                    mAveSPWeb.WebSettingInfo.ProjectExpirationDate = mAvePolicy.projectExpirationDate;
                    mAveSPWeb.WebSettingInfo.ProjectPolicyContentTypeId = mAvePolicy.ProjectPolicyContentType;
                    mAveSPWeb.WebSettingInfo.ProjectPolicyName = mAvePolicy.ProjectPolicyName;
                }
            }
            else
            {
                if (utility != null)
                {
                    utility.SetObjectData(mAveSPList.ParentSite.SPSite.ID, mAveSPList.ParentWeb.SPWeb.ID, mAveSPList.SPList.ID, mAveListPolicy);
                }
            }
        }

      
    }
}
