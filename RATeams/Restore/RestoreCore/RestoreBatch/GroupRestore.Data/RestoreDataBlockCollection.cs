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

namespace Office365GroupRestore
{
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.RA.CommonUtil;
    using ExchangeUtility.Graph;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    

    public class RestoreDataBlockCollection
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RestoreDataBlockCollection));
        private readonly int maxItemsCount;
        private readonly int maxSizeLimit;
        private const int LargeSizeItemLimit = 850 * 1024;

        private static RestoreDataBlockCollection currentCollection;

        private List<ExchangeDataBlockForBatch> items;

        //private static string currentTopicId = string.Empty;
        private static int currentMonth = 0;

        public IEnumerable<ExchangeDataBlockForBatch> Items
        {
            get { return items; }
        }

        public int ItemsCount
        {
            get { return items.Count; }
        }

        /// <summary>
        /// Item Size总和
        /// </summary>
        public long TotalSize { get; private set; }

        public ExchangeDataBlockType CollectionType { get; protected set; }

        //public string ParentFolderIntenalFolderPath { get; private set; }

        protected RestoreDataBlockCollection(int maxItemsCount, int maxSizeLimit, ExchangeDataBlockForBatch item)
            : this(maxItemsCount, maxSizeLimit)
        {
            Add(item);
        }

        protected RestoreDataBlockCollection(int maxItemsCount, int maxSizeLimit)
        {
            this.items = new List<ExchangeDataBlockForBatch>();
            this.TotalSize = 0L;
            this.maxItemsCount = maxItemsCount;
            this.maxSizeLimit = maxSizeLimit * 1024 * 1024;
        }

        private bool NeedChangeCollection(ExchangeDataBlockForBatch item)
        {
            return (this.items.Count >= this.maxItemsCount || this.TotalSize + item.FileTail.FileSize > maxSizeLimit);
        }

        private void Add(ExchangeDataBlockForBatch item)
        {
            this.items.Add(item);
            this.TotalSize += item.FileTail == null ? 0 : item.FileTail.FileSize;
            //this.ParentFolderIntenalFolderPath = item.FileHeader.ParentFullPath;
        }

        public static void GroupNormalDataBlock(ExchangeDataBlockForBatch dataBlock, BlockingCollection<RestoreDataBlockCollection> outputCollection, int maxItemCount, int maxSizeLimit)
        {
            if (dataBlock.FileHeader.DataType == ExchangeDataType.Mailbox) AddingMailboxDataBlock(outputCollection, dataBlock, maxItemCount, maxSizeLimit);
            if (dataBlock.FileHeader.DataType == ExchangeDataType.Folder) AddingFolderDataBlock(outputCollection, dataBlock, maxItemCount, maxSizeLimit);
            if (dataBlock.FileHeader.DataType == ExchangeDataType.Plan) AddingPlanDataBlock(outputCollection, dataBlock, maxItemCount, maxSizeLimit);
            if (dataBlock.FileHeader.DataType == ExchangeDataType.Task) AddingTaskDataBlock(outputCollection, dataBlock, maxItemCount, maxSizeLimit);
            if (dataBlock.FileHeader.DataType == ExchangeDataType.Item) AddingItemsDataBlock(outputCollection, dataBlock, maxItemCount, maxSizeLimit);
        }
        public static void AddEXODataBlock(ExchangeDataBlockForBatch dataBlock, BlockingCollection<RestoreDataBlockCollection> outputCollection)
        {
            if (dataBlock.FileHeader.DataType == ExchangeDataType.Post)
            {
                outputCollection.Add(new PostDataBlockCollection(dataBlock));
            }
            if (dataBlock.FileHeader.DataType == ExchangeDataType.CalendarEvent)
            {
                outputCollection.Add(new EventDataBlockCollection(dataBlock));
            }
            if (dataBlock.FileHeader.DataType == ExchangeDataType.Attachment)
            {
                outputCollection.Add(new AttachmentDataBlockCollection(dataBlock));
            }
        }

        public static void AddSiteDataBlock(ExchangeDataBlockForBatch dataBlock, BlockingCollection<RestoreDataBlockCollection> outputCollection)
        {
            switch (dataBlock.FileHeader.DataType)
            {
                case ExchangeDataType.SiteAttachmentItem:
                    outputCollection.Add(new SiteAttachmentDataBlockCollection(dataBlock));
                    break;
                case ExchangeDataType.SiteDocumentItem:
                    outputCollection.Add(new SiteDocumentDataBlockCollection(dataBlock));
                    break;
                case ExchangeDataType.SiteVersionItem:
                    outputCollection.Add(new SiteVersionDataBlockCollection(dataBlock));
                    break;
                case ExchangeDataType.SiteCollection:
                    outputCollection.Add(new SiteCollectionDataBlockCollection(dataBlock));
                    break;
                case ExchangeDataType.SiteFolder:
                    outputCollection.Add(new SiteFolderDataBlockCollection(dataBlock));
                    break;
                case ExchangeDataType.SiteList:
                    outputCollection.Add(new SiteListDataBlockCollection(dataBlock));
                    break;
                case ExchangeDataType.Web:
                    outputCollection.Add(new WebDataBlockCollection(dataBlock));
                    break;
            }
        }

        public static void GroupFinishOrExceptionDataBlock(ExchangeDataBlockForBatch dataBlock, BlockingCollection<RestoreDataBlockCollection> outputCollection)
        {
            if (dataBlock.IsFinish) AddingFinishDataBlock(outputCollection, dataBlock);
            if (dataBlock.IsException) AddingExceptionDataBlock(outputCollection, dataBlock);
        }

        public static void GroupReportDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock, int maxItemCount, int maxSizeLimit, string reportMsg, ReportType reportType)
        {
            AddingReportDataBlock(outputCollection, dataBlock, maxItemCount, maxSizeLimit, reportMsg, reportType);
        }

        private static void AddingMailboxDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock, int maxItemCount, int maxSizeLimit)
        {
            if (currentCollection != null && currentCollection.ItemsCount > 0)
            {
                outputCollection.Add(currentCollection);
                currentCollection = new ItemDataBlockCollection(maxItemCount, maxSizeLimit);
            }
            outputCollection.Add(new MailboxDataBlockCollection(dataBlock));
        }

        private static void AddingFolderDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock, int maxItemCount, int maxSizeLimit)
        {
            if (currentCollection != null && currentCollection.ItemsCount > 0)
            {
                outputCollection.Add(currentCollection);
                currentCollection = new ItemDataBlockCollection(maxItemCount, maxSizeLimit);
            }
            outputCollection.Add(new FolderDataBlockCollection(dataBlock));
        }

        private static void AddingPlanDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock, int maxItemCount, int maxSizeLimit)
        {
            if (currentCollection != null && currentCollection.ItemsCount > 0)
            {
                outputCollection.Add(currentCollection);
                currentCollection = new ItemDataBlockCollection(maxItemCount, maxSizeLimit);
            }
            outputCollection.Add(new PlanDataBlockCollection(dataBlock));
        }

        private static void AddingTaskDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock, int maxItemCount, int maxSizeLimit)
        {
            if (currentCollection != null && currentCollection.ItemsCount > 0)
            {
                outputCollection.Add(currentCollection);
                currentCollection = new ItemDataBlockCollection(maxItemCount, maxSizeLimit);
            }
            outputCollection.Add(new TaskDataBlockCollection(dataBlock));
        }

        private static void AddingItemsDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock, int maxItemCount, int maxSizeLimit)
        {
            if (currentCollection == null) currentCollection = new ItemDataBlockCollection(maxItemCount, maxSizeLimit);
            if (currentCollection.ItemsCount == 0) currentMonth = 0;
            var dataBlockMonth = (new DateTime(RestoreConfig.ItemCreateTimeInfo[GetExchangeId(dataBlock)])).Month;
            var sameId = IsTheSameTopicId(dataBlock);
            if (currentCollection.ItemsCount > 0 && ((currentMonth != dataBlockMonth && !sameId) || (currentCollection.NeedChangeCollection(dataBlock) && !sameId)))
            {
                outputCollection.Add(currentCollection);
                currentCollection = new ItemDataBlockCollection(maxItemCount, maxSizeLimit);
            }
            if (currentMonth != dataBlockMonth && !sameId) currentMonth = dataBlockMonth;
            currentCollection.Add(dataBlock);
        }

        private static bool IsTheSameTopicId(ExchangeDataBlockForBatch dataBlock)
        {
            try
            {
                if (dataBlock.FileHeader.NodeType == 945) return false;
                var topicId = GetDataBlockTopicId(dataBlock);
                var currentCollectionLastItem = currentCollection.ItemsCount == 0 ? null : currentCollection.Items.Last();
                var lastItemTopicId = GetDataBlockTopicId(currentCollectionLastItem);
                if (topicId.Equals(lastItemTopicId, StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while to check the same topicid. Reason: {0}", ex.ToString());
                return false;
            }
        }

        private static string GetExchangeId(ExchangeDataBlockForBatch dataBlock)
        {
            string exchangeId = string.Empty;
            if (!string.IsNullOrEmpty(dataBlock.FileHeader?.Name) && dataBlock.FileHeader.Name.Contains(ExchangeConstants.PathParser))
            {
                exchangeId = dataBlock.FileHeader.Name;
            }
            if (string.IsNullOrEmpty(exchangeId))
            {
                exchangeId = dataBlock.RestoreData.Metadata.Title + ((char)0x12).ToString() + dataBlock.RestoreData.Metadata.ExchangeId;
            }
            return exchangeId;
        }

        private static string GetDataBlockTopicId(ExchangeDataBlockForBatch dataBlock)
        {
            string currentTopicId = string.Empty;
            string tempName = string.Empty;
            try
            {
                if (dataBlock == null)
                    return currentTopicId;
                if (dataBlock.FileHeader != null)
                {
                    tempName = dataBlock.FileHeader.Name;
                    tempName = tempName.Substring(0, tempName.LastIndexOf(ExchangeConstants.PathParser));
                    string[] conversationInfo = tempName.Split('/');
                    if (conversationInfo.Length == 2)
                    {
                        currentTopicId = conversationInfo[1];
                    }
                    else if (conversationInfo.Length == 3)
                    {
                        currentTopicId = Regex.IsMatch(conversationInfo[1], @"[\d]{13}") ? conversationInfo[1] : conversationInfo[2];
                    }
                    else if (conversationInfo.Length > 3)
                    {
                        currentTopicId = conversationInfo[2];
                    }
                    else
                    {
                        logger.Warn("The item title may be incorrect . ItemTitle: {0}.", tempName);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while to get item topic. ItemName: {0}. Reason: {1}", tempName, ex.ToString());
            }
            return currentTopicId;
        }

        private static void AddingFinishDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock)
        {
            if (currentCollection != null && currentCollection.ItemsCount > 0)
            {
                outputCollection.Add(currentCollection);
            }
            outputCollection.Add(new FinishDataBlockCollection(dataBlock));
            outputCollection.CompleteAdding();
        }

        private static void AddingExceptionDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock)
        {
            if (currentCollection != null && currentCollection.ItemsCount > 0)
            {
                outputCollection.Add(currentCollection);
            }
            outputCollection.Add(new ExceptionDataBlockCollection(dataBlock));
        }

        private static void AddingReportDataBlock(BlockingCollection<RestoreDataBlockCollection> outputCollection, ExchangeDataBlockForBatch dataBlock, int maxItemCount, int maxSizeLimit, string reportMsg, ReportType reportType)
        {
            if (currentCollection != null && currentCollection.ItemsCount > 0)
            {
                outputCollection.Add(currentCollection);
                currentCollection = new ItemDataBlockCollection(maxItemCount, maxSizeLimit);
            }
            outputCollection.Add(new ReportDataBlockCollection(dataBlock, reportMsg, reportType));
        }
    }

    internal class MailboxDataBlockCollection : RestoreDataBlockCollection
    {
        public MailboxDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Mailbox;
        }
    }

    internal class FolderDataBlockCollection : RestoreDataBlockCollection
    {
        public FolderDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Folder;
        }
    }

    internal class PlanDataBlockCollection : RestoreDataBlockCollection
    {
        public PlanDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Plan;
        }
    }

    internal class TaskDataBlockCollection : RestoreDataBlockCollection
    {
        public TaskDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Task;
        }
    }

    internal class ItemDataBlockCollection : RestoreDataBlockCollection
    {
        public ItemDataBlockCollection(int maxItemCount, int maxSizeLimit) : base(maxItemCount, maxSizeLimit)
        {
            CollectionType = ExchangeDataBlockType.Item;
        }
    }

    internal class PostDataBlockCollection : RestoreDataBlockCollection
    {
        public PostDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Post;
        }
    }
    internal class EventDataBlockCollection : RestoreDataBlockCollection
    {
        public EventDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Event;
        }
    }
    internal class AttachmentDataBlockCollection : RestoreDataBlockCollection
    {
        public AttachmentDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Attachment;
        }
    }

    internal class SiteDocumentDataBlockCollection : RestoreDataBlockCollection
    {
        public SiteDocumentDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.SiteDocumentItem;
        }
    }

    internal class SiteVersionDataBlockCollection : RestoreDataBlockCollection
    {
        public SiteVersionDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.SiteVersionItem;
        }
    }

    internal class SiteAttachmentDataBlockCollection : RestoreDataBlockCollection
    {
        public SiteAttachmentDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.SiteAttachmentItem;
        }
    }

    internal class SiteCollectionDataBlockCollection : RestoreDataBlockCollection
    {
        public SiteCollectionDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.SiteCollection;
        }
    }

    internal class SiteListDataBlockCollection : RestoreDataBlockCollection
    {
        public SiteListDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.SiteList;
        }
    }

    internal class SiteFolderDataBlockCollection : RestoreDataBlockCollection
    {
        public SiteFolderDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.SiteFolder;
        }
    }

    internal class WebDataBlockCollection : RestoreDataBlockCollection
    {
        public WebDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Web;
        }
    }

    internal class FinishDataBlockCollection : RestoreDataBlockCollection
    {
        public FinishDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            CollectionType = ExchangeDataBlockType.Finish;
        }
    }

    internal class ExceptionDataBlockCollection : RestoreDataBlockCollection
    {
        public ExceptionDataBlockCollection(ExchangeDataBlockForBatch dataBlock) : base(-1, -1, dataBlock)
        {
            base.CollectionType = ExchangeDataBlockType.Exception;
            this.ExceptionMessage = dataBlock.ExceptionMessage;
        }

        public string ExceptionMessage { get; private set; }
    }

    internal class ReportDataBlockCollection : RestoreDataBlockCollection
    {
        public ReportDataBlockCollection(ExchangeDataBlockForBatch dataBlock, string reportMsg, ReportType reportType) : base(-1, -1, dataBlock)
        {
            base.CollectionType = ExchangeDataBlockType.Report;
            this.ReportMsg = reportMsg;
            this.ReportType = reportType;
        }

        public string ReportMsg { get; private set; }

        public ReportType ReportType { get; private set; }
    }

    public enum ExchangeDataBlockType
    {
        Mailbox = 0,
        Folder = 1,
        Item = 2,
        Report = 3,
        Finish = 4,
        Exception = 5,
        Plan = 6,
        Task = 7,
        Post = 8,
        Event = 9,
        Attachment = 10,
        SiteAttachmentItem = 11,
        SiteDocumentItem = 12,
        SiteVersionItem = 13,
        SiteCollection = 14,
        Web = 15,
        SiteList = 16,
        SiteFolder = 17,
    }

    public enum ReportType
    {
        Success = 0,
        Skip = 1,
        Failed = 2
    }
}