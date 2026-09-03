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
using System.Collections.Specialized;
using System.Xml;

namespace AvePoint.ObjectModel.Common.Workflow
{
    class AveWorkflowTemplate : AveClientObject, IAveWorkflowTemplate
    {
        public AveWorkflowTemplate(IDictionary<string, object> prop)
        {
            base.DataCache.AddPropertyies(prop);
        }
        
        public bool AllowDefaultContentApproval
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("AllowDefaultContentApproval");
            }
            set 
            {
                base.DataCache.AddChangedProperty("AllowDefaultContentApproval", value);
            }
        }

        public bool AutoStartChange 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("AutoStartChange");
            }
            set 
            {
                base.DataCache.AddChangedProperty("AutoStartChange", value);
            }
        }

        public bool AutoStartCreate 
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AutoStartCreate");
            }
            set
            {
                base.DataCache.AddChangedProperty("AutoStartCreate", value);
            }
        }

        public Guid BaseId
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("BaseId");
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

        public Guid ID 
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }

        public AveBasePermissions PermissionsManual
        {
            get 
            {
                return base.DataCache.GetProperty<AveBasePermissions>("PermissionsManual");
            }
            set 
            {
                base.DataCache.AddChangedProperty("PermissionsManual", value);
            }
        }

        public IAveWorkflowTemplateIdSet TemplateIdSet
        {
            get 
            {
                return base.DataCache.GetProperty<IAveWorkflowTemplateIdSet>("TemplateIdSet");
            }
        }

        public StringCollection GetStatusChoices(IAveWeb web) 
        {
            throw new NotImplementedException();
        }

        public XmlDocument GetStateXml()
        {
           // return (XmlDocument)AveAssemblyUtility.InvokeMethod(mAutoSerializingObject, "GetStateXml", new Type[] { }, new object[] { });
            throw new NotImplementedException();
        }
    }
}
