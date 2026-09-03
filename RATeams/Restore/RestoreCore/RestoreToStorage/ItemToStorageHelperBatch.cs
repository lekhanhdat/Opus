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

//namespace Office365GroupRestore
//{
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
//    using ExchangeCommonWrapper;
//    using ExchangeUtility.Graph;
//    using Job.ModernManagement.Report;

//    public class ItemToStorageHelperBatch : ItemRestoreHelperBatch
//    {
//        protected override bool NeedRestore() => true;

//        protected override void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataCollection)
//        {
//            var dataSource = dataCollection.First().FileHeader.NodeType == 909 ? DataSource.EWS : DataSource.Graph;
//            var instance = (RestoreConversationAsHtml)RestoreConversationFactory.Create(dataSource, RestoreConversationType.Html);

//            try
//            {
//                var (folderName, fileName) = GenerateFileInfo(dataCollection.First(), instance);

//                using (var content = instance.GenerateConversationHtml(dataCollection))
//                {
//                    WriteToLocal(content, Path.Combine(folderName, fileName));
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Error("Restore to storage error: {0}.", ex);
//                ReportDto.Status = ReportStatus.Failed;
//                ReportDto.ErrorMessage = ex.Message;
//                Report.AddReport(ReportDto);
//            }

//            dataCollection.ForEach(disposeStream => disposeStream.Dispose());
//        }

//        private (string, string) GenerateFileInfo(ExchangeDataBlockForBatch datablock, RestoreConversationAsHtml htmlOjbect)
//        {
//            var metadata = datablock.RestoreData.Metadata;

//            var channelName = string.Empty;
//            if(CurrentChannel?.DisplayName is not null)
//            {
//                channelName = CurrentChannel.DisplayName;
//            } 
//            else
//            {
//                var parentFullPathInfo = datablock.FileHeader.ParentFullPath.Split(ExchangeConstants.PathParser);
//                channelName = string.IsNullOrEmpty(parentFullPathInfo[1]) ? "General" : parentFullPathInfo[1];
//            }
//            var fileName = htmlOjbect.GenerateFileName(metadata, channelName);

//            return (RestoreToStorageConstants.HtmlFilesParentPath, fileName);
//        }

//        private void WriteToLocal(MemoryStream content, string fileFullName)
//        {
//            using (var fileStream = new FileStream(fileFullName, FileMode.OpenOrCreate))
//            {
//                content.WriteTo(fileStream);
//            }
//            RestoreToStorageConstants.HtmlFiles.Add(fileFullName);
//            logger.Info("Record html file: {0}.", fileFullName);
//        }
//    }
//}