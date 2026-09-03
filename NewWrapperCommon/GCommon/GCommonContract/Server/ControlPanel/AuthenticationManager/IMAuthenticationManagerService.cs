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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Server.SingleSignOn.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.AuthenticationManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMAuthenticationManagerService
    {
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateAuthenticationDefaultType(AveAuthenticationTypes type);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateAuthenticationManager(AuthenticationManagerDto dto);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AuthenticationManagerDto GetAuthenticationManagerConfig();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AuthenticationManagerDto GetAuthenticationManager();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AuthenticationTypeCatalogDto> LoadAuthenticationCatalog();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateAuthenticationCatalog(AveAuthenticationTypes specialType, int status);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AuthenticationCatalogDto GetAuthenticationCatalog(AveAuthenticationTypes type);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void ChangeDomainStatus(string domainId, DomainStatus status);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void ChangeDomainStatusByIds(List<string> domainIds, DomainStatus status, bool isRemoveOrActiveADAccount = false, List<string> selectedIds = null, List<string> notDeleteIds = null);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void CreateSSOSetting(SingleSignOnSettingDto dto);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void CreateWindowsAuthSetting(WindowsAuthenticationSettingDto dto, ServiceDto webService);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void ModificationIisPropertyInHandler();
        /// <summary>
        /// 添加一个domain
        /// </summary>
        /// <param name="domain"></param>
        /// <returns>如果已经添加过了，返回Result.AlreadyExisted;如果成功了返回Result.Successful;</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result AddDomain(DomainDto domain, AccountProfileDto managedAccount);

        //// <summary>
        //// 删除选定的domain集合
        //// </summary>
        //// <param name="idArray">要删除的domain的id集合</param>
        //// <returns>如果有不能删除的，就会加入一个ErrorMessage</returns>
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //List<ErrorMessage> DeleteDomains(IEnumerable<string> idArray, bool removeUsers);

        ///// <summary>
        ///// 校验Domain下Users或者Groups是否能被删除
        ///// </summary>
        ///// <param name="idArray">要删除domain下Users或者Groups的id集合</param>
        ///// <returns>如果有不能删除的，就会加入一个ErrorMessage，ErrorMessage包含该Domain下不能被删除的User或者Group的Id，还有原因类型</returns>
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //List<ErrorMessage> ValidateADUsersOrGroupsCanDelete(IEnumerable<string> idArray);

        /// <summary>
        /// 取得SSO配置(有当前Accounts所使用到的Claim Types列表)
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        SingleSignOnSettingDto GetSSOSetting();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        bool NeedToRestoreUsers(string realDomainName);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        int RestoreUsers(string realDomainName, bool restoreUsers);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AccountProfileDto> GetAllAccountProfiles();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountProfileDto GetDomainAccountProfile(DomainDto domain);

        /// <summary>
        /// add by zqxu@avepoint.com
        /// 只用于更新domain的description，account profile，port。名字和状态不会修改，所以不涉及disable或enable user
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result UpdateDomainDto(DomainDto domain, AccountProfileDto accountDto);
        /// <summary>
        /// add by zqxu@avepoint.com
        /// </summary>
        /// <param name="idArray"></param>
        /// <param name="removeUsers"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void DeleteDomains(IEnumerable<string> idArray, bool removeUsers);
    }
}
