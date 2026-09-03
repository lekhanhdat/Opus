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
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MachineLearning
{
    public enum RMMLUnderReview
    { 
        None = 0,
        IsManual = 1,
        DirectAssign = 2,
    }
    public enum RMMLClassificationType
    {
        None = 0,
        // 1.ApplySettingJob直接给文件赋值预测Term
        // 2.DataSyncJob直接使用预测Term
        // 3.ManualReview页面点击Apporved
        //以上三种情况都算是AutoClassfied
        AutoClassfied = 1,
        /// <summary>
        ///在ManualReview页面通过ChangeTerm操作赋值的 
        /// </summary>
        ManualClassified = 2,
        Rejected = 3, //Need Review yyang
    }
    public enum RMMLApprovalStatus
    {
        None = 0,
        WaitingApprove = 1,
        Approved = 2,
        Rejected = 3,
        Exception = 4,
    }

    public class MLManualEmailDto
    {
        public string JobId { get; set; }
        public List<int> ReviewerIds{ get; set; }
    }

    public enum TrainingAddType
    {
        None = 0,
        AddManually = 1,
        Reclassify = 2,
    }


    public class ApplySettingPredictResult
    {
        public Guid TermId { get; set; }
        public string TermName { get; set; }
        public double TermScore { get; set; }
        public RMMLUnderReview UnderReviewMethod { get; set; } 
        public bool IsUpdateSharePoint { get; set; }
        public bool IsSyncCosmosDB { get; set; }
        public SourceFlag Source { get; set; }
        public bool IsApplyDefaultTerm { get; set; }
    }

    public class PredictScoreResult
    {
        public Guid TermId { get; set; }
        public double TermScore { get; set; }
    }

    [DataContract]
    public enum TrainingFilterColumn
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Status = 1,
        [EnumMember]
        TrainingTerm = 2,
        [EnumMember]
        IntelligentTerm = 3,
        [EnumMember]
        ReclassifyTerm = 4,
        [EnumMember]
        ApprovalStatus = 5,
        [EnumMember]
        PredictTime = 6,
    }

    public enum PredictionJobRunningAction
    {
        ChangePredictionMode = 0,
        ModifyPredictionLabels = 1,
    }
}
