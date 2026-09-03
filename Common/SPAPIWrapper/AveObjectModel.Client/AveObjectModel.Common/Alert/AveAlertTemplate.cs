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

namespace AvePoint.ObjectModel.Common.Alert
{
    class AveAlertTemplate : AveClientObject, IAveAlertTemplate
    {
        private AveWeb mWeb;
        private AveAlertCollection mAlertCollection;
        private IAveRequest mRequest;

        public AveAlertTemplate(AveWeb web, AveAlertCollection alertCollection, IAveRequest request, Dictionary<string, object> alertProperties)
        {
            mWeb = web;
            mRequest = request;
            mAlertCollection = alertCollection;
            base.DataCache.AddPropertyies(alertProperties);
        }

        #region IAveAlertTemplate Members

        string IAveAlertTemplate.GetLocalizedXml(uint lcid)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAvePersistedObject Members

        IAveConfigurationDatabase IAvePersistedObject.ConfigurationDatabase
        {
            get { throw new NotImplementedException(); }
        }

        string IAvePersistedObject.DisplayName
        {
            get { throw new NotImplementedException(); }
        }

        IAveFarm IAvePersistedObject.Farm
        {
            get { throw new NotImplementedException(); }
        }

        Guid IAvePersistedObject.ID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string IAvePersistedObject.Name
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

        IAvePersistedObject IAvePersistedObject.Parent
        {
            get { throw new NotImplementedException(); }
        }

        AveObjectStatus IAvePersistedObject.Status
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string IAvePersistedObject.TypeName
        {
            get { throw new NotImplementedException(); }
        }

        System.Collections.Hashtable IAvePersistedObject.Properties
        {
            get { throw new NotImplementedException(); }
        }

        bool IAvePersistedObject.WasCreated
        {
            get { throw new NotImplementedException(); }
        }

        long IAvePersistedObject.Version
        {
            get { throw new NotImplementedException(); }
        }

        IAveLastUpdateInfo IAvePersistedObject.LastUpdateInfo
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        void IAvePersistedObject.Provision()
        {
            throw new NotImplementedException();
        }

        void IAvePersistedObject.Unprovision()
        {
            throw new NotImplementedException();
        }

        void IAvePersistedObject.Update()
        {
            throw new NotImplementedException();
        }

        void IAvePersistedObject.Update(bool ensure)
        {
            throw new NotImplementedException();
        }

        void IAvePersistedObject.Delete()
        {
            throw new NotImplementedException();
        }

        void IAvePersistedObject.Uncache()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAveAutoSerializingObject Members

        System.Xml.XmlDocument IAveAutoSerializingObject.GetStateXml()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
