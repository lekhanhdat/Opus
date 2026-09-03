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

namespace AvePoint.ObjectModel.Common
{
    class AveUserProfile:AveClientObject,IAveUserProfile
    {
        private IAveRequest mRequest;
        private AveBPOSAccountInfo mAccountInfo;
        public AveUserProfile(IAveRequest request, AveBPOSAccountInfo accountInfo, Dictionary<string, object> properties)
        {
            mRequest = request;
            mAccountInfo = accountInfo;
            base.DataCache.AddPropertyies(properties);
        }
        public string AccountName
        {
            get 
            {
                throw new NotImplementedException();
            }
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

        public bool IsPeopleListPublic
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsPrivacySettingOn
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSelf
        {
            get { throw new NotImplementedException(); }
        }

        public string JobTitle
        {
            get { throw new NotImplementedException(); }
        }

        public int MySiteFirstRunExperience
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

        public string MySiteHostUrl
        {
            get { throw new NotImplementedException(); }
        }

        public int O15FirstRunExperience
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

        public IAveSite PersonalSite
        {
            get { throw new NotImplementedException(); }
        }

        public AvePersonalSiteCapabilities PersonalSiteCapabilities
        {
            get { throw new NotImplementedException(); }
        }

        public string PersonalSiteFirstCreationError
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime PersonalSiteFirstCreationTime
        {
            get { throw new NotImplementedException(); }
        }

        public AvePersonalSiteInstantiationState PersonalSiteInstantiationState
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime PersonalSiteLastCreationTime
        {
            get { throw new NotImplementedException(); }
        }

        public int PersonalSiteNumberOfRetries
        {
            get { throw new NotImplementedException(); }
        }

        public bool PictureImportEnabled
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

        public string PictureUrl
        {
            get { throw new NotImplementedException(); }
        }

        public string PublicUrl
        {
            get { throw new NotImplementedException(); }
        }

        public string SipAddress
        {
            get { throw new NotImplementedException(); }
        }

        public string UrlToCreatePersonalSite
        {
            get { throw new NotImplementedException(); }
        }

        public void CreatePersonalSite()
        {
            throw new NotImplementedException();
        }

        public void CreatePersonalSiteEnque()
        {
            throw new NotImplementedException();
        }

        public void ShareAllSocialData(bool shareAll)
        {
            throw new NotImplementedException();
        }

        public void SetMySiteFirstRunExperience(int value)
        {
            throw new NotImplementedException();
        }

        public void CreatePersonalSite(int lcid)
        {
            throw new NotImplementedException();
        }

        public void CreatePersonalSiteEnque(bool isInteractive)
        {
            throw new NotImplementedException();
        }


        public bool IsPeopleList
        {
            get { throw new NotImplementedException(); }
        }

        public string Url
        {
            get { throw new NotImplementedException(); }
        }
    }
}
