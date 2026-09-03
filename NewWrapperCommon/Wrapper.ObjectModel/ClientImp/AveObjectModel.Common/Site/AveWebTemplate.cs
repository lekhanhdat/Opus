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
    class AveWebTemplate : AveClientObject, IAveWebTemplate
    {
        public AveWebTemplate(Dictionary<string, object> properties)
        {
            base.DataCache.AddPropertyies(properties);
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
        }
        public string DisplayCategory
        {
            get
            {
                return base.DataCache.GetProperty<string>("DisplayCategory");
            }
        }
        public int ID
        {
            get
            {
                return base.DataCache.GetProperty<int>("ID");
            }
        }
        public string ImageUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ImageUrl");
            }
        }
        public bool IsHidden
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsHidden");
            }
        }
        public bool IsRootWebOnly
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsRootWebOnly");
            }
        }
        public bool IsSubWebOnly
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSubWebOnly");
            }
        }
        public uint Lcid
        {
            get
            {
                return base.DataCache.GetProperty<uint>("Lcid");
            }
        }
        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }
        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
        }


        public string WebTemplateBLOG
        {
            get { throw new NotImplementedException(); }
        }

        public string WebTemplateMWS
        {
            get { throw new NotImplementedException(); }
        }

        public string WebTemplateSTS
        {
            get { throw new NotImplementedException(); }
        }

        public string WebTemplateWIKI
        {
            get { throw new NotImplementedException(); }
        }

        public Guid VisibilityFeatureDependencyId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("VisibilityFeatureDependencyId");
            }
        }

        public bool SupportsMultilingualUI
        {
            get 
            {
                return true; 
            }
        }

        #region add for SP2013
        public int CompatibilityLevel
        {
            get { return base.DataCache.GetProperty<int>("CompatibilityLevel"); }
        }
        #endregion
    }
}
