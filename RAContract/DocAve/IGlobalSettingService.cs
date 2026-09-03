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
using AvePoint.GCommon.Contract.Server.Common.ExportReport.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.DocAve
{
    /// <summary>
    /// 此接口 Global Setting和DocAve connection 共用
    /// </summary>
    [RACodeReview("Allen Yin")]
    public interface IGlobalSettingService
    {
        /// <summary>
        /// 获取DocAve Export Location，Storage Police等信息
        /// </summary>
        /// <returns></returns>
        //SORulesAndSettings LoadMetaData();
        Task<List<ExportReportDto>> GetAllExportLocationAsync();

        string GetCurrentExportLocationId();

        System.Threading.Tasks.Task SaveExportLocationInfoAsync(string ExportLocationId);

        //GlobalStorageSetting LoadGlobalSettingInfoFromRA();
        //bool SaveOrUpdate(GlobalStorageSetting newGlobalStorageSettings);

        /// <summary>
        /// 加载CP中的设置到内存中
        /// </summary>
        //void InitDocAveControlSetting();

        /// check if configure DocAveConnection or globalstoragesetting 
        /// </summary>
        /// <param name="Type">check type,eg:only docaveconnection</param>
       // ValidationMessage CheckDocAveConnectionGlobalStorageSetting();
        ValidationMessage CheckExportSetting(ValidationType Type, int sourceFlag);
        ValidationMessage CheckDocAveConnectionSetting();
        //ValidationMessage CheckGlobalStorageSetting();
        Task<Dictionary<Guid, int>> GetExportLocationTypesAsync();
    }
}
