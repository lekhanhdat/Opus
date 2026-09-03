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
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    internal abstract class EXOObjectBackup : IDisposable
    {
        /// <summary>
        /// Real backup method.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="entity"></param>
        /// <exception cref=""></exception>
        public abstract int Backup(EXOArchiveData entity, string ruleName, string subJobid, int ruleLevel, string mediaName);

        protected AvePoint.RA.Contract.Services.IRALogger mLog { get; set; }

        //protected static AveBackupOMFactory mFactory = AveBackupOMFactory.CreateBackupOMFactory();

        //protected static AveObjectModelFactory mFactory = new AveObjectModelFactory();

        //public BackupInfoSender AveSender { get; set; }

        public EXOConfiguration Configuration { get; set; }

        //public VaultBefArcInfo VaultBeforeArcInfo { get; set; }

        public EXOObjectBackup VaultExport { get; set; }

        //add  this for life cycle rule,目前仅为了获取Size,如果以后想要获取更多属性，可以封成对象.
        public long BackupSize { get; set; }
        public VaultBefArcInfo VaultBeforeArcInfo { get; set; }
        public EXOExportBeforeArcInfo EXOExportBeforeArcInfo { get; set; }
        public Dictionary<int, object> MicroFeedCache = new Dictionary<int, object>();

        protected string EnsureUniqueFilePath(string folderPath, string itemPath)
        {
            string fullPath = $"{folderPath}\\{itemPath}";
            if (this.Configuration.ItemFileNameCounter.TryGetValue(fullPath, out int count))
            {
                count++;
                this.Configuration.ItemFileNameCounter[fullPath] = count;
                var extension = itemPath.LastIndexOf('.') >= 0 ? itemPath.Substring(itemPath.LastIndexOf('.')) : string.Empty;
                itemPath = itemPath.Substring(0, itemPath.Length - extension.Length) + $"_{count:D3}" + extension;
            }
            else
            {
                this.Configuration.ItemFileNameCounter[fullPath] = 0;
            }
            return itemPath;
        }

        public void Dispose()
        { }
    }
}
