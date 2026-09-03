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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class SPMoveRestore: IExplorerMoveRestore
    {
        private RALogger logger = RALogger.GetInstance(typeof(SPMoveRestore));

        internal SPImport spImport;

        public SPMoveRestore(MoveDestination destinationInfo, MoveSettingInfo moveSetting, AppendItemMapping appendMapping)
        {
            spImport = new SPImport(destinationInfo, appendMapping, moveSetting);
        }

        public async Task<JobResult> RestoreAsync(SourceBase source)
        {
            var jobResult = new JobResult();
            if (source.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.FSFile
                || source.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.FSFolder)
            {
            }
            else
            {
                jobResult = await RestoreSPAsync(source);
            }
            return jobResult;
        }

        private async Task<JobResult> RestoreSPAsync(SourceBase source)
        {
            var jobResult = new JobResult();
            using (RAFileReceiver fileReceiver = new RAFileReceiver(source.ExportFilePath))
            {
                var fileReceiveWrapper = new FileReceiverWrapper(fileReceiver);
                using (var importStream = new WrapperRestoreStreamV1(fileReceiveWrapper))
                {
                    try
                    {
                        var spSource = source as SPSource;
                        spImport.Init(importStream, spSource.IsFirstItem);
                        spImport.RestoreParentInfo();
                        jobResult = await spImport.RestoreAveSPDocAsync(spSource);
                        //?
                        //if (isFirstVersion)
                        //{
                        //    spImport.SetDestAveSPFolder(spImport.GetParentFolder);
                        //}
                    }
                    catch (ConetentSkipException contentExp)
                    {
                        jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                        jobResult.ErrorMessage = contentExp.ToString();
                        logger.Warn(string.Format("Content Skip: FileName: {0}, reason: {1}.", source.FileName,contentExp.ToString()));
                        throw;
                    }
                    //File length exceed 128 catch exception
                    catch (PathTooLongException e)
                    {
                        jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                        jobResult.ErrorMessage = e.ToString();
                        logger.Warn(string.Format("File name or list URL is too long. Reason : {0}.", e.ToString()));
                        throw;
                    }
                    catch (SkipException skipEx)
                    {
                        jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped;
                        jobResult.ErrorMessage = skipEx.ToString();
                        logger.Warn("Content Type Or Column conflict, Skip Current file : {0}, Message : {1}", source.FileName, skipEx.ToString());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                        jobResult.ErrorMessage = ex.ToString();
                        logger.Error("Error in move to destination library," + ex.ToString());
                        throw;
                    }
                }
            }
            return jobResult;
        }

        public void Dispose()
        {
            using(spImport as IDisposable) { }
        }

        public string UpdateBCSColumn(bool useExisting, string columnName, Guid termId)
        {
            return spImport.UpdateBCSColumn(useExisting, columnName, termId);
        }

        public Guid GetDestinationTermId(string columnName)
        {
            return spImport.GetDestinationTermId(columnName);
        }

        public Guid UpdateClassificationColumnWithDestination(bool useExisting, string columnName, bool forceSetNull = false)
        {
            return spImport.UpdateClassificationColumnWithDestination(useExisting, columnName, forceSetNull);
        }
    }
}
