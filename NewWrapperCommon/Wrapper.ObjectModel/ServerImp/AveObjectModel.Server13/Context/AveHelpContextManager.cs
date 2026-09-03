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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveHelpContextManager : IAveHelpContextManager
    {
        private object mHelpContextManager;
        private const string mHelpContextManager_Type = "Microsoft.SharePoint.Help.HelpContextManager";

        internal object HelpContextManager
        {
            get
            {
                return mHelpContextManager;
            }
        }

        public AveHelpContextManager(object helpContextManager)
        {
            mHelpContextManager = helpContextManager;
        }

        public AveHelpContextManager()
        {
            mHelpContextManager = AveAssemblyUtility.CreateInstance(mHelpContextManager_Type);
        }

        #region IAveHelpContextManager Members

        public string[] GetSiteDisabledHelpCollections(IAveSite site)
        {
            return (string[])AveAssemblyUtility.InvokeStaticMethod(mHelpContextManager.GetType(), "GetSiteDisabledHelpCollections", ((AveSite)site).Site);
        }

        public string[] GetSiteEnabledHelpCollections(IAveSite site)
        {
            return (string[])AveAssemblyUtility.InvokeStaticMethod(mHelpContextManager.GetType(), "GetSiteEnabledHelpCollections", ((AveSite)site).Site);
        }

        public string ContextWebHelpUrl
        {
            get
            {
                return (string)AveAssemblyUtility.GetStaticPropertyValue(mHelpContextManager.GetType(), "ContextWebHelpUrl");
            }
            set
            {
                AveAssemblyUtility.SetStaticPropertyValue(mHelpContextManager.GetType(), "ContextWebHelpUrl", value);
            }
        }

        public string ProductHelpLibraryUrl
        {
            get { return (string)AveAssemblyUtility.GetStaticPropertyValue(mHelpContextManager.GetType(), "ProductHelpLibraryUrl"); }
        }

        public bool IsValidHelpLibraryUrl(Uri helpLibraryUrl)
        {
            return (bool)AveAssemblyUtility.InvokeStaticMethod(mHelpContextManager.GetType(), "IsValidHelpLibraryUrl", helpLibraryUrl);
        }

        public void SetSiteDisabledHelpCollections(IAveSite site, string[] disabledHelpCollections)
        {

            AveAssemblyUtility.InvokeStaticMethod(mHelpContextManager.GetType(), "SetSiteDisabledHelpCollections", ((AveSite)site).Site, disabledHelpCollections);
        }

        public void SetSiteEnabledHelpCollections(IAveSite site, string[] enabledHelpCollections)
        {
            AveAssemblyUtility.InvokeStaticMethod(mHelpContextManager.GetType(), "SetSiteEnabledHelpCollections", ((AveSite)site).Site, enabledHelpCollections);
        }

        #endregion

        public Dictionary<string, string> AvailableCollections
        {
            get { throw new NotImplementedException(); }
        }
    }
}
