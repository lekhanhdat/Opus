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
namespace AvePoint.Wrapper.Backup
{
    public class AveSPPolicy:IDisposable
    {
        private AveSPSite mAveSPSite = null;
        private AveSPWeb mAveSPWeb=null;
        private AveSPList mAveSPList = null;
       
        private IAveProjectPolicyItemListUtility utility = null;
        public AveSPPolicy(AveSPSite aveSite,AveSPWeb aveWeb)
        {
            mAveSPSite = aveSite;
            mAveSPWeb = aveWeb;
            if (aveSite.SPContextKind.IsServerMode13Upper() && AvePoint.Common.AveEnv.IsMoss)
            {
                utility = ((AveObjectModelFactoryExtension)aveSite.ObjectModelFactory).CreatePolicyItemListUtility();
            }
        }

        public AveSPPolicy(AveSPSite aveSite, AveSPWeb aveWeb, AveSPList aveList)
        {
            mAveSPSite = aveSite;
            mAveSPWeb = aveWeb;
            mAveSPList = aveList;

            if (aveSite.SPContextKind.IsServerMode13Upper() && AvePoint.Common.AveEnv.IsMoss)
            {
                utility = ((AveObjectModelFactoryExtension)aveSite.ObjectModelFactory).CreatePolicyItemListUtility();
            }

        }
       
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWebProjectPolicy.WebInfo"))
            {
                if (utility != null)
                {
                    if (this.mAveSPList == null)
                    {
                        output.WriteMetadata(AveMetadataType.WebProjectPolicy, GetWebProjectPolicyInfo());
                    }
                    else
                    {
                        output.WriteMetadata(AveMetadataType.ListPolicy, GetListPolicyInfo());
                    }
                }
            }
        }

        public AveProjectPolicyInfo GetWebProjectPolicyInfo()
        {
            return utility.GetObjectData(mAveSPSite.SPSite.ID,mAveSPWeb.SPWeb.ID);
        }

        public AveListPolicyInfo GetListPolicyInfo()
        {
            return utility.GetObjectData(mAveSPSite.SPSite.ID, mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID);
        }
    }
}
