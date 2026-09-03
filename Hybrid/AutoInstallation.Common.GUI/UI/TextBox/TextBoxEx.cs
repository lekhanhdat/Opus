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


using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoInstallation.Common.GUI
{
    public class TextBoxEx : TextBox
    {
        public TextBoxEx()
        {
            DefaultStyleKey = typeof(TextBoxEx);
            OverridesDefaultStyle = true;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //<Setter Property="OverridesDefaultStyle" Value="True"/>

            mRoot = GetTemplateChild("RootElement") as Grid;

            if (TextBoxHeight == 0)
            {
                mRoot.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                mRoot.RowDefinitions[1].Height = GridLength.Auto;
                TextBoxHeight = double.NaN;
            }

            if (TextBoxWidth == 0)
            {
                mRoot.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                mRoot.ColumnDefinitions[1].Width = GridLength.Auto;
                TextBoxWidth = double.NaN;
            }

            mValidationMsg = GetTemplateChild("ValidationMsg") as TextBlock;
            mValidationPanel = GetTemplateChild("ValidationPanel") as Grid;


            Initialized = true;
            if (!ErrorMsg.Equals(string.Empty))
                if (mValidationMsg != null)
                {
                    mValidationMsg.Text = ErrorMsg;
                    mValidationPanel.Visibility = !NotifyOnErrorMsgChanged || ErrorMsg.Equals(string.Empty)
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }

            //to show the error message, 
            //you should add "ShowMsgOnBottom = 'true'" in TextBox Properties( XAML)
            if (ShowMsgOnBottom)
            {
                Grid.SetRowSpan(mValidationPanel, 1);
                Grid.SetRow(mValidationPanel, 1);
                Grid.SetColumn(mValidationPanel, 0);
                mValidationMsg.HorizontalAlignment = HorizontalAlignment.Center;
                mValidationPanel.Margin = new Thickness(0, 3, 0, 0);
            }
            else
            {
                mValidationPanel.Visibility = Visibility.Collapsed;
            }
        }

        #region Properties

        /// <summary>
        ///     存放错误信息的TextBlock
        /// </summary>
        private TextBlock mValidationMsg;

        /// <summary>
        ///     存放错误信息的TextBlock
        /// </summary>
        private Grid mValidationPanel;

        /// <summary>
        ///     模板的根节点
        /// </summary>
        private Grid mRoot;

        /// <summary>
        ///     存放错误信息
        /// </summary>
        private string mErrorMsg;

        /// <summary>
        ///     是否已经初始化
        /// </summary>
        public bool Initialized;

        /// <summary>
        ///     自定义验证方法,在调用Validate()方法时会触发这个事件
        /// </summary>
        public event EventHandler ValidationEvent;

        /// <summary>
        ///     当HasError属性改变时会触发这个事件
        /// </summary>
        public event EventHandler HasErrorChanged;

        /// <summary>
        ///     当ErrorMsg变化时会触发这个事件
        /// </summary>
        public event EventHandler ErrorMsgChanged;

        #endregion

        #region propdp

        /// <summary>
        ///     设置文本框的宽度,如设置了这个属性,则文本框和验证错误信息区域的宽度会分开,<see cref="Width" />属性将代表验证错误信息区域的宽度
        ///     <br />如果没有设置这个属性或者设置为0 则<see cref="Width" />属性会同时作用于文本框和验证错误信息区域.
        /// </summary>
        public double TextBoxWidth
        {
            get { return (double) GetValue(TextBoxWidthProperty); }
            set { SetValue(TextBoxWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotleWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxWidthProperty =
            DependencyProperty.Register(
                "TextBoxWidth",
                typeof(double),
                typeof(TextBoxEx),
                new PropertyMetadata(0.0));

        public double TextBoxHeight
        {
            get { return (double) GetValue(TextBoxHeightProperty); }
            set { SetValue(TextBoxHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotleWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxHeightProperty =
            DependencyProperty.Register(
                "TextBoxHeight",
                typeof(double),
                typeof(TextBoxEx),
                new PropertyMetadata(0.0));


        public bool ShowMsgOnBottom
        {
            get { return (bool) GetValue(ShowMsgOnBottomProperty); }
            set { SetValue(ShowMsgOnBottomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowMsgOnRight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowMsgOnBottomProperty =
            DependencyProperty.Register(
                "ShowMsgOnBottom",
                typeof(bool),
                typeof(TextBoxEx),
                new PropertyMetadata(false));


        public string ErrorMsg
        {
            get { return (string) GetValue(ErrorMsgProperty); }
            set { SetValue(ErrorMsgProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMsg.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMsgProperty =
            DependencyProperty.Register(
                "ErrorMsg",
                typeof(string),
                typeof(TextBoxEx),
                new PropertyMetadata(string.Empty, OnErrorMsgChanged));

        private static void OnErrorMsgChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var tbEx = obj as TextBoxEx;
            var oldValue = args.OldValue as string;
            var newValue = args.NewValue as string;
            if (tbEx != null)
            {
                if (oldValue == null || !oldValue.Equals(newValue))
                {
                    tbEx.mErrorMsg = newValue;
                    if (tbEx.ErrorMsgChanged != null) tbEx.ErrorMsgChanged(tbEx, new EventArgs());
                }

                if (tbEx.ErrorMsg != null)
                {
                    tbEx.HasError = !tbEx.ErrorMsg.Equals(string.Empty);
                    if (tbEx.mValidationMsg != null)
                    {
                        tbEx.mValidationMsg.Text = newValue;
                        tbEx.mValidationPanel.Visibility =
                            !tbEx.NotifyOnErrorMsgChanged || newValue.Equals(string.Empty)
                                ? Visibility.Collapsed
                                : Visibility.Visible;
                    }
                }
            }
        }


        /// <summary>
        ///     取得当前AUITextBox是否存在验证错误.其值取决于<see cref="ErrorMsg" />是否为空
        /// </summary>


        public bool HasError
        {
            get { return (bool) GetValue(HasErrorProperty); }
            set { SetValue(HasErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register(
                "HasError",
                typeof(bool),
                typeof(TextBoxEx),
                new PropertyMetadata(false, OnHasErrorChanged));

        private static void OnHasErrorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var source = o as TextBoxEx;
            if (source != null)
                if (source.HasErrorChanged != null)
                    source.HasErrorChanged(source, new EventArgs());
        }

        /// <summary>
        ///     设置是否在调用<see cref="Validate" />并有错误信息时将错误信息显示出来,默认为true,可以设置为false来阻止默认错误信息的显示.
        /// </summary>
        public bool NotifyOnErrorMsgChanged
        {
            get { return (bool) GetValue(NotifyOnErrorMsgChangedProperty); }
            set { SetValue(NotifyOnErrorMsgChangedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotifyOnErrorMsgChanged.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotifyOnErrorMsgChangedProperty =
            DependencyProperty.Register(
                "NotifyOnErrorMsgChanged",
                typeof(bool),
                typeof(TextBoxEx),
                new PropertyMetadata(true));

        #endregion
    }
}