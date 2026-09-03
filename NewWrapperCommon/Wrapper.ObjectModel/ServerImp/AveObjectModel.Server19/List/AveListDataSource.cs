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



using System.Xml;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.Server19
{
    class AveListDataSource : IAveListDataSource
    {
        private SPListDataSource mListDataSource;

        public AveListDataSource(SPListDataSource spListDataSource)
        {
            mListDataSource = spListDataSource;
        }

        public AveListDataSource(string dataSourceXmlStr)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListDataSource.AveListDataSource"))
            {

                mListDataSource = new SPListDataSource();
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataSourceXmlStr);
                foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Attributes["Name"].Value)
                    {
                        case AveBDCProperties.LobSystemInstance:
                            mListDataSource.SetProperty(AveBDCProperties.LobSystemInstance, node.Attributes["Value"].Value);
                            break;
                        case AveBDCProperties.EntityNamespace:
                            mListDataSource.SetProperty(AveBDCProperties.EntityNamespace, node.Attributes["Value"].Value);
                            break;
                        case AveBDCProperties.Entity:
                            mListDataSource.SetProperty(AveBDCProperties.Entity, node.Attributes["Value"].Value);
                            break;
                        case AveBDCProperties.SpecificFinder:
                            mListDataSource.SetProperty(AveBDCProperties.SpecificFinder, node.Attributes["Value"].Value);
                            break;
                        default:
                            break;
                    }
                }

            }

        }

        internal SPListDataSource ListDataSource
        {
            get
            {
                return mListDataSource;
            }
        }

        #region IAveListDataSource Members

        public void SetProperty(string key, string value)
        {
            mListDataSource.SetProperty(key, value);
        }

        public string GetProperty(string key)
        {
            return mListDataSource.GetProperty(key);
        }

        public string ToXml()
        {
            return (string)AveAssemblyUtility.InvokeMethod(mListDataSource, "ToXml", new Type[] { }, new object[] { });
        }

        #endregion
    }
}
