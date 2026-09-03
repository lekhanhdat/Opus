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

namespace AvePoint.ObjectModel.Common
{
    abstract class AveUserResource : IAveUserResource
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(AveUserResource));

        protected Dictionary<string, string> keyValues;
        protected List<string> changedCulture;

        public AveUserResource()
        {
            keyValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            changedCulture = new List<string>();
        }

        public string GetValueForUICulture(string cultureName)
        {
            string resourceValue;

            if (string.IsNullOrEmpty(cultureName))
            {
                throw new ArgumentNullException("cultureName");
            }

            var needLoad = false;

            lock (keyValues)
            {
                if (!keyValues.TryGetValue(cultureName, out resourceValue))
                {
                    needLoad = true;
                    resourceValue = null;
                }
            }

            if (needLoad)
            {
                resourceValue = GetValueForUICultureWithRequest(cultureName);

                EnsureResource(cultureName, resourceValue);
            }

            return resourceValue;
        }

        protected abstract string GetValueForUICultureWithRequest(string cultureName);

        public void SetValueForUICulture(string cultureName, string value,bool forceSet = false)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                throw new ArgumentNullException("cultureName");
            }

            lock (keyValues)
            {
                string resourceValue;
                var needEnsure = true;
                if (keyValues.TryGetValue(cultureName, out resourceValue))
                {
                    needEnsure = forceSet || string.Compare(resourceValue, value, StringComparison.Ordinal) != 0;
                }

                if (needEnsure)
                {
                    keyValues[cultureName] = value;
                    if (!changedCulture.Contains(cultureName))
                    {
                        changedCulture.Add(cultureName);
                    }
                }
            }
        }

        internal void EnsureResource(string cultureName, string value)
        {
            lock (keyValues)
            {
                keyValues[cultureName] = value;
            }
        }

        public void Update()
        {
            if (changedCulture.Count > 0)
            {
                Dictionary<string, string> changedTitle = new Dictionary<string, string>();
                lock (keyValues)
                {
                    foreach (var item in changedCulture)
                    {
                        changedTitle[item] = keyValues[item];
                    }
                    changedCulture.Clear();
                }

                try
                {
                    InternalUpdate(changedTitle);
                }
                catch (Exception ex)
                {
                    logger.Error("update changed resource:{0} failed:{1}", Convert(changedTitle), ex);
                    lock (keyValues)
                    {
                        foreach (var item in changedTitle.Keys)
                        {
                            keyValues.Remove(item);
                        }
                    }
                }
            }
        }

        protected string Convert(Dictionary<string, string> items)
        {
            if(items != null && items.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach(KeyValuePair<string,string> keyValue in items)
                {
                    builder.Append(keyValue.Key);
                    builder.Append('=');
                    builder.Append(keyValue.Value);
                    builder.Append(';');
                }

                return builder.ToString();
            }

            return string.Empty;
        }

        protected abstract void InternalUpdate(Dictionary<string, string> changedTitle);
    }
}
