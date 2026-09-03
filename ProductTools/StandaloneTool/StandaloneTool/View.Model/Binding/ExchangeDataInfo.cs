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
using AvePoint.Media.Service.DomainModel;
using CommunityToolkit.Mvvm.Input;
using DataExportCore;
using StandaloneTool.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StandaloneTool.View.Model.Binding
{
    public class ExchangeDataInfo : BaseINotifyPropertyChanged
    {
        private static readonly ExchangeDataInfo _instance = new ExchangeDataInfo();
        public static ExchangeDataInfo GetInstance() => _instance;

        #region Properties

        private string storagePageSummary = string.Empty;
        private string restoreTime = string.Empty;
        private string exportSize = string.Empty;
        private bool isCheckingConfig = false;
        private string password = string.Empty;
        private string passwordCache = string.Empty;

        public string StoragePageSummary
        {
            get => storagePageSummary;
            set => Set(ref storagePageSummary, value);
        }

        public string BackupTime
        {
            get => restoreTime;
            set => Set(ref restoreTime, value);
        }

        public string ExportSize
        {
            get => exportSize;
            set => Set(ref exportSize, value);
        }

        public bool IsCheckingConfig
        {
            get => isCheckingConfig;
            set => Set(ref isCheckingConfig, value);
        }
        public string Passworde
        {
            get => password;
            set => Set(ref password, value);
        }

        public string PasswordCache
        {
            get => passwordCache;
            set => Set(ref passwordCache, value);
        }

        private string columnOneHeaderText = I18NEntity.GetString("SATool_MailboxTitle");
        public string ColumnOneHeaderText
        {
            get { return columnOneHeaderText; }
            set
            {
                columnOneHeaderText = value;
                OnPropertyChanged("ColumnOneHeaderText");
            }
        }

        private List<ArchiverSiteMasterIndexExportDto> archiverObjects = new List<ArchiverSiteMasterIndexExportDto>();

        public List<ArchiverSiteMasterIndexExportDto> ArchiverObjects
        {
            get { return archiverObjects; }
            set
            {
                archiverObjects = value;
            }
        }


        private ObservableCollection<ArchiverSite> archiverSites = new ObservableCollection<ArchiverSite>();

        public ObservableCollection<ArchiverSite> ArchiverSites
        {
            get { return archiverSites; }
            set
            {
                archiverSites = value;
                OnPropertyChanged("ArchiverSites");
            }
        }

        private List<ArchiverSite> selectionList => archiverSiteWhole.Where(s => s.IsChecked == true).ToList();

        public List<ArchiverSite> SelectionList => selectionList;

        private ObservableCollection<ArchiverSite> archiverSiteWhole = new ObservableCollection<ArchiverSite>();

        public ObservableCollection<ArchiverSite> ArchiverSiteWhole
        {
            get { return archiverSiteWhole; }
            set
            {
                archiverSiteWhole = value;
                OnPropertyChanged("ArchiverSiteWhole");
            }
        }


        private bool isSearch = false;
        public bool IsSearch
        {
            get { return isSearch; }
            set
            {
                isSearch = value;
                Set(ref isAllChecked, false);
            }
        }


        private string searchText;
        public string SearchText
        {
            get => searchText;
            set => Set(ref searchText, value);
        }

        private bool isAllChecked = false;
        public bool IsAllChecked
        {
            get { return isAllChecked; }
            set
            {
                isAllChecked = value;
                OnPropertyChanged("IsAllChecked");
            }
        }

        private bool isSelectedMailbox = false;
        public bool IsSelectedMailbox
        {
            get { return isSelectedMailbox; }
            set
            {
                isSelectedMailbox = value;
                OnPropertyChanged("IsSelectedMailbox");
            }
        }

        private ObservableCollection<ArchiverSite> searchArchiverSiteWhole = new ObservableCollection<ArchiverSite>();

        public ObservableCollection<ArchiverSite> SearchArchiverSiteWhole
        {
            get { return searchArchiverSiteWhole; }
            set
            {
                searchArchiverSiteWhole = value;
            }
        }


        private uint _dataListCount;
        public uint DataListCount
        {
            get { return _dataListCount; }
            set
            {
                _dataListCount = value;
                OnPropertyChanged("DataListCount");
            }
        }


        public ICommand PageRefreshCmd
        {
            get
            {
                return new RelayCommand<Tuple<uint, uint>>(tuple =>
                {
                    if (DataListPagingStartLengthChangeEvent != null)
                    {
                        DataListPagingStartLengthChangeEvent(tuple.Item1, tuple.Item2);
                    }
                });
            }
        }

        public event Action<uint, uint> DataListPagingStartLengthChangeEvent;

        #endregion
    }

    public class ArchiverSite : ArchiverSiteBase
    {
        public bool IsChecked { get; set; }
        public string JobId { get; set; }
    }

}
