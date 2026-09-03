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
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Office365GroupRetention
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

        private static ExchangeRetentionIndexService _ExchangeRetentionIndexService;
        public static ExchangeRetentionIndexService ExchangeRetentionIndexService
        {
            get
            {
                if (_ExchangeRetentionIndexService == null)
                {
                    _ExchangeRetentionIndexService = new ExchangeRetentionIndexService()
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
                return _ExchangeRetentionIndexService;
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


        private static ExchangeIndexService _ExchangeSubIndexService;
        public static ExchangeIndexService ExchangeSubIndexService
        {
            get
            {
                if (_ExchangeSubIndexService == null)
                {
                    _ExchangeSubIndexService = new ExchangeIndexService()
                    {
                        //ExchangeBackupIndexService = ExchangeBackupIndexService,
                        IndexProcessor = SubIndexProcessor,
                        IndexSynchronizer = new IndexDatabaseSynchronizer(),
                    };
                }
                return _ExchangeSubIndexService;
            }
        }

        private static IndexProcessor<ExchangeIndexProcessorParameter> _SubIndexProcessor;
        public static IndexProcessor<ExchangeIndexProcessorParameter> SubIndexProcessor
        {
            get
            {
                if (_SubIndexProcessor == null)
                {
                    _SubIndexProcessor = new IndexProcessor<ExchangeIndexProcessorParameter>();
                }

                return _SubIndexProcessor;
            }
        }

        private static ExchangeRetentionIndexService _ExchangeRetentionSubIndexService;
        public static ExchangeRetentionIndexService ExchangeRetentionSubIndexService
        {
            get
            {
                if (_ExchangeRetentionSubIndexService == null)
                {
                    _ExchangeRetentionSubIndexService = new ExchangeRetentionIndexService()
                    {
                        AgentIndexService = new ExchangeAgentIndexService()
                        {
                            IndexProcessor = IndexProcessor,
                        },
                        ContainerItemIndexService = new ExchangeContainerAndItemIndexService() { IndexProcessor = SubIndexProcessor },
                        //DataMd5IndexService = new ExchangeDataMd5IndexService() { IndexProcessor = IndexProcessor },
                        SiteMasterIndexService = new ExchangeMasterIndexService() { IndexProcessor = SubIndexProcessor },
                    };
                }
                return _ExchangeRetentionSubIndexService;
            }
        }
    }
}
