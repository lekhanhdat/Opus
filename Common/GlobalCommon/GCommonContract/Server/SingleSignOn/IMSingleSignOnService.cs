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
using System.ServiceModel;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.SingleSignOn.Object;
using AvePoint.GCommon.Contract.Wcf;



namespace AvePoint.GCommon.Contract.Server.SingleSignOn
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMSingleSignOnService
    {
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountMappingDto GetCurrentLoginClaim();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        string GetClaimNameByType(string type);

        


        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void SaveTrustConfigs(List<FederationTrustConfigsDto> dtos);


        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<FederationTrustConfigsDto> GetTrustConfigs();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<ClaimTypeMappingDto> GetClaimTypes();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        SingleSignOnSettingDto ProcessSTSMetadata(string metadataUrl);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        string GetApplicationMetadataFromHandler();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        string GetApplicationMetadata(SingleSignOnSettingDto dto);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<string> GetSettingNames();
        /// <summary>
        /// 登录STS服务器的重定向地址
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        string GetLoginRedirectUrl(string applicationUrl, string subServerName);


        /// <summary>
        /// 取得SSO配置(没有当前Account所使用的Cliam Types)
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        SingleSignOnSettingDto GetSSOSetting();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        bool ValidateIssuer(string issuer);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        WindowsAuthenticationSettingDto GetWindowsAuthSetting();


    }
} 
