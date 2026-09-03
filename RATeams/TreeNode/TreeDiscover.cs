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

namespace Office365GroupBackup
{
    #region namespaces

    using System;
    using System.Collections.Generic;

    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.RA.CommonUtil;
    using AvePoint.Wrapper.Common;

    using ExchangeBackupUtility;
    using ExchangeBackupUtility.Graph;
    using ExchangeUtility.Graph;

    

    #endregion

    public class TreeDiscover : ITreeDiscover
    {
        private RALogger logger = RALogger.GetInstance(typeof(TreeDiscover));
        private List<DiscoverMailboxEntity> mailboxes = new List<DiscoverMailboxEntity>();
        private List<string> accountNames = new List<string>();

        public AuthorizationManager AuthorizationManager { get; set; }

        /// <summary>
        /// Parse tree.
        /// </summary>
        /// <param name="backupMessage"></param>
        /// <returns>In case one process, multiple mailboxes.</returns>
        public List<DiscoverMailboxEntity> Discover(ExchangeOnlineMessage backupMessage)
        {
            logger.Info("Start to discover tree.");
            List<DiscoverMailboxEntity> mailboxEntries = DiscoverTreeNodes(backupMessage, backupMessage.TreeNode);
            backupMessage.ConfigForMedia.UserAddressList = accountNames;
            return mailboxEntries;
        }

        private List<DiscoverMailboxEntity> DiscoverTreeNodes(ExchangeOnlineMessage backupMessage, ExchangeOnlineTreeNodeDto treeNode)
        {
            using (new AvePerformanceScope("ExchangeOnlineBackup.TreeDiscover.DiscoverTreeNode"))
            {
                if (treeNode.Level == NodeLevel.ExchangeOnlineMailboxGroup)
                {
                    backupMessage.ConfigForMedia.GroupName = treeNode.DisplayName;
                }
                foreach (ExchangeOnlineTreeNodeDto subNode in treeNode.Children)
                {
                    if (subNode.Level == NodeLevel.ExchangeOnlineFarm || subNode.Level == NodeLevel.ExchangeOnlineMailboxGroup || subNode.Level == NodeLevel.ExchangeOnlineO365GroupGroup)
                    {
                        DiscoverTreeNodes(backupMessage, subNode);
                    }
                    else
                    {
                        accountNames.Add(subNode.DisplayName);
                        mailboxes.Add(new DiscoverMailboxEntity(subNode));
                    }
                }
                return mailboxes;
            }
        }

        public int GetTotalCount(List<DiscoverMailboxEntity> mailboxes)
        {
            using (new AvePerformanceScope("ExchangeOnlineBackup.TreeDiscover.GetTotalCount"))
            {
                int result = mailboxes.Count;
                foreach (var mailboxEntiry in mailboxes)
                {
                    result += GetAllFoldersCount(mailboxEntiry);
                }
                logger.Info("Folder count : {0}", result);
                return result;
            }
        }

        private int GetAllFoldersCount(DiscoverMailboxEntity mailboxEntity)
        {
            try
            {
                var mailbox = mailboxEntity.ToExchangeMailbox();
                if (mailbox.IsPublicFolder) return 1;

                GlobalExchangeSetting.SetImpersonateIdToDictionary(mailboxEntity.MailboxAddress, AuthorizationManager);
                var rootFolder = new ExchangeRootFolder(mailbox, AuthorizationManager.GetAuthObjectForEWS(mailboxEntity.MailboxAddress));
                //mailbox.SetImpersonateId(mailboxEntity.mailboxAddress);
                rootFolder.Open();
                //AccessConfig.UseImpersonateList[mailboxEntity.mailboxAddress] = useImpersonate;
                //return mailbox.SyncSubFolders(string.Empty);
                return rootFolder.ChildFolderCount;
            }
            catch (ExchangeUtility.Graph.AccessdeniedException ex)
            {
                logger.Error("Get mailbox {1} all folders count with AccessdeniedException : {0}", ex.ToString(), mailboxEntity.MailboxAddress);
                mailboxEntity.specifiedException = ex.Message;
                return 0;
            }
            catch (Exception ex)
            {
                logger.Warn("Get mailbox {1} all folders count with exception : {0}", ex.ToString(), mailboxEntity.MailboxAddress);
                return 0;
            }
        }
    }
}