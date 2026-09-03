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

using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveWorkflowDefinition : AveClientObject, IAveWorkflowDefinition
    {
        private IAveRequest mRequest;
        private IAveWeb mWeb;
        private string mWorkflowSource;

        public AveWorkflowDefinition()
        { }

        public AveWorkflowDefinition(IAveWeb web, string workfolwSource, Dictionary<string, object> props)
        {
            mWeb = web;
            mWorkflowSource = workfolwSource;
            base.DataCache.AddPropertyies(props);
        }

        public void SetProperties(IDictionary<string, string> value) 
        {
            base.DataCache.AddChangedProperties((Dictionary<string, object>)value);
        }

        public void SetProperty(string propertyName, string value)
        {
            if(base.DataCache.GetProperty<IDictionary<string, string>>("Properties") == null) 
            {
                Dictionary<string, string> props = new Dictionary<string, string>();
                base.DataCache.AddChangedProperty("Properties", props);
            }
            if (!base.DataCache.GetProperty<IDictionary<string, string>>("Properties").ContainsKey(propertyName))
            {
                base.DataCache.GetProperty<IDictionary<string, string>>("Properties").Add(propertyName, value);
            }
            else
            {
                base.DataCache.GetProperty<IDictionary<string, string>>("Properties")[propertyName] = value;
            }
            switch(propertyName)
            {
                case "RequiresInitiationForm":
                    base.DataCache.AddChangedProperty(propertyName, bool.Parse(value));
                    break;
                default:
                    base.DataCache.AddChangedProperty(propertyName, value);
                    break;
            }
            
        }

        // Properties

        public string AssociationUrl 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("AssociationUrl");
            }
            set 
            {
                base.DataCache.AddChangedProperty("AssociationUrl", value);
            }
        }

        //internal string CanonicalId { get; }

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

        public string DisplayName 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("DisplayName");
            }
            set 
            {
                base.DataCache.AddChangedProperty("DisplayName", value);
            }
        }

        public string DraftVersion 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("DraftVersion");
            }
            set 
            {
                base.DataCache.AddChangedProperty("DraftVersion", value);
            }
        }

        public string FormField
        {
            get 
            {
                return base.DataCache.GetProperty<string>("FormField");
            }
            set
            {
                base.DataCache.AddChangedProperty("FormField", value);
            }
        }

        public Guid Id
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
            set 
            {
                base.DataCache.AddChangedProperty("Id", value);
            }
        }

        public string InitiationUrl 
        {
            get
            {
                return base.DataCache.GetProperty<string>("InitiationUrl");
            }
            set 
            {
                base.DataCache.AddChangedProperty("InitiationUrl", value);
            }
        }

        public IDictionary<string, string> Properties 
        {
            get 
            {
                return base.DataCache.GetProperty<IDictionary<string, string>>("Properties");
            }
        }

        public bool Published 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("Published");
            }
            set 
            {
                base.DataCache.AddChangedProperty("Published", value);
            }
        }

       public bool RequiresAssociationForm 
       {
           get 
           {
               return base.DataCache.GetProperty<bool>("RequiresAssociationForm");
           }
           set 
           {
               base.DataCache.AddChangedProperty("RequiresAssociationForm", value);
           }
       }

       public bool RequiresInitiationForm
       {
           get 
           {
               return base.DataCache.GetProperty<bool>("RequiresInitiationForm");
           }
           set 
           {
               base.DataCache.AddChangedProperty("RequiresInitiationForm", value);
           }
       }

       public string RestrictToScope 
       {
           get 
           {
               return base.DataCache.GetProperty<string>("RestrictToScope");
           }
           set 
           {
               base.DataCache.AddChangedProperty("RestrictToScope", value);
           }
       }

       public string RestrictToType
       {
           get 
           {
               return base.DataCache.GetProperty<string>("RestrictToType");
           }
           set 
           {
               base.DataCache.AddChangedProperty("RestrictToType", value);
           }
       }

       public IDictionary<string, string> UpdatedProperties
       {
           get 
           {
               return base.DataCache.GetProperty<IDictionary<string, string>>("UpdatedProperties");
           }
       }

       public string Xaml 
       {
           get 
           {
               return base.DataCache.GetProperty<string>("Xaml");
           }
           set 
           {
               base.DataCache.AddChangedProperty("Xaml", value);
           }
       }
    }
}
