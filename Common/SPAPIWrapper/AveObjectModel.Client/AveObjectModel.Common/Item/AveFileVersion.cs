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
using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;

namespace AvePoint.ObjectModel.Common
{
    class AveFileVersion : AveClientObject, IAveFileVersion
    {
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private AveFileVersionCollection mFileVersionCollection;

        public AveFileVersion(AveWeb parentWeb, AveFileVersionCollection versionCollection, IAveRequest request, IDictionary<string, object> versionProperties)
        {
            mRequest = request;
            mParentWeb = parentWeb;
            mFileVersionCollection = versionCollection;
            base.DataCache.AddPropertyies(versionProperties);
        }        

        public string CheckInComment 
        {
            get
            {
                return base.DataCache.GetProperty<string>("CheckInComment");
            } 
        }
        public DateTime Created
        {
            get
            {   
                return base.DataCache.GetProperty<DateTime>("Created");
            } 
        }
        
        public IAveUser CreatedBy
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("CreatedBy"))
                {
                    string loginName = base.DataCache.GetProperty<string>("CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix);
                    if (string.IsNullOrEmpty(loginName))
                    {
                        return null;
                    }
                    AveUser createBy = this.mParentWeb.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.AddProperty("CreatedBy",createBy);
                }
                return base.DataCache.GetProperty<IAveUser>("CreatedBy");
            }
        }
        public int ID
        {
            get
            {
                return base.DataCache.GetProperty<int>("ID");
            } 
        }
        public bool IsCurrentVersion
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsCurrentVersion");
            }
        }

        /// <summary>
        /// client api为了支持大于2G的文件，添加了long型的Length属性
        /// </summary>
        public long Size
        {
            get
            {                
                return base.DataCache.GetProperty<long>("Length");
            }
        }

        public string Url
        {
            get
            {
                //site relative url
                return base.DataCache.GetProperty<string>("Url");
            }
        }
        public string VersionLabel
        {
            get
            {
                return base.DataCache.GetProperty<string>("VersionLabel");
            }
        }
        public long StreamLength
        {
            get
            {
                return base.DataCache.GetProperty<long>("StreamLength");
            }
        }
        public void Delete()
        {
            this.mFileVersionCollection.DeleteByID(this.ID);
        }
        public void Recycle()
        {
            this.mFileVersionCollection.RecycleByID(this.ID);
        }
        /// <summary>
        /// it's a read only stream, that should be closed after used
        /// </summary>
        /// <returns>return a connect stream</returns>
        public Stream OpenBinaryStream()
        {
            var stream = mRequest.GetFileVersionStream(this.mParentWeb.ServerRelativeUrl, this.mFileVersionCollection.File.ServerRelativeUrl, this.Url, this.ID, this.mFileVersionCollection.File.UniqueId);
            ThrowIfFileContentDismatch(stream.Length);
            return stream;
        }

        #region IAveFileVersion Members


        public byte[] OpenBinary()
        {
            Stream streamData = OpenBinaryStream();
            long length = streamData.Length;
            byte[] resultBytes = new byte[streamData.Length];

            if (streamData.CanSeek)
                streamData.Seek(0, SeekOrigin.Begin);
            streamData.ReadExInternal(resultBytes, 0, (int)length);
            if (streamData.CanSeek)
                streamData.Seek(0, SeekOrigin.Begin);
            return resultBytes;
        }

        #endregion


        public AveFileLevel Level
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Hashtable Properties
        {
            get { return null; }
        }

        /// <summary>
        /// SAAS-37060
        /// Compare discover length with rest api stream length
        /// </summary>
        /// <param name="contentLength"></param>
        private void ThrowIfFileContentDismatch(long contentLength)
        {
            if (contentLength < this.Size)
            {
                mLogger.Error($"ThrowIfFileContentDismatch, discover length:{this.Size}, rest api result length:{contentLength}, url:{this.Url}");
                throw new AveWrapperException(string.Format(WrapperRestoreReportResource.FileContentLengthDismatch, this.Url));
            }
        }
    }
}
