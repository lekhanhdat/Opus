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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager.Object;
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMUpdateManagerService
    {
        /// <summary>
        /// get can update patch info for gui
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        UpdatePatchInfoDtos CheckUpdateInformations();

        /// <summary>
        /// 为GUI提供上次Check的Patch信息
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        UpdatePatchInfoDtos GetLastCheckInfos();
        /// <summary>
        /// 为GUI提供所有的Service
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        UpdateServicesDto GetAllServiceForUpdate(List<UpdatePatchInfoDto> needInstallPatchs);

        /// <summary>
        /// 安装Patch
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="patchs"></param>
        /// <param name="services"></param>
        [OperationContract]
        InstallCallBackDto Install(List<UpdatePatchInfoDto> patchs, List<ServiceDto> services);

        /// <summary>
        /// 安装Patch,根据Agent Type重启IIS
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="patchs"></param>
        /// <param name="services"></param>
        [OperationContract]
        InstallCallBackDto InstallWithResetIIS(List<UpdatePatchInfoDto> patchs, List<ServiceDto> services);

        /// <summary>
        /// 为GUI提供进度
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        InstallMessage CheckProgressAndDetail();

        [OperationContract]
        void SaveSettings(UpdateSettingDto settingDto);

        [OperationContract]
        void MovePatch(UpdateSettingDto newSettingDto);

        [OperationContract]
        UpdateSettingDto GetUpdateSetting();

        [OperationContract]
        int TestUNCPath(ExportLocationDto dto, bool passwrodchanged);

        [OperationContract]
        List<UpdateHistoryDto> GetAllUpdateHistory();

        /// <summary>
        /// 为NLB环境提供方法
        /// </summary>
        [OperationContract]
        ReturnResult CopyPatchFromUNC(List<UpdatePatchInfoDto> patchs);

        [OperationContract]
        void StartInstaller(string mainControlAddress, int mainControlport, string selfControlAddress, int selfControlPort, UpdatePatchInfoDto patchDto);

        [OperationContract]
        string StartPatchControl(string allInfoXML, string mainControlServiceXML, int patchControlPort, UpdatePatchInfoDto patchDto);


        [OperationContract]
        List<string> GetUpdateHistory();

        [OperationContract]
        void SaveSettingsAndRemovePatch(UpdateSettingDto newSettingDto, bool deleteOldPatch, bool copyNewPatch);

        /// <summary>
        /// Test Run方法，检测要安装的服务是否已经安装了要安装Patch没有包含的CI
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="patchs"></param>
        /// <param name="services"></param>
        /// <returns>ServiceDto：有问题的服务，List这个服务已经安装的CI，并且是Patch里没有的CI</returns>
        [OperationContract]
        TestInstallCallBackDto TestRunInstall(List<UpdatePatchInfoDto> patchs, List<ServiceDto> services);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="patchFileName"></param>
        /// <returns></returns>
        [OperationContract]
        ImportCallBackDto ImportCIPatch2Location(string folderName, string patchFileName);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="patchFileName"></param>
        /// <returns></returns>
        [OperationContract]
        ImportCallBackDto CheckImportPatchIsExist(string folderName, string patchFileName);

        /// <summary>
        /// <remarks>import patch时，检测本地磁盘空间是否为1G</remarks>
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="patchFileName"></param>
        /// <returns></returns>
        [OperationContract]
        bool CheckDiskSpaceIsEnough();

        /// <summary>
        /// <remarks>import patch时，检测UNC磁盘空间是否为1G</remarks>
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="patchFileName"></param>
        /// <returns></returns>
        [OperationContract]
        bool CheckUNCDiskSpaceIsEnough(UpdateSettingDto _setting,AccountProfileDto _account);

        [OperationContract]
        List<UpdatePatchInfoDto> DeletePatch(List<UpdatePatchInfoDto> patchs);

        [OperationContract]
        ServiceInfoWithUpdateHistory GetUpdateHistoryForPatch(UpdatePatchInfoDto patchDto);

        [OperationContract]
        InstallCallBackDto UnInstallPatch(UpdatePatchInfoDto patchInfo, List<ServiceDto> services);

        /// <summary>
        /// ValidateProxy为废弃验证代理方法，TestProxy验证匿名HTTP、透明HTTP或SOCKET代理是否连通
        /// </summary>

        [OperationContract]
        bool ValidateProxy(Proxy proxy);

        //[OperationContract]
        //bool TestProxy(UpdateSettingDto _setting);

        [OperationContract]
        void OpenAutoDownload(bool isOpen);

        [OperationContract]
        ViewDeployIDDto GetDeployId();

        /// <summary>
        /// Enforce DocAve and GA installation order [ADO-134177]
        /// 这个方法目前只对GA+ Service升级时进行判断，只判断当前处于Active 的Control Service的对应GA+ Version
        /// 当有Control Service的GA+ version 小于 GA+ Release版本时将给出提示
        /// </summary>
        /// <param name="needInstallPatchs"></param>
        /// <returns></returns>
        [OperationContract]
        GAIntallationOrder ValidateGAInstallOrder(List<UpdatePatchInfoDto> needInstallPatchs);

        /// <summary>
        /// 取得最新安装的UpdateHistoryDTO，前台需要显示时间
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        UpdateHistoryDto GetLastInstallUpdateHistory();
    }
}
