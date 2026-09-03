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
namespace StandaloneTool.View;

using AvePoint.RA.CommonUtil;
using StandaloneTool.View.Model;
using StandaloneTool.View.Model.Command;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

public partial class StorageInformationPage : Page
{
    private RALogger logger = RALogger.GetInstance(typeof(StorageInformationPage));
    private readonly StorageInformationViewModel storageInformationViewModel = StorageInformationViewModel.Instance;
    private readonly BaseDataContext context = BaseDataContext.Instance;

    public StorageInformationPage()
    {
        InitializeComponent();
        DataContext = storageInformationViewModel;
        storageInformationViewModel.CleanMessage();
        storageInformationViewModel.IsSelectedAzure = true;
    }

    private void StackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        context.NextOperator.Command.OnCanExecuteChanged();
        context.BackOperator.Command.OnCanExecuteChanged();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        e.Handled = true;
    }
}