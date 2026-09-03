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

namespace Office365GroupRestore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
    using AvePoint.Wrapper.Common;
    using ExchangeCommonWrapper;
    using Job.ModernManagement.Report;
    using Office365GroupBackup;

    public class ItemRestoreHelperBatch : BaseRestoreHelperBatch
    {
        public ItemRestoreHelperBatch(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {

        }
        protected override void InitReport(MetadataEntity baseEntity, string sourceUrlPath)
        {
            base.InitReport(baseEntity, sourceUrlPath);
            ReportDto.Title = TeamsConst.ConversationMessageReportTitle;
            ReportDto.Type = ReportNodeHeader.Conversation;
        }

        protected override bool NeedRestore() =>
            !string.IsNullOrEmpty(RestoreConfig.CurrentRestoreMailbox)
            && (Config.RestoreConversationType == RestoreConversationType.Html || !string.IsNullOrEmpty(_CurrentChannel?.Id));

        protected override void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataCollection)
        {
            var data = dataCollection.First().RestoreData;
            var entity = data.Metadata;
            var dataSource = entity.Type == "IPM.SkypeTeams.Message" ? DataSource.EWS : DataSource.Graph;
            InitReport(entity, data.SourceUrlPath);

            logger.Info($"Start to restore {ReportDto.Type}, itemCount:{dataCollection.Count()} name:{ReportDto.Name}, path: {ReportDto.Path}, type:{dataSource}");
            try
            {
                RestoreConversationFactory
                    .Create(this, dataSource, Config.RestoreConversationType)
                    .Restore(Config, AuthorizationManager, dataCollection);
            }
            catch (AveWrapperI18NException ex)
            {
                ReportDto.Status = ReportStatus.Failed;
                ///var errorDetail = ex.Message; //ex.GetErrorDetial();
                ReportDto.ErrorMessage = ex.Message;
                Report.AddRestoreReport(ReportDto);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to restore {ReportDto.Type}, error:{ex}");
                ReportDto.Status = ReportStatus.Failed;
                ReportDto.ErrorMessage = ex.Message;
                Report.AddRestoreReport(ReportDto);
            }
            dataCollection.ForEach(disposeStream => disposeStream.Dispose());
        }
    }
}