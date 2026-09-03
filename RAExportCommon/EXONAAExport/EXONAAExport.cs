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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Wrapper.Backup;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Media.StorageService;
using LOGRESOURCE = Merged18NResources.Export;
using LOGRESOURCEInternationalization = Merged18NResources.ExportForInternationalization;
using System.Reflection;
using AvePoint.Wrapper.Common;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Exchange.WebServices.Data;
using System.Security.Cryptography;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;
using ExchangeBackupUtility.Graph;

namespace RAExportCommon
{
    internal class EXONAAExport : EXOExportBase, IEXOExport
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool HasInitObjs = false;
        private EXONAAColumnContainer columnArray = null;
        public List<CsvMetaData> csvMetadatas = new List<CsvMetaData>();
        private EXOExportInfo medataFileinfo = null;
        private string mDisposalClass = string.Empty;
        private readonly object lockObj = new object();
        public string DisposalClass
        {
            get
            {
                return mDisposalClass;
            }
        }
        private void InitObjects()
        {
            HasInitObjs = true;
            columnArray = new EXONAAColumnContainer();
        }

        public EXONAAExport(PhysicalDeviceDto deviceDto, string jobId, string disposalClass, VaultExportFormat format, byte[] EXONAAConfigFile, byte[] encryptionKey, byte[] encryptionIV)
            : base(deviceDto, jobId, format, encryptionKey, encryptionIV)
        {
            InitClass(EXONAAConfigFile, disposalClass);
        }

        private void InitClass(byte[] exonaaConfigFile, string disposalClass)
        {
            EXONAAData.InitConfig(exonaaConfigFile);
            mDisposalClass = disposalClass;
        }

        public EXONAAExport(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, string disposalClass, VaultExportFormat format, byte[] EXONAAConfigFile, byte[] encryptionKey, byte[] encryptionIV)
            : base(spoDto, user, siteUrl, jobId, format, encryptionKey, encryptionIV)
        {
            InitClass(EXONAAConfigFile, disposalClass);
        }

        public ExportStatus ExportEXOMailBox(Mailbox EXOMailbox, EXOExportInfo info)
        {
            ExportStatus exportStatus = new ExportStatus();
            if (!HasInitObjs)
            {
                InitObjects();
            }
            CsvMetaData metaData = new CsvMetaData
            {
                CsvMetadataInfo = columnArray.GetCSVColumnHeadList()
            };
            this.csvMetadatas.Add(metaData);
            exportStatus.State = ExportState.Succeed;
            return exportStatus;
        }

        public ExportStatus ExportEXOFolder(Folder EXOFolder, EXOExportInfo info)
        {
            mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export EXOFolder(NAA).", EXOFolder.Id.ToString());
            ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
            lock (lockObj)
            {
                if (!HasInitObjs)
                {
                    InitObjects();
                }
                if (this.medataFileinfo == null)
                {
                    this.medataFileinfo = new EXOExportInfo() { ContentFilePath = info.MetaDataFileName, FolderPath = info.MetaDataFilePath };
                }
            }
            string filePath = Path.Combine(info.FolderPath, info.ContentFilePath);
            CsvMetaData metaData = new CsvMetaData
            {
                CsvMetadataInfo = columnArray.GetCSVListFromColumnValue(EXOFolder, this.DisposalClass, filePath)
            };
            this.csvMetadatas.Add(metaData);
            exportStatus.State = ExportState.Succeed;
            return exportStatus;
        }

        public ExportStatus ExportEXOItem(Item EXOItem, EXOExportInfo info)
        {
            mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export EXOItem(NAA).", EXOItem.Id.ToString());
            try
            {
                ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                lock (lockObj)
                {
                    if (!HasInitObjs)
                    {
                        InitObjects();
                    }
                    if (this.medataFileinfo == null)
                    {
                        this.medataFileinfo = new EXOExportInfo() { ContentFilePath = info.MetaDataFileName, FolderPath = info.MetaDataFilePath };
                    }
                }
                string tempFilePath = ExchangeUtils.GetEXOItemLocalMSGFilePath(info.JobID, EXOItem.Id.ToString(), info.service);
                using (Stream docStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    ExportInfo contentInfo = new ExportInfo();
                    //keep datetime
                    contentInfo.Created = EXOItem.DateTimeCreated.ToLocalTime();
                    contentInfo.Modified = EXOItem.LastModifiedTime.ToLocalTime();
                    //export content
                    ExportResultInfo result = RealVaultExport.ExportContent(contentInfo, info, docStream);
                    exportStatus.ExportSize += result.Size;
                }
                string exportPath = Path.Combine(info.FolderPath, info.ContentFilePath);
                columnArray.RevIMColumnName = info.Extension;
                //export csv medata
                CsvMetaData metaData = new CsvMetaData
                {
                    CsvMetadataInfo = columnArray.GetCSVListFromColumnValue(EXOItem, this.DisposalClass, exportPath, info.MailFullPath, tempFilePath, info.JobID, info.service)
                };
                this.csvMetadatas.Add(metaData);
                exportStatus.State = ExportState.Succeed;
                return exportStatus;
            }
            catch (Exception ex)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export ExportEXOItem(NAA).", EXOItem.Id.ToString(), ex.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = ex.Message.ToString() };
            }
        }
        
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public void ExtensionMethod(params object[] parameter)
        {
            //to do rule test..
            //headline
            using (AvePerformanceScope pc = new AvePerformanceScope("EXONAAExport_ExtensionMethod"))
            {
                List<CsvMetaData> metadataObjs = parameter[0] as List<CsvMetaData>;
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(new Byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);
                    bool WriteHead = false;
                    foreach (var metadata in metadataObjs)
                    {
                        if (!WriteHead)
                        {
                            string head = Generate(metadata, true);
                            byte[] headLine = Encoding.UTF8.GetBytes(head);
                            stream.Write(headLine, 0, headLine.Length);
                            WriteHead = true;
                        }
                        string content = Generate(metadata, false);
                        byte[] contentLine = Encoding.UTF8.GetBytes(content);
                        stream.Write(contentLine, 0, contentLine.Length);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    RealVaultExport.ExportContent(new ExportInfo(), this.medataFileinfo, stream);
                }
            }
        }
        private string Generate(MetaData metaData, Boolean isHeaderLine)
        {
            Char Comma = '\u002C';

            /// <summary>
            /// Quote ASCII code is 34
            /// </summary>
            Char Quote = '\u0022';
            var csvMetadataInfo = new StringBuilder();
            var properties = metaData.CsvMetadataInfo;
            for (Int32 i = 0; i < properties.Count; i++)
            {
                var origainalStringValue = isHeaderLine ? properties[i].Name : properties[i].Value;
                origainalStringValue = string.IsNullOrEmpty(origainalStringValue) ? string.Empty : origainalStringValue;
                var stringValue = origainalStringValue.Contains("\"") ? origainalStringValue.Replace("\"", "\"\"") : origainalStringValue;
                if (i < properties.Count - 1)
                    csvMetadataInfo.AppendFormat("{0}{1}{0}{2}", Quote, stringValue, Comma);
                else csvMetadataInfo.AppendFormat("{0}{1}{0}{2}", Quote, stringValue, Environment.NewLine);
            }
            return csvMetadataInfo.ToString();
        }
        public void Dispose()
        {
            try
            {
                this.csvMetadatas.Clear();
            }
            catch (Exception ex)
            {
                mLog.Warn("Dispose metadata error {0}", ex.ToString());
            }
            base.Dispose();
        }

        public List<CsvMetaData> GetCSVMetadata()
        {
            return this.csvMetadatas;
        }

        public ExportStatus ExportEXOItem(IExchangeItem EXOItem, EXOExportInfoV2 info)
        {
            mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export EXOItem(NAA).", EXOItem.ItemId.ToString());
            try
            {
                ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                lock (lockObj)
                {
                    if (!HasInitObjs)
                    {
                        InitObjects();
                    }
                    if (this.medataFileinfo == null)
                    {
                        this.medataFileinfo = new EXOExportInfo() { ContentFilePath = info.MetaDataFileName, FolderPath = info.MetaDataFilePath };
                    }
                }
                string tempFilePath = ExchangeUtils.GetEXOItemLocalMSGFilePath(info.JobID, EXOItem).ExecuteAsyncTask();
                using (Stream docStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    ExportInfo contentInfo = new ExportInfo();
                    //keep datetime
                    contentInfo.Created = EXOItem.Created.ToLocalTime();
                    contentInfo.Modified = EXOItem.Modified.ToLocalTime();
                    //export content
                    ExportResultInfo result = RealVaultExport.ExportContent(contentInfo, info, docStream);
                    exportStatus.ExportSize += result.Size;
                }
                string exportPath = Path.Combine(info.FolderPath, info.ContentFilePath);
                columnArray.RevIMColumnName = info.Extension;
                //export csv medata
                CsvMetaData metaData = new CsvMetaData
                {
                    CsvMetadataInfo = columnArray.GetCSVListFromColumnValue(EXOItem, this.DisposalClass, exportPath, info.MailFullPath, tempFilePath, info.JobID, info.service)
                };
                this.csvMetadatas.Add(metaData);
                exportStatus.State = ExportState.Succeed;
                return exportStatus;
            }
            catch (Exception ex)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export ExportEXOItem(NAA).", EXOItem.ItemId.ToString(), ex.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = ex.Message.ToString() };
            }
        }
    }
}
