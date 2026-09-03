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




using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Agent.ExchangeBrowser.Object;
using AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.APIHelper;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.SharePointBrowser
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BrowserMessage : AveMessage
    {
        [DataMember]
        public TreeType TreeType { get; set; }

        [DataMember]
        public BrowserContractBase BrowserContract { get; set; }

        /// <summary>
        /// Deployment Manager Dash Board获得Export磁盘空间用这个属性
        /// </summary>
        [DataMember]
        public List<BrowserContractBase> BrowserContracts { get; set; }

        [DataMember]
        public BrowserContractBase CreateContract { get; set; }
        
        [DataMember]
        public Office365MessageContract MessageContract{ get; set; }
    }

    [KnownType(typeof(BackupDataSearchContract))]
    [KnownType(typeof(NetSharePathContract))]
    [KnownType(typeof(FileSystemBrowserContract))]
    [KnownType(typeof(SharePointBrowserContract))]
    [KnownType(typeof(AdminSearchContract))]
    [KnownType(typeof(Office365MessageContract))]
    [KnownType(typeof(WFEPortContract))]
    [KnownType(typeof(SharePointPropertyBrowserContract))]
    [KnownType(typeof(TreeNodeParserContract))]
    [KnownType(typeof(Office365UserContract))]
    [KnownType(typeof(ExchangeOnlineBrowserContract))]
    [KnownType(typeof(Office365CreateMessageContract))]
    [KnownType(typeof(Office365PersonalSiteRegistrationContract))]
    [KnownType(typeof(Office365CheckChangeMessageContract))]
    [KnownType(typeof(APISPObjectResolverContract))]
    [KnownType(typeof(Office365CustomSolutionContract))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BrowserContractBase
    {
    }
}
