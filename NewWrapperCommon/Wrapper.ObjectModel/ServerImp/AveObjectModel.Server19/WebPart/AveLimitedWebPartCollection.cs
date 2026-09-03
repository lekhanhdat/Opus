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



using Microsoft.SharePoint.WebPartPages;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveLimitedWebPartCollection : AveAbstractCommonCollection<IAveWebPart>, IAveLimitedWebPartCollection
    {
        private SPLimitedWebPartCollection mLimitedWebPartCollection;
        private AveLimitedWebPartManager mManager;

        public AveLimitedWebPartCollection(AveLimitedWebPartManager manager, SPLimitedWebPartCollection limitedWebParts)
            : base(limitedWebParts)
        {
            mLimitedWebPartCollection = limitedWebParts;
            mManager = manager;
        }

        public override IAveWebPart this[int index]
        {
            get
            {
                System.Web.UI.WebControls.WebParts.WebPart webPart = mLimitedWebPartCollection[index];
                if (webPart == null)
                {
                    return null;
                }
                return AveWebPart.CreateInstance(mManager, webPart);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return AveWebPart.CreateInstance(mManager, t as System.Web.UI.WebControls.WebParts.WebPart);
        }

        public override int Count
        {
            get { return mLimitedWebPartCollection.Count; }
        }
    }
}
