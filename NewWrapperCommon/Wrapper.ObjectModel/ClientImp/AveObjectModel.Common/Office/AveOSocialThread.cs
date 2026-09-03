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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOSocialThread : AveClientObject, IAveOSocialThread
    {
        public AveOSocialThread()
        {
        }
        public AveOSocialThread(Dictionary<string, object> prop)
        {
            base.DataCache.AddPropertyies(prop);
        }
        public IAveOSocialActor[] Actors
        {
            get { return base.DataCache.GetProperty<IAveOSocialActor[]>("Actors"); }
            set { base.DataCache.ChangedProperties["Actors"] = value; }
        }

        public AveOSocialThreadAttributes Attributes
        {
            get { return base.DataCache.GetProperty<AveOSocialThreadAttributes>("Attributes"); }
            set { base.DataCache.ChangedProperties["Attributes"] = value; }
        }

        public string Id
        {
            get { return base.DataCache.GetProperty<string>("Id"); }
            set { base.DataCache.ChangedProperties["Id"] = value; }
        }

        public int OwnerIndex
        {
            get { return base.DataCache.GetProperty<int>("OwnerIndex"); }
            set { base.DataCache.ChangedProperties["OwnerIndex"] = value; }
        }

        public Uri Permalink
        {
            get { return base.DataCache.GetProperty<Uri>("Permalink"); }
            set { base.DataCache.ChangedProperties["Permalink"] = value; }
        }

        public IAveOSocialPostReference PostReference
        {
            get { return base.DataCache.GetProperty<IAveOSocialPostReference>("PostReference"); }
            set { base.DataCache.ChangedProperties["PostReference"] = value; }
        }

        public IAveOSocialPost[] Replies
        {
            get { return base.DataCache.GetProperty<IAveOSocialPost[]>("Replies"); }
            set { base.DataCache.ChangedProperties["Replies"] = value; }
        }

        public IAveOSocialPost RootPost
        {
            get { return base.DataCache.GetProperty<IAveOSocialPost>("RootPost"); }
            set { base.DataCache.ChangedProperties["RootPost"] = value; }
        }

        public AveOSocialStatusCode Status
        {
            get { return base.DataCache.GetProperty<AveOSocialStatusCode>("Status"); }
            set { base.DataCache.ChangedProperties["Status"] = value; }
        }

        public AveOSocialThreadType ThreadType
        {
            get { return base.DataCache.GetProperty<AveOSocialThreadType>("ThreadType"); }
            set { base.DataCache.ChangedProperties["ThreadType"] = value; }
        }

        public int TotalReplyCount
        {
            get { return base.DataCache.GetProperty<int>("TotalReplyCount"); }
            set { base.DataCache.ChangedProperties["TotalReplyCount"] = value; }
        }
    }
}
