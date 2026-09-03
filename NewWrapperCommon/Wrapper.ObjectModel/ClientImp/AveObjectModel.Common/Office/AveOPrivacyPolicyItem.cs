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
    class AveOPrivacyPolicyItem : AveClientObject, IAveOPrivacyPolicyItem
    {
        private IAveRequest mRequest;

        public AveOPrivacyPolicyItem(IAveRequest request,Dictionary<string,object>privacyPolicyItemProp)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(privacyPolicyItemProp);
        }
        public bool AllowPolicyOverride
        {
            get { return base.DataCache.GetProperty<bool>("AllowPolicyOverride"); }
        }
        public string DisplayName 
        {
            get { return base.DataCache.GetProperty<string>("DisplayName"); }
            set { base.DataCache.AddChangedProperty("DisplayName",value); }
        }
        public bool FilterPrivacyItems 
        {
            get { return base.DataCache.GetProperty<bool>("FilterPrivacyItems"); }
        }
        public string Group 
        {
            get { return base.DataCache.GetProperty<string>("Group"); }
            set { base.DataCache.AddChangedProperty("Group",value); }
        }
        public object Parent 
        {
            get { throw new NotImplementedException(); }
        }
        public bool UserOverridePrivacy 
        {
            get { return base.DataCache.GetProperty<bool>("UserOverridePrivacy"); }
            set { base.DataCache.AddChangedProperty("UserOverridePrivacy",value); }
        }
        public void Commit()
        {
            throw new NotImplementedException();
        }
        public void Delete()
        {
            throw new NotImplementedException();
        }
        public AvePrivacy DefaultPrivacy 
        {
            get { return base.DataCache.GetProperty<AvePrivacy>("DefaultPrivacy"); }
            set { base.DataCache.AddChangedProperty("DefaultPrivacy",value); }
        }
        public AvePrivacyPolicy PrivacyPolicy 
        {
            get { return base.DataCache.GetProperty<AvePrivacyPolicy>("PrivacyPolicy"); }
            set { base.DataCache.AddChangedProperty("PrivacyPolicy",value); }
        }

        #region IAveOPrivacyPolicyItem Members


        public Guid ID
        {
            get { return base.DataCache.GetProperty<Guid>("ID"); }
        }

        #endregion
    }
}
