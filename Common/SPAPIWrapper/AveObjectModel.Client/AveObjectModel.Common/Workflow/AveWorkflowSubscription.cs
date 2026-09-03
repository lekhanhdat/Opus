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
    class AveWorkflowSubscription : AveClientObject, IAveWorkflowSubscription
    {
        private string mWorkflowSource;
        private IAveWeb mWeb;


        public AveWorkflowSubscription() 
        {

        }

        public AveWorkflowSubscription(IAveWeb web, string workfolwSource, IDictionary<string, object> props)
        {
            mWeb = web;
            mWorkflowSource = workfolwSource;
            base.DataCache.AddPropertyies(props);
        }

        public string GetProperty(string name)
        {
            if (base.DataCache.GetProperty<IDictionary<string, string>>("PropertyDefinitions").ContainsKey(name))
            {
                return base.DataCache.GetProperty<IDictionary<string, string>>("PropertyDefinitions")[name];
            }
            return string.Empty;
        }

        public void SetProperty(string name, string value) 
        {
            if (base.DataCache.GetProperty<IDictionary<string, string>>("PropertyDefinitions") == null)
            {
                Dictionary<string, string> propsDefinitions = new Dictionary<string, string>();
                base.DataCache.AddChangedProperty("PropertyDefinitions", propsDefinitions);
            }
            if (!base.DataCache.GetProperty<IDictionary<string, string>>("PropertyDefinitions").ContainsKey(name))
            {
                base.DataCache.GetProperty<IDictionary<string, string>>("PropertyDefinitions").Add(name, value);
            }
            else
            {
                base.DataCache.GetProperty<IDictionary<string, string>>("PropertyDefinitions")[name] = value;
            }
        }

        public Guid DefinitionId 
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("DefinitionId");
            }
            set 
            {
                base.DataCache.AddChangedProperty("DefinitionId", value);
            }
        }

        public bool Enabled 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("Enabled");
            }
            set 
            {
                base.DataCache.AddChangedProperty("Enabled", value);
            }
        }

        public Guid EventSourceId 
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("EventSourceId");
            }
            set 
            {
                base.DataCache.AddChangedProperty("EventSourceId", value);
            }
        }

        public List<string> EventTypes
        {
            get 
            {
                return base.DataCache.GetProperty<List<string>>("EventTypes");
            }
            set
            {
                base.DataCache.AddChangedProperty("EventTypes", value);
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

        public bool ManualStartBypassesActivationLimit 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("ManualStartBypassesActivationLimit");
            }
            set 
            {
                base.DataCache.AddChangedProperty("ManualStartBypassesActivationLimit", value);
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

        public IDictionary<string, string> PropertyDefinitions 
        {
            get 
            {
                return base.DataCache.GetProperty<IDictionary<string, string>>("PropertyDefinitions");
            }
        }

        public bool StatusColumnCreated 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("StatusColumnCreated");
            }
            set 
            {
                base.DataCache.AddChangedProperty("StatusColumnCreated", value);
            }
        }

        public string StatusFieldName 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("StatusFieldName");
            }
            set 
            {
                base.DataCache.AddChangedProperty("StatusFieldName", value);
            }
        }
    }
}
