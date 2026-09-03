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
    class AveObjectSharingInformation : AveClientObject, IAveObjectSharingInformation
    {
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private string mSource;

        public AveObjectSharingInformation(IAveRequest request, AveWeb parentWeb, string source, Dictionary<string, object> properties)
        {
            mRequest = request;
            mParentWeb = parentWeb;
            mSource = source;
            base.DataCache.AddPropertyies(properties);
        }

        public string AnonymousEditLink
        {
            get { return base.DataCache.GetProperty<string>("AnonymousEditLink"); }
        }
        public string AnonymousViewLink 
        {
            get { return base.DataCache.GetProperty<string>("AnonymousViewLink"); }
        }
        public bool CanBeShared 
        {
            get { return base.DataCache.GetProperty<bool>("CanBeShared"); }
        }
        public bool CanBeUnshared 
        {
            get { return base.DataCache.GetProperty<bool>("CanBeUnshared"); }
        }
        public bool CanManagePermissions 
        {
            get { return base.DataCache.GetProperty<bool>("CanManagePermissions"); }
        }
        public bool HasPendingAccessRequests 
        {
            get { return base.DataCache.GetProperty<bool>("HasPendingAccessRequests"); }
        }
        public bool HasPermissionLevels 
        {
            get { return base.DataCache.GetProperty<bool>("HasPermissionLevels"); }
        }
        public bool IsSharedWithCurrentUser 
        {
            get { return base.DataCache.GetProperty<bool>("IsSharedWithCurrentUser"); }
        }
        public bool IsSharedWithGuest 
        {
            get { return base.DataCache.GetProperty<bool>("IsSharedWithGuest"); }
        }
        public bool IsSharedWithMany 
        {
            get { return base.DataCache.GetProperty<bool>("IsSharedWithMany"); }
        }
        public bool IsSharedWithSecurityGroup 
        {
            get { return base.DataCache.GetProperty<bool>("IsSharedWithSecurityGroup"); }
        }
        public string PendingAccessRequestsLink 
        {
            get { return base.DataCache.GetProperty<string>("PendingAccessRequestsLink"); }
        }
        public IAveObjectSharingInformationUserCollection GetSharedWithUsers()
        {
            AveObjectSharingInformationUserCollection userCollection = new AveObjectSharingInformationUserCollection(mRequest, mParentWeb, mSource, base.DataCache.GetProperty<List<Dictionary<string, object>>>("SharedWithUsers"));
            return userCollection;
        }

    }
}
