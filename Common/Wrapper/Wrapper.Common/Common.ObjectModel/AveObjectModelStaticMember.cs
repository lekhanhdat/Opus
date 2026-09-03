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




//using System;
//using System.Collections.Generic;
//using System.Text;
//using AvePoint.GCommon;

//namespace AvePoint.Wrapper.Common
//{
//    public delegate void RunWithElevatedPrivileges(CodeToRunWithElevated code);

//    public static class AveObjectModelStaticMember
//    {
//        private const string ClientAssemblyName = "AveObjectModel.Client";
//        private const string ClientNameSpace = "AvePoint.ObjectModel.Client.";
//        private const string ServerAssemblyName = "SP2010WrapperServer";
//        private const string ServerNameSpace = "AvePoint.ObjectModel.Server.";

//        public static IAveContext mCurrentContext;
//        public static IAveAdministrationWebApplication mAveAdministrationWebApp;
//        public static IAveFarm mFarm;
//        public static IAveServer mServer;
//        public static RunWithElevatedPrivileges mRunWithElevated;
//        public static IAveWebService mContentService;

//        public static IAveWebService ContentService
//        {
//            get 
//            {
//                return mContentService; 
//            }
//        }

//        public static IAveContext ContextCurrent
//        {
//            get
//            {                
//                return mCurrentContext;
//            }
//        }

//        public static IAveAdministrationWebApplication AdministrationWebAppLocal
//        {
//            get
//            {
//                return mAveAdministrationWebApp;
//            }
//        }

//        public static IAveFarm FarmLocal
//        {
//            get
//            {
//                return mFarm;
//            }
//        }

//        public static IAveServer ServerLocal
//        {
//            get
//            {
//                return mServer;
//            }
//        }

//        public static IAveWebApplication Lookup(Uri uri)
//        {
//            return AveAssemblyUtility.InvokeMethod(null, AveAssemblyUtility.GetType(AveObjectModelFactory.ServerAssemblyName, AveObjectModelFactory.ServerNameSpace + "AveWebApplication"), "Lookup", new object[] { uri }) as IAveWebApplication;
//        }

//        public static IAveView GetView(Guid viewId)
//        {
//            return AveAssemblyUtility.InvokeMethod(null, AveAssemblyUtility.GetType(AveObjectModelFactory.ServerAssemblyName, AveObjectModelFactory.ServerNameSpace + "AveList"), "GetView", new object[] { viewId }) as IAveView;
//        }

//        public static IAveNavigationNode CreateSPNavigationNode(string name, string url, AveNodeTypes nodeType, IAveNavigationNodeCollection collection)
//        {
//            return null;
//        }

//        public static void RunWithElevatedPrivileges(CodeToRunWithElevated secureCode)
//        {
//            mRunWithElevated(secureCode);
//        }

//        public static IAveWebApplication Lookup(Uri uri, AveContextKind kind)
//        {
//            IAveWebApplication aveWebApplication;
//            switch (kind)
//            { 
//                case AveContextKind.ServerObjectModel:
//                    aveWebApplication = AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AveWebApplication"), "Lookup", new object[] { uri }) as IAveWebApplication;
//                    break;
//                default:
//                    throw new NotSupportedException();
//            }
//            return aveWebApplication;
//        }

//        public static bool IsPublishingWeb(IAveWeb web, AveContextKind kind)
//        {
//            bool result;
//            switch (kind)
//            { 
//                case AveContextKind.ServerObjectModel:
//                    result = (bool)AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingWeb"), "GetPublishingWeb", new object[] { web });
//                    break;
//                default:
//                    throw new NotSupportedException();
//            }
//            return result;
//        }

//        public static Guid GetPagesListId(IAveWeb web, AveContextKind kind)
//        {
//            Guid guid;
//            switch (kind)
//            { 
//                case AveContextKind.ServerObjectModel:
//                    guid = (Guid)AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingWeb"), "GetPagesListId", new object[] { web });
//                    break;
//                default:
//                    throw new NotSupportedException();
//            }
//            return guid;

//        }

//        public static IAvePublishingWeb GetPublishingWeb(IAveWeb web, AveContextKind kind)
//        {
//            IAvePublishingWeb avePublishingWeb;
//            switch (kind)
//            {
//                case AveContextKind.ServerObjectModel:
//                    avePublishingWeb = AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingWeb"), "GetPublishingWeb", new object[] { web }) as IAvePublishingWeb;
//                    break;
//                default:
//                    throw new NotSupportedException();
//            }
//            return avePublishingWeb;

//        }
//        public static IAvePublishingSite GetPublishingSite(IAveSite site, AveContextKind kind)
//        {
//            IAvePublishingSite avePublishingSite;
//            switch (kind)
//            {
//                case AveContextKind.ServerObjectModel:
//                    avePublishingSite = AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "IAvePublishingSite"), "GetPublishingSite", new object[] { site }) as IAvePublishingSite;
//                    break;
//                default:
//                    throw new NotSupportedException();
//            }
//            return avePublishingSite;

//        }
//        public static bool IsPublishingSite(IAveSite site, AveContextKind kind)
//        {
//            bool result;
//            switch (kind)
//            {
//                case AveContextKind.ServerObjectModel:
//                    result = (bool)AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingSite"), "GetPublishingSite", new object[] { site });
//                    break;
//                default:
//                    throw new NotSupportedException();
//            }
//            return result;
//        }
//        public static void UnlockItem(IAveListItem item)
//        {
//            AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingSite"), "UnlockItem", new object[] { item });
//        }

//        public static void LockItem(IAveListItem holdItem, IAveListItem item, string comment)
//        {
//            AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingSite"), "LockItem", new object[] { holdItem, item, comment });
//        }

//        public static IAveList GetHoldsList(IAveWeb web)
//        {
//           return  AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingSite"), "GetHoldsList", new object[] { web }) as IAveList;
//        }

//        public static void SetSiteLockProperty(IAveSite site)
//        {
//            AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingSite"), "SetSiteLockProperty", new object[] { site });
//        }

//        public static void ProvisionWeb(IAveWeb web)
//        {
//            AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingSite"), "ProvisionWeb", new object[] { web });
//        }

//        public static void ProvisionList(IAveList list)
//        {
//            AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(ServerAssemblyName, ServerNameSpace + "AvePublishingSite"), "ProvisionList", new object[] { list });
//        }
//    }
//}
