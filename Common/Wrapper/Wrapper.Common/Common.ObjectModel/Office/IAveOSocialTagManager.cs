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
using System.Text;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOSocialTagManager : IAveOSocialDataManager
    {
        IAveOSocialTag[] GetTags(IAveOUserProfile user);
        IAveOSocialTag[] GetTags(string url, Dictionary<long, string> profiles);

        void AddTag(Uri uri, IAveTerm term, string tagTitle, bool isPrivate);
        void AddTag(Uri url, IAveTerm term, string tagTitle, bool isPrivate, long recordId, Guid id, DateTime lastTime);
        void DeleteTags(Uri uri);
        void DeleteTag(Uri uri, IAveTerm term);
        //Guid PartitionID { get; }
        //IAveOUserProfileApplicationProxy UserProfileApplicationProxy { get; }
        //List<IAveOSocialTag> CreateSocialTags(List<Guid> termIDs, List<DateTime> lmts, List<string> titles, List<long> rgUserRecordIds, List<Uri> urls, List<bool> isPrivates, List<string> inputTermLabels);
        //List<IAveOSocialTag> CreateSocialTags(List<Guid> termIDs, List<DateTime> lmts, List<string> titles, List<long> rgUserRecordIds, List<Uri> urls, List<bool> isPrivates, List<string> inputTermLabels, Dictionary<long, string> profiles);
    }
}
