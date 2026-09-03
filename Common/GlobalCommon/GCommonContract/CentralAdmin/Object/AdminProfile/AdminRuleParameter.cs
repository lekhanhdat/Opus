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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [KnownType(typeof(AdminBooleanRuleParameter))]
    [KnownType(typeof(AdminSelectRuleParameter))]
    [KnownType(typeof(AdminIntRuleParameter))]
    [KnownType(typeof(AdminStringRuleParameter))]
    [KnownType(typeof(AdminGridParameter))]
    [KnownType(typeof(AdminAccountRuleParameter))]
    [KnownType(typeof(AdminAccessListComboRuleParameter))]
    [KnownType(typeof(AdminGroupParameter))]
    [KnownType(typeof(NewAdminGroupParameter))] // Add new
    [KnownType(typeof(AdminRadioParameter))]
    [KnownType(typeof(AdminDateTimeRuleParameter))]
    [KnownType(typeof(AdminAddedParameter))]
    [KnownType(typeof(AdminLabelRuleParameter))]
    [KnownType(typeof(AdminFilterRuleParameter))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public abstract class AdminRuleParameter
    {
        /// <summary>
        /// 一个rule中所有parameter的唯一标示
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public AdminParameterType ParameterType { get; set; }

        [DataMember]
        public bool IsRequired { get; set; }

        [DataMember]
        public string DisplayName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminFilterRuleParameter : AdminRuleParameter
    {
        [DataMember]
        public FilterPolicyInfo FilterPolicy { get; set; }
    }   
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminAccountRuleParameter : AdminRuleParameter
    {
        [DataMember]
        public List<UserDetail> Users { get; set; }

        [DataMember]
        public AccountSearchFlag Flag { get; set; }

        [DataMember]
        public bool AllowMultiple { get; set; }

        public override string ToString()
        {
            if (Users != null)
            {
                StringBuilder builder = new StringBuilder();
                foreach (UserDetail user in Users)
                {
                    builder.Append(user.DisplayName).Append(",");
                }
                string result = builder.ToString().TrimEnd(new char[] { ',' });
                if (string.IsNullOrEmpty(result))
                {
                    return string.Empty;
                }
                else
                {
                    return string.Format("{0}: {1}", Name, result);
                }
            }
            return string.Empty;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminSelectRuleParameter : AdminRuleParameter
    {
        [DataMember]
        public List<string> Source { get; set; }

        [DataMember]
        public int SelectedIndex { get; set; }

        [DataMember]
        public List<int> SelectedIndexs { get; set; }

        [DataMember]
        public bool IsMultiple { get; set; }

        public override string ToString()
        {
            if (Source != null)
            {
                StringBuilder result = new StringBuilder();
                int count = Source.Count;
                foreach (int index in SelectedIndexs)
                {
                    if (index >= 0 && index < count)
                    {
                        result.AppendFormat("{0},", Source[index]);
                    }
                }
                if (result.Length == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return string.Format("{0}: {1}", Name, result);
                }
            }
            return string.Empty;
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminAccessListComboRuleParameter : AdminRuleParameter
    {
        /// <summary>
        /// DefindGroup可以修改name 直接使用name显示有可能是旧数据
        /// </summary>
        [DataMember]
        public List<string> SelectedNames { get; set; }

        [DataMember]
        public List<DefinedGroupInfo> DefinedGroups { get; set; }

        //[DataMember]
        //public List<UserDetail> AccessUsers { get; set; }

        [DataMember]
        public List<string> SelectedIds { get; set; }

        //[DataMember]
        //public List<UserPropertyFilter> AccessUserPropertyFilters { get; set; }

        //public override string ToString()
        //{
        //    if (DefinedGroups != null)
        //    {
        //        StringBuilder builder = new StringBuilder();
        //        foreach (UserDetail user in DefinedGroups.AccessUsers)
        //        {
        //            builder.Append(user.DisplayName).Append(",");
        //        }
        //        string result = builder.ToString().TrimEnd(new char[] { ',' });
        //        if (string.IsNullOrEmpty(result))
        //        {
        //            return string.Empty;
        //        }
        //        else
        //        {
        //            return string.Format("{0}: {1}", Name, result);
        //        }
        //    }
        //    return string.Empty;
        //}
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DefinedGroupInfo
    {
        [DataMember]
        public List<UserDetail> AccessUsers { get; set; }

        [DataMember]
        public List<UserPropertyFilter> AccessUserPropertyFilters { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserPropertyFilter
    {
        [DataMember]
        public UserPropertyName UserPropertyName { get; set; }

        [DataMember]
        public UserPropertyCondition UserPropertyCondition { get; set; }

        [DataMember]
        public UserOrGroupSelecter UserOrGroupSelecter { get; set; }

        [DataMember]
        public string UserPropertyValue { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UserPropertyName
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Department,
        [EnumMember]
        JobTitle,
        [EnumMember]
        Office,
        [EnumMember]
        Email,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UserPropertyCondition
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Contains,
        [EnumMember]
        Equal,
        [EnumMember]
        Match,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UserOrGroupSelecter
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        User,
        [EnumMember]
        Group,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]

    public class AdminLabelRuleParameter : AdminRuleParameter
    {

    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminBooleanRuleParameter : AdminRuleParameter
    {
        [DataMember]
        public bool BooleanValue { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, BooleanValue);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminIntRuleParameter : AdminRuleParameter
    {
        [DataMember]
        public int? IntValue { get; set; }

        [DataMember]
        public int? ValidateIntValue { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, IntValue);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminStringRuleParameter : AdminRuleParameter
    {
        [DataMember]
        public string StringValue { get; set; }

        [DataMember]
        public bool Multiline { get; set; }

        [DataMember]
        public bool NeedHscrollbar { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, StringValue);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminRadioParameter : AdminRuleParameter
    {
        [DataMember]
        public List<AdminRuleParameter> Parameters { get; set; }
        [DataMember]
        public bool IsChecked { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, IsChecked);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminDateTimeRuleParameter : AdminRuleParameter
    {
        [DataMember]
        public bool DateOnly { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DataTime")]
        public override string ToString()
        {
            if (DateOnly)
            {
                return string.Format("{0}: {1}({2})", Name, DateTime.ToString("yyyy-MM-dd"), DateTime.Kind);
            }
            else
            {
                return string.Format("{0}: {1}({2})", Name, DateTime.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Kind);
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminPopUpPageParameter : AdminRuleParameter
    {
        [DataMember]
        public Dictionary<string, string> Source { get; set; }

        [DataMember]
        public List<string> SelectedValue { get; set; }

        [DataMember]
        public bool IsMultiple { get; set; }

        public override string ToString()
        {
            if (Source != null)
            {
                StringBuilder result = new StringBuilder();
                int count = Source.Count;
                if (count <= 0)
                {
                    return string.Empty;
                }
                else
                {
                    foreach (string key in Source.Keys)
                    {
                        result.Append("<" + key + "," + Source[key] + ">,");
                    }
                }
                return string.Format("{0}: {1}", Name, result);

            }
            return string.Empty;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminGridParameter : AdminRuleParameter
    {
        [DataMember]
        public List<AdminRuleParameter> Parameters { get; set; }

        [DataMember]
        public bool OrientationHorizontal { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminAddedParameter : AdminRuleParameter
    {
        [DataMember]
        public AdminGridParameter GridTemplate { get; set; }

        [DataMember]
        public List<AdminGridParameter> Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminGroupParameter : AdminRuleParameter
    {
        [DataMember]
        public Dictionary<AdminRuleParameter, List<AdminRuleParameter>> Parameters { get; set; }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(string.Format("{0}:", Name));
            foreach (AdminRuleParameter keyParameter in Parameters.Keys)
            {
                result.AppendLine(keyParameter.ToString());
                switch (keyParameter.ParameterType)
                {
                    case AdminParameterType.Radio:
                        AdminRadioParameter radio = keyParameter as AdminRadioParameter;
                        if (!radio.IsChecked)
                        {
                            continue;
                        }
                        break;
                    case AdminParameterType.Bool:
                        AdminBooleanRuleParameter boolParameter = keyParameter as AdminBooleanRuleParameter;
                        if (!boolParameter.BooleanValue)
                        {
                            continue;
                        }
                        break;
                    default:
                        break;
                }
                foreach (AdminRuleParameter valueParameter in Parameters[keyParameter])
                {
                    result.AppendLine(valueParameter.ToString());
                }
            }
            return result.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NewAdminGroupParameter : AdminRuleParameter
    {
        [DataMember]
        public List<NewGroupParameter> Parameters { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}:\n", Name);
            Parameters.ForEach(i => sb.AppendLine(i.ToString()));
            return sb.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NewGroupParameter
    {
        [DataMember]
        public AdminRuleParameter Key { get; set; }

        [DataMember]
        public List<AdminRuleParameter> Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AdminParameterType
    {
        [EnumMember]
        Int = 0,
        [EnumMember]
        String,
        [EnumMember]
        Bool,
        [EnumMember]
        Account,
        [EnumMember]
        Dropdown,
        [EnumMember]
        Radio,
        [EnumMember]
        Group,
        [EnumMember]
        DateTime,
        [EnumMember]
        Grid,
        [EnumMember]
        Added,
        [EnumMember]
        PopUpPage,
        [EnumMember]
        AccessListCombo,
        [EnumMember]
        Label,
        [EnumMember]
        Filter,
    }
}
