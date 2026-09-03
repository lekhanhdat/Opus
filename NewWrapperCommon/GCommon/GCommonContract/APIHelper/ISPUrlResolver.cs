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
using System.ServiceModel;
using AvePoint.GCommon.Contract.AgentService.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.APIHelper
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ResolveSPObjMessage : AveMessage
    {
        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string SiteGroupName { get; set; }

        [DataMember]
        public string WebAppUrl { get; set; }

        [DataMember]
        public string ContentDBName { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public string WebUrl { get; set; }

        [DataMember]
        public string ListTitle { get; set; }

        [DataMember]
        public Guid AppProductId { get; set; }

        [DataMember]
        public Guid AppInstanceId { get; set; }

        [DataMember]
        public string FolderPath { get; set; }

        [DataMember]
        public Guid ItemGuid { get; set; }

        public bool IsOnlineObject 
        {
            get
            {
                return string.IsNullOrEmpty(FarmName);
            }
        }
    }

    /// <summary>
    /// this contract is used for sending message between control service and
    /// agent.
    /// </summary>
    public class APISPObjectResolverContract : BrowserContractBase
    {
        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string WebAppUrl { get; set; }

        [DataMember]
        public string ContentDBName { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public string WebUrl { get; set; }

        [DataMember]
        public string ListTitle { get; set; }

        [DataMember]
        public Guid AppProductId { get; set; }

        [DataMember]
        public Guid AppInstanceId { get; set; }

        [DataMember]
        public string FolderPath { get; set; }

        [DataMember]
        public Guid ItemGuid { get; set; }

        [DataMember]
        public SPTreeNodeDto ResolvedWebappNode { get; set; }

        /// <summary>
        /// when dealing with office 365 object, agent side needs to know 
        /// sp version of target sharepoint.
        /// </summary>
        [DataMember]
        public int SPVersion { get; set; }

        /// <summary>
        /// when dealing with office 365 object, agent side needs to get a
        /// credential to access remote sharepoint.
        /// </summary>
        [DataMember]
        public BposInfo BposInfo { get; set; }

        /// <summary>
        /// This is a string from an exception object. Need to use 
        /// SerializerHelper.SerializeToBase64String for serialization.
        /// </summary>
        /// <remarks>No longer used since 6.5, use ExceptionMessage instead.</remarks>
        [DataMember]
        public String ExceptionB64Str { get; set; }

        /// <summary>
        /// Add since 6.5.
        /// </summary>
        [DataMember]
        public string ExceptionMessage { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ResolveSPObjResponse : AveMessage
    {
        [DataMember]
        public SPTreeNodeDto ResolvedWebappNode { get; set; }

        /// <summary>
        /// This is a string from an exception object. Need to use 
        /// SerializerHelper.SerializeToBase64String for serialization.
        /// </summary>
        /// <remarks>No longer used since 6.5, use ExceptionMessage instead.</remarks>
        [DataMember]
        public String ExceptionB64Str { get; set; }

        /// <summary>
        /// Add since 6.5.
        /// </summary>
        [DataMember]
        public string ExceptionMessage { get; set; }
    }

    [ServiceContract]
    public interface ISPUrlResolver
    {
        [OperationContract]
        ResolveSPObjResponse ResolveWebApp(ResolveSPObjMessage message);

        [OperationContract]
        ResolveSPObjResponse ResolveContentDB(ResolveSPObjMessage message);

        [OperationContract]
        ResolveSPObjResponse ResolveSite(ResolveSPObjMessage message);

        [OperationContract]
        ResolveSPObjResponse ResolveWeb(ResolveSPObjMessage message);

        [OperationContract]
        ResolveSPObjResponse ResolveList(ResolveSPObjMessage message);

        [OperationContract]
        ResolveSPObjResponse ResolveFolder(ResolveSPObjMessage message);

        [OperationContract]
        ResolveSPObjResponse ResolveItem(ResolveSPObjMessage message);

        [OperationContract]
        ResolveSPObjResponse ResolveApp(ResolveSPObjMessage message);
    }
}
