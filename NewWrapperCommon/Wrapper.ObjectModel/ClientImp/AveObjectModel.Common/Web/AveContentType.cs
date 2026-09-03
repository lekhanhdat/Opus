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
using System.Net;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveContentType : AveClientObject, IAveContentType
    {
        private bool mContentTypeIdLoaded;
        public AveContentType()
        {
            mContentTypeIdLoaded = false;
        }

        public string Name
        {
            get {
                return base.DataCache.GetProperty<string>("Name");
            }
            set {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }
        public IAveContentTypeId Id
        {
            get {
                if ( mContentTypeIdLoaded == false )
                {
                    AveContentTypeId ID = new AveContentTypeId(base.DataCache.GetProperty<string>("ID"));
                    base.DataCache.PropertiesCache["ID"] = ID;
                    mContentTypeIdLoaded = true;
                }
                return base.DataCache.GetProperty<IAveContentTypeId>("ID");
            }
        }
        public string Description
        {
            get {
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
        public string Scope
        {
            get
            {
                return base.DataCache.GetProperty<string>("Scope");
            }
        }
        public string DisplayFormTemplateName 
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public string DisplayFormUrl
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public string DocumentTemplate
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public string DocumentTemplateUrl
        {
            get { throw new NotImplementedException(); }
        }
        public string EditFormTemplateName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public string EditFormUrl
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public IAveFieldLinkCollection FieldLinks
        {
            get { throw new NotImplementedException(); }
        }
        public IAveFieldCollection Fields
        {
            get {
                return base.DataCache.GetProperty<IAveFieldCollection>("Fields");
            }
        }
        public bool Hidden
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public string NewFormTemplateName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public string NewFormUrl
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public IAveContentType Parent { get { throw new NotImplementedException(); } }
        public IAveList ParentList { get { return base.DataCache.GetProperty<IAveList>("ParentList"); } }
        public IAveWeb ParentWeb { get { return base.DataCache.GetProperty<IAveWeb>("ParentWeb"); } }
        public bool ReadOnly
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IAveWorkflowAssociationCollection WorkflowAssociations 
        {
            get { throw new NotImplementedException(); } 
        }
        public IAveFolder ResourceFolder { get { throw new NotImplementedException(); } }
        public string SchemaXml { get { throw new NotImplementedException(); } }
        public bool Sealed { get { throw new NotImplementedException(); } }
        public IAveXmlDocumentCollection XmlDocuments { get { throw new NotImplementedException(); } }


        public void Update() { throw new NotImplementedException(); }
        public void Update(bool updateChildren) { throw new NotImplementedException(); }
    }
}
