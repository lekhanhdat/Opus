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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;
using System.Threading;
using AvePoint.Hybrid.Utility;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.CommonUtil;
using System.Reflection;

using AvePoint.Hybrid.ClientLibrary.SDK;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary;
using AvePoint.RA.Hybrid.Browser.Browser;
using AvePoint.RA.Hybrid.Browser.Contract;
using AvePoint.RA.Hybrid.Browser.Util;
using AvePoint.RA.Common.Global.Utils;
using System.Linq;
using AvePoint.GCommon;

namespace AvePoint.RA.Hybrid.Browser
{
    public class FileSystemBrowser: IBrowser
    {
        private static AvePoint.GCommon.AveLogger logger = AvePoint.GCommon.AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public HybridBrowserType BrowserType => HybridBrowserType.FileSystem;

        public string Browse(string message)
        {
            BrowserResult result = new BrowserResult();
            try
            {
                if (message == null || message.Length == 0)
                {
                    logger.Warn("Invalid parameter, thread exit.");
                    return SerializerHelper.SerializeByJsonSerializer(result);
                }

                TreeBrowserArgs args = SerializerHelper.DeserializeByJsonConvert<TreeBrowserArgs>(message);
                List<HBTreeNode> nodes = new List<HBTreeNode>();
                logger.Info("Browser  start type : " + args.Type);

                if (args.Type == (int)TreeBrowserType.Browser)
                {
                    nodes = ListNode(args.RootDir, args.TenantId, args.BatchId);
                }
                else if (args.Type == (int)TreeBrowserType.Validation)
                {
                    nodes = Validation(args.RootDir, args.TenantId, args.BatchId);
                }

                PushNode(nodes);
                result.Result = BrowserResultEnum.Succeed;
            }
            catch (Exception e)
            {
                logger.Error("List folder error : " + e.Message);
                result.Result = BrowserResultEnum.Failed;
                result.Message = e.Message;
            }
            return SerializerHelper.SerializeByJsonSerializer(result);
        }

        public void PushNode(List<HBTreeNode> nodes)
        {
            string apiUrl = CommonConfiguration.getConfig(HybridAppSettingKey.RecordAPIServer);
            string tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                HBNodeRequestInfo nodeRequestInfo = new HBNodeRequestInfo();
                nodeRequestInfo.tenantId = tenantId;
                nodeRequestInfo.nodes = nodes;

                logger.Info("Begin to push node to Api server , api address : " + apiUrl);

                var result = Task.Run(() => HybridAgentApiClientUtil.Client.treeBrowserService.NodeSave(nodeRequestInfo)).Result;

                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                logger.Info("Push node to Api server [ " + ts.Milliseconds + " ] ms");
            }
            catch (Exception e)
            {
                logger.Error("Fail to push node to api server." + e.Message);
                logger.Error(e.ToString());
            }

            logger.Info("Finish to push node to Server, node count : " + nodes.Count);

        }

        public List<HBTreeNode> ListNode(string rootdir, string tenantId, string batchId)
        {
            List<HBTreeNode> fileNodes = new List<HBTreeNode>();

            try
            {
                logger.Info("Begin to list node at url : " + rootdir);

                var _system = StorageUtil.OpenXSystem(rootdir);
                var dirs = _system.ListDirectories(new Media.Storage.StorageInfo());
                logger.Info(String.Join(Environment.NewLine, dirs.Select(r => r.DirFullPath)));
                foreach (var dir in dirs)
                {
                    HBTreeNode node = new HBTreeNode();
                    node.Name = dir.Name;
                    node.Url = dir.OriginalDirFullPath + "\\" + dir.Name;
                    node.BatchId = batchId;
                    node.Id = node.Url;
                    fileNodes.Add(node);
                }

                //// get list of files
                //string[] files = Directory.GetFiles(rootdir);
                //logger.Info(String.Join(Environment.NewLine, files));

                //// get list of directories
                //string[] dirs = Directory.GetDirectories(rootdir);
                //logger.Info(String.Join(Environment.NewLine, dirs));


                //foreach (string dir in dirs)
                //{
                //    HBTreeNode node = new HBTreeNode();
                //    node.Name = dir.Substring(dir.LastIndexOf(@"\") + 1, (dir.Length - dir.LastIndexOf(@"\") - 1));
                //    node.Url = dir;
                //    node.BatchId = batchId;
                //    node.Id = node.Url;
                //    fileNodes.Add(node);
                //}
                if (fileNodes.Count == 0)
                {
                    HBTreeNode node = new HBTreeNode();
                    node.BatchId = batchId;
                    fileNodes.Add(node);
                }
            }
            catch (Exception e)
            {
                logger.Error($"ListNode error, ", e);
                throw e;
            }

            return fileNodes;
        }

        public List<HBTreeNode> Validation(string rootdir, string tenantId, string batchId)
        {
            List<HBTreeNode> fileNodes = new List<HBTreeNode>();
            string tempFileForValidate = System.Guid.NewGuid().ToString() + "." + System.DateTime.Now.Ticks + "_Records.tmp";
            string fileName = rootdir.TrimEnd('\\') + "\\" + tempFileForValidate;
            var validationTestSuccess = true;
            try
            {
                logger.Info("Begin to validate  node file");
                var _system = StorageUtil.OpenXSystem(rootdir);
                if (!_system.DirectoryExists(new Media.Storage.StorageInfo()))
                {
                    validationTestSuccess = false;
                }
                else
                {
                    byte[] bytes = new byte[1];
                    bytes[0] = 0x00;
                    Media.Storage.StorageInfo storageInfo = new Media.Storage.StorageInfo("", tempFileForValidate);
                    using (Stream s = new MemoryStream(bytes))
                    {
                        _system.CommitStream(s, storageInfo);
                    }
                    bool hasException = false;
                    try
                    {
                        _system.DeleteFile(storageInfo);
                    }
                    catch (Exception ex)
                    {
                        hasException = true;
                        logger.Warn($"delete test file error:{ex}");
                    }

                    if (_system.FileExists(storageInfo) || hasException)
                    {
                        validationTestSuccess = false;
                    }
                }
                //if (!Directory.Exists(rootdir))
                //{
                //    validationTestSuccess = false;
                //    //Directory.CreateDirectory(rootdir);
                //}
                //else
                //{
                //    using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                //    {
                //        fs.WriteByte(0x00);
                //    }
                //    bool hasException = false;
                //    try
                //    {
                //        File.Delete(fileName);
                //    }
                //    catch (Exception ex)
                //    {
                //        hasException = true;
                //        logger.Warn($"delete test file error:{ex}");
                //    }

                //    if (File.Exists(fileName) || hasException)
                //    {
                //        validationTestSuccess = false;
                //    }
                //}
            }
            catch (Exception e)
            {
                logger.Warn($"validate test error: ", e);
                validationTestSuccess = false;
            }

            //AveMd5 md5 = new AveMd5();
            HBTreeNode node = new HBTreeNode();
            node.BatchId = batchId;
            if (validationTestSuccess)
            {
                logger.Info("Validate test url success.");
                node.Name = rootdir;
                node.Url = rootdir;
                node.Id = rootdir;
            }
            fileNodes.Add(node);
            return fileNodes;
        }

    }
}
