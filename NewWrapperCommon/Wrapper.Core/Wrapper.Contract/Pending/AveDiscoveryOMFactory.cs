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
using AvePoint.Wrapper.Discovery;
using System;

namespace AvePoint.Wrapper.Contract
{
    public abstract class AveDiscoveryOMFactory
    {
        internal const string DiscoveryAssemblyName = "AgentCommonWrapperDiscovery";
        internal const string DiscoveryTypeName = "AvePoint.Wrapper.Discovery.AveWrapperDiscoveryOMFactory";

        public abstract IAveDiscoverFilterUtility FilterUtility { get; }

        public static AveDiscoveryOMFactory CreateDiscoveryOMFactory()
        {
            return AveAssemblyUtility.CreateInstance(DiscoveryAssemblyName, DiscoveryTypeName) as AveDiscoveryOMFactory;
        }

        #region =============================Site=============================
        #region Full Discovery
        public abstract IAveDiscoverSite CreateDiscoverSite(IAveSite aveSite, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module);
        
        public abstract IAveDiscoverSite CreateDiscoverSite(IAveSite aveSite, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory);

        #endregion
        public abstract IAveDiscoverWebApp CreateDiscoverWebApp(IAveWebApplication webApp);
        #region Incremental Discovery
        public abstract IAveDiscoverSite CreateDiscoverSite(IAveSite aveSite, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module, DateTime startTime, DateTime endTime);
        
        public abstract IAveDiscoverSite CreateDiscoverSite(IAveSite aveSite, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory, DateTime startTime, DateTime endTime);

        #endregion
        #endregion

        #region =============================Web=============================
        public abstract IAveDiscoverWeb CreateDiscoverWeb();
        
        public abstract IAveDiscoverWeb CreateDiscoverWeb(IAveSite site, string webRelativeUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory);
        
        
        public abstract IAveDiscoverWeb CreateDiscoverWeb(IAveSite site, string webRelativeUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory);
        //Not used, any question please contact Qinglong.Luo
        //public abstract IAveDiscoverWeb CreateDiscoverWeb(IAveDiscoverFilterBase parent);
        #endregion

        #region =============================List=============================
      
        public abstract IAveDiscoverList CreateDiscoverList(IAveSite site, Guid webId, string listRootFolderUrl, DiscoverModule module,AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory);
        public abstract IAveDiscoverList CreateDiscoverList(IAveSite site, Guid webId, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module,AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory);
        public abstract IAveDiscoverList CreateDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory);
        public abstract IAveDiscoverList CreateDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory);
        #endregion

        #region =============================Folder=============================
        public abstract IAveDiscoverFolder CreateDiscoverFolder();
        
        public abstract IAveDiscoverFolder CreateDiscoverFolder(IAveSite site, Guid webId, string folderRelativeUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory, Guid listId = default(Guid), IAveWeb web = null);
        #endregion
    }
}
