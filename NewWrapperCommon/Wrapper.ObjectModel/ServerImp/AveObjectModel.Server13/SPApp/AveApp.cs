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

using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace AvePoint.ObjectModel.Server13
{
    public class AveApp : IAveApp
    {
        internal SPApp App;

        #region Methods
        public AveApp(SPApp app)
        {
            App = app;
        }
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo")]
        public Guid CreateAppInstance(IAveWeb web)
        {
            return App.CreateAppInstance((web as AveWeb).Web);
        }

        /// <summary>
        /// we change the SqlStream To MemoryStream
        /// </summary>
        /// <returns></returns>
        public Stream GetPackage()
        {
            MemoryStream mStream = new MemoryStream();
            object obj = AveAssemblyUtility.InvokeMethod(App, typeof(SPApp), "GetPackage", null, null);
            (obj as Stream).CopyTo(mStream);
            mStream.Seek(0, SeekOrigin.Begin);
            return mStream;
        }

        [SuppressMessage("FxCopCustomRules","C100007:SpellCheckStringValues", Justification = "The wrong word is method name.")]
        /// <summary>
        /// 反射实现GetPackage方法，因为方法中有New SPSite过程，PRItem调用会有问题
        /// </summary>
        /// <returns></returns>
        public Stream GetPackageForPRItem13(IAveWeb web)
        {
            MemoryStream mStream = new MemoryStream();
            AveAssemblyUtility.InvokeStaticMethod("Microsoft.SharePoint.Administration.SPAppAdministrationSecurityContext", "DemandPackageManagementRights", new object[] { });
            AveQuerySession sqlSession = (web as AveWeb).Site.SqlSession as AveQuerySession;
            object obj = AveAssemblyUtility.InvokeStaticMethod("Microsoft.SharePoint.Lifecycle.SprocWrappers", "ReadPackage", new object[] { sqlSession.SqlSession, Fingerprint, web.Site.ID });
            (obj as Stream).CopyTo(mStream);
            mStream.Seek(0, SeekOrigin.Begin);
            return mStream;
        }

        #endregion

        #region Properties
        public Guid ProductId
        {
            get { return App.ProductId; }
        }

        public Guid SiteId
        {
            get { throw new NotImplementedException(); }
        }

        public string VersionString
        {
            get { return App.VersionString; }
        }

        public AveAppSource Source
        {
            get { return (AveAppSource)App.Source; }
        }

        public bool IsUpdateAvailable
        {
            get
            {
                object obj = AveAssemblyUtility.GetPropertyValue(App, "IsUpdateAvailable");
                return obj != null ? Convert.ToBoolean(obj) : false;
            }
        }

        public Guid SourceInfoId
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetPropertyValue(App, "SourceInfoId");
            }
        }

        public string AppManifest
        {
            get
            {
                byte[] appFingerprint = (byte[])AveAssemblyUtility.InvokeMethod(App, "GetFingerprint", null, null);
                Guid siteId = (Guid)AveAssemblyUtility.GetPropertyValue(App, "SiteId");
                
                return string.Empty;
            }
        }
        public byte[] GetFingerprint()
        {
            if (Fingerprint != null)
            {
                return (byte[])Fingerprint.Clone();
            }
            return null;
        }
        #endregion



        public byte[] Fingerprint
        {
            get { return (byte[])AveAssemblyUtility.GetFieldValue(App, "fingerprint"); }
        }

        #region IAveApp Members


        public string AssetId
        {
            get { return App.AssetId; }
        }

        public string ContentMarket
        {
            get { return App.ContentMarket; }
        }

        #endregion
    }
}
