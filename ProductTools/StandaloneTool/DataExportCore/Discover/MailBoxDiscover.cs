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
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Discover.Base;
using DataExportCore.Discover.Node;
using DataExportCore.Utils;
using Merged18NResources.MediaServiceExchangeBackUp;
namespace DataExportCore.Discover
{
    public class MailBoxDiscover : TeamsDiscoveryBase<ExchangeDiscoverNode>
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(MailBoxDiscover));

        public MailBoxDiscover(string groupAddress, IndexDatabaseHelper dbHelper, IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor, string siteUrl)
            : base(groupAddress, dbHelper, IndexProcessor, siteUrl)
        {
        }

        public void Process()
        {
            try
            {
                logger.Info($"Starting the process of discovering mail box {GroupAddress}");

                LoadAllDataEncryptionInfo();

                var mailBoxes = GetMailBoxNodes();
                mailBoxes.ForEach(ExportQueue.Enqueue);

                Task.Run(() => ProcessMailBox(mailBoxes)).Wait();
                ExportQueue.Finish();
                logger.Info("Finished processing the mail box export queue.");
            }
            catch(Exception e)
            {
                logger.Error($"An error occur while discover mailbox {GroupAddress}. Ex: {e}");
            }
        }

        private void ProcessMailBox(List<MailBoxDiscoveryNode> mailBoxes)
        {
            foreach(var mailBox in mailBoxes)
            {
                try
                {
                    mailBox.SitePath = SiteUrl;
                    var itemNodes = GetItemNodes(mailBox);
                    foreach (var item in itemNodes)
                    {
                        ExportQueue.Enqueue(item);
                    }
                    logger.Info($"[{mailBox.Level}][{mailBox.PathMD5}] Processed mail box node: {mailBox.Name} successfully.");
                }
                catch (Exception e)
                {
                    logger.Error($"[{mailBox.Level}][{mailBox.PathMD5}] An error occurred while processing mail box node: {mailBox.Name}. Error: {e}");

                }
            }
        }

        private List<MailDiscoveryNode> GetItemNodes(MailBoxDiscoveryNode mailBox)
        {
            var indexes = GetItemNodesByParentPath(mailBox.PathMD5);
            return indexes.Select(_ => new MailDiscoveryNode(_, IndexProcessor, GroupAddress, mailBox.SitePath, mailBox.Name)).ToList();
        }

        private List<ExchangeBasicIndex> GetItemNodesByParentPath(string parentMD5Path)
        {
            var parameters = new Dictionary<string, object>();
            var sql = "select * from " + IndexConstants.TableNameExchangeItem + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 group by COL_PATH_MD5 order by COL_BACKUP_TIME desc";
            parameters.Add("@COL_PARENT_PATH_MD5", parentMD5Path);
            logger.Info(MediaServiceExchangeBackupResource.ExchangeContainerAndItemIndexServiceSearchStartExecutingStructuredQueryLanguage, sql.ToString(), CollectionExpand.Expand(parameters));
            return IndexProcessor.ExecuteQuery<ExchangeBasicIndex>(sql.ToString(), parameters);
        }

        private List<MailBoxDiscoveryNode> GetMailBoxNodes()
        {
            var sql = "select COL_PATH_MD5,COL_NAME from " + IndexConstants.TableNameExchangeContainer + " group by COL_PATH_MD5 order by COL_BACKUP_TIME desc";
            var parameters = new Dictionary<string, object>();
            List<MailBoxDiscoveryNode> result = new();
            var infoList = IndexProcessor.ExecuteQuery<ExchangeBasicIndex>(sql, parameters);
            foreach (var info in infoList)
            {
                result.Add(new MailBoxDiscoveryNode(info));
            }
            return result;
        }
    }
}
