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
    class AveFieldLinkCollection : AveAbstractCommonCollection<IAveFieldLink>, IAveFieldLinkCollection
    {
        private IAveRequest mRequest;
        private AveContentType mContentType;
        public AveFieldLinkCollection(IAveRequest request, AveContentType contentType, IDictionary<string, object> fieldLinkColProperties)
        {
            mRequest = request;
            mContentType = contentType;
            base.DataCache.AddPropertyies(fieldLinkColProperties);
            InitAveFieldLinkCollection();
        }
        internal void InitAveFieldLinkCollection()
        {
            var fieldLinkList = base.DataCache.GetChildren();
            mListData = new List<IAveFieldLink>();
            foreach(var fieldLinkProperties in fieldLinkList )
            {
                AveFieldLink fieldLink = new AveFieldLink(this.mContentType,this,this.mRequest,fieldLinkProperties);                
                mListData.Add(fieldLink);
            }
        }
        public IAveFieldLink this[int index]
        {
            get 
            {
                return mListData[index];
            }
        }
        public IAveFieldLink this[Guid id]
        {
            get
            {
                return mListData.Find(f => f.ID.Equals(id));
            }
        }
        public IAveFieldLink this[string name] 
        {
            get
            {                
                return mListData.Find(f => string.Compare(f.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
            }
        }
        
        public void Add(IAveFieldLink fieldLink)
        {
            if (!this.mContentType.DataCache.ChangedProperties.ContainsKey("AddFieldLink"))
            {
                List<Dictionary<string, object>> newFieldLink = new List<Dictionary<string, object>>();
                newFieldLink.Add(((AveFieldLink)fieldLink).DataCache.ChangedProperties["AddFieldLink"] as Dictionary<string, object>);
                this.mContentType.DataCache.AddChangedProperty("AddFieldLink", newFieldLink);
            }
            else
            {
                ((List<Dictionary<string, object>>)this.mContentType.DataCache.ChangedProperties["AddFieldLink"]).Add(((AveFieldLink)fieldLink).DataCache.ChangedProperties["AddFieldLink"] as Dictionary<string, object>);
            }
            mListData.Add(fieldLink);
        }
        public void Delete(Guid id)
        {
            AveFieldLink fieldLink = this[id] as AveFieldLink;
            if (fieldLink != null)
            {
                fieldLink.Delete();
                mListData.Remove(fieldLink);
            }
        }
        public void Delete(string fieldName)
        {
            AveFieldLink fieldLink = this[fieldName] as AveFieldLink ;
            if (fieldLink != null)
            {
                fieldLink.Delete();
                mListData.Remove(fieldLink);
            }
        }
        public void Reorder(string[] fieldlinks)
        {
            List<string> fieldLinklist = new List<string>();
            foreach (string str in fieldlinks)
            {
                fieldLinklist.Add(str);
            }
            this.mContentType.fieldLinksOrder = fieldlinks;
            this.mContentType.DataCache.AddChangedProperty("Reorder", fieldLinklist);
        }
    }
}
