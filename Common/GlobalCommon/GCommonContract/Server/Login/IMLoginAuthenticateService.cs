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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Wcf;
using AvePoint.GCommon.Contract.Server.ControlPanel.AuthenticationManager;

namespace AvePoint.GCommon.Contract.Server.Login
{
    /// <summary>
    /// IMAuthenticateService should be used in login progress
    /// (not support Desktop Client yet)
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMLoginAuthenticateService
    {
        /// <summary>
        /// Authenticate Method for DocAve and its Features, and it will validate some information about request identifier(not yet imple).
        /// IsTrusted must return true.
        /// </summary>
        /// <param name="credential"></param>
        /// <returns>a list of account Id</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<string> Authenticate(BaseCredential credential);

        /// <summary>
        /// Prepare credential use to Authenticate
        /// </summary>
        /// <param name="logonType"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        BaseCredential PrepareCredential(AveAuthenticationTypes logonType);

        /// <summary>
        /// Get all supported authenticationTypes in current running DocAve6 control platform
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AuthenticationTypeCatalogDto> GetSupportedAuthenticationTypes();

        /// <summary>
        /// Get Supported Active Domains.
        /// </summary>
        /// <returns>If ADIntegration was closed,return null</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Dictionary<string, string> GetSupportedActiveDomains();

        /// <summary>
        /// Register Feature endpoint by its url in HttpContext.Current.Request,and return the register result
        /// (not support Desktop Client yet)
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        bool RegisterEndpointFromRequest();

        /// <summary>
        /// check if current application be trusted.
        /// (not support Desktop Client yet)
        /// </summary>
        /// <param name="identifierData"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        bool IsTrustedEndpoint();


    }
}
