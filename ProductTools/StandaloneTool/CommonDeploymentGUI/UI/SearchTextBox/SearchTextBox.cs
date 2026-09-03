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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvePoint.Deployment.CommonGUI
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:OCT.DocumentManagement.Client.GUI.CustomControls.UIControls.SearchTextBox"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:OCT.DocumentManagement.Client.GUI.CustomControls.UIControls.SearchTextBox;assembly=OCT.DocumentManagement.Client.GUI.CustomControls.UIControls.SearchTextBox"
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
    ///     <MyNamespace:SearchTextBox/>
    ///
    /// </summary>
    public class SearchTextBox : TextBox
    {
        #region    ==Constructs==

        static SearchTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextBox), new FrameworkPropertyMetadata(typeof(SearchTextBox)));
        }

        public SearchTextBox()
        {
            this.Loaded += OnLoaded;
            this.TextChanged -= OnTextChanged;
            this.TextChanged += OnTextChanged;
        }

        #endregion ==Constructs==

        #region == Memebers ==
        #region    ==Private Memners==
        /// <summary>
        /// indicate the element of watermark
        /// </summary>
        private ContentControl mWatermarkContent;

        /// <summary>
        /// indicate focus state
        /// </summary>
        private bool mHasFocus;

        // indicate the search bar
        private SearchToggleButton mIndicateBar;

        // 标识当前是否为原始Search状态，方便当Text为空时，能Stop
        private bool mIsOriginalState = true;
        private string SearchText;

        // vsm flag
        private bool mIsMouseOver;
        private bool mIsKeyPress;

        #endregion ==Private Memners==

        #region    ==Public Members==
        /// <summary>
        /// 当点击查询按钮时触发。
        /// </summary>
        public EventHandler OnSearch;

        /// <summary>
        /// 当点击查询按钮时触发。
        /// </summary>
        public event EventHandler OnSearchEvent;

        /// <summary>
        /// 当点击停止查询按钮时触发。
        /// </summary>
        public EventHandler OnStop;

        /// <summary>
        /// 当点击停止查询按钮时触发。
        /// </summary>
        public event EventHandler OnStopEvent;

        /// <summary>
        /// 获取或设置一个值，该值表示查询按钮是否处在查询后的状态。
        /// </summary>
        public bool? IndicatorIsChecked
        {
            get
            {
                if (mIndicateBar == null)
                {
                    return null;
                }
                else
                {
                    return mIndicateBar.IsChecked;
                }
            }
        }

        #endregion ==Public Members==
        #endregion == Memebers ==

        #region    ==Properties==
        #region == Watermark ==
        /// <summary>
        /// 获取或设置水印。
        /// </summary>
        public object Watermark
        {
            get { return (object)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
            "Watermark",
            typeof(object),
            typeof(SearchTextBox),
            new PropertyMetadata("Input Keyword", OnWatermarkPropertyChanged)
            );

        private static void OnWatermarkPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            SearchTextBox self = sender as SearchTextBox;
            if (null != self)
            {
                self.OnWatermarkChanged();
                self.ChangeVisualState();
            }
        }

        /// <summary>
        /// make the watermark element can not get tab
        /// </summary>
        private void OnWatermarkChanged()
        {
            if (mWatermarkContent != null)
            {
                Control watermarkControl = this.Watermark as Control;
                if (watermarkControl != null)
                {
                    watermarkControl.IsTabStop = false;
                    watermarkControl.IsHitTestVisible = false;
                }
            }
        }

        #endregion ==Watermark ==
        #endregion ==Properties==



        //public ICommand SearchCommand
        //{
        //    get { return (ICommand)GetValue(SearchCommandProperty); }
        //    set { SetValue(SearchCommandProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SearchCommand.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SearchCommandProperty =
        //    DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(SearchTextBox), new PropertyMetadata(null));


        //public ICommand StopCommand
        //{
        //    get { return (ICommand)GetValue(StopCommandProperty); }
        //    set { SetValue(StopCommandProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for StopCommand.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty StopCommandProperty =
        //    DependencyProperty.Register("StopCommand", typeof(ICommand), typeof(SearchTextBox), new PropertyMetadata(null));


        #region == Methods ==
        #region    ==Private Methods==

        /// <summary>
        /// update vsm
        /// </summary>
        internal void ChangeVisualState()
        {
            ChangeVisualState(true);
        }

        /// <summary>
        /// update vsm
        /// </summary>
        internal void ChangeVisualState(bool useTransitions)
        {
            if (!mHasFocus && this.Watermark != null && string.IsNullOrEmpty(this.Text))
            {
                VisualStates.GoToState(this, useTransitions, "Watermarked");
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, "Unwatermarked");
            }
        }

        /// <summary>
        /// 处理搜索状态 & 处理空值
        /// </summary>
        private void ValidateSearchState()
        {
            if (mIndicateBar == null) return;

            if (string.IsNullOrWhiteSpace(Text) && string.IsNullOrWhiteSpace(SearchText))
            {
                if (mIsOriginalState)
                {
                    // 不能Search和Stop
                    mIndicateBar.IsChecked = false;
                    mIndicateBar.IsEnabled = false;
                }
                else
                {
                    // 不能Search但能Stop
                    mIndicateBar.IsChecked = true;
                }
            }
            else if (mIndicateBar.IsEnabled == false)
            {
                mIndicateBar.IsEnabled = true;
            }
            UpdateVisualStates();
        }

        /// <summary>
        /// 执行Search
        /// </summary>
        private void DoSearch()
        {
            if (null != OnSearch)
            {
                OnSearch(this, new EventArgs());
            }
            if (null != OnSearchEvent)
            {
                OnSearchEvent(this, new EventArgs());
            }
            // reset flag
            mIsOriginalState = false;
            SearchText = this.Text;
        }

        /// <summary>
        /// 执行Stop
        /// </summary>
        private void DoStop()
        {
            Text = string.Empty;

            if (null != OnStop)
            {
                OnStop(this, new EventArgs());
            }
            if (null != OnStopEvent)
            {
                OnStopEvent(this, new EventArgs());
            }
            // reset flag
            mIsOriginalState = true;
            SearchText = null;
        }

        /// <summary>
        /// update vsm
        /// </summary>
        private void UpdateVisualStates()
        {
            if (!mIndicateBar.IsEnabled)
            {
                return;
            }
            if (mIsKeyPress)
            {
                VisualStates.GoToState(mIndicateBar, true, "Pressed");
            }
            else if (mIsMouseOver)
            {
                VisualStates.GoToState(mIndicateBar, true, "MouseOver");
            }
            else
            {
                VisualStates.GoToState(mIndicateBar, true, "Normal");
            }
        }

        #endregion ==Private Methods==

        #region    ==Public Methods==
        /// <summary>
        /// 重置控件。
        /// </summary>
        public void Reset(bool doStop = true)
        {
            // update UI
            mIsOriginalState = true;
            SearchText = null;
            Text = string.Empty;


            // invoke method
            if (doStop)
            {
                DoStop();
            }
            ChangeVisualState();
        }

        /// <summary>
        /// 重置输入框到初始状态。
        /// </summary>
        public void ClearText()
        {
            // update UI
            mIsOriginalState = true;
            SearchText = null;
            Text = string.Empty;
        }

        #endregion ==Public Methods==

        #region    ==Base Override Methods==

        public override void OnApplyTemplate()
        {
            mWatermarkContent = Template.FindName("Watermark", this) as ContentControl;
            mIndicateBar = Template.FindName("IndicateBar", this) as SearchToggleButton;

            if (mIndicateBar != null)
            {
                mIndicateBar.Click -= InvokeSearchOrStop;
                mIndicateBar.Click += InvokeSearchOrStop;
            }

            base.OnApplyTemplate();

            if (string.IsNullOrEmpty(this.Watermark.ToString()))
            {
                this.Watermark = "Search DMS";
            }
        }

        #endregion ==Base Override Methods==
        #endregion == Methods ==

        #region == Handlers ==

        /// <summary>
        /// 初始化Watermark
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyTemplate();
            ChangeVisualState(false);
        }

        /// <summary>
        /// override
        /// </summary>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            mHasFocus = false;
            ChangeVisualState();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// override
        /// </summary>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                mHasFocus = true;
                if (!string.IsNullOrEmpty(this.Text))
                {
                    Select(0, this.Text.Length);
                }
                ChangeVisualState();
            }
            base.OnGotFocus(e);
        }

        /// <summary>
        /// event of check or uncheck
        /// </summary>
        private void InvokeSearchOrStop(object sender, RoutedEventArgs e)
        {
            SearchToggleButton _btu = sender as SearchToggleButton;
            if (null != _btu)
            {
                if ((bool)_btu.IsChecked)
                {
                    DoSearch();
                }
                else
                {
                    DoStop();
                }
            }
            ValidateSearchState();
        }

        /// <summary>
        /// text changed event
        /// </summary>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // 值刷新，可再搜索
            if (null != mIndicateBar && mIndicateBar.IsChecked == true && SearchText != null && !SearchText.Equals(this.Text))
            {
                mIndicateBar.IsChecked = false;
            }
            else if (null != mIndicateBar && mIndicateBar.IsChecked == false && SearchText != null && SearchText.Equals(this.Text))
            {
                mIndicateBar.IsChecked = true;
            }

            ValidateSearchState();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            mIsMouseOver = true;
            UpdateVisualStates();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            mIsMouseOver = false;
            UpdateVisualStates();
            base.OnMouseLeave(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Key.Enter == e.Key && mIndicateBar.IsEnabled)
            {
                mIsKeyPress = true;
                UpdateVisualStates();
            }
            else if (Key.Escape == e.Key && mIndicateBar.IsEnabled)
            {
                mIsKeyPress = true;
                UpdateVisualStates();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (Key.Enter == e.Key && mIndicateBar.IsEnabled)
            {
                if (!(bool)mIndicateBar.IsChecked)
                {
                    mIndicateBar.IsChecked = true;
                    DoSearch();
                }

                mIsKeyPress = false;
                UpdateVisualStates();
                ValidateSearchState();
            }
            else if (Key.Escape == e.Key && mIndicateBar.IsEnabled)
            {
                if ((bool)mIndicateBar.IsChecked)
                {
                    mIndicateBar.IsChecked = false;
                    DoStop();
                }

                mIsKeyPress = false;
                UpdateVisualStates();
                mIndicateBar.IsChecked = false;
                ValidateSearchState();
            }
            base.OnKeyUp(e);
        }
        #endregion == Handlers ==
    }
}