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
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using DataExportCore;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using StandaloneTool.Model.Common;
using StandaloneTool.Model.Verify;
using StandaloneTool.View.Model.Binding;
using StandaloneTool.View.Model.Command;
using System.ComponentModel;
using System.IO;

namespace StandaloneTool.View.Model.Handler
{
    public class VerifyFileInfoHandler : BackgroundWorkerBase
    {
        private readonly BaseDataContext baseDataContext = BaseDataContext.Instance;
        private readonly ExchangeDataInfo exchangeDataInfo = ExchangeDataInfo.GetInstance();
        private readonly ImportEncryptionKeyViewModel importEncryptionKeyViewModel = ImportEncryptionKeyViewModel.Instance;
        private readonly DatabaseHelper dbHelper = DatabaseHelper.Instance;
        private readonly RALogger logger = RALogger.GetInstance(typeof(VerifyFileInfoHandler));

        public override void Execute()
        {
            InitializeBackgroundWorker(this);
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var instance = (VerifyFileInfoHandler)e.Argument;
            e.Result = instance.ProcessVerify();
        }

        protected override void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (VerifyResult)e.Result;
            if (result == VerifyResult.Success)
            {
                baseDataContext.NavigationOperator.AutoSwitchPageNext();
            }
            else
            {
                UpdateErrorMessage(result);
            }
            importEncryptionKeyViewModel.IsCheckingConfig = false;
        }

        private VerifyResult ProcessVerify()
        {
            importEncryptionKeyViewModel.IsCheckingConfig = true;
            var result = ProcessVerifyFileInfo();
            return result;
        }

        private void UpdateErrorMessage(VerifyResult verifyResult)
        {
            switch (verifyResult)
            {
                case VerifyResult.PathEmpty:
                    importEncryptionKeyViewModel.EncryptionFilePathMsg = I18NEntity.GetString("SATool_EncryptionFilePathEmptyMsg");
                    return;
                case VerifyResult.ZipFilePathError:
                case VerifyResult.FailedWithException:
                    importEncryptionKeyViewModel.EncryptionFilePathMsg = I18NEntity.GetString("SATool_EncryptionFilePathInvalidMsg");
                    return;
                case VerifyResult.IncorrectPwdError:
                    importEncryptionKeyViewModel.EncryptionPwdMsg = I18NEntity.GetString("SATool_EncryptionPwdIncorrectMsg");
                    return;
                case VerifyResult.ZipFileContentInvalid:
                    importEncryptionKeyViewModel.EncryptionFilePathMsg = I18NEntity.GetString("SATool_EncryptionFileContentInvalidMsg");
                    return;
            }
        }

        private VerifyResult ProcessVerifyFileInfo()
        {
            if (string.IsNullOrEmpty(importEncryptionKeyViewModel.EncryptionFilePath))
            {
                return VerifyResult.PathEmpty;
            }
            if (string.IsNullOrEmpty(importEncryptionKeyViewModel.EncryptionPwd))
            {
                return VerifyResult.IncorrectPwdError;
            }

            var verifyResult = UnzipFile(importEncryptionKeyViewModel.EncryptionFilePath);

            if (verifyResult != VerifyResult.Success)
            {
                return verifyResult;
            }

            StoreDataFromExportFile(ref verifyResult);

            return verifyResult;
        }

        private VerifyResult UnzipFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath) || !Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return VerifyResult.ZipFilePathError;
                }

                #region Unzip export db file.
                using (var zipFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var inputStream = new ZipInputStream(zipFileStream))
                {
                    inputStream.Password = importEncryptionKeyViewModel.EncryptionPwd;

                    ZipEntry entry = inputStream.GetNextEntry();
                    if (entry == null) return VerifyResult.ZipFileContentInvalid;

                    var extractZipLocation = Path.Combine(GlobalInfo.ExtractZipLocation, DateTime.UtcNow.Ticks.ToString());
                    Directory.CreateDirectory(extractZipLocation);
                    var extractedFilePath = Path.Combine(extractZipLocation, entry.Name);
                    if (File.Exists(extractedFilePath)) File.Delete(extractedFilePath);

                    using (var outputStream = File.Create(extractedFilePath))
                    {
                        inputStream.CopyTo(outputStream);
                    }

                    GlobalInfo.ExportDBFilePath = extractedFilePath;
                    GlobalInfo.EncryptExportDBPassword = importEncryptionKeyViewModel.EncryptionPwd;
                    return VerifyResult.Success;
                }


                #endregion
            }
            catch (SharpZipBaseException ex) when (ex.Message.Contains("password"))
            {
                logger.Error($"Password error when unzipping file path [{filePath}]: {ex}");
                return VerifyResult.IncorrectPwdError;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when unzip file path [{filePath}]: {ex}");
                return VerifyResult.FailedWithException;
            }
        }

        private void StoreDataFromExportFile(ref VerifyResult verifyResult)
        {
            try
            {
                dbHelper.Open(GlobalInfo.ExportDBFilePath, GlobalInfo.EncryptExportDBPassword);
                var archiverObjects = GlobalInfo.Module == Module.Teams ? GetTeamsArchiver() : dbHelper.GetAllArchiverSites(GlobalInfo.Module);
                exchangeDataInfo.ArchiverObjects.Clear();
                logger.Info($"There are {archiverObjects.Count} archiver sites, module [{GlobalInfo.Module}], file path [{GlobalInfo.ExportDBFilePath}].");
                switch (GlobalInfo.Module)
                {
                    case Module.SharePointOnline:
                    case Module.OneDrive:
                    case Module.Teams:
                        if (!archiverObjects.Any())
                        {
                            return;
                        }
                        exchangeDataInfo.ArchiverObjects.AddRange(archiverObjects);
                        GlobalInfo.EncryptionInfoCache.TryAdd(GlobalInfo.ExportDBFilePath, GlobalInfo.EncryptExportDBPassword);
                        break;
                }
            }
            catch (Exception ex)
            {
                importEncryptionKeyViewModel.IsCheckingConfig = false;
                verifyResult = VerifyResult.ZipFileContentInvalid;
                logger.Error($"An error occurred when store data from export file [{GlobalInfo.ExportDBFilePath}]: {ex}");
            }
        }

        private List<ArchiverSiteMasterIndexExportDto> GetTeamsArchiver()
        {
            var commonTeams = dbHelper.GetAllTeamsArchiver(GlobalInfo.Module);
            var archiverSiteTeams = dbHelper.GetAllTeamsArchiverInArchiverSiteMasterIndex(GlobalInfo.Module);
            var teamsRestoreDoNotArchiverTeams = archiverSiteTeams.Where(_ => !commonTeams.Any(c => c.SiteURL.Equals(_.GroupMailboxAddress))).ToList();
            commonTeams.AddRange(teamsRestoreDoNotArchiverTeams);
            return commonTeams.DistinctBy(x => x.GroupMailboxAddress).ToList();
        }
    }
}
