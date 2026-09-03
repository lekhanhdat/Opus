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
using System;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using FSTreeNodeDto = AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto;
using RAFileSystem.FileSystem.BaseProcessor;

namespace RAFileSystem.FileSystem.DataSync.Utils
{
    internal class FSDataSyncJobContext : FSJobProcessorContext
    {
        public FSDataSyncJobContext(
            FSJobMessage message,
            FSTreeNodeDto node,
            Tuple<FSTreeNodeDto, FSTreeNodeDto, FSTreeNodeDto> top3Nodes,
            string rootPath,
            IXSystem system,
            StorageInfo directoryInfo,
            Guid settingScopeId,
            FSSettingDto setting,
            NodeLevel classificationRule)
            : base(message, node, top3Nodes, rootPath, system, directoryInfo, settingScopeId, setting, classificationRule)
        {
        }
    }
}