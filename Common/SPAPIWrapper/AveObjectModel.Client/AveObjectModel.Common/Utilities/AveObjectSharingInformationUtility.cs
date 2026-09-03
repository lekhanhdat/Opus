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
using System.Threading.Tasks;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    class AveObjectSharingInformationUtility : AveClientObject, IAveObjectSharingInformationUtility
    {
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private string mSource;
        public AveObjectSharingInformationUtility(IAveRequest request, AveWeb parentWeb, string source)
        {
            mRequest = request;
            mParentWeb = parentWeb;
            mSource = source;
        }
        public IAveObjectSharingInformation GetListItemSharingInformation(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests)
        {
            Dictionary<string, object> dic = mRequest.GetListItemSharingInformation(listID, itemID, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests);
            return new AveObjectSharingInformation(mRequest, mParentWeb, mSource, dic);
        }
        public IAveObjectSharingInformation GetObjectSharingInformation(IAveSecurableObject securableObject, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (securableObject is AveWeb)
            {
                dic = mRequest.GetWebSharingInformation2(excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels);
            }
            if (securableObject is AveList)
            {
                dic = mRequest.GetListSharingInformation((securableObject as AveList).ID, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels);
            }
            if (securableObject is AveListItem)
            {
                dic = mRequest.GetListItemSharingInformation2((securableObject as AveListItem).ParentList.ID, (securableObject as AveListItem).ID, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels);
            }
            if (dic == null || dic.Count == 0)
            {
                return null;
            }

            return new AveObjectSharingInformation(mRequest, mParentWeb, mSource, dic);
        }
        public IAveObjectSharingInformation GetObjectSharingInformation2(IAveSecurableObject securableObject, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (securableObject is AveWeb)
            {
                dic = mRequest.GetWebSharingInformation3(excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels, forceRetrievePermissionLevels);
            }
            if (securableObject is AveList)
            {
                dic = mRequest.GetListSharingInformation2((securableObject as AveList).ID, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels, forceRetrievePermissionLevels);
            }
            if (securableObject is AveListItem)
            {
                dic = mRequest.GetListItemSharingInformation3((securableObject as AveListItem).ParentList.ID, (securableObject as AveListItem).ID, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels, forceRetrievePermissionLevels);
            }
            if (dic == null || dic.Count == 0)
            {
                return null;
            }

            return new AveObjectSharingInformation(mRequest, mParentWeb, mSource, dic);
        }
        public IAveObjectSharingInformation GetObjectSharingInformationByUrl(string objectUrl, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels)
        {
            Dictionary<string, object> dic = mRequest.GetObjectSharingInformationByUrl(objectUrl, excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests, retrievePermissionLevels);
            return new AveObjectSharingInformation(mRequest, mParentWeb, mSource, dic);
        }
        public IAveObjectSharingInformation GetWebSharingInformation(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests)
        {
            Dictionary<string, object> dic = mRequest.GetWebSharingInformation(excludeCurrentUser, excludeSiteAdmin, excludeSecurityGroups, retrieveAnonymousLinks, retrieveUserInfoDetails, checkForAccessRequests);
            return new AveObjectSharingInformation(mRequest, mParentWeb, mSource, dic);
        }
        public AveUserSharingCapabilities CanCurrentUserShare(string docId)
        {
            int i =mRequest.CanCurrentUserShare(docId);
            return (AveUserSharingCapabilities)i;
        }
        public AveUserSharingCapabilities CanCurrentUserShareRemote(string docId)
        {
            int i = mRequest.CanCurrentUserShareRemote(docId);
            return (AveUserSharingCapabilities)i;
        }

    }
}
