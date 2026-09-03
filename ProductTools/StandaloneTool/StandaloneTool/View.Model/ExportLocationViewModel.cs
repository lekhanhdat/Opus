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
using AvePoint.RA.Common.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataExportCore;
using Microsoft.Win32;
using StandaloneTool.Common;
using StandaloneTool.Model;
using StandaloneTool.Model.Common;
using StandaloneTool.View.Model.Command;
using System.IO;
using Location = StandaloneTool.Model.Common.LocationType;

namespace StandaloneTool.View.Model
{
    public partial class ExportLocationViewModel : ObservableObject
    {
        private static readonly Lazy<ExportLocationViewModel> instance = new();
        public static ExportLocationViewModel Instance => instance.Value;

        private readonly BaseDataContext context = BaseDataContext.Instance;
        private readonly StringVerification checker = new();


        [ObservableProperty]
        private string locationType = Location.LocalLocation.ToDescription();

        [ObservableProperty]
        private bool isCheckingConfig = false;

        #region Local location

        [ObservableProperty]
        private bool isSelectedLocal = false;
        [ObservableProperty]
        private string exportLocation = string.Empty;
        [ObservableProperty]
        private string exportLocationErrorMsg = string.Empty;
        [ObservableProperty]
        private string failedFilesErrorMessage = string.Empty;
        [ObservableProperty]
        private bool failedFilesIsExist;

        #endregion

        #region Microsoft Azure Blob

        [ObservableProperty]
        private bool isSelectedAzure = false;

        [ObservableProperty]
        private string accessPoint = string.Empty;

        [ObservableProperty]
        private string accessPointMsg = string.Empty;

        [ObservableProperty]
        private string containerName = string.Empty;

        [ObservableProperty]
        private string containerNameMsg = string.Empty;

        [ObservableProperty]
        private string accountName = string.Empty;

        [ObservableProperty]
        private string accountNameMsg = string.Empty;

        [ObservableProperty]
        private string accountKey = string.Empty;

        [ObservableProperty]
        private string accountKeyMsg = string.Empty;

        #endregion

        #region SFTP

        [ObservableProperty]
        private bool isSelectedSftp = false;

        [ObservableProperty]
        private string sftpHost = string.Empty;

        [ObservableProperty]
        private string sftpHostMsg = string.Empty;

        [ObservableProperty]
        private string sftpPort = string.Empty;

        [ObservableProperty]
        private string sftpPortMsg = string.Empty;

        [ObservableProperty]
        private string sftpFolder = string.Empty;

        [ObservableProperty]
        private string sftpFolderMsg = string.Empty;

        [ObservableProperty]
        private string sftpUsername = string.Empty;

        [ObservableProperty]
        private string sftpUsernameMsg = string.Empty;

        [ObservableProperty]
        private string sftpPassword = string.Empty;

        [ObservableProperty]
        private string sftpPasswordMsg = string.Empty;

        [ObservableProperty]
        private string sftpPrivateKeyFile = string.Empty;

        [ObservableProperty]
        private string sftpPrivateKeyFileMsg = string.Empty;

        [ObservableProperty]
        private string sftpPrivateKeyPassword = string.Empty;

        [ObservableProperty]
        private string sftpPrivateKeyPasswordMsg = string.Empty;

        public string sftpPasswordCache = string.Empty;

        #endregion

        #region FTP

        private bool isSelectedFtp = false;
        private string host = string.Empty;
        private string hostMsg = string.Empty;
        private string port = string.Empty;
        private string portMsg = string.Empty;
        private string folder = string.Empty;
        private string folderMsg = string.Empty;
        private string username = string.Empty;
        private string usernameMsg = string.Empty;
        private string passwordMsg = string.Empty;
        private string password = string.Empty;
        private string passwordCache = string.Empty;

        #endregion

        #region Amazon S3

        private bool isSelectedAmazonS3 = false;
        private string amazonS3BucketName = string.Empty;
        private string amazonS3BucketNameMsg = string.Empty;
        private string amazonS3AccessKeyID = string.Empty;
        private string amazonS3AccessKeyIDMsg = string.Empty;
        private string amazonS3SecretAccessKey = string.Empty;
        private string amazonS3SecretAccessKeyMsg = string.Empty;
        private string amazonS3ValidationErrorMsg = string.Empty;
        private string storageRegion = string.Empty;

        #endregion

        #region Amazon S3 Compatible

        private bool isSelectedAmazonS3Compatible = false;
        private string amazonS3CompatibleBucketName = string.Empty;
        private string amazonS3CompatibleBucketNameMsg = string.Empty;
        private string amazonS3CompatibleAccessKeyId = string.Empty;
        private string amazonS3CompatibleAccessKeyIdMsg = string.Empty;
        private string amazonS3CompatibleSecretAccessKey = string.Empty;
        private string amazonS3CompatibleSecretAccessKeyMsg = string.Empty;
        private string amazonS3CompatibleEndpoint = string.Empty;
        private string amazonS3CompatibleEndpointMsg = string.Empty;
        private string amazonS3CompatibleErrorValidationMsg = string.Empty;

        #endregion

        #region Dropbox

        private bool isSelectedDropBox = false;
        private string dropboxRootFolderName = string.Empty;
        private string dropboxRootFolderNameMsg = string.Empty;
        private string dropboxTokenSecret = string.Empty;
        private string dropboxTokenSecretMsg = string.Empty;
        private string dropboxUri = "https://www.dropbox.com/oauth2/authorize?"
                        + "redirect_uri=https://www.avepointonlineservices.com/getcloudtoken/dropbox"
                        + "&client_id=p9kxswndtb7f6gp"
                        + "&response_type=code";

        #endregion


        partial void OnExportLocationChanged(string value)
        {
            ExportLocationErrorMsg = checker.VerifyDirectory(value) ? string.Empty : I18NEntity.GetString("SATool_ExportLocationErrorMsg");
            if (context.NavigationOperator.CurrentPage is not PageFeatures.ExportLocationPage)
            {
                return;
            }
            context.NextOperator.Command.OnCanExecuteChanged();
            context.BackOperator.Command.OnCanExecuteChanged();
        }

        [RelayCommand]
        private void Load()
        {
            context.NextOperator.Command.OnCanExecuteChanged();
            context.BackOperator.Command.OnCanExecuteChanged();
        }

        [RelayCommand]
        private void SelectFolder()
        {
            var folderBrowserDialog = new OpenFolderDialog();
            folderBrowserDialog.ShowDialog();
            if (!string.IsNullOrEmpty(folderBrowserDialog.FolderName))
            {
                ExportLocation = folderBrowserDialog.FolderName;
                GlobalCache.ExportLocation = ExportLocation;
            }
        }

        [RelayCommand]
        private void SelectFile()
        {
            var browserDialog = new OpenFileDialog();
            browserDialog.ShowDialog();
            if (!string.IsNullOrEmpty(browserDialog.FileName))
            {
               SftpPrivateKeyFile = Path.GetFileName(browserDialog.FileName);
               GlobalInfo.SftpPrivateKeyFileContent = File.ReadAllText(browserDialog.FileName);
            }
        }



        public void CleanMessage()
        {
            //Local location
            ExportLocationErrorMsg = string.Empty;
            ExportLocation = string.Empty;

            //Microsoft azure blob 
            AccessPoint = string.Empty;
            ContainerName = string.Empty;
            AccountName = string.Empty;
            AccountKey = string.Empty;
            AccessPointMsg = string.Empty;
            ContainerNameMsg = string.Empty;
            AccountNameMsg = string.Empty;
            AccountKeyMsg = string.Empty;

            //SFTP
            SftpHost = string.Empty;
            SftpPort = string.Empty;
            SftpFolder = string.Empty;
            SftpUsername = string.Empty;
            SftpPassword = string.Empty;
            SftpHostMsg = string.Empty;
            SftpPortMsg = string.Empty;
            SftpFolderMsg = string.Empty;
            SftpUsernameMsg = string.Empty;
            SftpPrivateKeyFile = string.Empty;
            SftpPrivateKeyFileMsg = string.Empty;
            SftpPrivateKeyPassword = string.Empty;
            SftpPrivateKeyPasswordMsg = string.Empty;

            /*    
               if (!IsSelectedFtp)
                {
                    Host = string.Empty;
                    Port = string.Empty;
                    Folder = string.Empty;
                    Username = string.Empty;
                    Password = string.Empty;
                    PasswordCache = string.Empty;
                    HostMsg = string.Empty;
                    PortMsg = string.Empty;
                    FolderMsg = string.Empty;
                    UsernameMsg = string.Empty;
                 }
                 if (!IsSelectedAmazonS3)
                 {
                     AmazonS3BucketName = string.Empty;
                     AmazonS3AccessKeyID = string.Empty;
                     AmazonS3SecretAccessKey = string.Empty;
                     AmazonS3BucketNameMsg = string.Empty;
                     AmazonS3AccessKeyIDMsg = string.Empty;
                     AmazonS3SecretAccessKeyMsg = string.Empty;
                 }
                 if (!IsSelectedAmazonS3Compatible)
                 {
                     AmazonS3CompatibleBucketName = string.Empty;
                     AmazonS3CompatibleAccessKeyId = string.Empty;
                     AmazonS3CompatibleSecretAccessKey = string.Empty;
                     AmazonS3CompatibleEndpoint = string.Empty;
                     AmazonS3CompatibleBucketNameMsg = string.Empty;
                     AmazonS3CompatibleAccessKeyIdMsg = string.Empty;
                     AmazonS3CompatibleSecretAccessKeyMsg = string.Empty;
                     AmazonS3CompatibleEndpointMsg = string.Empty;
                 }
                 if (!IsSelectedDropBox)
                 {
                     DropboxRootFolderName = string.Empty;
                     DropboxTokenSecret = string.Empty;
                     DropboxRootFolderNameMsg = string.Empty;
                     DropboxTokenSecretMsg = string.Empty;
                 }*/
        }

        public void InitErrorMessage()
        {
            ExportLocationErrorMsg = string.Empty;
            AccessPointMsg = string.Empty;
            ContainerNameMsg = string.Empty;
            AccountNameMsg = string.Empty;
            AccountKeyMsg = string.Empty;
            SftpHostMsg = string.Empty;
            SftpPortMsg = string.Empty;
            SftpFolderMsg = string.Empty;
            SftpUsernameMsg = string.Empty;
            SftpPasswordMsg = string.Empty;
            SftpPrivateKeyFileMsg = string.Empty;
            SftpPrivateKeyPasswordMsg = string.Empty;

            /* 
               HostMsg = string.Empty;
               PortMsg = string.Empty;
               FolderMsg = string.Empty;
               UsernameMsg = string.Empty;
               AmazonS3BucketNameMsg = string.Empty;
               AmazonS3AccessKeyIDMsg = string.Empty;
               AmazonS3SecretAccessKeyMsg = string.Empty;
               AmazonS3CompatibleBucketNameMsg = string.Empty;
               AmazonS3CompatibleAccessKeyIdMsg = string.Empty;
               AmazonS3CompatibleSecretAccessKeyMsg = string.Empty;
               AmazonS3CompatibleEndpointMsg = string.Empty;
               DropboxRootFolderNameMsg = string.Empty;
               DropboxTokenSecretMsg = string.Empty;
            */
        }

        public void RevertSelection()
        {
            IsSelectedLocal = false;
            IsSelectedAzure = false;
            IsSelectedSftp = false;
            /*
            IsSelectedFtp = false;
            IsSelectedAmazonS3 = false;
            IsSelectedAmazonS3Compatible = false;
            IsSelectedDropBox = false;
            */
        }

    }
}
