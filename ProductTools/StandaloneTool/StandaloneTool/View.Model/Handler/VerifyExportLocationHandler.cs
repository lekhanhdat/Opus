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
using AvePoint.Deployment.CommonGUI;
using AvePoint.RA.CommonUtil;
using DataExportCore;
using StandaloneTool.Model.Common;
using StandaloneTool.Model.StorageInfo;
using StandaloneTool.Model.Verify;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace StandaloneTool.View.Model.Handler
{
    public class VerifyExportLocationHandler : BackgroundWorkerBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(VerifyExportLocationHandler));
        private ExportLocationViewModel exportLocationVM = ExportLocationViewModel.Instance;
        public override void Execute()
        {
            InitializeBackgroundWorker(this);
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            exportLocationVM.InitErrorMessage();
            exportLocationVM.IsCheckingConfig = true;
            if (!exportLocationVM.ExportLocation.StartsWith(@"\\"))
            {
                var instance = (VerifyExportLocationHandler)e.Argument;
                e.Result = instance.ProcessVerify();
                return;
            }
            else
            {
                e.Result = ValidateNetShareInfo(exportLocationVM.ExportLocation);
            }
        }

        protected override void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (VerifyResult)e.Result;
            exportLocationVM.IsCheckingConfig = false;
            if (result == VerifyResult.Success)
            {
                ShowExportConfirmation();
            }
            else
            {
                UpdateErrorMessage(result);
            }
        }

        private VerifyResult ProcessVerify()
        {
            if (exportLocationVM.IsSelectedLocal)
            {
                if (string.IsNullOrWhiteSpace(exportLocationVM.ExportLocation))
                {
                    return VerifyResult.PathEmpty;
                }
            }

            if (exportLocationVM.IsSelectedAzure)
            {
                if (string.IsNullOrWhiteSpace(exportLocationVM.AccessPoint))
                {
                    return VerifyResult.AccessPointEmpty;
                }
                else if (string.IsNullOrWhiteSpace(exportLocationVM.ContainerName))
                {
                    return VerifyResult.ContainerNameEmpty;
                }
                else if (string.IsNullOrWhiteSpace(exportLocationVM.AccountName))
                {
                    return VerifyResult.AccountNameEmpty;
                }
                else if (string.IsNullOrWhiteSpace(exportLocationVM.AccountKey))
                {
                    return VerifyResult.AccountKeyEmpty;
                }

                var azureStorageInfo = new AzureStorageInfo
                {
                    AccessPoint = exportLocationVM.AccessPoint,
                    ContainerName = exportLocationVM.ContainerName,
                    AccountName = exportLocationVM.AccountName,
                    AccountKey = exportLocationVM.AccountKey,
                };

                if (!StorageValidator.ValidateAzureInfo(azureStorageInfo))
                {
                    return VerifyResult.AzureError;
                };
            }

            if (exportLocationVM.IsSelectedSftp)
            {
                var result = VerifyResult.Success;

                if (string.IsNullOrWhiteSpace(exportLocationVM.SftpHost))
                {
                    return VerifyResult.SFTPIPEmpty;
                }
                else if (string.IsNullOrWhiteSpace(exportLocationVM.SftpPort))
                {
                    return VerifyResult.SFTPPortEmpty;
                }
                else if (!StorageValidator.IsPortFormatCorrect(exportLocationVM.SftpPort))
                {
                    return VerifyResult.SFTPPortError;
                }
                else if (string.IsNullOrWhiteSpace(exportLocationVM.SftpUsername))
                {
                    return VerifyResult.SFTPUsernameEmpty;
                }
                else if (string.IsNullOrWhiteSpace(exportLocationVM.sftpPasswordCache))
                {
                    if (string.IsNullOrWhiteSpace(exportLocationVM.SftpPrivateKeyFile)
                        && string.IsNullOrWhiteSpace(exportLocationVM.SftpPrivateKeyPassword))
                    {
                        return VerifyResult.SFTPPasswordEmpty;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(exportLocationVM.SftpPrivateKeyFile))
                        {
                            return VerifyResult.SFTPPrivateKeyFileEmpty;
                        }
                        else if (string.IsNullOrWhiteSpace(exportLocationVM.SftpPrivateKeyPassword))
                        {
                            return VerifyResult.SFTPPrivateKeyPasswordEmpty;
                        }
                    }
                }

                var sftpStorageInfo = new SftpStorageInfo
                {
                    Host = exportLocationVM.SftpHost,
                    Port = exportLocationVM.SftpPort,
                    RootFolder = exportLocationVM.SftpFolder,
                    Username = exportLocationVM.SftpUsername,
                    Password = exportLocationVM.sftpPasswordCache,
                    PrivateKeyFile = GlobalInfo.SftpPrivateKeyFileContent,
                    PrivateKeyFilePassword = exportLocationVM.SftpPrivateKeyPassword
                };

                if (!StorageValidator.ValidateSftpInfo(sftpStorageInfo, ref result))
                {
                    return result;
                }
            }

            return VerifyResult.Success;
        }

        private void UpdateErrorMessage(VerifyResult result)
        {
            switch (result)
            {
                //Local location
                case VerifyResult.PathEmpty:
                    exportLocationVM.ExportLocationErrorMsg = I18NEntity.GetString("SATool_ExportLocationErrorMsg");
                    break;

                //Microsoft azure blob
                case VerifyResult.AccessPointEmpty:
                    exportLocationVM.AccessPointMsg = I18NEntity.GetString("SATool_AccessPointEmptyMsg");
                    break;
                case VerifyResult.ContainerNameEmpty:
                    exportLocationVM.ContainerNameMsg = I18NEntity.GetString("SATool_ContainerNameEmptyMsg");
                    break;
                case VerifyResult.AccountNameEmpty:
                    exportLocationVM.AccountNameMsg = I18NEntity.GetString("SATool_AccountNameEmptyMsg");
                    break;
                case VerifyResult.AccountKeyEmpty:
                    exportLocationVM.AccountKeyMsg = I18NEntity.GetString("SATool_AccountKeyEmptyMsg");
                    break;
                case VerifyResult.AzureError:
                    exportLocationVM.AccountKeyMsg = I18NEntity.GetString("SATool_AzureErrorMsg");
                    break;

                //SFTP
                case VerifyResult.SFTPIPEmpty:
                    exportLocationVM.SftpHostMsg = I18NEntity.GetString("SATool_HostErrorMsg");
                    break;
                case VerifyResult.SFTPIPError:
                    exportLocationVM.SftpHostMsg = I18NEntity.GetString("SATool_HostErrorMsg");
                    break;
                case VerifyResult.SFTPPortEmpty:
                    exportLocationVM.SftpPortMsg = I18NEntity.GetString("SATool_PortEmptyMsg");
                    break;
                case VerifyResult.SFTPPortError:
                    exportLocationVM.SftpPortMsg = I18NEntity.GetString("SATool_PortErrorMsg");
                    break;
                case VerifyResult.SFTPFolderPathInvalid:
                    exportLocationVM.SftpFolderMsg = I18NEntity.GetString("SATool_SftpFolderInvalidMsg");
                    break;
                case VerifyResult.SFTPUsernameEmpty:
                    exportLocationVM.SftpUsernameMsg = I18NEntity.GetString("SATool_UsernameEmptyMsg");
                    break;
                case VerifyResult.SFTPAuthenticationException:
                    exportLocationVM.SftpUsernameMsg = I18NEntity.GetString("SATool_SftpAuthenErrorMsg");
                    break;
                case VerifyResult.SFTPPasswordEmpty:
                    exportLocationVM.SftpPasswordMsg = I18NEntity.GetString("SATool_PasswordEmptyMsg");
                    break;
                case VerifyResult.SFTPPrivateKeyFileEmpty:
                    exportLocationVM.SftpPrivateKeyFileMsg = I18NEntity.GetString("SATool_EncryptionFilePathInvalidMsg");
                    break;
                case VerifyResult.SFTPPrivateKeyFileInvalid:
                    exportLocationVM.SftpPrivateKeyFileMsg = I18NEntity.GetString("SATool_EncryptionFileContentInvalidMsg");
                    break;
                case VerifyResult.SFTPPrivateKeyPasswordEmpty:
                    exportLocationVM.SftpPrivateKeyPasswordMsg = I18NEntity.GetString("SATool_PasswordEmptyMsg");
                    break;
                case VerifyResult.SFTPPrivateKeyPasswordInvalid:
                    exportLocationVM.SftpPrivateKeyPasswordMsg = I18NEntity.GetString("SATool_EncryptionFilePathInvalidMsg");
                    break;

                //Netshare
                case VerifyResult.NetShareError:
                    exportLocationVM.ExportLocationErrorMsg = I18NEntity.GetString("SATool_NetShareErrorMsg");
                    break;
                case VerifyResult.NetSharePathInvalid:
                    exportLocationVM.ExportLocationErrorMsg = I18NEntity.GetString("SATool_NetSharePathInvalidMsg");
                    break;
            }
        }

        private VerifyResult ValidateNetShareInfo(string sharedPath)
        {
            try
            {
                string remoteHostName = sharedPath.Split('\\')[2];
                IPAddress[] ipAddresses = Dns.GetHostAddresses(remoteHostName);

                if (!ipAddresses.Any())
                {
                    return VerifyResult.NetShareError;
                }

                var targetIP = ipAddresses.First();

                using (Ping pingSender = new Ping())
                {
                    PingReply reply = pingSender.Send(targetIP);

                    if (reply.Status == IPStatus.Success)
                    {
                        if (!Directory.Exists(sharedPath))
                        {
                            return VerifyResult.NetSharePathInvalid;
                        }
                        return VerifyResult.Success;
                    }
                    else
                    {
                        logger.Warn($"Cannot ping the target IP of remote host [{remoteHostName}], ping status [{reply.Status}].");
                        return VerifyResult.NetShareError;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when connect to remote host. Ex: {ex.Message}");
                return VerifyResult.NetShareError;
            }
        }

        public MessageResult GetExportConfirmationResult(string summary)
        {
            string title = string.Empty;
            MessageResult result = BaseMessage.Show(new BaseMessageConfig
            {
                FormText = "Title",
                ContentTitle = title,
                ContentSummary = summary,
                MessageType = MessageType.YesNo,
                MessageIconType = MessageIconType.Exit
            });
            return result;
        }

        public void ShowExportConfirmation()
        {
            var summary = I18NEntity.GetString("SATool_ExportPromptMsg");
            var result = GetExportConfirmationResult(summary);
            if (result == MessageResult.Yes)
            {
                new ExportProcessHandler().Execute();
            }
        }
    }
}
