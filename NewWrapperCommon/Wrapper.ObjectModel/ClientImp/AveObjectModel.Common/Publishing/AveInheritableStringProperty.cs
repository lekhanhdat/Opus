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
    class AveInheritableStringProperty : AveClientObject, IAveInheritableStringProperty
    {
        private List<object[]> mSetValueList = new List<object[]>();
        private List<object[]> mSetInheritList = new List<object[]>();

        public AveInheritableStringProperty(string propertyPrefix, Dictionary<string, object> belongedObjChangedProperties, Dictionary<string, object> inheritableProperties)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props["SetValueList"] = mSetValueList;
            props["SetInheritList"] = mSetInheritList;
            belongedObjChangedProperties.Add(propertyPrefix + "changed", props);            
            base.DataCache.AddPropertyies(inheritableProperties);
        }

        #region IAveInheritableStringProperty Members

        public void SetValue(string value)
        {
            this.SetValue(value, false);
        }

        public void SetValue(string value, bool forceAllSubWebInherit)
        {
            this.SetValue(value, forceAllSubWebInherit, null, null);
        }

        public void SetValue(string value, bool forceAllSubWebInherit, string successUrl, string failureUrl)
        {
            object[] args = new object[] { value, forceAllSubWebInherit, successUrl, failureUrl};
            mSetValueList.Add(args);
        }

        public void SetInherit(bool inherit, bool forceAllSubWebInherit)
        {
            this.SetInherit(inherit, forceAllSubWebInherit, null, null);
        }

        public void SetInherit(bool inherit, bool forceAllSubWebInherit, string successUrl, string failureUrl)
        {
            object[] args = new object[] { inherit, forceAllSubWebInherit, successUrl, failureUrl};
            mSetInheritList.Add(args);
        }

        public string Value
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("Value"); 
            }
        }

        public bool IsInheriting
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("IsInheriting"); 
            }
        }

        #endregion
    }
}
