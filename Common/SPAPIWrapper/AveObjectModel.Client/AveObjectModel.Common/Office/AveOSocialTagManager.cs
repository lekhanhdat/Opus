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
    class AveOSocialTagManager : AveAbstractCommonCollection<IAveOSocialTag>, IAveOSocialTagManager
    {
        private IAveRequest request;
        private AveOUserProfile profile;

        public AveOSocialTagManager(IAveServiceContext context)
        {
            request = (context as AveServiceContext).Request;
        }

        public IAveOSocialTag[] GetTags(IAveOUserProfile user)
        {
            profile = user as AveOUserProfile;
            Dictionary<string, object> SocialTagProp = profile.DataCache.GetProperty<Dictionary<string, object>>("Tags");
            var socialTagList = SocialTagProp.GetChildren();
            IAveOSocialTag[] socialTags = new IAveOSocialTag[socialTagList.Count];
            int i = 0;
            foreach (var socialTagProp in socialTagList)
            {
                AveOSocialTag socialTag = new AveOSocialTag(this.request, profile, socialTagProp);
                socialTags[i] = socialTag;
                i++;
            }
            return socialTags;
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

        public IAveOSocialTag[] GetTags(string url, Dictionary<long, string> profiles)
        {
            return null;
        }

        public void AddTag(Uri uri, IAveTerm term, string tagTitle, bool isPrivate)
        {
            throw new NotImplementedException();
        }

        public void AddTag(Uri url, IAveTerm term, string tagTitle, bool isPrivate, long recordId, Guid id, DateTime lastTime)
        {
            throw new NotImplementedException();
        }

        public void DeleteTags(Uri uri)
        {
            throw new NotImplementedException();
        }

        public void DeleteTag(Uri uri, IAveTerm term)
        {
            throw new NotImplementedException();
        }
    }
}
