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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.DB.Explorer.Model; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;

namespace AvePoint.RA.DB.Lite.Imple
{
    public class OfflineSearchDao : IOfflineSearchDao
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(OfflineSearchDao));
        private readonly string _defaultDBFilePrefix = "SearchResult_";
        public Tuple<List<Record>, int> Query(ExplorerOfflineResultQueryDto param)
        {
            string jobId = param.JobId;
            string filePath = AvePoint.RA.Common.Util.JobReportUtility.GetSearchResultFilePath(_defaultDBFilePrefix + jobId + ".db");
            if (CheckFile(filePath, jobId))
            {
                using (ExplorerOfflineSearchWrapper wrapper = new ExplorerOfflineSearchWrapper(filePath))
                {
                    int totalCount = param.PagingInfo.Total;
                    if (param.PagingInfo.Total == 0)
                    {
                        totalCount = wrapper.QueryCount();
                    }
                    if (param.OrderColumn == null)
                    {
                        param.OrderColumn = new ExplorerQueryOrderColumn() { Column = new ExplorerQueryColumn() { Name = "leafName" } };
                    }
                    //List<Record> records = wrapper.QueryAll(param.PagingInfo.PageIndex, param.PagingInfo.PageSize, BuildOrderColumn(param.OrderColumn), param.OrderColumn == null ? true :param.OrderColumn.OrderAsc);
                    List<Record> records = wrapper.QueryAllByPage(param.PagingInfo.PageIndex, param.PagingInfo.PageSize, param.OrderColumn);
                    return new Tuple<List<Record>, int>(records, totalCount); 
                }
            }
            return new Tuple<List<Record>, int>(new List<Record>(), 0);
        }

        private bool CheckFile(string filePath, string jobId)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }
                    string blobName = AvePoint.RA.Common.Util.JobReportUtility.GetSearchResultBlobPath(_defaultDBFilePrefix + jobId +".db");
                    logger.Info($"check file path, blob: {blobName}");
                    RAStorageUtil.DownloadReportBlobToFile(blobName, filePath);
                }
                Try2ClearOrphanTempDB(filePath);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return false;
            }
        }

        private void Try2ClearOrphanTempDB(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                FileInfo[] files = fileInfo.Directory.GetFiles("*.db", SearchOption.TopDirectoryOnly);
                foreach (FileInfo fi in files)
                {
                    DateTime temp = fi.LastAccessTime.AddMinutes(60); 
                    if (temp < DateTime.Now && fi.Name != fileInfo.Name)
                    {
                        try
                        {
                            logger.Info($"LAT of search result file, is {temp.ToString()}, need to clear");
                            fi.Delete();
                        }
                        catch 
                        {
                            logger.Info("Failed to delete file.");
                        }
                    }
                }
            }
            catch (Exception e)
            { 
                logger.Warn(e.Message, e);
            }
        }



    }
}
