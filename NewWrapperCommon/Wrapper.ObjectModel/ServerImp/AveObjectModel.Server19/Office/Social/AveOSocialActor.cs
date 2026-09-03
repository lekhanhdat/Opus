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



using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility;
using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Office.Server.Social;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSocialActor : IAveOSocialActor
    {
        private SPSocialActor mSPSocialActor;

        public AveOSocialActor()
        {
        }

        public AveOSocialActor(SPSocialActor spSocialActor)
        {
            mSPSocialActor = spSocialActor;
        }

        public AveOSocialActorType ActorType
        {
            get
            {
                return (AveOSocialActorType)mSPSocialActor.ActorType;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "ActorType", (SPSocialActorType)value);
            }
        }

        public Uri Uri
        {
            get
            {
                return mSPSocialActor.Uri;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "Uri", value);
            }
        }

        public string Name
        {
            get
            {
                return mSPSocialActor.Name;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "Name", value);
            }
        }

        public string AccountName
        {
            get
            {
                return mSPSocialActor.AccountName;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "AccountName", value);
            }
        }

        public string StatusText
        {
            get
            {
                return mSPSocialActor.StatusText;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "StatusText", value);
            }
        }

        public string Id
        {
            get
            {
                return mSPSocialActor.Id;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "Id", value);
            }
        }

        public Guid TagGuid
        {
            get
            {
                return mSPSocialActor.TagGuid;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "TagGuid", value);
            }
        }

        public bool CanFollow
        {
            get
            {
                return mSPSocialActor.CanFollow;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "CanFollow", value);
            }
        }

        public AveOSocialStatusCode Status
        {
            get
            {
                return (AveOSocialStatusCode)mSPSocialActor.Status;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSPSocialActor, "Status", (SPSocialStatusCode)value);
            }
        }


        public Uri ContentUri
        {
            get { return mSPSocialActor.ContentUri; }
        }

        public string EmailAddress
        {
            get { return mSPSocialActor.EmailAddress; }
        }

        public Uri FollowedContentUri
        {
            get { return mSPSocialActor.FollowedContentUri; }
        }

        public Uri ImageUri
        {
            get { return mSPSocialActor.ImageUri; }
        }

        public bool IsFollowed
        {
            get { return mSPSocialActor.IsFollowed; }
        }

        public Uri LibraryUri
        {
            get { return mSPSocialActor.LibraryUri; }
        }

        public Uri PersonalSiteUri
        {
            get { return mSPSocialActor.PersonalSiteUri; }
        }

        public string Title
        {
            get { return mSPSocialActor.Title; }
        }
    }
}
