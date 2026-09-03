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
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public abstract class WebPartPropertyExtractorBase : IWebPartPropertyExtractor
    {
        protected XmlNode webpartDefinition;

        public WebPartPropertyExtractorBase(XmlNode webpartDefinition)
        {
            this.webpartDefinition = webpartDefinition;
        }

        public bool? GetBoolProperty(string propertyName)
        {
            XmlNode propertyNode = GetPropertyValue(propertyName);
            if (propertyNode == null)
            {
                return null;
            }
            string propertyValue = propertyNode.InnerText;
            bool boolPropertyValue;
            if (Boolean.TryParse(propertyValue, out boolPropertyValue))
            {
                return boolPropertyValue;
            }
            else
            {
                return null;
            }
        }

        public int? GetIntProperty(string propertyName)
        {
            XmlNode propertyNode = GetPropertyValue(propertyName);
            if (propertyNode == null)
            {
                return null;
            }
            string propertyValue = propertyNode.InnerText;
            int intPropertyValue;
            if (int.TryParse(propertyValue, out intPropertyValue))
            {
                return intPropertyValue;
            }
            else
            {
                return null;
            }
        }

        public string GetProperty(string propertyName)
        {
            XmlNode propertyNode = GetPropertyValue(propertyName);
            if (propertyNode == null)
            {
                return null;
            }
            return propertyNode.InnerText;
        }

        public T? GetProperty<T>(string propertyName) where T : struct
        {
            XmlNode propertyNode = GetPropertyValue(propertyName);
            if (propertyNode == null)
            {
                return null;
            }
            string propertyValue = propertyNode.InnerText;
            return (T)Enum.Parse(typeof(T), propertyValue);
        }

        protected abstract XmlNode GetPropertyValue(string propertyName);
        public bool ContainsProperty(string propertyName)
        {
            return GetPropertyValue(propertyName) != null;
        }
        public abstract bool AddProperty(bool properties, string propertyName, object value);
        public abstract string TypeFullName { get; }

        public abstract Guid SolutionId { get; }
    }
}
