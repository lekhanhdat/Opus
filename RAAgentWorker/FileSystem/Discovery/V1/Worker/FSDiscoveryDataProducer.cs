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
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RAFileSystem.FileSystem.Discovery.V1.Worker
{
    public class FSDiscoveryDataProducer : IFSDiscoveryDataProcessor
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private FSDiscoveryDataQueue _folderQueue;

        private string _rootPath;

        public FSDiscoveryDataProducer(FSDiscoveryDataQueue queue, string rootPath)
        {
            _folderQueue = queue;
            _rootPath = string.Format("{0}{1}", @"\\?\UNC\", rootPath.TrimStart('\\')); ;
        }

        public void Execute()
        {
            var dirPath = string.Empty;
            try
            {
                var tempQueue = new Queue<string>();
                tempQueue.Enqueue(_rootPath);
                while (tempQueue.Count > 0)
                {
                    dirPath = tempQueue.Dequeue();
                    _folderQueue.Enqueue(dirPath);
                    try
                    {
                        var subDirs = Directory.GetDirectories(dirPath);
                        foreach (var subDir in subDirs)
                        {
                            tempQueue.Enqueue(subDir);
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        s_logger.Error($"Access denied to: {dirPath.LogBase64()}, skipping. Ex: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred while producing data, path [{dirPath.LogBase64()}]. Ex: {ex.Message}");
            }
            finally
            {
                _folderQueue.Complete();
            }
        }
    }
}
