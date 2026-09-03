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

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOProfileBaseChange : IAveOProfileBaseChange
    {
        private ProfileBaseChange mProfileBaseChange;
        private IAveOPrivacyPolicyItem mPrivacyPolicy;

        public AveOProfileBaseChange(ProfileBaseChange profileBaseChange)
        {
            mProfileBaseChange = profileBaseChange;
        }

        public AveChangeTypes ChangeType
        {
            get { return (AveChangeTypes)mProfileBaseChange.ChangeType; }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mProfileBaseChange, "ChangeType", (ChangeTypes)value);
            }
        }
        public DateTime EventTime
        {
            get { return mProfileBaseChange.EventTime; }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mProfileBaseChange, "EventTime", value);
            }
        }

        public virtual IAveOProfileBase ChangedProfile
        {
            get { return new AveOProfileBase(mProfileBaseChange.ChangedProfile); }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mProfileBaseChange, "ChangedProfile", value);
            }
        }


        public IAveOPrivacyPolicyItem PrivacyPolicy
        {
            get
            {
                if (mPrivacyPolicy == null && mProfileBaseChange.PrivacyPolicy != null)
                {
                    mPrivacyPolicy = new AveOPrivacyPolicyItem(mProfileBaseChange.PrivacyPolicy as PrivacyPolicyItem);
                }
                return mPrivacyPolicy;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mProfileBaseChange, "PrivacyPolicy", value);
            }
        }

        public AveObjectTypes ObjectType
        {
            get
            {
                return (AveObjectTypes)mProfileBaseChange.ObjectType;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mProfileBaseChange, "ObjectType", (ObjectTypes)value);
            }
        }


        public long RecordId
        {
            get
            {
                return (long)AveAssemblyUtility.GetPropertyValue(mProfileBaseChange, "RecordId");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mProfileBaseChange, "RecordId", value);
            }
        }
    }
}
