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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Setting
{
    public interface IStubSettingService
    {
        Task<RAReturnMessage> CreateStubSettingAsync(StubSettingDto dto);
        bool MagrateDAOStubSetting(StubSettingDto dto);
        Task<RAReturnMessage> UpdateStubSettingAsync(StubSettingDto dto);
        StubSettingResult GetAllStubSettings(StubSettingResult pageInfobool);
        List<int> GetAllUsingObsoleteStubTypes();
        List<StubSettingUIDto> GetAllStubSettingsNotPaged();
        StubSettingUIDto GetStubSettingById(string id);
        Task<RAReturnMessage> DeleteStubSettingAsync(List<string> ids);
        Task<StubSettingDto> GetStubTemplateByNameAsync(string name);
        Task<StubSettingDto> GetStubTemplateByIdAsync(string id);
        HashSet<string> GetAllStubSettingNames();
        RAReturnMessage RunConvertStubJob(ConvertStubDto dto);
        Task<string> RealRunConvertStubJob(JobRunBy jobRunBy, string jobRunByUser, string param);

        RAReturnMessage RunStubDisposalJob(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RealRunStubDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser);
    }
}
    