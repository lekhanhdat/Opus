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
using DataExportCore;
using StandaloneTool.View.Model.Command;
using System.ComponentModel;
using System.Windows;

namespace StandaloneTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        private BaseDataContext context;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            context = BaseDataContext.Instance;
            DataContext = context;
        }

        public MessageResult PromptExitMessageBox(string summary)
        {
            string title = string.Empty;
            MessageResult result = BaseMessage.Show(new BaseMessageConfig
            {
                FormText = "Title",
                ContentTitle = title,
                ContentSummary = summary,
                MessageType = MessageType.OKCancel,
                MessageIconType = MessageIconType.Exit
            });
            return result;
        }

        private void ShowCloseCancelMessageBox()
        {
            var summary = I18NEntity.GetString("SATool_CloseAppMsg");
            var result = PromptExitMessageBox(summary);
            if (result == MessageResult.OK)
            {
                Application.Current.Shutdown();
            }
            base.CloseButtonEvent();
        }

        private void BaseWindow_Closing(object sender, CancelEventArgs e) => ShowCloseCancelMessageBox();

        protected override void CloseButtonEvent() => ShowCloseCancelMessageBox();
    }
}