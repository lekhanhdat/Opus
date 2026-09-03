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
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace StandaloneTool.Model.CustomControl
{
    public partial class PaggingControl : Control
    {
        #region Register
        // Using a DependencyProperty as the backing store for TotalPages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalPagesProperty =
            DependencyProperty.Register("TotalPages", typeof(uint), typeof(PaggingControl),
                                        new PropertyMetadata(1u, TotalPagesPropertyChangeCallback));

        // Using a DependencyProperty as the backing store for CurrentPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(uint), typeof(PaggingControl),
                                        new PropertyMetadata(1u, CurrentPagePropertyChangeCallback));

        // Using a DependencyProperty as the backing store for PageSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register("PageSize", typeof(uint), typeof(PaggingControl),
                                        new PropertyMetadata(10u, PageSizePropertyChangecallback));

        // Using a DependencyProperty as the backing store for PageSizeList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageSizeListProperty =
            DependencyProperty.Register("PageSizeList", typeof(ObservableCollection<uint>), typeof(PaggingControl),
                                        new PropertyMetadata(new ObservableCollection<uint> { 10u, 20u, 50u, 100u }));

        // Using a DependencyProperty as the backing store for ItemsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsCountProperty =
            DependencyProperty.Register("ItemsCount", typeof(uint), typeof(PaggingControl),
                                        new PropertyMetadata(1u, ItemsCountPropertyChangeCallback));

        // Using a DependencyProperty as the backing store for PageRefreshCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageRefreshCommandProperty =
            DependencyProperty.Register("PageRefreshCommand", typeof(ICommand), typeof(PaggingControl),
                                        new PropertyMetadata(null));
        #endregion

        private bool isUpdating = false;

        static PaggingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaggingControl),
                                                     new FrameworkPropertyMetadata(typeof(PaggingControl)));
        }

        public PaggingControl()
        {
            // Refresh the list for the first time
            Loaded += delegate { RaisePageRefreshEvent(); };
        }

        public uint TotalPages
        {
            get => (uint)GetValue(TotalPagesProperty);
            set => SetValue(TotalPagesProperty, value);
        }

        public uint CurrentPage
        {
            get => (uint)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public uint PageSize
        {
            get => (uint)GetValue(PageSizeProperty);
            set => SetValue(PageSizeProperty, value);
        }

        /// The size list for each page, which is the data source of the page size selection box
        public ObservableCollection<uint> PageSizeList
        {
            get => (ObservableCollection<uint>)GetValue(PageSizeListProperty);
            set => SetValue(PageSizeListProperty, value);
        }

        /// Go to Home Page
        [RelayCommand(CanExecute = nameof(CanPreviousPage))]
        private void FirstPage() => CurrentPage = 1;

        /// Previous page
        [RelayCommand(CanExecute = nameof(CanPreviousPage))]
        private void PreviousPage() => CurrentPage -= 1;
        private bool CanPreviousPage() => CurrentPage > 1;

        /// Next Page
        [RelayCommand(CanExecute = nameof(CanNextPage))]
        private void NextPage() => CurrentPage += 1;
        private bool CanNextPage() => CurrentPage < TotalPages;

        /// Last Page
        [RelayCommand(CanExecute = nameof(CanNextPage))]
        private void LastPage() => CurrentPage = TotalPages;

        /// Go to a page
        public ICommand TurnToPageCmd => new RelayCommand(RaisePageRefreshEvent);

        /// Total data size
        public uint ItemsCount
        {
            get => (uint)GetValue(ItemsCountProperty);
            set => SetValue(ItemsCountProperty, value);
        }

        /// Parameters Select Tuple(uint, uint) to represent the number of pages and page size, i.e. index and length
        public ICommand PageRefreshCommand
        {
            get => (ICommand)GetValue(PageRefreshCommandProperty);
            set => SetValue(PageRefreshCommandProperty, value);
        }

        private static void TotalPagesPropertyChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PaggingControl;
            if (ctrl != null && !ctrl.isUpdating)
            {
                ctrl.isUpdating = true;

                if (ctrl.CurrentPage > ctrl.TotalPages)
                {
                    ctrl.CurrentPage = ctrl.TotalPages;
                }
                else if (ctrl.CurrentPage <= 1)
                {
                    ctrl.CurrentPage = 1;
                }

                Refresh(ctrl);

                ctrl.isUpdating = false;
            }
        }

        private static void CurrentPagePropertyChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PaggingControl;
            if (ctrl != null && !ctrl.isUpdating)
            {
                ctrl.isUpdating = true;

                if (ctrl.CurrentPage > ctrl.TotalPages)
                {
                    ctrl.CurrentPage = ctrl.TotalPages;
                }
                else if (ctrl.CurrentPage <= 1)
                {
                    ctrl.CurrentPage = 1;
                }

                Refresh(ctrl);

                ctrl.isUpdating = false;
            }
        }

        private static void PageSizePropertyChangecallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PaggingControl;
            if (ctrl != null)
            {
                ctrl.TotalPages = ctrl.ItemsCount / ctrl.PageSize + (ctrl.ItemsCount % ctrl.PageSize == 0 ? 0 : 1u);
                ctrl.RaisePageRefreshEvent();
            }
        }

        private static void ItemsCountPropertyChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PaggingControl;
            if (ctrl != null)
            {
                ctrl.TotalPages = ctrl.ItemsCount / ctrl.PageSize + (ctrl.ItemsCount % ctrl.PageSize == 0 ? 0 : 1u);
            }
        }

        private void RaisePageRefreshEvent()
        {
            PageRefreshCommand?.Execute(Tuple.Create(CurrentPage, PageSize));
        }

        private static void Refresh(PaggingControl ctrl)
        {
            ctrl.RaisePageRefreshEvent();
            ctrl.FirstPageCommand.NotifyCanExecuteChanged();
            ctrl.PreviousPageCommand.NotifyCanExecuteChanged();
            ctrl.NextPageCommand.NotifyCanExecuteChanged();
            ctrl.LastPageCommand.NotifyCanExecuteChanged();
        }
    }
}
