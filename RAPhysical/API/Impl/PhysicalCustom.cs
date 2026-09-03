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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAPhysical.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.API
{
   public class PhysicalCustom: PhysicalBaseObject, IPhysicalCustom
    {
        private IPhysicalLocation mLocation;
        /// <summary>
        /// 获取Open， close， destroy， missing状态的数据，使用过程外围需要自己区分
        /// </summary>
        public List<IPhysicalFile> Files { get { return ExplorerDao.QueryAll(r => r.SourceFlag == (int)SourceFlag.Physical && r.RecordStatus != 3 &&  r.ParentId == Record.Id && r.NodeType == (int)RMNodeLevel.PhysicalFile).Select(item => new PhysicalFile(ParentLocation, null, item) as IPhysicalFile).ToList(); } }

        public List<IPhysicalBox> Boxes { get { return ExplorerDao.QueryAll(r => r.SourceFlag == (int)SourceFlag.Physical && r.RecordStatus != 3 && r.ParentId == Record.Id && r.NodeType == (int)RMNodeLevel.PhysicalBox).Select(item => new PhysicalBox(ParentLocation, item) as IPhysicalBox).ToList(); } }

        public List<IPhysicalCustom> CustomContainers { get { return ExplorerDao.QueryAll(r => r.SourceFlag == (int)SourceFlag.Physical && r.RecordStatus != 3 && r.ParentId == Record.Id && r.NodeType == (int)RMNodeLevel.PhysicalCustom).Select(item => new PhysicalCustom(ParentLocation, item) as IPhysicalCustom).ToList(); } }

        public string DirPath
        {
            get
            {
                return ExplorerService.GetPhysicalObjectFullPath(this.Id, false) + "/" + this.Name;
            }
        }

        public object Barcode { get; }

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

        public long HoldReleaseTime { get { return base.Record.HoldReleaseTime; } set { base.Record.HoldReleaseTime = value; } }

        public string HoldBy { get { return base.Record.HoldBy; } set { base.Record.HoldBy = value; } }
        public string HoldId { get { return base.Record.HoldId; } set { base.Record.HoldId = value; } }
        public int HoldType { get { return base.Record.HoldType; } set { base.Record.HoldType = value; } }

        public PhysicalCustom(Record record)
        {
            base.Record = record;
        }

        public PhysicalCustom(IPhysicalLocation location, Record record)
            : this(record)
        {
            mLocation = location;
        }

        public PhysicalCustom(Guid customId)
        {
            base.Record = ExplorerDao.GetPhysicalRecordById(customId);
        }

        public void AddPhysicalFile(IPhysicalFile file)
        {
            base.Add((file as PhysicalFile).Record);
        }

        public void Dispose()
        {
        }

        public List<IPhysicalFile> GetFiles(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", (int)RMNodeLevel.PhysicalFile));
            allExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(Record), param, "Ancestors", this.Id));
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAll(finalExpression).Select(item => new PhysicalFile(ParentLocation, null, item) as IPhysicalFile).ToList();
        }

        public List<PhysicalFile> GetFilesOrderByDescending(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", (int)RMNodeLevel.PhysicalFile));
            allExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(Record), param, "Ancestors", this.Id));
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAllByDescending(finalExpression).Select(item => new PhysicalFile(ParentLocation, null, item)).ToList();
        }


        public long GetFilesCount(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", (int)RMNodeLevel.PhysicalFile));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", this.Id));
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAll(finalExpression).Count();
        }

        public List<IPhysicalBox> GetBoxes(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", (int)RMNodeLevel.PhysicalBox));
            allExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(Record), param, "Ancestors", this.Id));
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAll(finalExpression).Select(item => new PhysicalBox(ParentLocation, item) as IPhysicalBox).ToList();
        }

        public List<IPhysicalCustom> GetCustomContainers(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", (int)RMNodeLevel.PhysicalCustom));
            allExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(Record), param, "Ancestors", this.Id));
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAll(finalExpression).Select(item => new PhysicalCustom(ParentLocation, item) as IPhysicalCustom).ToList();
        }
    }
}
