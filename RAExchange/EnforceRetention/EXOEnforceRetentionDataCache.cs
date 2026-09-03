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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.RAExchange.EnforceRetention
{
    class EXOEnforceRetentionDataCache
    {
        private RALogger logger = RALogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly static object locker = new object();
        static EXOEnforceRetentionDataCache _instance;

        private Dictionary<Guid, TermSettingsInfo> mTermDeclaration;
        public Dictionary<Guid, TermSettingsInfo> TermDeclarationMapping
        {
            get
            {
                return mTermDeclaration;
            }
            private set
            {
                mTermDeclaration = value;
            }
        }
        
        public static EXOEnforceRetentionDataCache Instance
        {
            get
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new EXOEnforceRetentionDataCache();
                    }
                }
                return _instance;
            }
        }

        private List<Guid> mProcessedItems = new List<Guid>();
        public bool GetProcessedItem(Guid itemId)
        {
            return mProcessedItems.Contains(itemId);
        }
        public void AddProcessedItem(Guid itemId)
        {
            lock (locker)
            {
                if (!mProcessedItems.Contains(itemId))
                {
                    mProcessedItems.Add(itemId);
                }
            }
        }

        public void CacheTermChange(long startTime)
        {
            logger.Info("Begin to cache term retention setting.");
            IRMChangeClassificationDao TermChangeDao = new RMChangeClassificationDao();
            ITermDao TermDao = new TermDao();

            var tIds = TermChangeDao.GetAllChange(startTime, (int)TermChangeType.Retention);
            TermDeclarationMapping = TermDao.GetRetetionTermDic(tIds);
            logger.Info("Cache term retention setting success, total count:{0}.", TermDeclarationMapping.Count);
        }

        public void ClearData()
        {
            mProcessedItems.Clear();
        }
    }
}
