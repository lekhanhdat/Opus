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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveListDataSource : AveClientObject, IAveListDataSource
    {
        public AveListDataSource(Dictionary<string, object> dataSource )
        {
            base.DataCache.AddPropertyies(dataSource);
        }

        public AveListDataSource( string dataSourceXmlStr )
        {
            InitalDataSource(dataSourceXmlStr);
        }

        internal void InitalDataSource(string dataSourceXml) 
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(dataSourceXml);
            foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
            {
                switch (node.Attributes["Name"].Value)
                {
                    case AveBDCProperties.LobSystemInstance:
                        this.DataCache.PropertiesCache[AveBDCProperties.LobSystemInstance] = node.Attributes["Value"].Value;
                        break;
                    case AveBDCProperties.EntityNamespace:
                        this.DataCache.PropertiesCache[AveBDCProperties.EntityNamespace] = node.Attributes["Value"].Value;
                        break;
                    case AveBDCProperties.Entity:
                        this.DataCache.PropertiesCache[AveBDCProperties.Entity] = node.Attributes["Value"].Value;
                        break;
                    case AveBDCProperties.SpecificFinder:
                        this.DataCache.PropertiesCache[AveBDCProperties.SpecificFinder] = node.Attributes["Value"].Value;
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetProperty(string key, string value)
        {
            this.DataCache.AddChangedProperty(key, value);
        }
        public string GetProperty(string key)
        {
            return this.DataCache.GetProperty<string>(key);
        }

        public string ToXml()
        {
            StringBuilder builder = new StringBuilder(0x100);
            builder.Append("<DataSource");
            bool flag = false;

            foreach (KeyValuePair<string, object> kp in this.DataCache.PropertiesCache)
            {
                string name = kp.Key.ToString();
                string value = kp.Value.ToString();
                if (!flag)
                {
                    builder.Append(">");
                    flag = true;
                }
                builder.Append("<Property Name=\"");
                builder.Append(name);
                builder.Append("\" Value=\"");
                builder.Append(value);
                builder.Append("\"/>");
            }
            if (flag)
            {
                builder.Append("</DataSource>");
            }
            else
            {
                builder.Append("/>");
            }
            return builder.ToString();
        }
    }
}
