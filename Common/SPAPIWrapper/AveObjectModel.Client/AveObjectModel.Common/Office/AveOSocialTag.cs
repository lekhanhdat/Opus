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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOSocialTag : AveClientObject, IAveOSocialTag
    {
        private IAveRequest request;
        private IAveOUserProfile profile;

        public AveOSocialTag(IAveRequest request, AveOUserProfile profile, IDictionary<string, object> SocialTagProp)
        {
            this.request = request;
            this.profile = profile;
            base.DataCache.AddPropertyies(SocialTagProp);
        }

        #region IAveOSocialTag Members
        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
            set
            {
                base.DataCache.AddChangedProperty("Title", value);
            }
        }

        public IAveOUserProfile Owner
        {
            get
            {
                return profile;
            }
        }

        public Uri Url
        {
            get
            {
                return base.DataCache.GetProperty<Uri>("Url");
            }
            set
            {
                base.DataCache.AddChangedProperty("Url", value);
            }
        }

        public bool IsPrivate
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsPrivate");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsPrivate", value);
            }
        }

        public string OwnerName
        {
            get
            {
                return base.DataCache.GetProperty<string>("OwnerName");
            }
            set
            {
                base.DataCache.AddChangedProperty("OwnerName", value);
            }
        }

        public DateTime LastModifiedTime
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LastModifiedTime");
            }
            set
            {
                base.DataCache.AddChangedProperty("LastModifiedTime", value);
            }
        }

        public IAveTerm Term
        {
            get
            {
                Dictionary<string, object> terms = base.DataCache.GetProperty<Dictionary<string, object>>("Terms");
                AveTerm term = new AveTerm(request, null, null, terms);
                return term;
            }
            set
            {
                base.DataCache.AddChangedProperty("Terms", value as Dictionary<string, object>);
            }
        }
        #endregion
    }
}
