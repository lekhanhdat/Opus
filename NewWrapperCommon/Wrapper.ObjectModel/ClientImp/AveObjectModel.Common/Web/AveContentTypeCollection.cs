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
    class AveContentTypeCollection : AveAbstractCommonCollection<IAveContentType>, IAveContentTypeCollection
    {
        public AveContentTypeCollection()
        {
            mListData = new List<IAveContentType>();
        }

        public void Add(AveContentType contentType)
        {
            mListData.Add(contentType);
        }
        public new int Count
        {
            get
            {
                return base.Count;
            }
        }
        public new void CopyTo(Array array, int index)
        {
            base.CopyTo(array, index);
        }
        public new bool IsSynchronized
        {
            get
            {
                return base.IsSynchronized;
            }
        }
        public new object SyncRoot
        {
            get
            {
                return base.SyncRoot;
            }
        }

        #region IAveContentType Members
        public IAveContentType this[IAveContentTypeId contentTypeId]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveContentType contentType)
                    {
                        return contentType.Id.Equals(contentTypeId);
                    });
            }
        }
        public IAveContentType this[string name]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveContentType contentType)
                    {
                        return contentType.Name.Equals(name);
                    });
            }
        }
        public IAveContentType Add(AveContentTypeCreationInformation contentTypeCreationInfo)
        {
            throw new NotImplementedException();
        }
        public IAveContentType Add(IAveContentType contentType)
        {
            throw new NotImplementedException();
        }
        public IAveContentType AddExistingContentType(IAveContentType contentType)
        {
            throw new NotImplementedException();
        }
        public IAveContentTypeId BestMatch(IAveContentTypeId contentTypeId)
        {
            return GetById(contentTypeId.ToString()).Id;
        }
        public IAveContentType GetById(string contentTypeId)
        {
            return mListData.Find(
                delegate(IAveContentType contentType)
                {
                    return contentTypeId.Equals(contentType.Id.ToString());
                });
        }
        #endregion

        #region IAveContentTypeCollection Members


        public AveContentTypeCollectionInfo GetContentTypeInfos(bool backupParent)
        {
            throw new NotImplementedException();
        }

        public string GetContentTypeName(Guid siteId, byte[] contentTypeId)
        {
            throw new NotImplementedException();
        }

        public List<byte[]> GetParentContentTypeIdList(string id)
        {
            throw new NotImplementedException();
        }

        public List<AveContentTypeFileInfo> GetResources(Guid siteId, string folderUrl)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
