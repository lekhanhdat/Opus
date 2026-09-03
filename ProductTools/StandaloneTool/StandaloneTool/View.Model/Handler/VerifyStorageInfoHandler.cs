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
using DataExportCore;
using StandaloneTool.Model.Common;
using StandaloneTool.Model.StorageInfo;
using StandaloneTool.Model.Verify;
using StandaloneTool.View.Model.Command;
using System.ComponentModel;

namespace StandaloneTool.View.Model.Handler
{
    public class VerifyStorageInfoHandler : BackgroundWorkerBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(VerifyExportLocationHandler));
        private StorageInformationViewModel storageInfo = StorageInformationViewModel.Instance;
        private BaseDataContext context = BaseDataContext.Instance;
        public override void Execute()
        {
            InitializeBackgroundWorker(this);
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            storageInfo.IsCheckingConfig = true;
            storageInfo.InitErrorMessage();
            var instance = (VerifyStorageInfoHandler)e.Argument;
            e.Result = instance.ProcessVerify();
        }

        protected override void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            storageInfo.IsCheckingConfig = false;
            var result = (VerifyResult)e.Result;
            if (result == VerifyResult.Success)
            {
                context.NavigationOperator.AutoSwitchPageNext();
                return;
            }
            UpdateErrorMessage(result);
        }

        private VerifyResult ProcessVerify()
        {
            if (storageInfo.SkipRestoreDataInAvePointStorage)
            {
                GlobalInfo.IsSkipAPData = true;
                return VerifyResult.Success;
            }
            if (string.IsNullOrWhiteSpace(storageInfo.AccessPoint))
            {
                return VerifyResult.AccessPointEmpty;
            }
            else if (string.IsNullOrWhiteSpace(storageInfo.ContainerName))
            {
                return VerifyResult.ContainerNameEmpty;
            }
            else if (string.IsNullOrWhiteSpace(storageInfo.AccountName))
            {
                return VerifyResult.AccountNameEmpty;
            }
            else if (string.IsNullOrWhiteSpace(storageInfo.AccountKey))
            {
                return VerifyResult.AccountKeyEmpty;
            }

            var azureStorageInfo = new AzureStorageInfo
            {
                AccessPoint = storageInfo.AccessPoint,
                ContainerName = storageInfo.ContainerName,
                AccountName = storageInfo.AccountName,
                AccountKey = storageInfo.AccountKey,
            };

            if (!StorageValidator.ValidateAzureInfo(azureStorageInfo,true)) return VerifyResult.AzureError;

            return VerifyResult.Success;
        }

        private void UpdateErrorMessage(VerifyResult result)
        {
            switch (result)
            {
                case VerifyResult.AccessPointEmpty:
                    storageInfo.AccessPointMsg = I18NEntity.GetString("SATool_AccessPointEmptyMsg");
                    break;
                case VerifyResult.ContainerNameEmpty:
                    storageInfo.ContainerNameMsg = I18NEntity.GetString("SATool_ContainerNameEmptyMsg");
                    break;
                case VerifyResult.AccountNameEmpty:
                    storageInfo.AccountNameMsg = I18NEntity.GetString("SATool_AccountNameEmptyMsg");
                    break;
                case VerifyResult.AccountKeyEmpty:
                    storageInfo.AccountKeyMsg = I18NEntity.GetString("SATool_AccountKeyEmptyMsg");
                    break;
                case VerifyResult.AzureError:
                    storageInfo.AccountKeyMsg = I18NEntity.GetString("SATool_AzureErrorMsg");
                    break;
            }
        }
    }
}
