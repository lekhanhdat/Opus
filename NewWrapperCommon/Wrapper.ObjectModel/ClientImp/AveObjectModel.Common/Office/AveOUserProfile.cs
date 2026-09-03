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
    class AveOUserProfile : AveClientObject, IAveOUserProfile
    {
        private IAveRequest mRequest;
        private AveOUserProfileManager mUserProfileManager;
        private string mAccountName;
        private string[] multiloginAccounts;
        public AveOUserProfile(IAveRequest request, AveOUserProfileManager userProfileManager, string accountName, Dictionary<string, object> prop)
        {
            mRequest = request;
            mAccountName = accountName;
            mUserProfileManager = userProfileManager;
            base.DataCache.AddPropertyies(prop);
        }

        #region IAveOUserProfile Members

        public IAveOUserProfileValueCollection this[string strPropName]
        {
            get
            {
                AveOUserProfileValueCollection profileValueCol = null;
                List<Dictionary<string, object>> profileValueColList = base.DataCache.GetProperty<List<Dictionary<string, object>>>("ProfileValues");
                foreach (Dictionary<string, object> profileValueColProp in profileValueColList)
                {
                    if (profileValueColProp["NameValue"].ToString().Equals(strPropName))
                    {
                        profileValueCol= new AveOUserProfileValueCollection(this.mRequest,this,this.mUserProfileManager,strPropName, profileValueColProp);
                        break;
                    }
                }
                return profileValueCol;
            }
        }

        public IAveOMembershipManager Memberships
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Memberships"))
                {
                    Dictionary<string, object> membershipsProp = base.DataCache.GetProperty<Dictionary<string, object>>("Memberships" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveOMembershipManager membershipManager = new AveOMembershipManager(this.mRequest, this, membershipsProp);
                    base.DataCache.PropertiesCache["Memberships"] = membershipManager;
                }
                return base.DataCache.GetProperty<IAveOMembershipManager>("Memberships");
            }
        }

        public IAveOColleagueManager Colleagues
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Colleagues"))
                {
                    Dictionary<string, object> colleaguesProp = base.DataCache.GetProperty<Dictionary<string, object>>("Colleagues" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveOColleagueManager colleagueManager = new AveOColleagueManager(this.mRequest, this, colleaguesProp);
                    base.DataCache.PropertiesCache["Colleagues"] = colleagueManager;
                }
                return base.DataCache.GetProperty<IAveOColleagueManager>("Colleagues");
            }
        }

        public IAveOUserProfileManager ProfileManager
        {
            get
            {
                return mUserProfileManager;
            }
        }

        public IAveOQuickLinkManager QuickLinks
        {
            get
            {
                if(base.DataCache.IsPropertyNotLoaded("QuickLinks"))
                {
                    Dictionary<string, object> quickLinksProp = base.DataCache.GetProperty<Dictionary<string, object>>("QuickLinks"+AveObjectModelConstant.ObjectPropertySuffix);
                    AveOQuickLinkManager quickLinkManager = new AveOQuickLinkManager(this.mRequest,quickLinksProp);
                    base.DataCache.PropertiesCache["QuickLinks"]=quickLinkManager;
                }
                return base.DataCache.GetProperty<IAveOQuickLinkManager>("QuickLinks");
            }
        }

        public string[] MultiloginAccounts
        {
            get
            {
                if (multiloginAccounts == null)
                {
                    multiloginAccounts = new string[1];
                    multiloginAccounts[0] = mAccountName;
                }
                return multiloginAccounts;
            }
            set
            {
                multiloginAccounts = value;
            }
        }

        public IAveSite PersonalSite
        {
            get
            {
                //if (base.DataCache.IsPropertyNotLoaded("PersonalSite"))
                //{
                //    string serverRealtiveUrl = base.DataCache.GetProperty<string>("PersonalSite" + AveObjectModelConstant.ObjectPropertySuffix);
                //    AveSite site = null;
                //    if (serverRealtiveUrl!=null)
                //    {
                //    }
                //}
                throw new NotImplementedException();
            }
        }

        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("ID");
            }
        }

        public void CreatePersonalSite(int lcid)
        {
            this.mRequest.AddPersonalSite(this.mAccountName, lcid);
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public string DisplayName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public long RecordId
        {
            get { throw new NotImplementedException(); }
        }

        public IAveOUserProfileChangeCollection GetChanges()
        {
            throw new NotImplementedException();
        }
        
        #endregion


        public string AccountName
        {
            get { return null; }
        }

        public IAveOProfileValueCollectionBase GetProfileValueCollection(string propName)
        {
            return null;
        }

        public string[] SaveTempFile(byte[] content, string fileName)
        {
            throw new NotImplementedException();
        }

        public Uri PublicUrl 
        {
            get { return null; }
        }



        public IAveOProfileSubtype ProfileSubType
        {
            set { }
            get { return null; }
        }

        public IAveOFollowedContent FollowedContent
        {
            get
            {
                //API有实现，Restore层暂时不支持，暂时先不给实现。
                throw new NotImplementedException();
            }
        }

        public IAveOUserProfile[] GetPeers()
        {
            throw new NotImplementedException();
        }

        public IAveOUserProfile[] GetDirectReports()
        {
            throw new NotImplementedException();
        }
    }
}
