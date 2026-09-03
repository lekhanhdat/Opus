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




using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.DataManager.IndexManager;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.StorageOptimization.Archiver
{
    /// <summary>
    /// Archiver provider services to agent and gui. 
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMArchiverService
    {

        #region For Archiver Profile

        #endregion

        #region FOR ARCHIVER DATABASE
        #endregion
        [OperationContract]
        Task<SOReturnMessage> RunEndUserRestoreNow(EndUserRestoreJobConfig config, bool? runInWebRole = null);
        [OperationContract]
        Task<ArchiverStubLink> ParseStubStringAsync(string stubString);
        SOReturnMessage CheckPermissionForStubRestoreLink(RemoteSiteCollection site, string fileUrl, string userMail);
        SOReturnMessage CheckPermissionForSharePointSite(RemoteSiteCollection site, string userMail);
        SOReturnMessage CheckPermissionForGroupOrTeamSite(RemoteSiteCollection site, string groupId, string userMail);
        EndUserRestoreSettingUIDto GetEndUserRestoreSetting();
        Task<EndUserRestoreSettingUIDto> GetEndUserRestoreSettingAsync();
        bool IsExportSizeReachLimited();
    }
}
