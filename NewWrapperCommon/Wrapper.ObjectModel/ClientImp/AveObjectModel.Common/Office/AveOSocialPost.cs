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
    class AveOSocialPost : AveClientObject, IAveOSocialPost
    {
         public AveOSocialPost()
        {
        }
         public AveOSocialPost(Dictionary<string, object> prop)
        {
            base.DataCache.AddPropertyies(prop);
        }
        public IAveOSocialAttachment Attachment
        {
            get { return base.DataCache.GetProperty<IAveOSocialAttachment>("Attachment"); }
            set { base.DataCache.ChangedProperties["Attachment"] = value; }
        }

        public AveOSocialPostAttributes Attributes
        {
            get { return base.DataCache.GetProperty<AveOSocialPostAttributes>("Attributes"); }
            set { base.DataCache.ChangedProperties["Attributes"] = value; }
        }

        public int AuthorIndex
        {
            get { return base.DataCache.GetProperty<int>("AuthorIndex"); }
            set { base.DataCache.ChangedProperties["AuthorIndex"] = value; }
        }

        public DateTime CreatedTime
        {
            get { return base.DataCache.GetProperty<DateTime>("CreatedTime"); }
            set { base.DataCache.ChangedProperties["CreatedTime"] = value; }
        }

        public string Id
        {
            get { return base.DataCache.GetProperty<string>("Threads"); }
            set { base.DataCache.ChangedProperties["Threads"] = value; }
        }

        public IAveOSocialPostActorInfo LikerInfo
        {
            get { return base.DataCache.GetProperty<IAveOSocialPostActorInfo>("LikerInfo"); }
            set { base.DataCache.ChangedProperties["LikerInfo"] = value; }
        }

        public DateTime ModifiedTime
        {
            get { return base.DataCache.GetProperty<DateTime>("ModifiedTime"); }
            set { base.DataCache.ChangedProperties["ModifiedTime"] = value; }
        }

        public AveOSocialPostType PostType
        {
            get { return base.DataCache.GetProperty<AveOSocialPostType>("PostType"); }
            set { base.DataCache.ChangedProperties["PostType"] = value; }
        }

        public string Text
        {
            get { return base.DataCache.GetProperty<string>("Text"); }
            set { base.DataCache.ChangedProperties["Text"] = value; }
        }

        public IAveOSocialDataOverlay[] Overlays
        {
            get { return base.DataCache.GetProperty<IAveOSocialDataOverlay[]>("Overlays"); }
            set { base.DataCache.ChangedProperties["Overlays"] = value; }
        }

        public Uri PreferredImageUri
        {
            get { return base.DataCache.GetProperty<Uri>("PreferredImageUri"); }
            set { base.DataCache.ChangedProperties["PreferredImageUri"] = value; }
        }

        public IAveOSocialLink Source
        {
            get { return base.DataCache.GetProperty<IAveOSocialLink>("Source"); }
            set { base.DataCache.ChangedProperties["Source"] = value; }
        }
    }
}
