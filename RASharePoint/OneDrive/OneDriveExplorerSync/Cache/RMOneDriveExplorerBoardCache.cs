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
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.SharePoint.OneDriveExplorerSync.Cache
{
    public class RMOneDriveExplorerBoardCache
    {

        private readonly static object locker = new object();
        private readonly static object termChangeLocker = new object();
        private readonly static object collectionChangeLocker = new object();
        private readonly static object totalLocker = new object();
        private readonly static object dateLocker = new object();
        private readonly static object movedDataLocker = new object();
        public RMOneDriveExplorerBoardCache()
        {
            TermChangedDic = new Dictionary<Guid, long>();
            CollectionChangedDic = new Dictionary<Guid, long>();
            TotalChangedDic = new Dictionary<BoardRecordStatus, long>()
            {
                { BoardRecordStatus.Creation, 0 },
                { BoardRecordStatus.Destruction, 0 },
                { BoardRecordStatus.WaitingForApprove, 0 },
            };
            DataOfDateChangedDic = new Dictionary<BoardRecordStatus, Dictionary<string, long>>()
            {
                { BoardRecordStatus.Creation, new Dictionary<string, long>() },
                { BoardRecordStatus.Destruction, new Dictionary<string, long>() },
                { BoardRecordStatus.WaitingForApprove, new Dictionary<string, long>() },
            };

            MovedDataCache = new Dictionary<Guid, Guid>();
        }
        public Dictionary<Guid, long> TermChangedDic { get; private set; }
        public Dictionary<Guid, long> CollectionChangedDic { get; private set; }
        public Dictionary<BoardRecordStatus, long> TotalChangedDic { get; private set; }
        public Dictionary<BoardRecordStatus, Dictionary<string, long>> DataOfDateChangedDic { get; private set; }
        public Dictionary<Guid, Guid> MovedDataCache { get; private set; }


        static RMOneDriveExplorerBoardCache _instance;

        public static RMOneDriveExplorerBoardCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new RMOneDriveExplorerBoardCache();
                            _instance.Initialize();
                        }
                    }
                }
                return _instance;
            }
        }

        private void Initialize()
        {
        }

        public void AddTermChange(Guid termId, long count)
        {
            lock (termChangeLocker)
            {
                if (TermChangedDic.ContainsKey(termId))
                {
                    TermChangedDic[termId] += count;
                }
                else
                {
                    TermChangedDic.Add(termId, count);
                }
            }
        }

        public void AddCollectionChange(Guid collectionId, long count)
        {
            lock (collectionChangeLocker)
            {
                if (CollectionChangedDic.ContainsKey(collectionId))
                {
                    CollectionChangedDic[collectionId] += count;
                }
                else
                {
                    CollectionChangedDic.Add(collectionId, count);
                }
            }

        }

        public void AddTotalChange(BoardRecordStatus status, long count)
        {
            lock (totalLocker)
            {
                if (TotalChangedDic.ContainsKey(status))
                {
                    TotalChangedDic[status] += count;
                }
                else
                {
                    TotalChangedDic.Add(status, count);
                }
            }
        }

        public void AddDataOfDateChange(BoardRecordStatus status, string dater, long count)
        {
            lock (dateLocker)
            {
                var tempDic = DataOfDateChangedDic[status];
                if (tempDic.ContainsKey(dater))
                {
                    tempDic[dater] += count;
                }
                else
                {
                    tempDic.Add(dater, count);
                }
                DataOfDateChangedDic[status] = tempDic;
            }
        }

        public void AddMovedData(Guid id)
        {
            lock (movedDataLocker)
            {
                if (!MovedDataCache.ContainsKey(id))
                {
                    MovedDataCache.Add(id, Guid.Empty);
                }
            }
        }
    }  
}
