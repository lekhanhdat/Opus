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
using AvePoint.ObjectModel.Common.Office;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common
{
    class AveOSocialCommentManager : AveAbstractCommonCollection<IAveOSocialComment>, IAveOSocialCommentManager
    {
        private IAveRequest request;
        private AveOUserProfile profile;

        public AveOSocialCommentManager(IAveServiceContext context)
        {
            request = (context as AveServiceContext).Request;
        }

        public IAveOSocialComment[] GetComments(IAveOUserProfile user)
        {
            profile = user as AveOUserProfile;
            Dictionary<string, object> socialCommentProps = profile.DataCache.GetProperty<Dictionary<string, object>>("Comments");
            var socialCommentList = socialCommentProps.GetChildren();
            IAveOSocialComment[] socialComments = new IAveOSocialComment[socialCommentList.Count];
            int i = 0;
            foreach (var socialCommentProp in socialCommentList)
            {
                AveOSocialComment socialComment = new AveOSocialComment(this.request, profile, socialCommentProp);
                socialComments[i] = socialComment;
                i++;
            }
            return socialComments;
        }

        public IAveOProfileLoader ProfileLoader
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSocialAdmin
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveOSocialComment[] GetComments(string url, Dictionary<long, string> profiles)
        {
            throw new NotImplementedException();
        }

        public void AddComment(Uri uri, string comment, bool isHighPriority)
        {
            throw new NotImplementedException();
        }

        public void AddComment(Uri url, string comment, bool isHighPriority, string title, DateTime modifiedTime, long recordId, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
