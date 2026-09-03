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

namespace DataExportCore.Export;

public class LocalExportWorker : ExportWorkerBase
{
    public LocalExportWorker(Reporter report, ExportQueue exportQueue) : base(report, exportQueue, GlobalDeviceCache.ExportCacheSetting) { }

    public LocalExportWorker(Reporter report, ExportQueue exportQueue, string groupAddress) : base(report, exportQueue, GlobalDeviceCache.ExportCacheSetting, groupAddress) { }

    protected override string ExportSite(DiscoverNode dto)
    {
        CreateDirectory(dto.ExportPath);
        return base.ExportSite(dto);
    }

    protected override void ExportWeb(DiscoverNode dto)
    {
        if (!dto.Name.Equals(".")) { CreateDirectory(dto.ExportPath); }
        base.ExportWeb(dto);
    }

    protected override void ExportFolder(FolderDiscoverNode folderNode)
    {
        folderNode.ExportPath = folderNode.Level == NodeType.List ? folderNode.ExportPath :
            ExportUtility.BuildExportPath(string.IsNullOrEmpty(GroupAddress) ? GlobalCache.ExportLocation : Path.Combine(GlobalCache.ExportLocation, GroupAddress, I18NEntity.GetString("SATool_ExportPath_SiteCollections")), folderNode.Name, folderNode.SitePath, folderNode.Level);
        CreateDirectory(folderNode.ExportPath);

        base.ExportFolder(folderNode);
    }

    protected override string BuildStorageInfoExportPath(string exportPath)
    {
        return exportPath.Substring(GlobalDeviceCache.ExportCacheSetting.SystemLocation.Length);
    }
}

public class MailBoxLocalExportWorker : MailBoxExportWorkerBase
{
    public MailBoxLocalExportWorker(Reporter report, ExportQueue<ExchangeDiscoverNode> exportQueue, string groupAddress) : base(report, exportQueue, GlobalDeviceCache.ExportCacheSetting, groupAddress, false) { }

    protected override string BuildStorageInfoExportPath(string exportPath)
    {
        return exportPath.Substring(GlobalDeviceCache.ExportCacheSetting.SystemLocation.Length);
    }
}

public class ChannelConversationLocalExportWorker : ChannelConversationExportWorkerBase
{
    public ChannelConversationLocalExportWorker(Reporter report, ExportQueue<TeamsDiscoveryNode> exportQueue, string groupAddress) : base(report, exportQueue, GlobalDeviceCache.ExportCacheSetting, groupAddress, false) { }

    protected override string BuildStorageInfoExportPath(string exportPath)
    {
        return exportPath.Substring(GlobalDeviceCache.ExportCacheSetting.SystemLocation.Length);
    }
}