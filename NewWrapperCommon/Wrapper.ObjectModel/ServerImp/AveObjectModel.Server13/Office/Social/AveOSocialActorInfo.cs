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
using System.Threading.Tasks;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Social;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOSocialActorInfo : IAveOSocialActorInfo
    {
        private SPSocialActorInfo mActor;

        public AveOSocialActorInfo()
        {
            mActor = new SPSocialActorInfo();
        }

        public AveOSocialActorInfo(AveSocialActorInfo actor)
            : this()
        {
            if (actor != null)
            {
                mActor.AccountName = actor.AccountName;
                mActor.ActorType = (SPSocialActorType)actor.ActorType;
                mActor.ContentUri = actor.ContentUri;
                mActor.Id = actor.Id;
                mActor.TagGuid = actor.TagGuid;
            }
        }

        public string AccountName
        {
            get
            {
                return mActor.AccountName;
            }
            set
            {
                mActor.AccountName = value;
            }
        }

        public AveOSocialActorType ActorType
        {
            get
            {
                return (AveOSocialActorType)mActor.ActorType;
            }
            set
            {
                mActor.ActorType = (SPSocialActorType)value;
            }
        }

        public Uri ContentUri
        {
            get
            {
                return mActor.ContentUri;
            }
            set
            {
                mActor.ContentUri = value;
            }
        }

        public string Id
        {
            get
            {
                return mActor.Id;
            }
            set
            {
                mActor.Id = value;
            }
        }

        public Guid TagGuid
        {
            get
            {
                return mActor.TagGuid;
            }
            set
            {
                mActor.TagGuid = value;
            }
        }

        public SPSocialActorInfo Actor
        {
            get { return mActor; }
        }
    }
}
