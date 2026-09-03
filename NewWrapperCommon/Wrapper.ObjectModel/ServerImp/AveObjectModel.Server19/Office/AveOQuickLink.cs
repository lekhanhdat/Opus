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



using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOQuickLink : IAveOQuickLink
    {
        private AveOQuickLinkManager mQuickLinkManager;
        private QuickLink mQuickLink;
        private AveOPrivacyPolicyItem mPrivacyPolicyItem;

        public AveOQuickLink(AveOQuickLinkManager quickLinkManager, QuickLink quickLink)
        {
            mQuickLinkManager = quickLinkManager;
            mQuickLink = quickLink;
        }

        #region IAveQuickLink Members

        public void Commit()
        {
            mQuickLink.Commit();
        }

        public AvePrivacy PrivacyLevel
        {
            get
            {
                if (mQuickLink != null)
                {
                    return (AvePrivacy)mQuickLink.PrivacyLevel;
                }
                else
                {
                    return AvePrivacy.Private;
                }
            }
        }

        public IAveOPrivacyPolicyItem Policy
        {
            get
            {
                if (mPrivacyPolicyItem == null)
                {
                    mPrivacyPolicyItem = new AveOPrivacyPolicyItem(mQuickLink.Policy);
                }
                return mPrivacyPolicyItem;
            }
        }

        public string Title
        {
            get
            {
                return mQuickLink.Title;
            }
            set
            {
                mQuickLink.Title = value;
            }
        }

        public void Delete()
        {
            mQuickLink.Delete();
        }

        public int GroupType
        {
            get
            {
                return (int)mQuickLink.GroupType;
            }
        }

        public string Url
        {
            get
            {
                return mQuickLink.Url;
            }
            set
            {
                mQuickLink.Url = value;
            }
        }

        public string Group
        {
            get
            {
                return mQuickLink.Group;
            }
            set
            {
                mQuickLink.Group = value;
            }
        }

        #endregion
    }
}
