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
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAPhysical.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.API
{
    public class PhysicalLocation : PhysicalBaseObject, IPhysicalLocation
    {
        RMLocationDao rmLocationDao = null;
        private RMLocationDao RMLocationDao
        {
            get
            {
                if (rmLocationDao == null)
                {
                    rmLocationDao = new RMLocationDao();
                }
                return rmLocationDao;
            }
        }

        private RMLocation mLocation;
        private IPhysicalLocation mParentLocation;

        public bool IsRootLocation { get { return mLocation.NodeType == (int)RMNodeLevel.PhysicalRootLocation; } }
        public bool IsBottomLocation { get { return mLocation.NodeType == (int)RMNodeLevel.PhysicalBottomLocation; } }
        public bool Exist { get { return mLocation != null; } }
        public override string Name { get { return mLocation.Name; } set { mLocation.Name = value; } }
        public override string Description { get { return mLocation.Description; } set { mLocation.Description = value; } }

        public int CurrentLocationType { get { return mLocation.NodeType; } set { mLocation.NodeType = value; } }

        /// <summary>
        /// 获取Open， close， destroy， missing状态的数据，使用过程外围需要自己区分
        /// </summary>
        public List<IPhysicalFile> AllFiles { get { return IsBottomLocation ? ExplorerDao.QueryAll(f => f.SourceFlag == (int)SourceFlag.Physical && f.RecordStatus != 3 && f.LocationId == this.UniqueId && f.NodeType == (int)RMNodeLevel.PhysicalFile && f.BoxId == Guid.Empty).Select(item => new PhysicalFile(this, null, item) as IPhysicalFile).ToList() : null; } }

        /// <summary>
        /// 获取Open， close， destroy， missing状态的数据，使用过程外围需要自己区分
        /// </summary>
        public List<IPhysicalBox> AllBoxes { get { return IsBottomLocation ? ExplorerDao.QueryAll(f => f.SourceFlag == (int)SourceFlag.Physical && f.RecordStatus != 3 && f.LocationId == this.UniqueId && f.NodeType == (int)RMNodeLevel.PhysicalBox).Select(item => new PhysicalBox(this, item) as IPhysicalBox).ToList() : null; } }

        public List<IPhysicalCustom> AllContainers { get { return IsBottomLocation ? ExplorerDao.QueryAll(f => f.SourceFlag == (int)SourceFlag.Physical && f.RecordStatus != 3 && f.LocationId == this.UniqueId && f.NodeType == (int)RMNodeLevel.PhysicalCustom).Select(item => new PhysicalCustom(this, item) as IPhysicalCustom).ToList() : null; } }

        public override Dictionary<string, string> Fields { get; }//Get Properties from DB Later

        public int SubLocationTotalCount { get { return RMLocationDao.CountSubLocation(this.mLocation.Id); } }

        public bool IsFilterLocationPermission { get; private set; }

        public List<Guid> FilterLocationIds { get; private set; }

        public List<IPhysicalLocation> AllSubLocations 
        {
            get 
            {
                if (IsFilterLocationPermission)
                {
                    return RMLocationDao.GetAllSubLocationByParentIdAndUniqueIds(this.mLocation.Id, FilterLocationIds).Select(l => new PhysicalLocation(this, l) as IPhysicalLocation).ToList();
                } 
                return RMLocationDao.GetAllSubLocationByParentId(this.mLocation.Id).Select(l => new PhysicalLocation(this, l) as IPhysicalLocation).ToList(); 
            } 
        }

        public double TotalCapacity { get { return mLocation.AvailableSpace; } set { mLocation.AvailableSpace = value; } }

        public string DirPath => IsRootLocation ? this.Name : ParentLocation?.DirPath + "/" + this.Name;

        public string DirPathIds => mLocation.DirPath;

        public Guid UniqueId { get { return mLocation.UniqueId; } }
        public int IntId { get { return mLocation.Id; } }
        public int LocationParentId { get { return mLocation.ParentId; } set { mLocation.ParentId = value; } }

        public IPhysicalLocation ParentLocation
        {
            get
            {
                if (mParentLocation == null && !IsRootLocation)
                {
                    mParentLocation = new PhysicalLocation(LocationParentId);
                }
                return mParentLocation;
            }
            set
            {
                mParentLocation = value;
            }
        }

        public List<IPhysicalBox> Boxes
        {
            get { return IsBottomLocation ? ExplorerDao.QueryAll(f => f.SourceFlag == (int)SourceFlag.Physical && f.RecordStatus != 3 && f.LocationId == this.UniqueId && (f.ParentId == Guid.Empty || (f.Ancestors != null && f.ParentId == this.UniqueId)) && f.NodeType == (int)RMNodeLevel.PhysicalBox).Select(item => new PhysicalBox(this, item) as IPhysicalBox).ToList() : null; }
        }

        public List<IPhysicalFile> Files
        {
            get { return IsBottomLocation ? ExplorerDao.QueryAll(f => f.SourceFlag == (int)SourceFlag.Physical && f.RecordStatus != 3 && f.LocationId == this.UniqueId && ((f.ParentId == Guid.Empty && f.BoxId == Guid.Empty) || (f.Ancestors != null && f.ParentId == this.UniqueId)) && f.NodeType == (int)RMNodeLevel.PhysicalFile && f.BoxId == Guid.Empty).Select(item => new PhysicalFile(this, null, item) as IPhysicalFile).ToList() : null; }
        }

        public List<IPhysicalCustom> Containers
        {
            get { return IsBottomLocation ? ExplorerDao.QueryAll(f => f.SourceFlag == (int)SourceFlag.Physical && f.RecordStatus != 3 && f.ParentId == this.UniqueId && f.NodeType == (int)RMNodeLevel.PhysicalCustom).Select(item => new PhysicalCustom(this, item) as IPhysicalCustom).ToList() : null; }
        }

        /// <summary>
        /// For Physical only
        /// </summary>
        /// <param name="locationId"></param>
        public PhysicalLocation(int locationId)
        {
            mLocation = RMLocationDao.GetLocationById(locationId);
        }

        public PhysicalLocation(Guid locationUniqueId)
        {
            mLocation = RMLocationDao.GetLocationByUniqueId(locationUniqueId);
        }

        /// <summary>
        /// 传递Parent对象到子对象中
        /// </summary>
        /// <param name="parentLocation"></param>
        /// <param name="locationId"></param>
        public PhysicalLocation(IPhysicalLocation parentLocation, int locationId)
            :this(locationId)
        {
            mParentLocation = parentLocation;
        }

        internal PhysicalLocation(IPhysicalLocation parentLocation, RMLocation location)
        {
            mLocation = location;
        }

        public PhysicalLocation CreatePhysicalLocation(string name)
        {
            PhysicalLocation phyRecord = null;
            phyRecord = new PhysicalLocation(this, RMLocationDao.CreateLocation(name, mLocation.Id));
            return phyRecord;
        }

        public IPhysicalBox CreatePhysicalBox(string boxName)
        {
            throw new NotImplementedException();
        }

        public IPhysicalFile CreatePhysicalFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync()
        {
            return RMLocationDao.DeleteLocationAsync(mLocation.Id);
        }

        public void SetFilterSubLocationByUniqueId(List<Guid> uniqueIds)
        {
            IsFilterLocationPermission = true;
            FilterLocationIds = uniqueIds;
        }

        /// <summary>
        /// Do not support update DirPath now, as we do not support move Location.
        /// </summary>
        public Task UpdateAsync()
        {
            return RMLocationDao.UpdateAsync(mLocation);
        }

        public void Dispose()
        {
            if (rmLocationDao != null)
            {
                rmLocationDao = null;
            }
        }

        public long GetBoxesCount(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", (int)RMNodeLevel.PhysicalBox));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", this.UniqueId));

            PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
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
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", this.UniqueId));

            PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAll(finalExpression).Select(item => new PhysicalBox(this, item) as IPhysicalBox).ToList();
        }

        public List<Record> GetBoxesAndFoldOrderByDescending(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            List<Expression> nodeTypeExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalBox));
            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
            allExpressionList.Add(nodeTypeExpressionList.Aggregate(Expression.OrElse));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", this.UniqueId));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", Guid.Empty));
            PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAllByDescending(finalExpression).ToList();
        }

        public long GetFilesCount(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", (int)RMNodeLevel.PhysicalFile));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", this.UniqueId));

            PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAll(finalExpression).Count();
        }

        public List<IPhysicalFile> GetFiles(Expression<Func<Record, bool>> expression)
        {
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", (int)RMNodeLevel.PhysicalFile));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", this.UniqueId));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", Guid.Empty));
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            return ExplorerDao.QueryAll(finalExpression).Select(item => new PhysicalFile(this, null, item) as IPhysicalFile).ToList();
        }

        public Dictionary<RMNodeLevel, List<Object>> Query(Expression<Func<Record, bool>> expression)
        {
            var result = new Dictionary<RMNodeLevel, List<Object>>
            {
                { RMNodeLevel.PhysicalBox, new List<object>() },
                { RMNodeLevel.PhysicalFile, new List<object>() },
                { RMNodeLevel.PhysicalRecord, new List<object> () }
            };
            Expression<Func<Record, bool>> finalExpression = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", (int)SourceFlag.Physical));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", this.UniqueId));
            if (allExpressionList.Count > 0)
            {
                var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                PhysicalExpressionVisitor visitor = new PhysicalExpressionVisitor(param);
                finalExpression = Expression.Lambda<Func<Record, bool>>(Expression.AndAlso(queryExpr, visitor.Visit(expression.Body)), param);
            }
            var queryResult = ExplorerDao.QueryAll(finalExpression).ToList();
            queryResult.ForEach(r =>
            {
                if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                {
                    result[RMNodeLevel.PhysicalBox].Add(new PhysicalBox(this, r) as IPhysicalBox);
                }
                else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                {
                    result[RMNodeLevel.PhysicalFile].Add(new PhysicalFile(r) as IPhysicalFile);
                }
                else if (r.NodeType == (int)RMNodeLevel.PhysicalRecord)
                {
                    result[RMNodeLevel.PhysicalRecord].Add(new PhysicalRecord(r) as IPhysicalRecord);
                }
                else
                {
                    //todo log here
                }
            });
            return result;
        }
    }
}
