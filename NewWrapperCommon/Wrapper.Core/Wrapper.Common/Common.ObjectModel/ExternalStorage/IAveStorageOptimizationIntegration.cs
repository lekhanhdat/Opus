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
using System.Text;
using System.Data.SqlClient;
using System.IO;

using AvePoint.Common;

namespace AvePoint.Wrapper.Common
{
    public interface IAveStorageOptimizationIntegration
    {
        /// <summary>
        /// 初始化Restore对象
        /// </summary>
        /// <param name="sqlConn">对应的数据库连接，直接用即可，不用处理Open/Close.</param>
        /// <param name="isHighestVersion">Connector专用，表示是否是最高版本</param>
        /// <returns>数据初始化和判断逻辑，成功返回null，失败返回出错原因，不抛出异常</returns>
        string InitializeForRestoreExtender(Guid siteId, Guid itemId, int? internalVersion, int level, int size, AveSqlConnection sqlConn);

        /// <summary>
        ///将真实数据转化为EBS Stub或者RBS ID，并把真实内容放到对应的Device上。同时更新到AllDocStream表中。
        /// </summary>
        ///<param name="content">要还原的数据，如果是byte[]，可以转化为MemoryStream，
        ///这个Stream外围会做处理，如果小于临界值（目前定默认值为50M），我们会发MemoryStream，
        ///否则我们会存到临时文件中发FileStream，出错时请不要Close这个Stream。</param>
        /// <returns>还原是否成功, null表示成功，否则为出错原因，不抛出异常</returns>
        string Restore(Stream content);

        void InitializeForBackup(Guid webApplicationId, string webApplicationURL, Guid contentDBId, Guid siteId, Guid itemId, int nUIVersion, int nLevel, string contentDBConnectionString);
        void InitializeForRestore(Guid webApplicationId, string webApplicationURL, Guid contentDBId);
        string BackupStubDBInfo();
        bool RestoreStubDBInfo(string info);
    }

    public interface IAveStorageOptimizationIntegration13
    {
        /// <summary>
        /// 初始化Restore对象
        /// </summary>
        /// <param name="sqlConn">对应的数据库连接，直接用即可，不用处理Open/Close.</param>
        /// <param name="isHighestVersion">Connector专用，表示是否是最高版本</param>
        /// <returns>数据初始化和判断逻辑，成功返回null，失败返回出错原因，不抛出异常</returns>
        string InitializeForRestoreExtender(Guid siteId, Guid itemId, int? internalVersion, int level, int size, AveSqlConnection sqlConn);

        /// <summary>
        ///将真实数据转化为EBS Stub或者RBS ID，并把真实内容放到对应的Device上。同时更新到AllDocStream表中。
        /// </summary>
        ///<param name="content">要还原的数据，如果是byte[]，可以转化为MemoryStream，
        ///这个Stream外围会做处理，如果小于临界值（目前定默认值为50M），我们会发MemoryStream，
        ///否则我们会存到临时文件中发FileStream，出错时请不要Close这个Stream。</param>
        /// <returns>还原是否成功, null表示成功，否则为出错原因，不抛出异常</returns>
        string Restore(Stream content);

        void InitializeForBackup(Guid webApplicationId, string webApplicationURL, Guid contentDBId, Guid siteId, Guid itemId, int nUIVersion, int nLevel, string contentDBConnectionString,byte partition, long BSN);
        void InitializeForRestore(Guid webApplicationId, string webApplicationURL, Guid contentDBId);
        string BackupStubDBInfo();
        bool RestoreStubDBInfo(string info);
    }

    public class AveSPItemNativeInfo
    {
        #region Private Members
        private Guid mSiteId;
        private Guid mWebId;
        private Guid mItemId;
        private int? mInternalVersion;
        private int mLevel;
        private int mSize;
        private Guid mWebApplicationId;
        private Guid mContentDBId;
        private string mWebApplicationURL;
        private int mUIVersion;
        private string mContentDBConnectionString;
        private IAveFile mSPFile;
        private IAveFolder mSPFolder;
        #endregion

        #region Public Properties
        public Guid SiteId
        {
            get { return mSiteId; }
        }

        public Guid WebId
        {
            get { return mWebId; }
        }

        public Guid ItemId
        {
            get { return mItemId; }
        }

        //public Guid Id;
        public Guid WebApplicationId
        {
            get { return mWebApplicationId; }
        }

        public Guid ContentDBId
        {
            get { return mContentDBId; }
        }

        public int? InternalVersion
        {
            get { return mInternalVersion; }
        }

        public int Level
        {
            get { return mLevel; }
        }

        public int Size
        {
            get { return mSize; }
            set { mSize = value; }
        }
        /// <summary>
        /// 对于Connector而言，File始终不会为空，对于Extender而言，Attachment时这个对象就为空了。
        /// </summary>
        /// 
        public IAveFile File
        {
            get { return mSPFile; }
        }

        public IAveFolder Folder
        {
            get { return mSPFolder; }
        }

        #endregion

        public AveSPItemNativeInfo(Guid siteId, Guid webId, Guid id, int? internalVersion, int level, int size, IAveFile file, IAveFolder folder)
        {
            mSiteId = siteId;
            mWebId = webId;
            mItemId = id;
            mInternalVersion = internalVersion;
            mLevel = level;
            mSize = size;
            mSPFile = file;
            mSPFolder = folder;
        }

        public AveSPItemNativeInfo(Guid siteId, Guid webId, Guid id, int? internalVersion, int level, int size, IAveFile file)
        {
            mSiteId = siteId;
            mWebId = webId;
            mItemId = id;
            mInternalVersion = internalVersion;
            mLevel = level;
            mSize = size;
            mSPFile = file;
            //Folder = folder;
        }
    }
}
