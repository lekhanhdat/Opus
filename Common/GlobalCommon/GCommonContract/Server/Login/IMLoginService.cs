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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.AuthenticationManager;
using AvePoint.GCommon.Contract.Server.UserRegister;
using AvePoint.GCommon.Contract.Wcf;


namespace AvePoint.GCommon.Contract.Server.Login
{

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMLoginService
    {
        /// <summary>
        /// logout method : 
        /// -1 : Manual
        /// 0  : Session Time out
        /// 1  : Force Logout
        /// 2  : Login clear operation
        /// </summary>
        /// <param name="logoutMethod"></param>
        /// <param name="logonTime"></param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void Logout(int logoutMethod = -1, long logonTime = 0);
        /// <summary>
        /// 用户登陆DocAve系统的方法
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="domain">域名(登陆界面最下面的下拉框的值)，如果选择了LocalSystem，则传进来null或者空字符串</param>
        /// <returns>包含登陆状态以及相关信息，详情参见LoginResult类中的注释</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult Login(AvePoint.GCommon.Contract.Server.Login.BaseCredential credential);


        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AvePoint.GCommon.Contract.Server.Login.BaseCredential PrepareCredential(AveAuthenticationTypes logonType);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AuthenticationTypeCatalogDto> LoadAuthenticationCatalog();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void RegisterCookie(Dictionary<string, string> mapping);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LicenseAgreementResult GetLicenceAgreement();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult PortalLogin(string seed);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult PortalLoginByModel(CurrentUserModel user);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult PortalLoginByModelRole(CurrentUserModel user, ObjectRoleType role, bool needEnableSchedules);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult GALogin(string userName, string groupId, string signature);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult RevIMLogin(string userName, string groupId, string signature);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult APILogin(string userName, string groupId, string signature);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult LoginForSimple(string userName);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        UserRegisterResultDto RegisterForSimple(UserRegisterDto registerInfo);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LoginResult NewLogin(string userName, string groupId);
    }
}
