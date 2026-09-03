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
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.ExchangeBackup;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using Office365Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Office365GroupRestore
{
    public class RContainer
    {
        private static IndexProcessor<ExchangeIndexProcessorParameter> _IndexProcessor;
        public static IndexProcessor<ExchangeIndexProcessorParameter> IndexProcessor
        {
            get
            {
                if (_IndexProcessor == null)
                {
                    _IndexProcessor = new IndexProcessor<ExchangeIndexProcessorParameter>();
                }

                return _IndexProcessor;
            }
        }


        private static ExchangeBackupIndexService _ExchangeBackupIndexService;
        public static ExchangeBackupIndexService ExchangeBackupIndexService
        {
            get
            {
                if (_ExchangeBackupIndexService == null)
                {
                    _ExchangeBackupIndexService = new ExchangeBackupIndexService()
                    {
                        AgentIndexService = new ExchangeAgentIndexService()
                        {
                            IndexProcessor = IndexProcessor,
                        },
                        ContainerItemIndexService = new ExchangeContainerAndItemIndexService() { IndexProcessor = IndexProcessor },
                        //DataMd5IndexService = new ExchangeDataMd5IndexService() { IndexProcessor = IndexProcessor },
                        SiteMasterIndexService = new ExchangeMasterIndexService() { IndexProcessor = IndexProcessor },
                    };
                }
                return ExchangeBackupIndexService;
            }
        }

        private static ExchangeIndexService _ExchangeIndexService;
        public static ExchangeIndexService ExchangeIndexService
        {
            get
            {
                if (_ExchangeIndexService == null)
                {
                    _ExchangeIndexService = new ExchangeIndexService()
                    {
                        //ExchangeBackupIndexService = ExchangeBackupIndexService,
                        IndexProcessor = IndexProcessor,
                        IndexSynchronizer = new IndexDatabaseSynchronizer(),
                    };
                }
                return _ExchangeIndexService;
            }
        }

        private static IExchangeRestoreIndexService _IExchangeRestoreIndexService;
        public static IExchangeRestoreIndexService ExchangeRestoreIndexService
        {
            get
            {
                if (_IExchangeRestoreIndexService == null)
                {
                    _IExchangeRestoreIndexService = new ExchangeRestoreIndexService()
                    {
                        ContainerItemIndexService = new ExchangeContainerAndItemIndexService() { IndexProcessor = IndexProcessor },
                        SiteMasterIndexService = new ExchangeMasterIndexService() { IndexProcessor = IndexProcessor },
                        AgentIndexService = new ExchangeAgentIndexService()
                        {
                            IndexProcessor = IndexProcessor
                        }
                    };
                }
                return _IExchangeRestoreIndexService;
            }
        }


        private static IExchangeRestoreTreeHandler _IExchangeRestoreTreeHandler;
        public static IExchangeRestoreTreeHandler ExchangeRestoreTreeHandler
        {
            get
            {
                if (_IExchangeRestoreTreeHandler == null)
                {
                    _IExchangeRestoreTreeHandler = new ExchangeRestoreTreeHandler
                    {
                        RestoreIndexService = ExchangeRestoreIndexService,
                    };
                }
                return _IExchangeRestoreTreeHandler;
            }
        }

        private static IArchiverSiteMasterIndexDao _IArchiverSiteMasterIndexDao;
        public static IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao
        {
            get
            {
                if (_IArchiverSiteMasterIndexDao == null)
                {
                    _IArchiverSiteMasterIndexDao = new ArchiverSiteMasterIndexDao();
                }
                return _IArchiverSiteMasterIndexDao;
            }
        }
    }
}
