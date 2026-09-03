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
using RAFileSystem.FileSystem.Discovery.V1.Analyzer;
using System;
using System.IO;
using System.Reflection;

namespace RAFileSystem.FileSystem.Discovery.V1.Worker
{
    public class FSDiscoveryDataConsumer : IFSDiscoveryDataProcessor
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private FSDiscoveryDataQueue _folderQueue;

        private FSDiscoveryDataAnalyzer _dataAnalyzer;

        public FSDiscoveryDataConsumer(FSDiscoveryDataQueue folderQueue, FSDiscoveryDataAnalyzer dataAnalyzer)
        {
            _folderQueue = folderQueue;
            _dataAnalyzer = dataAnalyzer;
        }

        public void Execute()
        {
            foreach (var dirPath in _folderQueue.EnumerateDequeue())
            {
                ProcessFolder(dirPath);
            }
        }

        private void ProcessFolder(string folderPath)
        {
            try
            {
                foreach (var filePath in Directory.EnumerateFiles(folderPath))
                {
                    ProcessFile(filePath);
                }
            }
            catch (UnauthorizedAccessException uex)
            {
                s_logger.Error($"Access denied to folder: {Path.GetFileName(folderPath).LogBase64()}. Error: {uex.Message}");
            }
            catch (Exception ex)
            {
                s_logger.Error($"Unexpected error while processing folder: {Path.GetFileName(folderPath).LogBase64()}. Error: {ex.Message}");
            }
        }

        private void ProcessFile(string filePath)
        {
            try
            {
                FileInfo file = new FileInfo(filePath);
                if (file.Exists)
                {
                    var tagValues = FSDiscoveryTagRuleService.Instance.GetTagValues(file);
                    _dataAnalyzer.Analyze(file, tagValues);
                }
                else
                {
                    s_logger.Warn($"File does not exist: {Path.GetFileName(filePath).LogBase64()} so skip analyze.");
                }
            }
            catch (UnauthorizedAccessException uex)
            {
                s_logger.Error($"Access denied to file: {Path.GetFileName(filePath).LogBase64()}. Error: {uex.Message}");
            }
            catch (FileNotFoundException fnf)
            {
                s_logger.Error($"File not found: {Path.GetFileName(filePath).LogBase64()}. Error: {fnf.Message}");
            }
            catch (Exception ex)
            {
                s_logger.Error($"Unexpected error while processing file: {Path.GetFileName(filePath).LogBase64()}. Error: {ex.Message}");
            }
        }
    }
}
