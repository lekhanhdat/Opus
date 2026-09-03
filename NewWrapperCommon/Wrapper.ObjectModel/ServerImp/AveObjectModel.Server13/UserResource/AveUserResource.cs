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


namespace AvePoint.ObjectModel.Server13
{
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint;

    internal class AveUserResource : IAveUserResource
    {
        private SPUserResource userResource;

        internal AveUserResource(SPUserResource resource)
        {
            this.userResource = resource;
        }

        public AveUserResource(string name, SPResourceType type)
        {
            this.userResource = new SPUserResource(name, type);
        }
        public AveUserResource(string name, string value, SPResourceType type)
        {
            this.userResource = new SPUserResource(name, value, type);
        }

        public string Name
        {
            get { return this.userResource.Name; }
        }

        public object Parent
        {
            get { return this.userResource.Parent; }
        }

        public AveResourceScope Scope
        {
            get { return (AveResourceScope)this.userResource.Scope; }
        }

        public AveResourceType Type
        {
            get { return (AveResourceType)this.userResource.Type; }
        }

        public string GetValueForUICulture(System.Globalization.CultureInfo cultureInfo)
        {
            return this.userResource.GetValueForUICulture(cultureInfo);
        }

        public void SetVauleForUICulture(System.Globalization.CultureInfo cultureInfo, string value)
        {
            this.userResource.SetValueForUICulture(cultureInfo, value);
        }

        public void Update()
        {
            this.userResource.Update();
        }

        public bool ResxBased
        {
            get { return (bool)AveAssemblyUtility.GetPropertyValue(this.userResource, "ResxBased"); }
        }

        public string ResxResourceId
        {
            get { return (string)AveAssemblyUtility.GetPropertyValue(this.userResource, "ResxResourceId"); }
            set { AveAssemblyUtility.SetPropertyValue(userResource, "ResxResourceId", value); }
        }
    }
}
