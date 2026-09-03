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
using StandaloneTool.Model;
using System.Collections.ObjectModel;
using static AvePoint.Deployment.CommonGUI.PageWizardItem;

namespace StandaloneTool.View.Model.Binding
{
    public class WizardUnitDataInfo : BaseINotifyPropertyChanged
    {
        private static readonly Lazy<WizardUnitDataInfo> instance = new();

        private ObservableCollection<WizardItemInfo> _wizardCollection;

        public static WizardUnitDataInfo GetInstance() => instance.Value;

        public ObservableCollection<WizardItemInfo> WizardCollection { get => _wizardCollection; set => _wizardCollection = value; }

        public WizardUnitDataInfo() => WizardCollection = InitWizardCollection();

        public int WizardCollectionSize => _wizardCollection.Count;

        public static ObservableCollection<WizardItemInfo> InitWizardCollection()
        {
            return new ObservableCollection<WizardItemInfo>
            {
                new()
                {
                    Content = "01",
                    IsVisibility = true,
                    IsEnabled = true,
                    UnitState = WizardUnitState.Configuring,
                    IsConfigured = false,
                    PageLocation = "ImportEncryptionKeyPage.xaml",
                    PageFeatures = PageFeatures.ImportEncryptionKeyPage,
                },
                new()
                {
                    Content = "02",
                    IsVisibility = true,
                    IsEnabled = true,
                    UnitState = WizardUnitState.Waiting,
                    IsConfigured = false,
                    PageLocation = "RecoverDataPage.xaml",
                    PageFeatures = PageFeatures.RecoveryPage,
                },
                new()
                {
                    Content = "03",
                    IsVisibility = true,
                    IsEnabled = true,
                    UnitState = WizardUnitState.Waiting,
                    IsConfigured = false,
                    PageLocation = "StorageInformationPage.xaml",
                    PageFeatures = PageFeatures.StorageInformationPage,
                },
                new()
                {
                    Content = "04",
                    IsVisibility = true,
                    IsEnabled = true,
                    UnitState = WizardUnitState.Waiting,
                    IsConfigured = false,
                    PageLocation = "ExportLocationPage.xaml",
                    PageFeatures = PageFeatures.ExportLocationPage,
                },
                new()
                {
                    Content = "05",
                    IsVisibility = true,
                    IsEnabled = true,
                    UnitState = WizardUnitState.Waiting,
                    IsConfigured = false,
                    PageLocation = "ProcessPage.xaml",
                    PageFeatures = PageFeatures.ProcessPage,
                },
                 new()
                {
                    Content = "06",
                    IsVisibility = true,
                    IsEnabled = true,
                    UnitState = WizardUnitState.Finished,
                    IsConfigured = false,
                    PageLocation = "FinishPage.xaml",
                    PageFeatures = PageFeatures.FinishPage,
                },
            };
        }

    }
}
