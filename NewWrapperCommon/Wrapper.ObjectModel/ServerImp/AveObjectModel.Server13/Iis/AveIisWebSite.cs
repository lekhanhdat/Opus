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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using AvePoint.Common;
using System.Globalization;

namespace AvePoint.ObjectModel.Server13
{
    class AveIisWebSite : AveMetabaseObject, IAveIisWebSite
    {
        private SPIisWebSite mIisWebSite;

        public AveIisWebSite(SPIisWebSite iisWebSite)
            : base(iisWebSite)
        {
            mIisWebSite = iisWebSite;
        }

        public AveIisWebSite(int instanceId)
            : this(new SPIisWebSite(instanceId))
        { }

        /// <summary>
        /// This Construction Method is just for Static Method
        /// </summary>
        public AveIisWebSite()
        { }

        #region IAveIisWebSite Members

        public string[] SecureBindings
        {
            get
            {
                return (string[])AveAssemblyUtility.GetPropertyValue(mIisWebSite, "SecureBindings");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mIisWebSite, "SecureBindings", value);
            }
        }

        public string[] ServerBindings
        {
            get
            {
                return mIisWebSite.ServerBindings;
            }
            set
            {
                mIisWebSite.ServerBindings = value;
            }
        }

        public int InstanceId
        {
            get { return mIisWebSite.InstanceId; }
        }

        public string ServerComment
        {
            get
            {
                return mIisWebSite.ServerComment;
            }
            set
            {
                mIisWebSite.ServerComment = value;
            }
        }

        public int GetUnusedInstanceId(int preferredInstanceId)
        {
            return (int)AveAssemblyUtility.InvokeStaticMethod(typeof(SPIisWebSite), "GetUnusedInstanceId", new object[] { preferredInstanceId });
        }

        public bool LookupByServerComment(string serverComment, out int instanceId)
        {
            instanceId = -1;
            Type refType = instanceId.GetType().MakeByRefType();
            object[] paramObjs = new object[] { serverComment, instanceId };
            object retObj =AveAssemblyUtility.InvokeStaticMethod(typeof(SPIisWebSite), "LookupByServerComment", new Type[] { typeof(string), refType }, paramObjs); 
            instanceId = (int)paramObjs[1];
            return (bool)retObj;
        }

        public Uri GetUriFromBinding(string binding, bool secure)
        {
            string machineName;
            string[] strArray = binding.Split(":".ToCharArray());
            int num = int.Parse(strArray[1], NumberFormatInfo.InvariantInfo);
            if (!string.IsNullOrEmpty(strArray[2]))
            {
                machineName = strArray[2];
            }
            else if (!string.IsNullOrEmpty(strArray[0]))
            {
                machineName = strArray[0];
            }
            else
            {
                machineName = Environment.MachineName;
            }
            if (Uri.CheckHostName(machineName) == UriHostNameType.Unknown)
            {
                machineName = Environment.MachineName;
            }
            return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}://{1}:{2}", new object[] { secure ? Uri.UriSchemeHttps : Uri.UriSchemeHttp, machineName, num }));
        }

        public IAveIisVirtualDirectory RootVirtualDirectory
        {
            get
            {
                return new AveIisVirtualDirectory(AveAssemblyUtility.GetPropertyValue(mIisWebSite, "RootVirtualDirectory") as SPMetabaseObject);
            }
        }
        #endregion
    }
}
