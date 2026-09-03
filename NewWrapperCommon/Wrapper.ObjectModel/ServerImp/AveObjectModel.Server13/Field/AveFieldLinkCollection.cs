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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveFieldLinkCollection : AveAbstractCommonCollection<IAveFieldLink>, IAveFieldLinkCollection
    {
        private SPFieldLinkCollection mFieldLinkCollection;

        public AveFieldLinkCollection(SPFieldLinkCollection fieldLinkCollection)
            : base(fieldLinkCollection)
        {
            mFieldLinkCollection = fieldLinkCollection;
        }

        #region IAveFieldLinkCollection Members

        public IAveFieldLink this[Guid id]
        {
            get
            {
                SPFieldLink fieldLink = mFieldLinkCollection[id];
                if (fieldLink == null)
                {
                    return null;
                }
                return new AveFieldLink(mFieldLinkCollection[id]);
            }
        }

        public IAveFieldLink this[string name]
        {
            get
            {
                SPFieldLink fieldLink = mFieldLinkCollection[name];
                if (fieldLink == null)
                {
                    return null;
                }
                return new AveFieldLink(fieldLink);
            }
        }

        public void Add(IAveFieldLink fieldLink)
        {
            mFieldLinkCollection.Add((fieldLink as AveFieldLink).FieldLink);
        }

        public void Delete(Guid id)
        {
            mFieldLinkCollection.Delete(id);
        }

        public void Delete(string fieldName)
        {
            mFieldLinkCollection.Delete(fieldName);
        }

        public void Reorder(string[] fieldlinks)
        {
            mFieldLinkCollection.Reorder(fieldlinks);
        }

        #endregion

        public override IAveFieldLink this[int index]
        {
            get
            {
                SPFieldLink fieldLink = mFieldLinkCollection[index];
                if (fieldLink == null)
                {
                    return null;
                }
                return new AveFieldLink(fieldLink);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveFieldLink(t as SPFieldLink);
        }

        public override int Count
        {
            get { return mFieldLinkCollection.Count; }
        }
        public bool IsDirty
        {
            get
            {
                object result = AveAssemblyUtility.GetPropertyValue(mFieldLinkCollection, "IsDirty");
                return result != null ? (bool)result : false;
            }
        }
    }
}
