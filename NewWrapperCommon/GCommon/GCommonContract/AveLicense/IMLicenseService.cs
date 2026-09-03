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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.AveModuleContract;




namespace AvePoint.GCommon.Contract.AveLicense
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMLicenseService
    {
        [OperationContract]
        LicenseOperationResult ApplyLicense(byte[] data);

        [OperationContract]
        //[OperationProtocolConverterAttribute("GetLicenseDetailForLatestVersion")]
        LicenseOperationResult GetLicenseDetail();

        [OperationContract]
        LicenseOperationResult GetLicenseDetailCompatibly(string version);

        [OperationContract]
        void SaveCEIPSetting(bool isRegisterDocAve);

        /// <summary>
        /// 获取所有未过期的模块的名字
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<ModuleName> GetSupportedModules();

        [OperationContract]
        bool IsLicenseSupport(ModuleName module);

        /// <summary>此方法只供后台调用，所以不公开WCF接口</summary>
        /// <param name="node"></param>
        /// <param name="normalModuleName"></param>
        /// <param name="office365ModuleName"></param>
        /// <returns></returns>
        LicenseCheckResult ValidateNodeWithDetailResult(SPTreeNodeDto node, AveModule module);

        /// <summary>此方法只供后台调用，所以不公开WCF接口</summary>
        /// <param name="node"></param>
        /// <param name="normalModuleName"></param>
        /// <param name="office365ModuleName"></param>
        /// <returns></returns>
        LicenseCheckResult ValidateNodeWithDetailResultByUrl(SPTreeNodeDto node, string userId, AveModule module);
    }
}
