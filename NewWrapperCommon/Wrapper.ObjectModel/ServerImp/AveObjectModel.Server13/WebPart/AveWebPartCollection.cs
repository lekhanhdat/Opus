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
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveWebPartCollection : AveAbstractCommonCollection<IAveWebPart>, IAveWebPartCollection
    {
        private SPWebPartCollection mWebPartCollection;

        public AveWebPartCollection(SPWebPartCollection webpartCollection)
            : base(webpartCollection)
        {
            mWebPartCollection = webpartCollection;
        }

        public override IAveWebPart this[int index]
        {
            get
            {
                WebPart webPart = mWebPartCollection[index];
                if (webPart == null)
                {
                    return null;
                }
                return AveWebPart.CreateInstance(null, webPart);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return AveWebPart.CreateInstance(null, t as WebPart);
        }

        public override int Count
        {
            get { return mWebPartCollection.Count; }
        }

        public Guid Add(string dwp)
        {
            return mWebPartCollection.Add(dwp);
        }

        public void Delete(Guid storageKey)
        {
            mWebPartCollection.Delete(storageKey);
        }
    }
}
