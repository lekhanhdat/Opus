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

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOColleague : IAveOColleague
    {
        private AveOColleagueManager mColleagueManager;
        private Colleague mColleague;
        private AveOUserProfile mUserProfile;
        private AveOPrivacyPolicyItem mPorlicy;

        internal AveOColleague(AveOColleagueManager aveColleagueManager, Colleague colleague)
        {
            // TODO: Complete member initialization
            this.mColleagueManager = aveColleagueManager;
            this.mColleague = colleague;
        }

        #region IAveColleague Members

        public IAveOPrivacyPolicyItem Policy
        {
            get
            {
                if (mPorlicy == null)
                {
                    PrivacyPolicyItem privacyPolicyItem = mColleague.Policy;
                    if (privacyPolicyItem != null)
                    {
                        mPorlicy = new AveOPrivacyPolicyItem(privacyPolicyItem);
                    }
                }
                return mPorlicy;
            }
        }

        public string Title
        {
            get
            {
                return mColleague.Title;
            }
            set
            {
                mColleague.Title = value;
            }
        }

        public IAveOUserProfile Profile
        {
            get
            {
                if (mUserProfile == null)
                {
                    UserProfile userProfile = mColleague.Profile;
                    if (userProfile != null)
                    {
                        mUserProfile = new AveOUserProfile(userProfile);
                    }
                }
                return mUserProfile;
            }
        }

        public string Group
        {
            get
            {
                return mColleague.Group;
            }
            set
            {
                mColleague.Group = value;
            }
        }

        public AveColleagueGroupType GroupType
        {
            get
            {
                return (AveColleagueGroupType)mColleague.GroupType;
            }
            set
            {
                mColleague.GroupType = (ColleagueGroupType)value;
            }
        }

        public bool IsInWorkGroup
        {
            get
            {
                return mColleague.IsInWorkGroup;
            }
            set
            {
                mColleague.IsInWorkGroup = value;
            }
        }

        public AvePrivacy PrivacyLevel
        {
            get
            {
                return (AvePrivacy)mColleague.PrivacyLevel;
            }
            set
            {
                mColleague.PrivacyLevel = (Privacy)value;
            }
        }

        public bool IsAssistant
        {
            get
            {
                return mColleague.IsAssistant;
            }
        }

        public bool IsEditable
        {
            get
            {
                return mColleague.IsEditable;
            }
        }

        public bool IsPrivacyLevelEditable
        {
            get
            {
                return mColleague.IsPrivacyLevelEditable;
            }
        }

        public bool IsTitleEditable
        {
            get
            {
                return mColleague.IsTitleEditable;
            }
        }

        public bool IsUrlEditable
        {
            get
            {
                return mColleague.IsUrlEditable;
            }
        }

        public string Url
        {
            get
            {
                return mColleague.Url;
            }
            set
            {
                mColleague.Url = value;
            }
        }

        public void Delete()
        {
            mColleague.Delete();
        }

        public void Commit()
        {
            mColleague.Commit();
        }

        #endregion
    }

}
