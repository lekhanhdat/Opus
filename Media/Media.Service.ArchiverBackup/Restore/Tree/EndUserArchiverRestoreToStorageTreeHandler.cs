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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using Merged18NResources.MediaServiceArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Media.Service.ArchiverBackup
{
    public class EndUserArchiverRestoreToStorageTreeHandler
         : IRestoreServiceTreeHandler
    {
        readonly static Object syncIndexItemProceedObject = new Object();
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        String currentSiteCollectionUrl;
        Boolean isJustCalculateCount;
        ArchiverRestoreJob restoreJob;
        List<IndexItemProceedEventArgs> attachementInfos;

        EventHandler<IndexItemProceedEventArgs> indexItemProceed;
        public IArchiverRestoreIndexService RestoreIndexService { get; set; }

        public event EventHandler<IndexItemProceedEventArgs> IndexItemProceed
        {
            add
            {
                lock (syncIndexItemProceedObject)
                {
                    this.indexItemProceed += value;
                }
            }
            remove
            {
                lock (syncIndexItemProceedObject)
                {
                    this.indexItemProceed -= value;
                }
            }
        }

        public SPTreeNodeDto CutTree(SPTreeNodeDto rootTree)
        {
            SPTreeNodeDto treeNodeDto = null;
            if (rootTree.Level == NodeLevel.Items)
            {
                if (rootTree.CheckNumber == 1 || rootTree.SelectAll == SelectAllState.Checked)
                    treeNodeDto = rootTree;
                else
                {
                    if (rootTree.ChildrenCount > 0)
                    {
                        foreach (SPTreeNodeDto item in rootTree.Children)
                        {
                            if (item.CheckNumber == 1)
                            {
                                treeNodeDto = rootTree;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                List<SPTreeNodeDto> children = new List<SPTreeNodeDto>();
                foreach (SPTreeNodeDto child in rootTree.Children)
                {
                    SPTreeNodeDto selectedNodeDto = CutTree(child);
                    if (selectedNodeDto != null)
                        children.Add(selectedNodeDto);
                }
                rootTree.Children.Clear();
                rootTree.Children.AddRange(children);
                if (rootTree.CheckNumber == 1 || rootTree.Children.Count > 0)
                    treeNodeDto = rootTree;
            }
            return treeNodeDto;
        }

        public void ProcessTreeNode(TreeNodeParameter treeParam)
        {
            if (treeParam.RestoreJob is ArchiverRestoreJob)
            {
                this.restoreJob = (ArchiverRestoreJob)treeParam.RestoreJob;
                this.isJustCalculateCount = treeParam.IsJustCalculateCount;
                this.currentSiteCollectionUrl = treeParam.CurrentTree.Name;
                this.attachementInfos = new List<IndexItemProceedEventArgs>();
                this.ProcessNodeDtoInternalForRecords(treeParam.CurrentTree.Name, treeParam.CurrentTree);
            }
            else
            {
                throw new ArgumentException("Cannot convert restore job to ArchiverRestoreJob", "treeParam");
            }
        }

        private void ProcessNodeDtoInternalForRecords(string currentPath, SPTreeNodeDto nodeDto)
        {
            var index = this.RestoreIndexService.Load(currentPath, restoreJob.BackupTime);

            if (index == null)
            {
                throw new NullReferenceException(string.Format("Cannot find the index with the path:{0} and end time:{1}", currentPath, restoreJob.BackupTime));
            }

            TreeNodeInfo info = new TreeNodeInfo
            {
                Name = index.Name,
                Type = index.Type,
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.ItemName,
                ItemVersionNumber = index.ItemVersionNumber,
            };
            ProcessTreeNode(info.Index);
        }

        public void ProcessTreeNode(IndexBase Index)
        {
            OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = Index, MarkMessage = new RestoreMarkMessage() { Security = SecurityState.Checked, Property = PropertyState.Checked, IsChecked = true } });
        }

        protected virtual void OnIndexItemProceed(IndexItemProceedEventArgs args)
        {
            var temp = indexItemProceed;
            if (temp != null) temp(this, args);
        }



    }
}
