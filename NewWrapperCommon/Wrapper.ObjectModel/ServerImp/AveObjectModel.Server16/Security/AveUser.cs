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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Utilities;
using AvePoint.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;

namespace AvePoint.ObjectModel.Server16
{
    class AveUser : AvePrincipal, IAveUser
    {
        private SPUser mUser;
        private AveRegionalSettings mRegionalSettings;
        private AveUserToken mUserToken;
        private AveAlertCollection mAlerts;
        private AveGroupCollection mGroups;
        private AveGroupCollection mOwnedGroups;

        private AveUserInfo nativeUserInfo;


        public AveUser(AveWeb web, SPUser user)
            : base(web, user)
        {
            mUser = user;
        }

        internal AveUser(AveUserInfo userInfo)
        {
            nativeUserInfo = userInfo;
        }
        internal SPUser User
        {
            get 
            {
                CheckUser();
                return mUser; 
            }
        }

        #region IAveUser Members

        public string Email
        {
            get
            {
                CheckUser();
                return mUser.Email;
            }
            set
            {
                CheckUser();
                mUser.Email = value;
            }
        }

        public byte[] GetBinaryId()
        {
            return mUser.GetBinaryId();
        }

        public void Update()
        {
            CheckUser();
            mUser.Update();
        }

        public bool IsDomainGroup
        {
            get 
            {
                CheckUser();
                return mUser.IsDomainGroup; 
            }
        }

        public bool IsSiteAdmin
        {
            get
            {
                CheckUser();
                return mUser.IsSiteAdmin;
            }
            set
            {
                CheckUser();
                mUser.IsSiteAdmin = value;
            }
        }

        public string Notes
        {
            get
            {
                CheckUser();
                return mUser.Notes;
            }
            set
            {
                CheckUser();
                mUser.Notes = value;
            }
        }

        public IAveRegionalSettings RegionalSettings
        {
            get
            {
                CheckUser();
                if (mRegionalSettings == null)
                {
                    SPRegionalSettings regionalSettings = mUser.RegionalSettings;
                    if (regionalSettings != null)
                    {
                        mRegionalSettings = new AveRegionalSettings(regionalSettings);
                    }
                }
                return mRegionalSettings;
            }
            set
            {
                CheckUser();
                mRegionalSettings = value as AveRegionalSettings;
                if (mRegionalSettings != null)
                {
                    mUser.RegionalSettings = mRegionalSettings.RegionalSettings;
                }
                else
                {
                    mUser.RegionalSettings = null;
                }
            }
        }

        public IAveUserToken UserToken
        {
            get
            {
                CheckUser();
                if (mUserToken == null)
                {
                    mUserToken = new AveUserToken(mUser.UserToken.BinaryToken);
                }
                return mUserToken;
            }
        }

        public IAveAlertCollection Alerts
        {
            get
            {
                CheckUser();
                if (mAlerts == null)
                {
                    mAlerts = new AveAlertCollection(mWeb, mUser.Alerts);
                }
                return mAlerts;
            }
        }

        public override int ID
        {
            get 
            {
                if (mUser != null)
                {
                    return base.ID;
                }
                return nativeUserInfo.ID;
            }
        }

        public override string LoginName
        {
            get
            {
                if (mUser != null)
                {
                    return mUser.LoginName;
                }
                return nativeUserInfo.Login;
            }
        }

        //没有前缀的LoginName
        public string NoPrefixLoginName
        {
            get
            {
                if (mUser != null)
                {
                    if (mUser.LoginName.IndexOf('|') > 0)
                    {
                        return mUser.LoginName.Substring(mUser.LoginName.IndexOf('|') + 1);
                    }
                    else
                    {
                        return mUser.LoginName;
                    }
                }
                return nativeUserInfo.Login;
            }
        }

        public override string Name
        {
            get
            {
                if (mUser != null)
                {
                    return mUser.Name;
                }
                return nativeUserInfo.Title;
            }
            set
            {
                if (mUser != null)
                {
                    mUser.Name = value;
                }
                else
                {
                    nativeUserInfo.Title= value;
                }
            }
        }

        public override AvePrincipalType PrincipalType
        {
            get
            {
                CheckUser();
                return (AvePrincipalType)(SPPrincipalType)(AveAssemblyUtility.GetPropertyValue(mUser, "PrincipalType"));
            }
        }

        public IAveGroupCollection Groups
        {
            get
            {
                CheckUser();
                SPGroupCollection groups = mUser.Groups;
                if (groups != null)
                {
                    mGroups = new AveGroupCollection(mWeb, groups);
                }
                return mGroups;
            }
        }

        public IAveGroupCollection OwnedGroups
        {
            get
            {
                CheckUser();
                if (mOwnedGroups == null)
                {
                    mOwnedGroups = new AveGroupCollection(mWeb, mUser.OwnedGroups);
                }
                return mOwnedGroups;
            }
        }

        private void CheckUser()
        {
            if (mUser == null)
            {
                throw new UserNotFoundException(nativeUserInfo.ID);
            }
        }

        public string Sid
        {
            get
            {
                if (mUser != null)
                {
                    return mUser.Sid;
                }
                return AveDirectoryServiceUtility.ConvertBytesToStringSid(nativeUserInfo.SystemID);
            }
        }

        public IAveUserIdInfo UserId
        {
            get
            {
                CheckUser();
                return new AveUserIdInfo(mUser.UserId);
            }
        }
        #endregion
    }
}
