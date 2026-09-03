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
    class AveOMembership:AveClientObject,IAveOMembership
    {
        private IAveRequest mRequest;
        private AveOUserProfile mProfile;
        
        public AveOMembership(IAveRequest request,AveOUserProfile profile,Dictionary<string,object>membershipProp)
        {
            mRequest = request;
            mProfile = profile;
            base.DataCache.AddPropertyies(membershipProp);
        }

        public IAveOPrivacyPolicyItem Policy 
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Policy"))
                {
                    Dictionary<string, object> policyProp = base.DataCache.GetProperty<Dictionary<string, object>>("Policy" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveOPrivacyPolicyItem policyItem = new AveOPrivacyPolicyItem(this.mRequest, policyProp);
                    base.DataCache.PropertiesCache["Policy"] = policyItem;
                }
                return base.DataCache.GetProperty<IAveOPrivacyPolicyItem>("Policy"); 
            }
        }
        public IAveOMemberGroup MembershipGroup 
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("MembershipGroup"))
                {
                    Dictionary<string, object> membershipGroupProp = base.DataCache.GetProperty<Dictionary<string, object>>("MembershipGroup" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveOMemberGroup memberGroup = new AveOMemberGroup(membershipGroupProp);
                    base.DataCache.PropertiesCache["MembershipGroup"] = memberGroup;
                }
                return base.DataCache.GetProperty<IAveOMemberGroup>("MembershipGroup");
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
                base.DataCache.AddChangedProperty("Title",value); 
            }
        }
        public string Group 
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("Group"); 
            }
            set 
            {
                base.DataCache.AddChangedProperty("Group",value); 
            }
        }
        public AveMembershipGroupType GroupType 
        {
            get
            {
                return base.DataCache.GetProperty<AveMembershipGroupType>("GroupType");
            }
            set 
            {
                base.DataCache.AddChangedProperty("GroupType",value);
            }
        }
        public bool IsEditable 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("IsEditable"); 
            }
        }
        public bool IsPrivacyLevelEditable 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("IsPrivacyLevelEditable");  
            }
        }
        public bool IsTitleEditable 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("IsTitleEditable");
            }
        }
        public bool IsUrlEditable
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsUrlEditable");
            }
        }
        public AvePrivacy PrivacyLevel 
        {
            get 
            {
                return base.DataCache.GetProperty<AvePrivacy>("PrivacyLevel");
            }
            set 
            {
                base.DataCache.AddChangedProperty("PrivacyLevel",value); 
            }
        }
        public string Url 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("Url"); 
            }
            set 
            {
                base.DataCache.AddChangedProperty("Url",value);
            }
        }
        public void Commit()
        {
            throw new NotImplementedException();
        }

        public long ID
        {
            get 
            { 
                return base.DataCache.GetProperty<long>("ID"); 
            }
        }
    }
}
