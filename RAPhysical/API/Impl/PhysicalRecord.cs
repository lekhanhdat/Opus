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
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.API
{
    public class PhysicalRecord : PhysicalBaseObject, IPhysicalRecord
    {
        private IPhysicalLocation mLocation;
        private IPhysicalBox mBox;
        private IPhysicalFile mFile;

        public string DirPath
        {
            get
            {
                return ExplorerService.GetPhysicalObjectFullPath(this.Id, false) + "/" + this.Name;
            }
        }

        public object Barcode { get; }
        public Guid RuleId
        {
            get
            {
                return base.Record.RuleId;
            }

            set
            {
                base.Record.RuleId = value;
            }
        }
        public int DisposalStatus { get { return base.Record.DisposalStatus; } set { base.Record.DisposalStatus = value; } }
        public int RecordStatus { get { return base.Record.RecordStatus; } set { base.Record.RecordStatus = value; } }
        public long DisposalActionTime { get { return base.Record.DestroyedTime; } set { base.Record.DestroyedTime = value; } }
        public bool ExportToManual { get { return base.Record.ExportToRECO; } set { base.Record.ExportToRECO = value; } }
        public int DeleteRelatedRecords { get { return base.Record.DeleteRelatedRecords; } set { base.Record.DeleteRelatedRecords = value; } }
        public string PhysicalActionAudit { get { return base.Record.PhysicalActionAudit; } set { base.Record.PhysicalActionAudit = value; } }

        public IPhysicalLocation ParentLocation
        {
            get
            {
                if (mLocation == null)
                {
                    mLocation = new PhysicalLocation(base.Record.LocationId);
                }
                return mLocation;
            }
            set
            {
                mLocation = value;
            }
        }

        public IPhysicalBox ParentBox
        {
            get
            {
                if (mBox == null)
                {
                    Record record = null;
                    //目前不知道scopeid是否为必须，所以暂时不实现，稍后进行处理
                    //record = ExplorerDao.GetRecordByIds()
                    mBox = new PhysicalBox(record);
                }
                return mBox;
            }
            set
            {
                mBox = value;
            }
        }

        public IPhysicalFile ParentFile
        {
            get
            {
                if (mFile == null)
                {
                    Record record = null;
                    //目前不知道scopeid是否为必须，所以暂时不实现，稍后进行处理
                    //record = ExplorerDao.GetRecordByIds()
                    mFile = new PhysicalFile(record);
                }
                return mFile;
            }
            set
            {
                mFile = value;
            }
        }

        public bool HoldStatus
        {
            get
            {
                return base.Record.HoldStatus;
            }

            set
            {
                base.Record.HoldStatus = value;
            }
        }

        public PhysicalRecord(Record record)
        {
            base.Record = record;
        }

        public PhysicalRecord(IPhysicalFile file, Record record)
            : this(record)
        {
            this.mFile = file;
        }

         public PhysicalRecord(IPhysicalLocation location, IPhysicalBox box, Record record)
            : this(record)
        {
            this.mLocation = location;
            this.mBox = box;
        }

        public void Dispose()
        {
        }
    }
}
