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
using AvePoint.RA.CommonUtil;
using DataExportCore;
using StandaloneTool.Model.Common;
using StandaloneTool.View.Model.Binding;
using StandaloneTool.View.Model.Command;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StandaloneTool.View
{
    public partial class RecoverDataPage : Page
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RecoverDataPage));
        private readonly ExchangeDataInfo exchangeDataInfo = ExchangeDataInfo.GetInstance();
        private readonly BaseDataContext context = BaseDataContext.Instance;

        public RecoverDataPage()
        {
            InitializeComponent();
            DataContext = exchangeDataInfo;
            ColumnOneHeaderName.DataContext = exchangeDataInfo;
            PreSetupPage();
        }

        private void PreSetupPage()
        {
            switch (GlobalInfo.Module)
            {
                case Module.SharePointOnline:
                case Module.OneDrive:
                    exchangeDataInfo.ColumnOneHeaderText = I18NEntity.GetString("SATool_SiteCollectionURLTitle");
                    break;
                case Module.Teams:
                    exchangeDataInfo.ColumnOneHeaderText = I18NEntity.GetString("SATool_TeamsAndGroupTitle");
                    break;
            }
            exchangeDataInfo.SearchText = string.Empty;
            exchangeDataInfo.IsCheckingConfig = false;
            exchangeDataInfo.IsSearch = false;
            exchangeDataInfo.DataListCount = (uint)exchangeDataInfo.ArchiverObjects.Count;
            exchangeDataInfo.ArchiverSites.Clear();
            exchangeDataInfo.SelectionList.Clear();
            exchangeDataInfo.SearchArchiverSiteWhole.Clear();
            exchangeDataInfo.ArchiverSiteWhole.Clear();
        }

        private void Instance_DataListPagingStartLengthChangeEvent(uint start, uint length)
        {
            exchangeDataInfo.ArchiverSites.Clear();
            var batchToDisplay = new List<ArchiverSite>();
            if (exchangeDataInfo.IsSearch)
            {
                batchToDisplay = exchangeDataInfo.SearchArchiverSiteWhole.Skip((int)((start - 1) * length)).Take((int)length).ToList();
            }
            else
            {
                batchToDisplay = exchangeDataInfo.ArchiverSiteWhole.Skip((int)((start - 1) * length)).Take((int)length).ToList();
            }

            batchToDisplay.ForEach(pick => exchangeDataInfo.ArchiverSites.Add(pick));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            exchangeDataInfo.DataListPagingStartLengthChangeEvent += Instance_DataListPagingStartLengthChangeEvent;
            exchangeDataInfo.IsSelectedMailbox = GlobalInfo.Module == Module.OneDrive || GlobalInfo.Module == Module.SharePointOnline || GlobalInfo.Module == Module.Teams;
            try
            {
                var checkThread = new Thread(LoadedData);
                checkThread.SetApartmentState(ApartmentState.STA);
                checkThread.IsBackground = true;
                checkThread.Start();
                context.NextOperator.Command.OnCanExecuteChanged();
                context.BackOperator.Command.OnCanExecuteChanged();
            }
            catch (Exception ex)
            {
                logger.Error("Page loaded data failed: {0}.", ex);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e) => exchangeDataInfo.DataListPagingStartLengthChangeEvent -= Instance_DataListPagingStartLengthChangeEvent;

        private void LoadedData() => Application.Current.Dispatcher.BeginInvoke(new Action(() => GenerateData()), DispatcherPriority.Normal);

        private void GenerateData()
        {
            if (exchangeDataInfo.IsSearch) return;
            try
            {
                exchangeDataInfo.IsCheckingConfig = true;
                switch (GlobalInfo.Module)
                {
                    case Module.SharePointOnline:
                    case Module.OneDrive:
                        exchangeDataInfo.ArchiverObjects.ForEach(item =>
                        {
                            if (item != null)
                            {
                                exchangeDataInfo.ArchiverSiteWhole.Add(
                                new ArchiverSite
                                {
                                    SiteId = item.SiteId,
                                    SiteUrl = item.SiteURL,
                                    JobId = item.JobId,
                                    IsChecked = false
                                });
                            }
                        });
                        break;
                    case Module.Teams:
                        exchangeDataInfo.ArchiverObjects.ForEach(item =>
                        {
                            if (item != null && !string.IsNullOrEmpty(item.GroupMailboxAddress) && !exchangeDataInfo.ArchiverSiteWhole.Any(_ => _.SiteUrl.Equals(item.GroupMailboxAddress)))
                            {
                                exchangeDataInfo.ArchiverSiteWhole.Add(
                                new ArchiverSite
                                {
                                    SiteId = item.SiteId,
                                    SiteUrl = item.GroupMailboxAddress,
                                    JobId = item.JobId,
                                    IsChecked = false,
                                    GroupAddress = item.GroupMailboxAddress
                                });
                            }
                        });
                        break;
                }
                Instance_DataListPagingStartLengthChangeEvent(1, 10);
                exchangeDataInfo.IsCheckingConfig = false;
            }
            catch (Exception ex)
            {
                exchangeDataInfo.IsCheckingConfig = false;
                logger.Error("Page generate data with exception: {0}.", ex);
            }
        }

        private void SearchTextBox_OnSearchEventForEnable(object sender, EventArgs e)
        {
            try
            {
                exchangeDataInfo.IsCheckingConfig = true;
                var checkThread = new Thread(SearchEventForEnable);
                checkThread.SetApartmentState(ApartmentState.STA);
                checkThread.IsBackground = true;
                checkThread.Start(sender);
            }
            catch (Exception ex)
            {
                logger.Warn("Search event for archiver sites failed: {0}.", ex);
            }
        }

        private void SearchEventForEnable(object sender)
        {
            exchangeDataInfo.IsSearch = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                exchangeDataInfo.ArchiverSites.Clear();
                exchangeDataInfo.SearchArchiverSiteWhole.Clear();
                exchangeDataInfo.ArchiverSiteWhole.ForEach(s => s.IsChecked = false);
                string text = (sender as SearchTextBox).Text.Trim();
                if (text.Contains(","))
                {
                    var archiverSites = text.Trim(',').Split(',');
                    text.Trim(',').Split(',').ForEach(t =>
                    {
                        exchangeDataInfo.ArchiverSiteWhole.ForEach(m =>
                        {
                            if (m.SiteUrl.EqualsIgnoreCase(t.Trim()))
                            {
                                if (!exchangeDataInfo.SearchArchiverSiteWhole.Contains(m))
                                {
                                    exchangeDataInfo.SearchArchiverSiteWhole.Add(m);
                                }
                            }
                        });
                    });
                }
                else
                {
                    exchangeDataInfo.ArchiverSiteWhole.ForEach(m =>
                    {
                        if (m.SiteUrl.Contains(text, StringComparison.OrdinalIgnoreCase))
                        {
                            exchangeDataInfo.SearchArchiverSiteWhole.Add(m);
                        }
                    });
                }
                Instance_DataListPagingStartLengthChangeEvent(0, 10);
                exchangeDataInfo.DataListCount = (uint)exchangeDataInfo.SearchArchiverSiteWhole.Count != 0 ? (uint)exchangeDataInfo.SearchArchiverSiteWhole.Count : 1;
            }), DispatcherPriority.Normal);
            Thread.Sleep(500);
            exchangeDataInfo.IsCheckingConfig = false;
        }

        private void SearchTextBox_OnStopEventForEnable(object sender, EventArgs e)
        {
            try
            {
                exchangeDataInfo.IsCheckingConfig = true;
                exchangeDataInfo.IsSearch = false;
                var checkThread = new Thread(StopSearchEventForEnable);
                checkThread.SetApartmentState(ApartmentState.STA);
                checkThread.IsBackground = true;
                checkThread.Start();
            }
            catch (Exception ex)
            {
                logger.Warn("Stop search event for archiver sites failed: {0}.", ex);
            }
        }

        private void StopSearchEventForEnable()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                exchangeDataInfo.SearchArchiverSiteWhole.Clear();
                Instance_DataListPagingStartLengthChangeEvent(0, 10);
                exchangeDataInfo.DataListCount = (uint)exchangeDataInfo.ArchiverSiteWhole.Count;
            }), DispatcherPriority.Normal);
            Thread.Sleep(500);
            exchangeDataInfo.IsCheckingConfig = false;
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            exchangeDataInfo.IsAllChecked = true;
            if (exchangeDataInfo.IsSearch)
            {
                exchangeDataInfo.ArchiverSiteWhole.ForEach(s => s.IsChecked = false);
                exchangeDataInfo.SearchArchiverSiteWhole.ForEach(s => s.IsChecked = true);
            }
            else
            {
                exchangeDataInfo.ArchiverSiteWhole.ForEach(s => s.IsChecked = true);
            }
            var temp = exchangeDataInfo.ArchiverSites.Select(s => new ArchiverSite() { SiteId = s.SiteId, IsChecked = true, SiteUrl = s.SiteUrl });
            exchangeDataInfo.ArchiverSites = new System.Collections.ObjectModel.ObservableCollection<ArchiverSite>(temp);
            context.NextOperator.Command.OnCanExecuteChanged();
        }

        private void UnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            if (exchangeDataInfo.ArchiverSiteWhole.Any(s => !s.IsChecked))
            {
                return;
            }

            exchangeDataInfo.IsAllChecked = false;
            exchangeDataInfo.ArchiverSiteWhole.ForEach(s => s.IsChecked = false);
            var temp = exchangeDataInfo.ArchiverSites.Select(s => new ArchiverSite() { SiteId = s.SiteId, IsChecked = false, SiteUrl = s.SiteUrl });
            exchangeDataInfo.ArchiverSites = new System.Collections.ObjectModel.ObservableCollection<ArchiverSite>(temp);
            context.NextOperator.Command.OnCanExecuteChanged();
        }

        private void CheckSingle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox != null)
            {
                var dataContext = checkBox.DataContext as ArchiverSite;
                exchangeDataInfo.ArchiverSiteWhole.First(s => s.SiteId == dataContext.SiteId).IsChecked = dataContext.IsChecked;
                var isAllChecked = exchangeDataInfo.ArchiverSiteWhole.All(s => s.IsChecked);
                exchangeDataInfo.IsAllChecked = isAllChecked;
            }
            
            context.NextOperator.Command.OnCanExecuteChanged();
        }

        private void StackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            context.NextOperator.Command.OnCanExecuteChanged();
            context.BackOperator.Command.OnCanExecuteChanged();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => context.NextOperator.Command.OnCanExecuteChanged();

    }
}
