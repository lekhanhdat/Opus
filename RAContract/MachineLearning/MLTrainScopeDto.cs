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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MachineLearning
{
    public class MLTrainScopeDto
    {
        public Guid Id { set; get; }
        public string FileName { get; set; }
        public MLFileStatus Status { get; set; }
        public Guid TermId { get; set; }
        public string TermName { get; set; }
        public int SourceFlag { get; set; }
        public string FullPath { get; set; }
    }
    [DataContract]
    public class MLTrainingScopeQueryParam
    {
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public string PageIndex { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public string SortBy { get; set; }
        [DataMember]
        public bool IsAscending { get; set; }
        [DataMember]
        public List<TrainingScopeFilter> Filters { set; get; }
    }
    [DataContract]
    public class TrainingScopeFilter
    {
        [DataMember]
        public TrainingFilterColumn Column { get; set; }
        [DataMember]
        public List<string> ColumnValues { get; set; }
    }

    public class MLTrainingScopeResult
    {
        public bool HasError { get; set; }
        public string ErrorMsg { get; set; }
        public List<MLTrainScopeDto> TrainingScopes { get; set; }
        public int TotalCount { get; set; }
        public string PageIndex { get; set; }
    }

    public class MLTrainingScopeManage
    {
        public string LocationId { get; set; }
        public string Location { get; set; }
        public MTSSourceFlag SourceFlag { get; set; }
        public int TrainingScopeOption { get; set; }
    }

    public enum MTSSourceFlag
    {
        None = 0,
        SPO = 1,
        Google = 2
    }

    public enum MLFileStatus
    {
        None = 0,
        NotTrain = 1,
        Training = 2,
        Trained = 3
    }
    public enum MLModelStatus
    {
        None,
        Running,
        Succeeded,
        Failed,
        Exception
    }
}
