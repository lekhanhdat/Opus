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




//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using AvePoint.Wrapper.Common;
//using AvePoint.Wrapper.Common.Office;

////using AvePoint.ObjectModel.ClientExtension;

//namespace AvePoint.ObjectModel.Common.Office
//{
//    class AveOUserProfileManager : AveAbstractCommonCollection<IAveOUserProfile>, IAveOUserProfileManager
//    {
//        private IAveRequest mRequest;
//        private AveServiceContext mServiceContext;
//        //private AveUserAccountInfo mUserAccountInfo;
//        //private string mSiteUrl; 

//        public AveOUserProfileManager(AveServiceContext serviceContext, AveSite site)//(string siteUrl,AveUserAccountInfo userAccountInfo)//
//        {
//            mServiceContext = serviceContext;
//            mRequest = site.Request;
//            //mUserAccountInfo = userAccountInfo;
//            //mSiteUrl = siteUrl;
//            //mRequest = new AveClientExtensionRequest(mSiteUrl, mUserAccountInfo);
//            Dictionary<string, object> userProfileManagerProp = mRequest.GetUserProfileManager();
//            base.DataCache.AddPropertyies(userProfileManagerProp);
//            InitUserProfileManager();
//        }
//        internal void InitUserProfileManager()
//        {
//            var userProfileList = base.DataCache.GetChildren();
//            mListData = new List<IAveOUserProfile>(userProfileList.Count);
//            foreach (var userProfileProp in userProfileList)
//            {
//                AveOUserProfile userProfile = new AveOUserProfile(this.mRequest, this, null, userProfileProp);
//                mListData.Add(userProfile);
//            }
//        }

//        #region IAveUserProfileManager Members

//        public IAveOUserProfile GetUserProfile(string strAccountName)
//        {
//            Dictionary<string, object> userProfileProp = this.mRequest.GetUserProfileByName(strAccountName);
//            if (userProfileProp.Count > 0)
//            {
//                if (!base.DataCache.IsPropertyAvailable("DefaultProfileSubtypeProperties"))
//                {
//                    Dictionary<string, object> valueNames = new Dictionary<string, object>();
//                    var childrenProperties = new List<IDictionary<string, object>>();
//                    foreach (Dictionary<string, object> property in userProfileProp["ProfileValues"] as List<Dictionary<string, object>>)
//                    {
//                        Dictionary<string, object> valueName = new Dictionary<string, object>();
//                        valueName.Add("Name", property["NameValue"].ToString());
//                        childrenProperties.Add(valueName);
//                    }
//                    valueNames.AddChildren(childrenProperties);
//                    base.DataCache.AddChangedProperty("DefaultProfileSubtypeProperties" + AveObjectModelConstant.ObjectPropertySuffix, valueNames);
//                }
//                return new AveOUserProfile(mRequest, this, strAccountName, userProfileProp);
//            }
//            else
//            {
//                return null;
//            }
//        }
//        public List<AvePropertyInfo> GetUserProfileSchema()
//        {
//            return (this.mRequest as IAveRequest).GetUserProfileSchema();
//        }

//        public IAveOProfileSubtypePropertyManager DefaultProfileSubtypeProperties
//        {
//            get
//            {
//                if (base.DataCache.IsPropertyNotLoaded("DefaultProfileSubtypeProperties"))
//                {
//                    Dictionary<string, object> defaultProfileSubtypeProp = base.DataCache.GetProperty<Dictionary<string, object>>("DefaultProfileSubtypeProperties" + AveObjectModelConstant.ObjectPropertySuffix);
//                    AveOProfileSubtypePropertyManager profileSubtypePropertyManager = new AveOProfileSubtypePropertyManager(defaultProfileSubtypeProp);
//                    base.DataCache.AddProperty("DefaultProfileSubtypeProperties",profileSubtypePropertyManager);
//                }
//                return base.DataCache.GetProperty<IAveOProfileSubtypePropertyManager>("DefaultProfileSubtypeProperties");
//            }
//        }

//        public bool UserExists(string strAccountName)
//        {
//            if (this.GetUserProfile(strAccountName) != null)
//                return true;
//            else
//                return false;
//        }

//        public string MySiteHostUrl
//        {
//            get
//            {
//                return base.DataCache.GetProperty<string>("MySiteHostUrl");
//            }
//            set
//            {
//                base.DataCache.AddChangedProperty("MySiteHostUrl", value);
//            }
//        }

//        public IAveOUserProfile CreateUserProfile(string strAccountName)
//        {
//            Dictionary<string, object> userProfileProp = this.mRequest.AddUserProfile(strAccountName);
//            return new AveOUserProfile(this.mRequest, this, strAccountName, userProfileProp);
//        }

//        public IAveOPropertyCollection Properties
//        {
//            get
//            {
//                if (base.DataCache.IsPropertyNotLoaded("Properties"))
//                {
//                    Dictionary<string, object> propertyColProp = base.DataCache.GetProperty<Dictionary<string, object>>("Properties" + AveObjectModelConstant.ObjectPropertySuffix);
//                    AveOPropertyCollection propertyCol = new AveOPropertyCollection(this.mRequest, propertyColProp);
//                    base.DataCache.AddProperty("Properties",propertyCol);
//                }
//                return base.DataCache.GetProperty<IAveOPropertyCollection>("Properties");
//            }
//        }

//        public IAveOMemberGroupManager GetMemberGroups()
//        {
//            Dictionary<string, object> memberGroupsProp = base.DataCache.GetProperty<Dictionary<string, object>>("MemberGroupManager");
//            return new AveOMemberGroupManager(this.mRequest, memberGroupsProp);
//        }

//        public void UpdateDetails(string accountName, string xml)
//        {
//            this.mRequest.UpdateUserProfileDetails(accountName, xml);
//        }
//        public void UpdateMemberships(string accountName, string xml)
//        {
//            this.mRequest.UpdateUserProfileMemberships(accountName, xml);
//        }
//        public void UpdateColleagues(string accountName, string xml)
//        {
//            this.mRequest.UpdateUserProfileColleages(accountName, xml);
//        }
//        public void UpdateTags(string accountName, string xml)
//        {
//            this.mRequest.UpdateUserProfileTags(accountName, xml);
//        }

//        public IAveOPropertyCollection PropertiesWithSection
//        {
//            get
//            {
//                if (base.DataCache.IsPropertyNotLoaded("PropertiesWithSection"))
//                {
//                    Dictionary<string, object> propertyColProp = base.DataCache.GetProperty<Dictionary<string, object>>("PropertiesWithSection" + AveObjectModelConstant.ObjectPropertySuffix);
//                    AveOPropertyCollection propertyCol = new AveOPropertyCollection(this.mRequest, propertyColProp);
//                    base.DataCache.AddProperty("PropertiesWithSection",propertyCol);
//                }
//                return base.DataCache.GetProperty<IAveOPropertyCollection>("PropertiesWithSection");
//            }
//        }
//        #endregion


//        public void RemoveUserProfile(string strAccountName)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
