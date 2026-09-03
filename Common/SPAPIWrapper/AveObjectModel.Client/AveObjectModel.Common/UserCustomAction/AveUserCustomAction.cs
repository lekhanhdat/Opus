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
    abstract class AveUserCustomAction : AveClientObject, IAveUserCustomAction
    {
        protected IAveRequest Request { get; set; }
        protected AveUserCustomActionCollection Container { get; set; }
        public AveUserCustomAction(AveUserCustomActionCollection container, IAveRequest request, IDictionary<string, object> prop)
        {
            Request = request as IAveRequest;
            Container = container;
            DataCache.AddPropertyies(prop);
        }

        #region IAveUserCustomAction Members

        public string CommandUIExtension
        {
            get
            {
                return base.DataCache.GetProperty<string>("CommandUIExtension");
            }
            set
            {
                base.DataCache.AddChangedProperty("CommandUIExtension", value);
            }
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                base.DataCache.AddChangedProperty("Description", value);
            }
        }
        public string Group
        {
            get
            {
                return base.DataCache.GetProperty<string>("Group");
            }
            set
            {
                base.DataCache.AddChangedProperty("Group", value);
            }
        }
        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }
        public string ImageUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ImageUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("ImageUrl", value);
            }
        }
        public string Location
        {
            get
            {
                return base.DataCache.GetProperty<string>("Location");
            }
            set
            {
                base.DataCache.AddChangedProperty("Location", value);
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
        public virtual string RegistrationId
        {
            get
            {
                return base.DataCache.GetProperty<string>("RegistrationId");
            }
            set
            {
                base.DataCache.AddChangedProperty("RegistrationId", value);
            }
        }
        public virtual AveUserCustomActionRegistrationType RegistrationType
        {
            get
            {
                return base.DataCache.GetProperty<AveUserCustomActionRegistrationType>("RegistrationType");
            }
            set
            {
                base.DataCache.AddChangedProperty("RegistrationType", value);
            }
        }
        public AveBasePermissions Rights
        {
            get
            {
                return base.DataCache.GetProperty<AveBasePermissions>("Rights");
            }
            set
            {
                base.DataCache.AddChangedProperty("Rights", value);
            }
        }
        public AveUserCustomActionScope Scope
        {
            get
            {
                return base.DataCache.GetProperty<AveUserCustomActionScope>("Scope");
            }
        }
        public string ScriptBlock
        {
            get
            {
                return base.DataCache.GetProperty<string>("ScriptBlock");
            }
            set
            {
                base.DataCache.AddChangedProperty("ScriptBlock", value);
            }
        }
        public string ScriptSrc
        {
            get
            {
                return base.DataCache.GetProperty<string>("ScriptSrc");
            }
            set
            {
                base.DataCache.AddChangedProperty("ScriptSrc", value);
            }
        }
        public int Sequence
        {
            get
            {
                return base.DataCache.GetProperty<int>("Sequence");
            }
            set
            {
                base.DataCache.AddChangedProperty("Sequence", value);
            }
        }
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
        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
            set
            {
                base.DataCache.AddChangedProperty("Url", value);
            }
        }
        public string VersionOfUserCustomAction
        {
            get
            {
                return base.DataCache.GetProperty<string>("VersionOfUserCustomAction");
            }
        }

        public Guid ClientSideComponentId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("ClientSideComponentId");
            }
            set
            {
                base.DataCache.AddChangedProperty("ClientSideComponentId", value);
            }
        }
        public string ClientSideComponentProperties
        {
            get
            {
                return base.DataCache.GetProperty<string>("ClientSideComponentProperties");
            }
            set
            {
                base.DataCache.AddChangedProperty("ClientSideComponentProperties", value);
            }
        }
        public IAveUserResource DescriptionResource
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public IAveUserResource TitleResource
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual void DeleteObject()
        {
            Container.ListData.Remove(this);
        }
        public virtual void Update()
        {

        }
        #endregion

    }
}
