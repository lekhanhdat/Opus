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
    public class MLTermDto
    {
        public Guid Id { set; get; }
        public string Name { get; set; }
        public MLTermStatus Status { get; set; }
        public bool AutoApply { get; set; }
        public double Accuracy { get; set; }
        //public double ScopeChanged { get; set; }
        public int TrainingScope { get; set; }
        public bool Published { get; set; }
        public long ModifedTime { get; set; }
        public Guid ModeId { get; set; }
        public string FullPath { get; set; }
        public double PredictTermScore { get; set; }
        public string Description { get; set; }
        public long ZeroApprovalCount { get; set; }
        public long ZeroReclassifyCount { get; set; }
    }
    [DataContract]
    public class MLTermQueryParam
    {
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public string SortBy { get; set; }
        [DataMember]
        public bool IsAscending { get; set; }
        [DataMember]
        public List<TermFilter> Filters { set; get; }
    }
    [DataContract]
    public class TermFilter
    {
        [DataMember]
        public TermFilterColumn Column { get; set; }
        [DataMember]
        public List<string> ColumnValues { get; set; }
    }
    [DataContract]
    public enum TermFilterColumn
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Status,
        [EnumMember]
        AutoApply
    }
    [DataContract]
    public class SetAutoApplyParam
    {
        [DataMember]
        public Guid TermId { get; set; }
        [DataMember]
        public bool AutoApply { get; set; }
    }
    public class MLTermResponseResult
    {
        public bool HasError { get; set; }
        public string ErrorMsg { get; set; }
        public List<MLTermDto> MLTerms { get; set; }
        public List<UsageTermDto> UsageTerms { get; set; }
        public int TotalCount { get; set; }
    }
    [DataContract]
    public class UsageTermQueryParam
    {
        //public int SourceFlag { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
    }


    public class UsageTermDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FullPath { get; set; }
        //public long Active { get; set; }
    }

    public class TrainModeInfo
    {
        public TrainModeStatus Status { get; set; }
    }
    public enum TrainModeStatus
    {
        None,
        Inprogress,
        Finished
    }

    public class ValidateDefaultTermResult
    {
        public bool IsExists { get; set; }
        public List<string> DefaultTermNames { get; set; }
    }

    public enum MLTermStatus
    {
        NotTrain = 0,
        Training = 1,
        Trained = 2,
        Removed = 4,
    }
    public class MLTermStatusHelper
    {
        public static MLTermStatus[] ActiveTermStatus = new MLTermStatus[] { MLTermStatus.NotTrain, MLTermStatus.Training, MLTermStatus.Trained };
        public static int[] ActiveTermIntStatus = new int[] { (int)MLTermStatus.NotTrain, (int)MLTermStatus.Training, (int)MLTermStatus.Trained };
    }
}
