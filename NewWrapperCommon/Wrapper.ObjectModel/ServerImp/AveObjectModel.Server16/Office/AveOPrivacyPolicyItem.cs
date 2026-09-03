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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOPrivacyPolicyItem : IAveOPrivacyPolicyItem
    {
        private PrivacyPolicyItem mPrivacyPolicyItem;

        internal AveOPrivacyPolicyItem(PrivacyPolicyItem privacyPolicyItem)
        {
            // TODO: Complete member initialization
            this.mPrivacyPolicyItem = privacyPolicyItem;
        }

        #region IAvePrivacyPolicyItem Members

        public bool AllowPolicyOverride
        {
            get
            {
                return mPrivacyPolicyItem.AllowPolicyOverride;
            }
        }

        public string DisplayName
        {
            get
            {
                return mPrivacyPolicyItem.DisplayName;
            }
            set
            {
                mPrivacyPolicyItem.DisplayName = value;
            }
        }

        public bool FilterPrivacyItems
        {
            get
            {
                return mPrivacyPolicyItem.FilterPrivacyItems;
            }
        }

        public string Group
        {
            get
            {
                return mPrivacyPolicyItem.Group;
            }
            set
            {
                mPrivacyPolicyItem.Group = value;
            }
        }

        public Guid ID
        {
            get
            {
                return mPrivacyPolicyItem.Id;
            }
        }

        public object Parent
        {
            get
            {
                return mPrivacyPolicyItem.Parent;
            }
        }

        public bool UserOverridePrivacy
        {
            get
            {
                return mPrivacyPolicyItem.UserOverridePrivacy;
            }
            set
            {
                mPrivacyPolicyItem.UserOverridePrivacy = value;
            }
        }

        public void Commit()
        {
            mPrivacyPolicyItem.Commit();
        }

        public void Delete()
        {
            mPrivacyPolicyItem.Delete();
        }

        public AvePrivacy DefaultPrivacy
        {
            get
            {
                return (AvePrivacy)mPrivacyPolicyItem.DefaultPrivacy;
            }
            set
            {
                mPrivacyPolicyItem.DefaultPrivacy = (Privacy)value;
            }
        }

        public AvePrivacyPolicy PrivacyPolicy
        {
            get
            {
                return (AvePrivacyPolicy)mPrivacyPolicyItem.PrivacyPolicy;
            }
            set
            {
                mPrivacyPolicyItem.PrivacyPolicy = (PrivacyPolicy)value;
            }
        }

        #endregion
    }
}
