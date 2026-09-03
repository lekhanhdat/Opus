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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.Contract.SyncNode.GoogleSyncNode;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using AppType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType;

namespace RMSynchronize.SyncNodeFromAOS.Executors;

public class RMSyncGoogleSharedDriveContainerNodeExcutor(AosModernApiTenantClient tenantClient, List<TenantConnectionInfo> tenantConnectionInfos, RMSyncNodeAzureChangeLogger changeLogger) : RMSyncGoogleNodeExecutor(tenantClient, tenantConnectionInfos, changeLogger)
{
    protected override NodeLevel RecordContainerNodeLevel => NodeLevel.GoogleSharedDriveContainer;
    public override SourceFlag ContentSource => SourceFlag.Google;
    public override RemoteNodeType AosNodeType => RemoteNodeType.GoogleSharedDrive;
    protected override IEnumerable<RMGoogleNodeAdaption> ConvertAosNodesToAdaption(RMContainerInfoAdaption containerInfo, RemoteNodesResult queryResult)
    {
        return queryResult.GoogleDrives.Where(drive => drive.ContainerId == containerInfo.Id).ConvertAll(googleDrive =>
        {
            var res = new RMGoogleNodeAdaption
            {
                Id = googleDrive.Id,
                ObjectId = googleDrive.ObjectId,
                TenantId = googleDrive.TenantId,
                ContainerId = containerInfo.Id,
                ContainerName = containerInfo.Name,
                Name = googleDrive.Name,
                NodeLevel = NodeLevel.GoogleSharedDrive,
                AppType = AppType.Google,
                UserName = googleDrive.Name
            };
            return res;
        });
    }
}