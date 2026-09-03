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



namespace AvePoint.Common.FilterEngine
{
    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.CommonFilter;
    using System.Diagnostics.CodeAnalysis;
    using AvePoint.GCommon;
    using System.Reflection;
    using System.Linq;
    #endregion
    /// <summary>
    /// 该类及其子类不支持多线程, 如需对线程使用, 请每个线程构造一个实例.
    /// </summary>
    public abstract class FilterEngineBase : IFilterEngine
    {
        private FilterOption option;
        private FilterLogger logger;
        private List<FilterPolicy> applicablePolicies;
        private static AveLogger log = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected abstract PolicyLevel Level { get; }

        protected List<FilterPolicy> ApplicablePolicies
        {
            get
            {
                if (this.applicablePolicies == null)
                {
                    this.applicablePolicies = this.option.FilterPolicies.Where(p => p.Level == this.Level).ToList();
                }
                return this.applicablePolicies;
            }
        }

        public FilterEngineBase(FilterOption option)
        {
            CheckArgumentNull(option, "option");
            CheckArgumentNull(option.FilterPolicies, "option.FilterPolicies");
            CheckArgumentNull(option.FilterConditionExpressions, "option.FilterConditionExpressions");
            this.option = option;
            this.logger = new FilterLogger(option.LogLevel);
        }

        private void CheckArgumentNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        public bool IsQualified(ObjectInfoBase objectInfo)
        {
            if (this.ApplicablePolicies.Count == 0)
            {
                return !this.option.IsNoRuleFilterOut;
            }
            else
            {
                return new FilterConditionExpression(GetApplicableConditionExpression()).//构造
                Caculate(GetSequenceNoAndResultMapping(objectInfo))//计算表达式的值
                ^ this.option.IsRealFilterOut;
            }
        }

        protected abstract bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy);

        private Dictionary<int, bool> GetSequenceNoAndResultMapping(ObjectInfoBase objectInfo)
        {
            //SequenceNo不可能重复
            return this.ApplicablePolicies.ToDictionary(p => p.SequenceNo, p => IsQualified(objectInfo, p));
        }

        private string GetApplicableConditionExpression()
        {
            return this.option.FilterConditionExpressions[this.Level];
        }

        public virtual object GetColumnValue(FilterPolicy policy, Hashtable ColumnInfosOfDisplayName, Hashtable ColumnInfosOfInternalName, Hashtable intrNameToDispName, Hashtable specialCollection, string type = null)
        {
            string columnName = policy.Rule.Value1.ToLowerInvariant();
            //column name的格式为[xxx]时，表示的是internal name，则把中括号去掉特殊处理。
            if (columnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && columnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
            {
                if (ColumnInfosOfInternalName == null)
                {
                    return null;
                }
                var internalName = columnName.Trim(new char[] { '[', ']' }).ToLowerInvariant();
                if (specialCollection.ContainsKey(internalName))
                {
                    //如果special collection包含了internal name，则返回special collection对应的value。
                    return specialCollection[internalName];
                }
                //columnName = intrNameToDispName[internalName].ToString().ToLowerInvariant();
                if (!ColumnInfosOfInternalName.Contains(internalName) || null == ColumnInfosOfInternalName[internalName])
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(type) && !ColumnInfosOfInternalName[internalName].GetType().Name.ToString().Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                return ColumnInfosOfInternalName[internalName];
            }
            if (!ColumnInfosOfDisplayName.Contains(columnName) || null == ColumnInfosOfDisplayName[columnName])
            {
                return null;
            }
            //如果Column的类型是Boolean或者DateTime时，则需要传表示类型的string，进一步判断对应的value的类型是否匹配。
            //如果Column的类型是Text或者Number时，则不需要传值，默认为null即可。
            if (!string.IsNullOrEmpty(type) && !ColumnInfosOfDisplayName[columnName].GetType().Name.ToString().Equals(type, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return ColumnInfosOfDisplayName[columnName];
        }

        protected bool TryGetValue<TResult>(object value, out TResult result,bool ignoreType = false)
        {
            result = default(TResult);
            if (value != null && ((value is TResult) || ignoreType))
            {
                try
                {
                    result = (TResult)value;
                    return true;
                }
                catch (InvalidCastException e)
                {
                    log.Warn("Convert type failed. Error:{0}", e);
                }
            }
            return false;
        }

        /// <summary>
        /// 对Custom Property 进行处理。各个level 处理逻辑都一样。
        /// 1，判断是否存在此property
        /// 2，对property value 类型检查
        /// 3，进行比较。
        /// </summary>
        /// <param name="policy">GUI 传入后天过滤条件</param>
        /// <param name="properties">SP 中取出数据集合</param>
        /// <returns></returns>
        protected bool QualifyCustomProperty(FilterPolicy policy, Hashtable properties)
        {
            //如果不含有某属性，直接返回False。
            if (!properties.ContainsKey(policy.Rule.Value1))
            {
                return false;
            }

            var isQualified = default(bool);
            var logValue = string.Empty;
            var propertyValue = properties[policy.Rule.Value1];
            if (policy.Rule is CustomPropertyTextRule || policy.Rule is ParentSiteCollectionCustomPropertyColumnTextRule || policy.Rule is ParentSiteCustomPropertyColumnTextRule)
            {
                string value = string.Empty;//可能有Match *的情况，所以默认赋Empty
                if (propertyValue == null || TryGetValue(propertyValue, out value))
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, value, policy.Value);
                }
                else//类型不匹配。
                {
                    isQualified = false;
                }
                logValue = value.ToString();
            }
            else if (policy.Rule is CustomPropertyNumberRule)
            {
                int value;
                if (!TryGetValue(propertyValue, out value))
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = NumberConditionChecker.IsQualified(policy.Condition, value, policy.Value);
                }
                logValue = value.ToString();
            }
            else if (policy.Rule is CustomPropertyDateTimeRule)
            {
                DateTime value;
                if (!TryGetValue(propertyValue, out value))
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, value, policy.Value);
                }
                logValue = value.ToString();
            }
            else if (policy.Rule is CustomPropertyBooleanRule)
            {
                bool value = default(bool);
                if (propertyValue == null || (!TryGetValue(propertyValue, out value) && !bool.TryParse(propertyValue.ToString(), out value)))
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = BooleanConditionChecker.IsQualified(policy.Condition, value, policy.Value);
                }
                logValue = value.ToString();
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
            RecordFilterLog(isQualified, logValue, policy);
            return isQualified;
        }

        /// <summary>
        /// 计算给定表达式的值
        /// 你是否非常熟悉某种计算机语言编译原理中, 表达式的编译和解析相关内容? 如果不是, 建议不要修改此类。
        /// </summary>
        class FilterConditionExpression
        {
            private static readonly int AND = 0;
            private static readonly int OR = -1;
            private static readonly char AndSymbol = '&';
            private static readonly char OrSymbol = '|';

            private string filterConditionExpression;

            /// <summary>
            /// 构造表达式解析器
            /// </summary>
            /// <param name="filterExpression">
            /// 表达式, 支持()and or 数字序号, 例如(1 and 2) or (3 and 4). 
            /// and和or具有相同优先级, 必须使用括号显示提升某个字表达式的运算优先级
            /// 1 and 2 or 3 and 4 = ((1 and 2) or 3) and 4
            /// </param>
            public FilterConditionExpression(string filterExpression)
            {
                this.filterConditionExpression = filterExpression;
            }

            /// <summary>
            /// 计算表达式的值
            /// </summary>
            /// <param name="parameters">(子表达式序号, 字表达式值)的集合</param>
            /// <returns>表达式的值</returns>
            public bool Caculate(Dictionary<int, bool> parameters)
            {
                Stack<int> opStack = ConvertToOperationStack(this.filterConditionExpression);
                Stack<bool> tempStack = new Stack<bool>();
                //计算表达式的值, 不解释
                while (opStack.Count > 0)
                {
                    if (opStack.Peek() != AND && opStack.Peek() != OR)
                    {
                        int number = opStack.Pop();
                        tempStack.Push(parameters[number]);
                    }
                    else if (opStack.Peek() == AND)
                    {
                        bool op1 = tempStack.Pop();
                        bool op2 = tempStack.Pop();
                        tempStack.Push(op1 && op2);
                        opStack.Pop();
                    }
                    else if (opStack.Peek() == OR)
                    {
                        bool op1 = tempStack.Pop();
                        bool op2 = tempStack.Pop();
                        tempStack.Push(op1 || op2);
                        opStack.Pop();
                    }
                }
                return tempStack.Pop();
            }

            [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
            string HandleConditionString(string infixString)
            {
                char[] tempArray = infixString.ToLower().Replace("and", "&").Replace("or", "|").ToCharArray();
                Array.Reverse(tempArray);
                for (int i = 0; i < tempArray.Length; i++)
                {
                    if (tempArray[i] == '(')
                    {
                        tempArray[i] = ')';
                    }
                    else if (tempArray[i] == ')')
                    {
                        tempArray[i] = '(';
                    }
                }
                return new string(tempArray);
            }

            /// <summary>
            /// 将中缀表达式转换为后缀表达式(逆波兰表达式)
            /// </summary>
            /// <param name="infixExpression">中缀表达式 (1 and 2) or (3 and 4)</param>
            /// <returns>后缀表达式 4#3&#2#1&|, 其中#用于分隔数字序号</returns>
            string ConvertToSuffixExpression(string infixExpression)
            {
                //infixExpression= (1 and 2) or (3 and 4)
                string suffixString = string.Empty;
                string fixedString = HandleConditionString(infixExpression);
                //fixedString= (4 & 3) | (2 & 1)
                Stack<char> tempStack = new Stack<char>();
                tempStack.Push((char)127);
                foreach (char c in fixedString)
                {
                    if (c == AndSymbol || c == OrSymbol)
                    {
                        suffixString += "#";
                        tempStack.Push(c);
                    }
                    else if (c == '(')
                    {
                        tempStack.Push(c);
                    }
                    else if (c == ')')
                    {
                        while (tempStack.Peek() != '(')
                        {
                            suffixString += tempStack.Pop();
                        }
                        tempStack.Pop();
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        suffixString += c;
                    }
                }
                while (tempStack.Peek() != (char)127)
                {
                    suffixString += tempStack.Pop();
                }
                //suffixString= 4#3&#2#1&|
                return suffixString;
            }
            /// <summary>
            /// 将中缀表达式转换为后缀表达式(逆波兰表达式), 并逆序压入栈中
            /// </summary>
            /// <param name="infixExpression">中缀表达式 (1 and 2) or (3 and 4)</param>
            /// <returns>
            /// 后缀表达式栈
            ///栈顶
            ///[4]
            ///[3]
            ///[&]
            ///[2]
            ///[1]
            ///[&]
            ///[|]
            ///栈底   
            /// </returns>
            Stack<int> ConvertToOperationStack(string infixExpression)
            {
                //infixExpression= (1 and 2) or (3 and 4)
                Stack<int> stack = new Stack<int>();
                if (!string.IsNullOrEmpty(infixExpression))
                {
                    string suffixExpression = ConvertToSuffixExpression(infixExpression);
                    //suffixExpression= 4#3&#2#1&|
                    int temp = 0;
                    int n = 10;
                    for (int i = suffixExpression.Length - 1; i >= 0; i--)
                    {
                        if (suffixExpression[i] >= '0' && suffixExpression[i] <= '9')
                        {
                            temp = (suffixExpression[i] - 48) + temp * n;
                        }
                        else
                        {
                            if (temp != 0)
                            {
                                stack.Push(temp);
                                temp = 0;
                            }
                            if (suffixExpression[i] == AndSymbol)
                            {
                                stack.Push(AND);
                            }
                            if (suffixExpression[i] == OrSymbol)
                            {
                                stack.Push(OR);
                            }
                        }
                    }
                    stack.Push(temp);
                }
                ///栈顶
                ///[4]
                ///[3]
                ///[&]
                ///[2]
                ///[1]
                ///[&]
                ///[|]
                ///栈底                
                return stack;
            }
        }

        #region Logging
        class FilterLogger
        {
            private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
            private StringBuilder logBuffer;
            public RecordFilterPolicyLog level { get; private set; }
            public FilterLogger(RecordFilterPolicyLog level)
            {
                this.level = level;
                this.logBuffer = new StringBuilder();
            }

            public void RecordFilterLog(bool isQualified, string value, FilterPolicy policy)
            {
                if (this.level == RecordFilterPolicyLog.All ||
                    (this.level == RecordFilterPolicyLog.Portion && isQualified == false))
                {
                    this.logBuffer.AppendLine(string.Format("[{0}][{1}][The Filter Value: {2}|The Actual Value: {3}]",
                        isQualified ? "Filtered In" : "Filtered Out",//[0]
                        policy.Rule.ToStringPro(),//[1]
                        policy.Value.Value1,//[2]
                        value));//[3]
                }
            }

            public void Flush()
            {
                if (this.logBuffer.Length > 0)
                {
                    logger.Debug(this.logBuffer.ToString());
                    //clear buffer
                    this.logBuffer = new StringBuilder();
                }
            }
        }

        protected void RecordFilterLog(bool isQualified, List<string> values, FilterPolicy policy)
        {
            var builder = new StringBuilder("{");
            foreach (var v in values)
            {
                builder.Append(v).Append(", ");
            }
            builder.Append("}");
            RecordFilterLog(isQualified, builder.ToString(), policy);
        }

        protected void RecordFilterLog(bool isQualified, string value, FilterPolicy policy)
        {
            this.logger.RecordFilterLog(isQualified, value, policy);
        }

        #endregion
        public void Dispose()
        {
            this.logger.Flush();
        }

    }
}
