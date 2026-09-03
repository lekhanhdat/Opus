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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.API
{
    public class PhysicalBaseObject
    {
        public Record Record;

        private Dictionary<string, string> metaInfo = null;
        protected Dictionary<string, string> MetaInfo
        {
            get
            {
                if (metaInfo == null)
                {
                    metaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(Record.MetaInfo);
                }
                return metaInfo;
            }
            set
            {
                metaInfo = value;
                Record.MetaInfo = JsonConvert.SerializeObject(value);
            }
        }
        private ExplorerDao rmExplorerDao = null;
        protected ExplorerDao ExplorerDao
        {
            get
            {
                if (rmExplorerDao == null)
                {
                    rmExplorerDao = new ExplorerDao(true);
                }
                return rmExplorerDao;
            }
        }

        private IExplorerService mExplorerService;
        public IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }

        public int NodeType { get; set; }//稍后会替换成枚举对象
        public Guid RootLocationId { get { return Guid.Parse(Record.AveSiteId); } set { Record.AveSiteId = value.ToString(); } }
        public virtual Guid LocationId { get { return Record.LocationId; } set { Record.LocationId = value; } }
        public virtual Guid BoxId { get { return Record.BoxId; } set { Record.BoxId = value; } }
        public virtual Guid FileId { get { return Record.FileId; } set { Record.FileId = value; } }
        public virtual Guid ParentId { get { return Record.ParentId; } set { Record.ParentId = value; } }
        public virtual Guid Id { get { return Record.Id; } set { Record.Id = value; } }
        public virtual string Name { get { return this.Record.LeafName; } set { this.Record.LeafName = value; } }
        public virtual string Description { get { return this.Fields["Description"]; } set { this.Fields["Description"] = value; } }
        public virtual string RecordId { get { return this.Record.RecordsId; } set { this.Record.RecordsId = value; } }
        public virtual string CreateBy { get { return this.Record.CreatedBy; } set { this.Record.CreatedBy = value; } }
        public virtual string ModifiedBy { get { return this.Record.ModifiedBy; } set { this.Record.ModifiedBy = value; } }
        public virtual long CreateTimeTicks { get { return this.Record.TimeCreated; } set { this.Record.TimeCreated = value; } }
        public virtual long ModifiedTimeTicks { get { return this.Record.TimeModified; } set { this.Record.TimeModified = value; } }
        public virtual Guid TermId { get { return this.Record.TermId; } set { this.Record.TermId = value; } }
        public virtual Dictionary<string, string> Fields { get { return MetaInfo; } }
        public virtual int TemplateId { get { return this.Record.TemplateId; } set { this.Record.TemplateId = value; } }
        public virtual long DisposalDueDate { get { return this.Record.DisposalDueDate; } set { this.Record.DisposalDueDate = value; } }
        public virtual List<Guid> Ancestors { get { return Record.Ancestors; } set { Record.Ancestors = value; } }
        public virtual long PreviousDisposalDueDate { get { return this.Record.PreviosDisposalDueDate; } set { this.Record.PreviosDisposalDueDate = value; } }
        public virtual string RelatedRecords { get { return this.Record.RelatedRecords; } set { this.Record.RelatedRecords = value; } }
        public virtual int RelatedRecordsCount { get { return this.Record.RelatedRecordsCount; } set { this.Record.RelatedRecordsCount = value; } }       
        public virtual long ManualExtendTime { get { return this.Record.ManualExtendTime; } set { this.Record.ManualExtendTime = value; } }
        public virtual int ScopePermissionId
        {
            get
            {
                int permissionid = 0;
                try
                {
                    permissionid = this.Record.ScopePermissionId;
                }
                catch (Exception ex)
                {
                    //老数据升级上来没有此属性，默认赋值为0
                    permissionid = 0;
                }
                return permissionid;
            }
            set { this.Record.ScopePermissionId = value; }
        }
        public virtual string this[string name]
        {
            get
            {
                var field = this.Fields?.Where(f => f.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                return field?.Value;
            }
            set
            {
                this.Fields[name] = value;
                MetaInfo = Fields;
            }
        }

        public virtual void Add(Record record)
        {
            ExplorerDao.AddPhysicalRecord(record);
        }

        public virtual async Task DeleteAsync()
        {
            //2 means archived
            ExplorerDao.UpdateRecordState(this.Record, 2);
        }

        public virtual void Update(bool forceUpdate = false, bool isModifyPermissionId = false, bool isUpdateManualProperties = false)
        {
            ExplorerDao.UpdatePhysicalRecord(Record, forceUpdate, isModifyPermissionId, isUpdateManualProperties);
        }
    }
}
