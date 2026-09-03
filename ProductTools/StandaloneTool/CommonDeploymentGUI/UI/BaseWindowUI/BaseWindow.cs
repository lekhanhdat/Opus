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

namespace AvePoint.Deployment.CommonGUI
{
    #region ---namespace---

    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    #endregion

    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:AvePoint.CallAssist.CommonDeploymentGUI"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:AvePoint.CallAssist.CommonDeploymentGUI;assembly=AvePoint.CallAssist.CommonDeploymentGUI"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:BaseWindow/>
    ///
    /// </summary>
    public class BaseWindow : Window
    {
        public ImageSource TitleImg
        {
            get { return (ImageSource) GetValue(TitleImgProperty); }
            set { SetValue(TitleImgProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleImg.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleImgProperty = DependencyProperty.Register("TitleImg",
                                                                                                 typeof(ImageSource),
                                                                                                 typeof(BaseWindow),
                                                                                                 new UIPropertyMetadata(
                                                                                                     null));

        public string TitleText
        {
            get { return (string) GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleImg.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText",
                                                                                                  typeof(string),
                                                                                                  typeof(BaseWindow),
                                                                                                  new UIPropertyMetadata
                                                                                                      (null));


        public BaseWindow()
        {
            this.DefaultStyleKey = typeof(BaseWindow);
            this.Loaded += delegate { InitializeEvent(); };
            //repair
            FullScreenManager.RepairWpfWindowFullScreenBehavior(this);
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseWindow), new FrameworkPropertyMetadata(typeof(BaseWindow)));
        }

        private Button mMinButton;
        private Button mMaxButton;
        private Button mRestoreButton;
        private Button mCloseButton;
        private DockPanel mBorderTitle;

        public override void OnApplyTemplate()
        {
            mMinButton = this.GetTemplateChild("btnMin") as Button;
           
            mCloseButton = this.GetTemplateChild("btnClose") as Button;
            mBorderTitle = this.GetTemplateChild("borderTitle") as DockPanel;
            base.OnApplyTemplate();
        }

        private void InitializeEvent()
        {
            MinButtonClick();
            

            CloseButtonClick();

            BorderTitleMouseMove();

            BorderTitleMouseLeftButtonDown();
        }

        private void BorderTitleMouseLeftButtonDown()
        {
            mBorderTitle.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (e.ClickCount >= 2)
                {
                    mMaxButton?.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
            };
        }

        private void BorderTitleMouseMove()
        {
            mBorderTitle.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };
        }

        private void CloseButtonClick()
        {
            mCloseButton.Click += delegate { CloseButtonEvent(); };
        }



        private void MinButtonClick()
        {
            mMinButton.Click += delegate { this.WindowState = WindowState.Minimized; };
        }

        protected virtual void CloseButtonEvent()
        {
        }
    }
}