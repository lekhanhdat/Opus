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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
namespace AvePoint.ObjectModel.Common
{
    class AveFolderCollection : AveAbstractCommonCollection<IAveFolder>, IAveFolderCollection
    {
        private AveWeb mWeb;
        private AveList mList;
        private AveFolder mParentFolder;
        private IAveRequest mRequest;

        public AveFolderCollection(IAveRequest request, IAveWeb web, IAveList list, AveFolder parentFolder, Dictionary<string, object> folderProperties)
        {
            mWeb = web as AveWeb;
            mList = list as AveList;
            mParentFolder = parentFolder;
            mRequest = request;
            base.DataCache.AddPropertyies(folderProperties);
            InitFolderCollection();
        }

        internal void InitFolderCollection()
        {
            mListData = new List<IAveFolder>();
            var folderPropertiesList = base.DataCache.GetChildren();
            foreach (var folderProperties in folderPropertiesList)
            {
                AveFolder folder = new AveFolder(mRequest, mWeb, mList, mParentFolder, folderProperties);
                mListData.Add(folder);
            }
        }

        #region IAveFolderCollection Member

        public new IAveFolder this[int index]
        {
            get
            {
                return mListData[index];
            }
        }
        public IAveFolder this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }

        public void Add(IAveFolder folder)
        {
            mListData.Add(folder);
        }

        public IAveFolder Add(string url)
        {
            Dictionary<string, object> folderProperties = mRequest.AddFolder(mWeb.ServerRelativeUrl, mParentFolder.ServerRelativeUrl, url);
            AveFolder folder = new AveFolder(mRequest, mWeb, mList, mParentFolder, folderProperties);
            mListData.Add(folder);
            return folder;
        }
        public IAveFolder GetByName(string folderName)
        {
            return this.GetByName(folderName, true);
        }


        private IAveFolder GetByName(string folderName, bool throwable)
        {
            IAveFolder resultFolder = mListData.Find(
                    delegate(IAveFolder folder)
                    {
                        return folder.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase);
                    });
            if (resultFolder == null && throwable)
            {
                throw new Exception("folder:" + folderName + " not find");
            }
            return resultFolder;
        }
        #endregion



        public IAveWeb Web
        {
            get { throw new NotImplementedException(); }
        }

        //public System.Collections.IEnumerator GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}
        

        public IAveDocumentSet CreateDocumentSet(string name, IAveContentTypeId contentTypeId, Hashtable properties)
        {
            Dictionary<string, object> folderInfo = mRequest.AddDocumentSet(mWeb.ServerRelativeUrl, mList.Title, mList.ID, mParentFolder.ServerRelativeUrl, name, contentTypeId);
            IAveDocumentSet documentSet = new AveDocumentSet(mRequest, new AveFolder(mRequest, mWeb, mList, mParentFolder, folderInfo));
            return documentSet;
        }

        public IAveDocumentSet CreateDocumentSet(string name, Hashtable properties)
        {
            throw new NotImplementedException();
        }
    }
}
