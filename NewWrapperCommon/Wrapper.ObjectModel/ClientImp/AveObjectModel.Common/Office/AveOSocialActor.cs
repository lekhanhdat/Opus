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
    class AveOSocialActor : AveClientObject, IAveOSocialActor
    {
        public AveOSocialActor()
        {
        }
        public AveOSocialActor(Dictionary<string, object> prop)
        {
            base.DataCache.AddPropertyies(prop);
        }
        public AveOSocialActorType ActorType
        {
            get
            {
                return base.DataCache.GetProperty<AveOSocialActorType>("ActorType");
            }
            set
            {
                base.DataCache.ChangedProperties["ActorType"] = value;
            }
        }

        public Uri Uri
        {
            get
            {
                return base.DataCache.GetProperty<Uri>("Uri");
            }
            set
            {
                base.DataCache.ChangedProperties["Uri"] = value;
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                base.DataCache.ChangedProperties["Name"] = value;
            }
        }

        public string AccountName
        {
            get
            {
                return base.DataCache.GetProperty<string>("AccountName");
            }
            set
            {
                base.DataCache.ChangedProperties["AccountName"] = value;
            }
        }

        public string StatusText
        {
            get
            {
                return base.DataCache.GetProperty<string>("StatusText");
            }
            set
            {
                base.DataCache.ChangedProperties["StatusText"] = value;
            }
        }

        public string Id
        {
            get
            {
                return base.DataCache.GetProperty<string>("Id");
            }
            set
            {
                base.DataCache.ChangedProperties["Id"] = value;
            }
        }

        public Guid TagGuid
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("TagGuid");
            }
            set
            {
                base.DataCache.ChangedProperties["TagGuid"] = value;
            }
        }

        public AveOSocialStatusCode Status
        {
            get
            {
                return base.DataCache.GetProperty<AveOSocialStatusCode>("Status");
            }
            set
            {
                base.DataCache.ChangedProperties["Status"] = value;
            }
        }

        public bool CanFollow
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CanFollow");
            }
            set
            {
                base.DataCache.ChangedProperties["CanFollow"] = value;
            }
        }

        public Uri ContentUri
        {
            get
            {
                return base.DataCache.GetProperty<Uri>("ContentUri");
            }
            set
            {
                base.DataCache.ChangedProperties["ContentUri"] = value;
            }
        }

        public string EmailAddress
        {
            get
            {
                return base.DataCache.GetProperty<string>("EmailAddress");
            }
            set
            {
                base.DataCache.ChangedProperties["EmailAddress"] = value;
            }
        }

        public Uri FollowedContentUri
        {
            get
            {
                return base.DataCache.GetProperty<Uri>("FollowedContentUri");
            }
            set
            {
                base.DataCache.ChangedProperties["FollowedContentUri"] = value;
            }
        }

        public Uri ImageUri
        {
            get
            {
                return base.DataCache.GetProperty<Uri>("ImageUri");
            }
            set
            {
                base.DataCache.ChangedProperties["ImageUri"] = value;
            }
        }

        public bool IsFollowed
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsFollowed");
            }
            set
            {
                base.DataCache.ChangedProperties["IsFollowed"] = value;
            }
        }

        public Uri LibraryUri
        {
            get
            {
                return base.DataCache.GetProperty<Uri>("LibraryUri");
            }
            set
            {
                base.DataCache.ChangedProperties["LibraryUri"] = value;
            }
        }

        public Uri PersonalSiteUri
        {
            get
            {
                return base.DataCache.GetProperty<Uri>("PersonalSiteUri");
            }
            set
            {
                base.DataCache.ChangedProperties["PersonalSiteUri"] = value;
            }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
            set
            {
                base.DataCache.ChangedProperties["Title"] = value;
            }
        }
    }
}
