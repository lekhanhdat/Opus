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
using CommunityToolkit.Mvvm.ComponentModel;
using StandaloneTool.Model.Common;

namespace StandaloneTool.View.Model
{
    public partial class StorageInformationViewModel : ObservableObject
    {
        private static readonly Lazy<StorageInformationViewModel> instance = new();
        public static StorageInformationViewModel Instance => instance.Value;



        #region Properties

        [ObservableProperty]
        private bool isCheckingConfig = false;

        [ObservableProperty]
        private string defaultStorageType = LocationType.MSAzureBlob.GetEnumDescription();

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

        [ObservableProperty]
        private bool skipRestoreDataInAvePointStorage = false;

        #endregion


        public void CleanMessage()
        {
            AccessPoint = string.Empty;
            ContainerName = string.Empty;
            AccountName = string.Empty;
            AccountKey = string.Empty;
            AccessPointMsg = string.Empty;
            ContainerNameMsg = string.Empty;
            AccountNameMsg = string.Empty;
            AccountKeyMsg = string.Empty;
            SkipRestoreDataInAvePointStorage = false;
        }

        public void InitErrorMessage()
        {
            AccessPointMsg = string.Empty;
            ContainerNameMsg = string.Empty;
            AccountNameMsg = string.Empty;
            AccountKeyMsg = string.Empty;
        }
    }
}
