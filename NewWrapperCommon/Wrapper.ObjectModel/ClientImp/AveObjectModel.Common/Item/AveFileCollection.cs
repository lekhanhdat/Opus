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
using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Collections;

namespace AvePoint.ObjectModel.Common
{
    class AveFileCollection : AveAbstractLazyCollection<IAveFile>, IAveFileCollection,IDisposable
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveFileCollection));
        private AveWeb mWeb;
        private AveList mList;
        private AveFolder mParentFolder;
        private IAveRequest mRequest;
        private AveDocumentSerializer mDocumentSerializer;

        public AveFileCollection(IAveRequest request, IAveWeb web, IAveList list, AveFolder parentFolder, Dictionary<string, object> folderProperties)
        {
            mWeb = web as AveWeb;
            mList = list as AveList;
            mParentFolder = parentFolder;
            mRequest = request;
            base.DataCache.AddPropertyies(folderProperties);
            InitFileCollection();
        }

        public AveFileCollection(IAveRequest request, IAveWeb web, IAveList list, AveFolder parentFolder)
        {
            mWeb = web as AveWeb;
            mList = list as AveList;
            mParentFolder = parentFolder;
            mRequest = request;
        }

        protected override void InitCollection()
        {
            if (!IsCollectionInitialized)
            {
                lock (lockObject)
                {
                    //ensure flag in lock again
                    if (!IsCollectionInitialized)
                    {
                        var filesProp = this.mRequest.GetFiles(mWeb.ServerRelativeUrl, mList == null ? null : mList.Title, mParentFolder.ServerRelativeUrl);
                        DataCache.AddPropertyies(filesProp);
                        mListData = new List<IAveFile>();
                        InitFileCollection();
                        IsCollectionInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// used in lockObject
        /// </summary>
        private void InitFileCollection()
        {
            List<Dictionary<string, object>> filePropertiesList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            foreach (Dictionary<string, object> fileProperties in filePropertiesList)
            {
                AveFile file = new AveFile(mRequest, mWeb, mList, mParentFolder, fileProperties);
                mListData.Add(file);
            }
        }

        #region IAveFileCollection Member

        public IAveFile this[string urlOfFile]
        {
            get
            {
                if (string.IsNullOrEmpty(urlOfFile))
                {
                    throw new ArgumentNullException("urlOfFile");
                }
                int index = urlOfFile.LastIndexOf('/');
                string fileName = urlOfFile.Substring(index + 1);
                IAveFile resultFile= ListData.Find(
                    delegate(IAveFile file)
                    {
                        return file.Name.Equals(fileName);
                    });
                if (resultFile == null) 
                {
                    throw new Exception("File not find.");
                }
                return resultFile;
            }
        }

        public IAveFile Add(AveFileCreationInformation parameters)
        {
            return this.Add(parameters.Url, parameters.Content, parameters.Overwrite);
        }

        public IAveFile Add(string urlOfFile, AveTemplateFileType templateFileType)
        {
            Dictionary<string, object> fileProperties = mRequest.AddFile(mWeb.ServerRelativeUrl, mParentFolder.ServerRelativeUrl, urlOfFile, (int)templateFileType);
            AveFile newFile = new AveFile(mRequest, mWeb, mList, mParentFolder, fileProperties);
            ListData.Add(newFile);
            if (mList != null && newFile.Item != null)
            {
                if (mList.DataCache.IsPropertyAvailable("Items"))
                {
                    (mList.Items as AveListItemCollection).ListData.Add(newFile.Item);
                }
            }
            return newFile;
        }

        public IAveFile Add(string url, byte[] file, bool overwrite)
        {
            return this.Add(url, file, overwrite, string.Empty, false);
        }

        public IAveFile Add(string urlOrFile, Stream file, bool overwrite)
        {
            return this.Add(urlOrFile, file, overwrite, string.Empty, false);
        }

        public IAveFile Add(string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            Dictionary<string, object> fileProperties = mRequest.AddFile(mWeb.ServerRelativeUrl, mParentFolder.ServerRelativeUrl, urlOfFile, file, overwrite, checkInComment, checkRequiredFields);
            AveFile newFile = new AveFile(mRequest, mWeb, mList, mParentFolder, fileProperties);
            AddToCache(newFile);
            if (mList != null && newFile.Item != null)
            {
                if (mList.DataCache.IsPropertyAvailable("Items"))
                {
                    (mList.Items as AveListItemCollection).ListData.Add(newFile.Item);
                }
            }
            return newFile;
        }

        public IAveFile Add(string urlOfFile, Stream file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            string listName = mList != null ? mList.Title : string.Empty;
            Dictionary<string, object> fileProperties = mRequest.AddFile(mWeb.ServerRelativeUrl, mParentFolder.ServerRelativeUrl, urlOfFile, listName, file, overwrite, checkInComment, checkRequiredFields, this.mList != null ? this.mList.EnableMinorVersions : false);
            AveFile newFile = new AveFile(mRequest, mWeb, mList, mParentFolder, fileProperties);
            AddToCache(newFile);
            if (mList != null && newFile.Item != null)
            {
                if (mList.DataCache.IsPropertyAvailable("Items"))
                {
                    (mList.Items as AveListItemCollection).ListData.Add(newFile.Item);
                }
            }
            return newFile;
        }

        public IAveFile AddGhosted(string sourceFilePath, string targetFilePath, bool bIsPublishing)
        {
            throw new NotImplementedException();
        }

        public IAveFolder Folder
        {
            get
            {
                return mParentFolder;
            }
        }

        public IAveWeb Web
        {
            get
            {
                return mWeb;
            }
        }

        #endregion


        public bool ChangeContent(IAveFile file, AveDocumentInfo info)
        {
            return false;
        }

        /// <summary>
        /// Only workflow restore use now
        /// </summary>
        /// <param name="urlOfFile"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public IAveFile Add(string urlOfFile, byte[] file)
        {
            if (!urlOfFile.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !urlOfFile.StartsWith(this.mWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)) 
            {
                urlOfFile = "/" + mParentFolder.ServerRelativeUrl.Trim(new char[] { '/' }) + "/" + urlOfFile;
            }
            return this.Add(urlOfFile, file, true);
        }

        public IAveDocumentSerializer DocumentSerializer
        {
            get
            {
                if (mDocumentSerializer == null)
                {
                    mDocumentSerializer = new AveDocumentSerializer(mParentFolder, mList, mWeb, mRequest);
                }
                return mDocumentSerializer;
            }
        }

        public IAveFile AddStreamInternal(string urlOfFile, Stream stream, bool bIsMigrate, bool bIsPublish, bool bcheckRequiredProps, bool bAutoCheckoutOnInvalidData, bool bForceCreateVersion, string lockIdMatch, IAveUser createdBy, IAveUser modifiedBy, DateTime timeCreated, DateTime timeLastModified, object varProperties, string checkinComment, bool bOverwrite, Stream formatMetadata, string etagToMatch, bool bSyncUpdate, out AveVirusCheckStatus virusCheckStatus, out string virusCheckMessage, out string etagNew)
        {
            throw new NotImplementedException();
        }


        public IAveFile Add(string urlOfFile, byte[] file, Hashtable properties, IAveUser createdBy, IAveUser modifiedBy, DateTime timeCreated, DateTime timeLastModified, bool overwrite)
        {
            return this.Add(urlOfFile, file, overwrite);
        }

        public void Dispose()
        {
            
        }
    }
}
