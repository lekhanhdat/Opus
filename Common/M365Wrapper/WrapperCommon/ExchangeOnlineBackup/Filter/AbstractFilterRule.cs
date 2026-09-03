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


namespace ExchangeOnlineBackup
{
    #region namespace

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;

    #endregion namespace

    public abstract class AbstractFilterRule
    {
        public string BaseProperty { set; get; }

        public EOCategoryType CategoryType { set; get; }

        public EOAndOrType AndOrInfo { set; get; }

        public EOConditionType ConditionType { set; get; }

        public EORuleType RuleType { set; get; }

        public object FilterValue { set; get; }

        public abstract void Initialize(BaseFilterItem baseFilterItem);

        public abstract FilterResult CheckFilterStatus(Dictionary<string, ProposeInfo> propValueDic, EOCategoryType type);

        protected string GetProperty(EOCategoryType itemType)
        {
            string propertyName = string.Empty;
            switch (RuleType)
            {
                case EORuleType.Name:
                    propertyName = "Name";
                    break;
                case EORuleType.Subject:
                    propertyName = "Subject";
                    break;
                case EORuleType.CreateTime:
                    propertyName = "Created";
                    break;
                case EORuleType.ModifyTime:
                    propertyName = "Modified";
                    break;
                case EORuleType.ReceivedTime:
                    propertyName = "Received";
                    break;
                case EORuleType.Size:
                    propertyName = "Size";
                    break;
                case EORuleType.To:
                    propertyName = "To";
                    break;
                case EORuleType.From:
                    propertyName = "From";
                    break;
                case EORuleType.CreatedBy:
                    propertyName = "CreatedBy";
                    break;
                case EORuleType.ModifiedBy:
                    propertyName = "ModifiedBy";
                    break;
                case EORuleType.StartTime:
                    if (itemType == EOCategoryType.Task)
                    {
                        propertyName = "IPM.Task.StartDate";
                    }
                    else if (itemType == EOCategoryType.Journal)
                    {
                        propertyName = "IPM.Activity.Start";
                    }
                    else if (itemType == EOCategoryType.Event)
                    {
                        propertyName = "IPM.Appointment.EventDate";
                    }
                    break;
                case EORuleType.DueDate:
                    if (itemType == EOCategoryType.Task)
                    {
                        propertyName = "IPM.Task.DueDate";
                    }
                    else if (itemType == EOCategoryType.Journal)
                    {
                        propertyName = "IPM.Activity.End";
                    }
                    break;
                case EORuleType.EndTime:
                    propertyName = "IPM.Appointment.End";
                    break;
                case EORuleType.Status:
                    propertyName = "Status";
                    break;
                case EORuleType.Priority:
                    propertyName = "Priority";
                    break;
                case EORuleType.Conversation:
                    propertyName = "Conversation";
                    break;
                case EORuleType.PostedOn:
                    propertyName = "PostedOn";
                    break;
                case EORuleType.PostedTo:
                    propertyName = "PostedTo";
                    break;
                case EORuleType.EntryType:
                    propertyName = "EntryType";
                    break;
                case EORuleType.FullName:
                    propertyName = "FullName";
                    break;
                case EORuleType.LastName:
                    propertyName = "LastName";
                    break;
                case EORuleType.FirstName:
                    propertyName = "FirstName";
                    break;
                case EORuleType.StartDate:
                    if (itemType == EOCategoryType.Task)
                    {
                        propertyName = "IPM.Task.StartDate";
                    }
                    else if (itemType == EOCategoryType.Journal)
                    {
                        propertyName = "IPM.Activity.Start";
                    }
                    else if (itemType == EOCategoryType.Event)
                    {
                        propertyName = "IPM.Appointment.EventDate";
                    }
                    break;
                default:
                    break;
            }
            return propertyName;
        }
    }

    public class FilterResult
    {
        public static FilterResult PassedResult = new() { State = FilterState.Passed };

        public static FilterResult NoNeedExportResult => new() { State = FilterState.NoNeedExport };

        public FilterState State { set; get; }

        public string Message { set; get; }
    }

    public enum FilterState : byte
    {
        Passed = 0,
        Filtered = 1,
        NoNeedExport = 2
    }
}