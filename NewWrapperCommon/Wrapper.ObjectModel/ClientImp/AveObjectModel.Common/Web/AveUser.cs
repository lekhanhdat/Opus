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
    class AveUser : AveClientObject, IAveUser
    {
        public AveUser(Dictionary<string, object> properties)
        {
            base.DataCache.AddPropertyies(properties);
        }
        public int Id 
        { 
            get {
                   return base.DataCache.GetProperty<int>("ID");
            }
        }
        public string LoginName 
        {
            get {
                return base.DataCache.GetProperty<string>("LoginName");
            } 
        }
        public string Name 
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            } 
            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }
        public AvePrincipalType PrincipalType
        {
            get {
                return base.DataCache.GetProperty<AvePrincipalType>("PrincipalType");
            }
        }
        public String Email
        {
            get {
                return base.DataCache.GetProperty<String>("Email");
            }
            set {
                base.DataCache.AddChangedProperty("Email", value);
            }
        }
        public bool IsDomainGroup 
        {
            get{
                return base.DataCache.GetProperty<bool>("IsDomainGroup");
            } 
        }
        public bool IsSiteAdmin 
        {
            get{
                return base.DataCache.GetProperty<bool>("IsSiteAdmin");
            } 
            set{
                base.DataCache.AddChangedProperty("IsSiteAdmin", value);
            } 
        }
        public string Notes
        {
            get{
                return base.DataCache.GetProperty<string>("Notes");
            }
            set{
                base.DataCache.AddChangedProperty("Notes", value);
            }
        }
        public  IAveRegionalSettings RegionalSettings 
        { 
            get{
                return base.DataCache.GetProperty<IAveRegionalSettings>("RegionalSettings");
            }
            set{
                base.DataCache.AddChangedProperty("RegionalSettings", value);
            } 
        }
        public object SPUser 
        { 
            get{
                return base.DataCache.GetProperty<object>("SPUser");
            }
        }
        public IAveUserToken UserToken 
        {
            get{
                return base.DataCache.GetProperty<IAveUserToken>("UserToken");
            }
        }

        public IAveAlertCollection Alerts 
        {
            get { throw new NotFiniteNumberException(); }
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
