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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveObjectSharingInformationUser : AveClientObject, IAveObjectSharingInformationUser
    {
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private string mSource;

        public AveObjectSharingInformationUser(IAveRequest request, AveWeb parentWeb, string source, Dictionary<string, object> properties)
        {
            mRequest = request;
            mParentWeb = parentWeb;
            mSource = source;
            base.DataCache.AddPropertyies(properties);
        }
        public string CustomRoleNames 
        {
            get { return base.DataCache.GetProperty<string>("CustomRoleNames"); } 
        }
        public string Department 
        {
            get { return base.DataCache.GetProperty<string>("Department"); } 
        }
        public string Email
        {
            get { return base.DataCache.GetProperty<string>("Email"); } 
        }
        public bool HasEditPermission
        {
            get { return base.DataCache.GetProperty<bool>("HasEditPermission"); } 
        }
        public bool HasViewPermission
        {
            get { return base.DataCache.GetProperty<bool>("HasViewPermission"); } 
        }
        public int Id
        {
            get { return base.DataCache.GetProperty<int>("Id"); } 
        }
        public bool IsDomainGroup
        {
            get { return base.DataCache.GetProperty<bool>("IsDomainGroup"); } 
        }
        public bool IsSiteAdmin
        {
            get { return base.DataCache.GetProperty<bool>("IsSiteAdmin"); } 
        }
        public string JobTitle
        {
            get { return base.DataCache.GetProperty<string>("JobTitle"); } 
        }
        public string LoginName
        {
            get { return base.DataCache.GetProperty<string>("LoginName"); } 
        }
        public string Name
        {
            get { return base.DataCache.GetProperty<string>("Name"); } 
        }
        public string Picture
        {
            get { return base.DataCache.GetProperty<string>("Picture"); } 
        }
        public IAvePrincipal Principal
        {
            get 
            { 
                Dictionary<string, object> dic = base.DataCache.GetProperty<Dictionary<string, object>>("Principal");
                base.DataCache.AddPropertyies(dic);
                return new AvePrincipal();
            } 
        }
        public string SipAddress
        {
            get { return base.DataCache.GetProperty<string>("SipAddress"); } 
        }
        public IAveUser User 
        { 
            get { return new AveUser(mRequest, mParentWeb, mSource, base.DataCache.GetProperty<Dictionary<string, object>>("User")); } 
        }
    }
}
