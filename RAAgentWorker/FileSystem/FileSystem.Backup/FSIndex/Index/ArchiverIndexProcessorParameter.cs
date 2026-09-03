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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon;
    using System.Reflection;
    using System.Diagnostics;
    #endregion

    public class ArchiverIndexProcessorParameter : IndexProcessorParameter
    {
        //private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public ArchiverIndexProcessorParameter(string tenantGroupId) : base()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //base.DBPassWord = SettingProfileService.GetDBSEEMasterKey();
            this.logger.Info($"Get DB see master key");
            sw.Stop();
            this.logger.Info($"GetDBSEEMasterKey cost time:{sw.Elapsed.TotalMilliseconds}");
        }

        public ArchiverIndexProcessorParameter() : base()
        {
        }
    }
}