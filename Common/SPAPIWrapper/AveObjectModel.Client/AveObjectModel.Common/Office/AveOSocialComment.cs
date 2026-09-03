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
using AvePoint.ObjectModel.Common.Office;

namespace AvePoint.ObjectModel.Common
{
    class AveOSocialComment : AveClientObject, IAveOSocialComment
    {
        private IAveRequest request;
        private IAveOUserProfile profile;

        public AveOSocialComment(IAveRequest request, AveOUserProfile profile, IDictionary<string, object> SocialCommentProp)
        {
            this.request = request;
            this.profile = profile;
            base.DataCache.AddPropertyies(SocialCommentProp);
        }

        #region IAveOSocialComment Members
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

        public bool IsHighPriority
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsHighPriority");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsHighPriority", value);
            }
        }

        public string Comment
        {
            get
            {
                return base.DataCache.GetProperty<string>("Comment");
            }
            set
            {
                base.DataCache.AddChangedProperty("Comment", value);
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

        public IAveOUserProfile Owner
        {
            get
            {
                return this.profile;
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
        #endregion
    }
}
