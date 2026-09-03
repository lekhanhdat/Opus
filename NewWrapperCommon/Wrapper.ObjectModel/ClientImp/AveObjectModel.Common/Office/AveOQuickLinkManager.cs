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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOQuickLinkManager:AveAbstractCommonCollection<IAveOQuickLink>,IAveOQuickLinkManager
    {
        private IAveRequest mRequest;

        public AveOQuickLinkManager(IAveRequest request,Dictionary<string,object>quickLinksProp)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(quickLinksProp);
            InitQuickLinkManager();
        }

        internal void InitQuickLinkManager()
        {
            List<Dictionary<string, object>> quickLinkList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveOQuickLink>(quickLinkList.Count);
            foreach(Dictionary<string,object>quickLinkProp in quickLinkList )
            {
                AveOQuickLink quickLink = new AveOQuickLink(this.mRequest,quickLinkProp);
                mListData.Add(quickLink);
            }
        }

        public IAveOQuickLink Create(string strTitle, string strUrl, AveQuickLinkGroupType groupType, string strGroup, AvePrivacy privacyLevel)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<IAveOQuickLink> GetItems()
        {
            IAveOQuickLink[] quickLinks = new IAveOQuickLink[mListData.Count];
            for (int i = 0; i < mListData.Count; i++)
            {
                quickLinks[i] = mListData[i];
            }
            return quickLinks;
        }
    }
}
