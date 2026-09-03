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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveXmlDocumentCollection : AveAbstractCommonCollection<string>, IAveXmlDocumentCollection
    {
        private IAveRequest mRequest;
        private AveContentType mContentType;
        private HybridDictionary mDict;
        private Dictionary<string, string> mXmlDocProperties;

        public AveXmlDocumentCollection(AveContentType contentType, IAveRequest request, Dictionary<string, string> xmlDocumentsColProperties)
        {
            mContentType = contentType;
            mRequest = request;
            mXmlDocProperties = xmlDocumentsColProperties;
            mDict = new HybridDictionary();
            InitXmlDocumentCollection();
        }

        internal void InitXmlDocumentCollection()
        {
            mListData = new List<string>(mXmlDocProperties.Count);
            foreach (KeyValuePair<string, string> kv in mXmlDocProperties)
            {
                mListData.Add(kv.Value);
                mDict.Add(kv.Key, kv.Value);
            }
        }

        internal Dictionary<string, string> XmlDocumentData
        {
            get
            {
                return mXmlDocProperties;
            }
        }

        #region IAveXmlDocumentCollection Members

        public string this[string namespaceUri]
        {
            get
            {
                return mDict[namespaceUri] as string;
            }
        }

        public void Add(System.Xml.XmlDocument document)
        {
            if (!mDict.Contains(document.DocumentElement.NamespaceURI))
            {
                mDict[document.DocumentElement.NamespaceURI] = document.DocumentElement.OuterXml;
                Dictionary<string, string> addedList = null;
                if (!mContentType.DataCache.ChangedProperties.ContainsKey("AddedDocuments"))
                {
                    addedList = new Dictionary<string, string>();
                    mContentType.DataCache.ChangedProperties.Add("AddedDocuments", addedList);
                }
                else
                {
                    addedList = mContentType.DataCache.ChangedProperties["AddedDocuments"] as Dictionary<string, string>;
                }
                addedList[document.DocumentElement.NamespaceURI] = document.DocumentElement.OuterXml;
            }
        }

        public void Delete(string namespaceUri)
        {
            if (mDict.Contains(namespaceUri))
            {
                mDict.Remove(namespaceUri);
                List<string> deletedList = null;
                if (!mContentType.DataCache.ChangedProperties.ContainsKey("DeletedDocuments"))
                {
                    deletedList = new List<string>();
                    mContentType.DataCache.ChangedProperties["DeletedDocuments"] = deletedList;
                }
                else
                {
                    deletedList = mContentType.DataCache.ChangedProperties["DeletedDocuments"] as List<string>;
                }
                if (!deletedList.Contains(namespaceUri))
                {
                    deletedList.Add(namespaceUri);
                }
            }
        }

        #endregion

        internal static Dictionary<string, string> GetXmlDocumentDataFromSchemalXml(string schemalXml)
        {
            Dictionary<string, string> xmlDocumentMap = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(schemalXml))
            {
                return xmlDocumentMap;
            }
            XmlDocument document = new XmlDocument();
            document.LoadXml(schemalXml);
            XmlNode xmlDocumentsNode = document.SelectSingleNode("/ContentType/XmlDocuments");
            if (xmlDocumentsNode != null)
            {
                foreach (XmlNode node in xmlDocumentsNode.ChildNodes)
                {
                    xmlDocumentMap[node.Attributes["NamespaceURI"].Value] = node.InnerXml;
                }
            }
            return xmlDocumentMap;
        }
    }
}
