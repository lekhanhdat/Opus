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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System.Xml;

namespace AvePoint.ObjectModel.Server19
{
    class AveXmlDocumentCollection : AveAbstractCommonCollection<string>, IAveXmlDocumentCollection
    {
        private SPXmlDocumentCollection mXmlDocumentCollection;

        public AveXmlDocumentCollection(SPXmlDocumentCollection xmlDocuments)
            : base(xmlDocuments)
        {
            mXmlDocumentCollection = xmlDocuments;
        }

        #region IAveXmlDocumentCollection Members

        public void Add(XmlDocument document)
        {
            mXmlDocumentCollection.Add(document);
        }

        public void Delete(string namespaceUri)
        {
            mXmlDocumentCollection.Delete(namespaceUri);
        }

        public string this[string namespaceUri]
        {
            get { return mXmlDocumentCollection[namespaceUri]; }
        }

        #endregion

        public override string this[int index]
        {
            get
            {
                return mXmlDocumentCollection[index];
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return t;
        }

        public override int Count
        {
            get { return mXmlDocumentCollection.Count; }
        }
    }
}
