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
    using AvePoint.Common.FilterEngine.Engines.Box;
    using AvePoint.Common.FilterEngine.Engines.Connector;
    using AvePoint.Common.FilterEngine.Engines.Google;
    using AvePoint.Common.FilterEngine.Engines.Teams;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.RA.CommonUtil;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    #endregion

    internal abstract class FilterEngineBase : IFilterEngine
    {
        private List<FilterPolicy> filterPolicies = new List<FilterPolicy>();
        private Dictionary<PolicyLevel, string> filterConditionExpressions;
        private FilterEngine filterEngine;
        protected FilterLogger logger;
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(FilterEngineBase));

        public FilterEngineBase(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterExpressionLists, FilterEngine engine)
        {
            filterPolicies = policyLists;
            filterConditionExpressions = filterExpressionLists;
            filterEngine = engine;
            logger = new FilterLogger(RecordFilterPolicyLog.All);
        }

        public bool IsQualified(ObjectInfoBase objectInfo)
        {
            Dictionary<int, bool> filterCheckingResults = new Dictionary<int, bool>();
            List<FilterPolicy> currentLevelPolicies = GetApplicablePolicies();
            if (currentLevelPolicies.Count == 0)
            {
                if (filterEngine.IsFilterOut) return false;
                return true;
            }
            foreach (FilterPolicy policy in currentLevelPolicies)
            {   
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                bool checkingResult = IsQualified(objectInfo, policy);
                filterCheckingResults.Add(policy.SequenceNo, checkingResult);
                stopwatch.Stop();
                mLog.Info($"LATPerformance linkli check this rule {policy.RuleType.ToString()} filter policy cost time:{stopwatch.ElapsedMilliseconds}");
            }
            string conditionExpression = GetApplicableConditionExpression();
            FilterConditionExpression filterException = new FilterConditionExpression(conditionExpression);
            return filterException.Caculate(filterCheckingResults);
        }

        protected abstract bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy);

        private List<FilterPolicy> GetApplicablePolicies()
        {
            PolicyLevel level = PolicyLevel.None;
            if (this.GetType() == typeof(WebApplicationFilterEngine))
            {
                level = PolicyLevel.WebApplication;
            }
            else if (this.GetType() == typeof(SiteCollectionFilterEngine))
            {
                level = PolicyLevel.SiteCollection;
            }
            if (this.GetType() == typeof(SiteFilterEngine))
            {
                level = PolicyLevel.Site;
            }
            if (this.GetType() == typeof(ListFilterEngine))
            {
                level = PolicyLevel.List;
            }
            if (this.GetType() == typeof(FolderFilterEngine))
            {
                level = PolicyLevel.Folder;
            }
            if (this.GetType() == typeof(DocumentFilterEngine))
            {
                level = PolicyLevel.Document;
            }
            if (this.GetType() == typeof(DocumentVersionFilterEngine))
            {
                level = PolicyLevel.DocumentVersion;
            }
            if (this.GetType() == typeof(ItemFilterEngine))
            {
                level = PolicyLevel.Item;
            }
            if (this.GetType() == typeof(ItemVersionFilterEngine))
            {
                level = PolicyLevel.ItemVersion;
            }
            if (this.GetType() == typeof(AttachmentFilterEngine))
            {
                level = PolicyLevel.Attachment;
            }
            if (this.GetType() == typeof(TreeNodeFilterEngine))
            {
                level = PolicyLevel.AdvancedSearch;
            }
            if (this.GetType() == typeof(ExchangeMailboxFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineMailbox;
            }
            if (this.GetType() == typeof(ExchangeFolderFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineFolder;
            }
            if (this.GetType() == typeof(ExchangeItemFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem;
            }
            if (this.GetType() == typeof(ExchangeContactFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem_Contact;
            }
            if (this.GetType() == typeof(ExchangeDocumentFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem_Document;
            }
            if (this.GetType() == typeof(ExchangeEventFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem_Event;
            }
            if (this.GetType() == typeof(ExchangeJournalFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem_Journal;
            }
            if (this.GetType() == typeof(ExchangeMessageFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem_Message;
            }
            if (this.GetType() == typeof(ExchangeNoteFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem_Note;
            }
            if (this.GetType() == typeof(ExchangePostFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem_Post;
            }
            if (this.GetType() == typeof(ExchangeTaskFilterEngine))
            {
                level = PolicyLevel.ExchangeOnlineItem_Task;
            }
            if (this.GetType() == typeof(PhysicalBoxFilterEngine))
            {
                level = PolicyLevel.PhysicalBox;
            }
            if (this.GetType() == typeof(PhysicalFileFilterEngine))
            {
                level = PolicyLevel.PhysicalFile;
            }
            if (this.GetType() == typeof(FSFileFilterEngine))
            {
                level = PolicyLevel.FileSysFile;
            }
            if (this.GetType() == typeof(FSFolderFilterEngine))
            {
                level = PolicyLevel.FileSysFolder;
            }
            if (this.GetType() == typeof(AzureFileFilterEngine))
            {
                level = PolicyLevel.AzureFileDocument;
            }
            if (this.GetType() == typeof(CustomizeConnectorFilterEngine))
            {
                level = PolicyLevel.Document;
            }
            if (this.GetType() == typeof(BoxFilterEngine))
            {
                level = PolicyLevel.BoxDocument;
            }
            if (this.GetType() == typeof(GoogleFilterEngine))
            {
                level = PolicyLevel.GoogleDriveDocument;
            }
            if(this.GetType() == typeof(TeamsFilterEngine))
            {
                level = PolicyLevel.Teams;
            }
            List<FilterPolicy> policies = new List<FilterPolicy>();
            foreach (FilterPolicy policy in filterPolicies)
            {
                if (policy.Level == level)
                {
                    policies.Add(policy);
                }
            }
            return policies;
        }

        private string GetApplicableConditionExpression()
        {
            if (this.GetType() == typeof(WebApplicationFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.WebApplication];
            }
            else if (this.GetType() == typeof(SiteCollectionFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.SiteCollection];
            }
            if (this.GetType() == typeof(SiteFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.Site];
            }
            if (this.GetType() == typeof(ListFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.List];
            }
            if (this.GetType() == typeof(FolderFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.Folder];
            }
            if (this.GetType() == typeof(DocumentFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.Document];
            }
            if (this.GetType() == typeof(DocumentVersionFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.DocumentVersion];
            }
            if (this.GetType() == typeof(ItemFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.Item];
            }
            if (this.GetType() == typeof(ItemVersionFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ItemVersion];
            }
            if (this.GetType() == typeof(AttachmentFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.Attachment];
            }
            if (this.GetType() == typeof(TreeNodeFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.AdvancedSearch];
            }
            if (this.GetType() == typeof(ExchangeMailboxFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineMailbox];
            }
            if (this.GetType() == typeof(ExchangeFolderFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineFolder];
            }
            if (this.GetType() == typeof(ExchangeItemFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem];
            }
            if (this.GetType() == typeof(ExchangeContactFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem_Contact];
            }
            if (this.GetType() == typeof(ExchangeDocumentFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem_Document];
            }
            if (this.GetType() == typeof(ExchangeEventFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem_Event];
            }
            if (this.GetType() == typeof(ExchangeJournalFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem_Journal];
            }
            if (this.GetType() == typeof(ExchangeMessageFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem_Message];
            }
            if (this.GetType() == typeof(ExchangeNoteFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem_Note];
            }
            if (this.GetType() == typeof(ExchangePostFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem_Post];
            }
            if (this.GetType() == typeof(ExchangeTaskFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.ExchangeOnlineItem_Task];
            }
            if (this.GetType() == typeof(PhysicalBoxFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.PhysicalBox];
            }
            if (this.GetType() == typeof(PhysicalFileFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.PhysicalFile];
            }
            if (this.GetType() == typeof(FSFileFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.FileSysFile];
            }
            if (this.GetType() == typeof(FSFolderFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.FileSysFolder];
            }
            if (this.GetType() == typeof(AzureFileFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.AzureFileDocument];
            }
            if (this.GetType() == typeof(CustomizeConnectorFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.Document];
            }
            if (this.GetType() == typeof(BoxFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.BoxDocument];
            }
            if(this.GetType() == typeof(GoogleFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.GoogleDriveDocument];
            }
            if (this.GetType() == typeof(TeamsFilterEngine))
            {
                return filterConditionExpressions[PolicyLevel.Teams];
            }
            throw new ExpressionNotFoundException(this.GetType().FullName);
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
                if (specialCollection != null && specialCollection.ContainsKey(internalName))
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
        protected bool TryGetValue<TResult>(object value, out TResult result, bool ignoreType = false)
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
                    //logger.Warn("Convert type failed. Error:{0}", e);
                    throw e;
                }
            }
            return false;
        }

        class FilterConditionExpression
        {
            private static readonly int AND = 0;
            private static readonly int OR = -1;
            private static readonly char AndSymbol = '&';
            private static readonly char OrSymbol = '|';

            private string filterConditionExpression;

            public FilterConditionExpression(string filterExpression)
            {
                filterConditionExpression = filterExpression;
            }

            /*private string AddSpecialToExpression(string filterConditionExpression)
            {
                filterConditionExpression = filterConditionExpression.Replace("Or", "|").Replace("And", "&");
                string[] valueList = filterConditionExpression.Split(new char[] { ' ' });
                string[] realValueList = new string[2 * valueList.Length + 1];
                bool notFinish = false;
                for (int index = 0; index < valueList.Length; index++)
                {
                    realValueList[2 * index + 1] = valueList[index];
                    if (valueList[index] == "&")
                    {
                        notFinish = true;
                        realValueList[2 * (index - 1)] = "(";
                        index++;
                        while (index < valueList.Length)
                        {
                            realValueList[2 * index + 1] = valueList[index];
                            if (valueList[index] == "|")
                            {
                                realValueList[2 * (index - 1) + 2] = ")";
                                notFinish = false;
                                break;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                if (notFinish)
                {
                    realValueList[realValueList.Length - 1] = ")";
                }

                StringBuilder sb = new StringBuilder();
                foreach (string value in realValueList)
                {
                    if (!string.IsNullOrEmpty(value))
                        sb.Append(value + " ");
                }

                return sb.ToString().Replace("|", "Or").Replace("&", "And");
            }*/
            public bool Caculate(Dictionary<int, bool> parameters)
            {
                //SAAS-14194 make expression executed in sequence.
                Stack<int> opStack = ConvertToOperationStack(filterConditionExpression);
                Stack<bool> tempStack = new Stack<bool>();
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

            string ConvertToSuffixExpression(string infixExpression)
            {
                string suffixString = string.Empty;
                string fixedString = HandleConditionString(infixExpression);
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
                return suffixString;
            }

            #region old logic for convert to operation stack
            //Stack<int> ConvertToOperationStack(string infixExpression)
            //{
            //    Stack<int> stack = new Stack<int>();
            //    if (!string.IsNullOrEmpty(infixExpression))
            //    {
            //        string suffixExpression = ConvertToSuffixExpression(infixExpression);
            //        int temp = 0;
            //        int n = 1;
            //        for (int i = suffixExpression.Length - 1; i >= 0; i--)
            //        {
            //            if (suffixExpression[i] == AndSymbol)
            //            {
            //                if (temp != 0)
            //                {
            //                    stack.Push(temp);
            //                    temp = 0;
            //                    n = 1;
            //                }
            //                stack.Push(AND);
            //            }
            //            else if (suffixExpression[i] == OrSymbol)
            //            {
            //                if (temp != 0)
            //                {
            //                    stack.Push(temp);
            //                    temp = 0;
            //                    n = 1;
            //                }
            //                stack.Push(OR);
            //            }
            //            else if (suffixExpression[i] >= '0' && suffixExpression[i] <= '9')
            //            {
            //                temp = (suffixExpression[i] - 48) + temp * n;
            //                n = n * 10;
            //            }
            //            else if (suffixExpression[i] == '#')
            //            {
            //                if (temp != 0)
            //                {
            //                    stack.Push(temp);
            //                    temp = 0;
            //                    n = 1;
            //                }
            //            }
            //        }
            //        stack.Push(temp);
            //    }
            //    return stack;
            //}
            #endregion
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

        protected void RecordFilterLog(string objType,bool isQualified, List<string> values, FilterPolicy policy)
        {
            var builder = new StringBuilder("{");
            foreach (var v in values)
            {
                builder.Append(v).Append(", ");
            }
            builder.Append("}");
            RecordFilterLog(objType,isQualified, builder.ToString(), policy);
        }

        protected void RecordFilterLog(string objType, bool isQualified, string value, FilterPolicy policy)
        {
            //Note sensitive information in the current log file.
            //this.logger.RecordFilterLog(objType,isQualified, value, policy);
        }

        public virtual void Dispose()
        {
            if (logger != null)
            {
                logger.Flush();
            }
        }
    }
}
