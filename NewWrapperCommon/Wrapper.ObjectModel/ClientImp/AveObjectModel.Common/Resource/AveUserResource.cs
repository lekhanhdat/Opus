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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;

namespace AvePoint.ObjectModel.Common
{
    abstract class AveUserResource : IAveUserResource
    {
        #region Properties
        protected static AveLogger logger = AveLogger.GetInstance(typeof(AveUserResource));
        protected Dictionary<string, string> cultureAndValueMappings = new Dictionary<string, string>();
        protected AveClientObjectData mDataCache;
        protected string mResourceName;
        protected AveWeb mWeb;
        protected IAveRequest mRequest;
        public string Name
        {
            get
            {
                return string.Empty;
            }
        }

        public object Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool ResxBased
        {
            get
            {
                return false;
            }
        }

        public string ResxResourceId
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

        public AveResourceScope Scope
        {
            get
            {
                return AveResourceScope.Web;
            }
        }

        public AveResourceType Type
        {
            get
            {
                return AveResourceType.SingleLine;
            }
        }
        #endregion
        public AveUserResource(AveWeb web, string resourceName, AveClientObjectData dataCache)
        {
            mWeb = web;
            mRequest = (mWeb.Site as AveSite).Request ;
            mResourceName = resourceName;
            mDataCache = dataCache;
        }
        public string GetValueForUICulture(CultureInfo cultureInfo)
        {
            lock (cultureAndValueMappings)
            {
                if (cultureAndValueMappings.Count == 0)
                {
                    RetrieveValuesForAllLanguage();
                }
                string value;
                if (cultureAndValueMappings.TryGetValue(cultureInfo.Name, out value))
                {
                    return value;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// For performance, Retrieve all values in one request.
        /// </summary>
        protected abstract void RetrieveValuesForAllLanguage();

        virtual public void SetVauleForUICulture(CultureInfo cultureInfo, string value)
        {
            object changeObj;
            var changeProperties = new Dictionary<string, string>();
            if(!mDataCache.ChangedProperties.TryGetValue(mResourceName,out changeObj))
            {
                mDataCache.AddChangedProperty(mResourceName, changeProperties);
            }
            else
            {
                changeProperties = changeObj as Dictionary<string, string>;
            }
            changeProperties[cultureInfo.Name] = value;

            lock (cultureAndValueMappings)
            {
                if (cultureAndValueMappings.Count > 0)//只有当Load过Resouce后，新添加的value才加到集合里,否则影响GetValueForUICulture.
                {
                    cultureAndValueMappings[cultureInfo.Name] = value;
                }
            }
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
