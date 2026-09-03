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
using DataExportCore.Cache;
using DataExportCore.Discover.Node;
using DataExportCore.Utils;
using Google.Api.Gax.ResourceNames;

namespace DataExportCore.Export
{
    public class CloudExportWorker : ExportWorkerBase
    {
        public CloudExportWorker(Reporter report, ExportQueue exportQueue) : base(report, exportQueue, GlobalDeviceCache.GetDestinationDevice()) { }

        public CloudExportWorker(Reporter report, ExportQueue exportQueue, string groupAddress) : base(report, exportQueue, GlobalDeviceCache.GetDestinationDevice(), groupAddress) { }

        protected override void ExportFolder(FolderDiscoverNode folderNode)
        {
            folderNode.ExportPath = ExportUtility.BuildExportPath(string.IsNullOrEmpty(GroupAddress) ? string.Empty : Path.Combine(GroupAddress, I18NEntity.GetString("SATool_ExportPath_SiteCollections")), folderNode.Name, folderNode.SitePath, folderNode.Level);

            base.ExportFolder(folderNode);
        }

        protected override string BuildStorageInfoExportPath(string exportPath)
        {
            return exportPath;
        }
    }

    public class MailBoxCloudExportWorker : MailBoxExportWorkerBase
    {
        public MailBoxCloudExportWorker(Reporter report, ExportQueue<ExchangeDiscoverNode> exportQueue, string groupAddress) : base(report, exportQueue, GlobalDeviceCache.GetDestinationDevice(), groupAddress, true) { }

        protected override string BuildStorageInfoExportPath(string exportPath)
        {
            return ExportUtility.BuildExportPath(string.IsNullOrEmpty(GroupAddress) ? string.Empty : Path.Combine(GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes")), "", "", NodeType.ExchangeOnlineMailbox);
        }
    }

    public class ChannelConversationCloudExportWorker : ChannelConversationExportWorkerBase
    {
        public ChannelConversationCloudExportWorker(Reporter report, ExportQueue<TeamsDiscoveryNode> exportQueue, string groupAddress) : base(report, exportQueue, GlobalDeviceCache.GetDestinationDevice(), groupAddress, true) { }
        protected override string BuildStorageInfoExportPath(string exportPath)
        {
            try
            {
                return exportPath.Substring(GlobalDeviceCache.ExportCacheSetting.SystemLocation.Length);
            }
            catch
            {
                return ExportUtility.BuildExportPath(string.IsNullOrEmpty(GroupAddress) ? string.Empty : Path.Combine(GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes")), "", "", NodeType.ExchangeOnlineMailbox);
            }
        }
    }
}
