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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using RAExportCommon;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    internal class ExchangeExportController : IBackupController
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExchangeExportController));
        private EXOConfiguration config = null;
        private EXOExportBeforeArcInfo EXOExportBefArcInfo = null;
        public ExchangeExportController(EXOConfiguration mConfig, EXOExportBeforeArcInfo EXOExportBefArcInfo)
        {
            config = mConfig;
            this.EXOExportBefArcInfo = EXOExportBefArcInfo;
        }

        public void Finish()
        {
            logger.Info("Export action finished");
        }

        public void Process(EXOArchiveData archiveData)
        {
            //TODO Add folder level and other types here, also need to add report for multi process         
            string comment = string.Empty;

            if (EXOExportBefArcInfo != null && EXOExportBefArcInfo.EXOExport != null && EXOExportBefArcInfo.EXOExportPathGenerator != null)
            {
                ExchangeItemExport exoItemExport = new ExchangeItemExport(logger) { Configuration = config };
                exoItemExport.EXOExportBeforeArcInfo = EXOExportBefArcInfo;
                exoItemExport.VaultExport(archiveData.ItemId, archiveData, config.SubJobId, config.RuleName);
            }
        }
    }
}