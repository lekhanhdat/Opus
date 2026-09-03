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


using System.Collections.ObjectModel;
using AutoInstallation.Contract;
using AutoInstallation.Contract.Interface;
using AutoInstallation.Contract.Interface.Logic;
using AutoInstallation.Contract.Interface.Navigation;
using AutoInstallation.Contract.Navigation;

namespace AutoInstallation.ViewModel.Handler
{
    public abstract class BaseSecondaryInit : IInitialization
    {
        protected ApplicationData ContentData;

        public BaseSecondaryInit(ApplicationData data)
        {
            ContentData = data;
        }

        public void Init()
        {
            BuildNavigationViewModel(ContentData.NavigationViewModel);
        }

        protected virtual void BuildNavigationViewModel(INavigationViewModel data)
        {
            BuildWizardItems(data.Wizards);
        }

        protected abstract void BuildWizardItems(ObservableCollection<LogicWizardItem> items);
        protected abstract LogicWizardItem BuildPreviewItem();
        protected abstract void BuildPreviewViewModel(IPreviewViewModel data);
        protected abstract ILicenseAgreementViewModel BuildLicenseAgreementViewModel();
        protected abstract LogicWizardItem BuildLicenseAgreementWizardItem();
        protected abstract LogicWizardItem BuildInstallProgressItem();
        protected abstract LogicWizardItem BuildFinishItem();

        protected string GetXPath(string namespaceName, string parentPath, string path)
        {
            if (string.IsNullOrEmpty(parentPath))
                return namespaceName + ":" + path;
            return parentPath + "/" + namespaceName + ":" + path;
        }
    }
}