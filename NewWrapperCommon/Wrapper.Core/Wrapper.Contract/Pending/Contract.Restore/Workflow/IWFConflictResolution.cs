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
using AvePoint.Wrapper.Common;
namespace AvePoint.Wrapper.Restore
{
    public interface IWFConflictResolution : IDisposable
    {
        /// <summary>
        /// 还原workflow definition时使用
        /// </summary>
        WFAssociationConflictResolutionOption AssociationOption { get; set; }
        /// <summary>
        /// 还原workflow instance 过程中反插workflow definition时使用
        /// </summary>
        WFAssociationConflictResolutionOption ParentAssociationOption { get; set; }
        /// <summary>
        /// 还原workflow instance时使用
        /// </summary>
        WFInstanceConflictResolutionOption InstanceOption { get; set; }
        bool WebContentTypeAssociation { get; set; }
        object AssociationParentObject { get; set; }
        void CacheAssociationData(AveWorkflowInfo wfInfo);
        IReport GetReport();
        void RestoreScheduleData(AveWorkflowInfo wfInfo, IAveListItem item);
        void RestoreScheduleData(AveWorkflowInfo wfInfo, IAveWeb web);
        /// <summary>
        /// 还原web(web contentType)上的workflow definition
        /// </summary>
        /// <param name="wfInfo">workflow的备份数据</param>
        /// <param name="web">parent AveSPWeb</param>
        /// <param name="contentType">还原web workflow association时为null,还原web contentType workflow时为关联的web contentType</param>
        void RestoreAssociationData(AveWorkflowInfo wfInfo, IAveSPWeb web, IAveContentType contentType = null);
        /// <summary>
        /// 还原list(list contentType)上的workflow definition
        /// </summary>
        /// <param name="wfInfo">workflow definition的备份数据</param>
        /// <param name="list">parent AveSPList</param>
        /// <param name="contentType">还原web workflow association时为null,还原web contentType workflow时为关联的list contentType</param>
        void RestoreAssociationData(AveWorkflowInfo wfInfo, IAveSPList list, IAveContentType contentType = null);
        /// <summary>
        /// 还原listItem上的workflow instance
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="item"></param>
        void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveSPListItem item);
        /// <summary>
        /// 还原document上的workflow instance
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="doc"></param>
        void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveSPDoc doc);
        /// <summary>
        /// 还原folder上的workflow instance
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="folder"></param>
        void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveSPFolder folder);
        /// <summary>
        /// 还原web上的workflow instance
        /// </summary>
        /// <param name="wfInfo"></param>
        /// <param name="folder"></param>
        void RestoreInstanceData(AveWorkflowInfo wfInfo, IAveSPWeb web);
        void RestoreNintexWorkflowTemplates(AveWorkflowInfo wfInfo, IAveWeb web);
        void SetNWDBConnectionString(string connStr);
        void SetWorkflowProcessorRuntime(AveSPWorkflowRestoreOption workflowRestoreOption);
    }
}
