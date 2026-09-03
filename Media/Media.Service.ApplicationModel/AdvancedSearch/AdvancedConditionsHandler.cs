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




namespace AvePoint.Media.Service
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.Media.Service.DomainModel;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Common;

    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/5/16",
    "yhzhang@avepoint.com",
    "dwxue@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_LOG_2 },
    null,
    true)]
    #endregion
    /// <summary>
    /// 实现了根据AdvancedConditions合并branches的功能
    /// </summary>
    public class AdvancedConditionsHandler : IAdvancedConditionsHandler
    {
        private static readonly int And = 0;

        private static readonly int Or = -1;

        private static readonly char AndSymbol = '&';

        private static readonly char OrSymbol = '|';

        private bool isGoogle { get; set; } = false;

        public AdvancedConditionsHandler()
        {

        }
        public AdvancedConditionsHandler(bool isGoogle)
        {
            this.isGoogle = isGoogle;
        }
        /// <summary>
        /// 唯一公开的方法，实现了根据AdvancedConditions合并branches的功能
        /// </summary>
        /// <param name="paramDictionary">key是用户设置的filter的序号，value是有待合并的branches</param>
        /// <param name="advancedConditions">用户输入的AdvancedConditions字符串，例如“1and（2or3）”</param>
        /// <returns>合并之后，要显示在界面上的trees</returns>
        public List<TreeNode> AssembleTreeByAdvancedConditions(List<TreeNode> paramDictionary, string advancedConditions, Boolean isObjectSearch = false)
        {
            //logger.Debug(MediaServiceApplicationModelResource.AdvancedConditionsHandlerPrintLogBegin);
            var resultTrees = default(List<TreeNode>);
            string fixedAdvancedConditions = HandleConditionString(advancedConditions);
            Stack<int> advancedStack = GetStackFromAdvancedFilters(fixedAdvancedConditions);
            Stack<List<TreeNode>> tempStack = new Stack<List<TreeNode>>();
            while (advancedStack.Count > 0)
            {
                if (advancedStack.Peek() != And && advancedStack.Peek() != Or)
                {
                    int number = advancedStack.Pop();
                    tempStack.Push(paramDictionary);
                }
                else if (advancedStack.Peek() == And)
                {
                    tempStack.Push(CombineTwoTreeByAnd(tempStack.Pop(), tempStack.Pop(), isObjectSearch));
                    advancedStack.Pop();
                }
                else if (advancedStack.Peek() == Or)
                {
                    tempStack.Push(CombineTwoTreeByOr(tempStack.Pop(), tempStack.Pop(), isObjectSearch));
                    advancedStack.Pop();
                }
            }
            resultTrees = CombineBranchesToTree(tempStack.Pop());
            return resultTrees;
        }

        /// <summary>
        /// 处理 or 关系下，两组branches的合并   
        /// isSPObjectSearch是用来区分Granular Advance Search和Object Search的   Exchange的isSPObjectSearch为false
        /// </summary>
        /// <param name="firstTree">第一组branches</param>
        /// <param name="secondTree">第二组branches</param>
        /// <returns>合并之后的branches</returns>
        private List<TreeNode> CombineTwoTreeByOr(List<TreeNode> firstTree, List<TreeNode> secondTree, Boolean isSPObjectSearch)
        {
            List<TreeNode> resultTree = new List<TreeNode>();
            if (isSPObjectSearch)
            {
                if (firstTree.Count != 0 && secondTree.Count != 0)
                {
                    resultTree.AddRange(secondTree);
                    foreach (var first in firstTree)
                    {
                        var isHaveFullPath = false;
                        var firstType = first;
                        while (firstType.Children[0].Type != TreeNodeType.Document && firstType.Children.Count != 0)
                        {
                            firstType = firstType.Children[0];
                        }
                        foreach (var second in secondTree)
                        {
                            var secondType = second;
                            while (secondType.Children[0].Type != TreeNodeType.Document && secondType.Children.Count != 0)
                            {
                                secondType = secondType.Children[0];
                            }
                            if (secondType.Children[0].FullPath == firstType.Children[0].FullPath && secondType.Children[0].Name == firstType.Children[0].Name && secondType.Children[0].Children[0].Name == firstType.Children[0].Children[0].Name)
                            {
                                isHaveFullPath = true;
                                break;
                            }
                        }
                        if (!isHaveFullPath)
                            resultTree.Add(first);
                    }
                }
                else if (secondTree.Count == 0 || firstTree.Count == 0)
                    resultTree = firstTree.Count != 0 ? firstTree : secondTree;
            }
            else if (!isSPObjectSearch)
            {
                resultTree = firstTree;
                resultTree.AddRange(secondTree);
            }
            return resultTree;
        }

        /// <summary>
        /// 通过叶节点的全路径，处理 and 关系下，两组branches的合并
        /// </summary>
        /// <param name="firstTree">第一组branches</param>
        /// <param name="secondTree">第二组branches</param>
        /// <returns>合并之后的branches</returns>
        private List<TreeNode> CombineTwoTreeByAnd(List<TreeNode> firstTree, List<TreeNode> secondTree, Boolean isObjectSearch)
        {
            List<TreeNode> resultTree = new List<TreeNode>();
            if (isObjectSearch && firstTree.Count != 0 && secondTree.Count != 0)
            {
                foreach (var first in firstTree)
                {
                    var firstType = first;
                    while (firstType.Children[0].Type != TreeNodeType.Document && firstType.Children.Count != 0)
                    {
                        firstType = firstType.Children[0];
                    }
                    foreach (var second in secondTree)
                    {
                        var secondType = second;
                        while (secondType.Children[0].Type != TreeNodeType.Document && secondType.Children.Count != 0)
                        {
                            secondType = secondType.Children[0];
                        }
                        if (secondType.Children[0].FullPath == firstType.Children[0].FullPath && secondType.Children[0].Name == firstType.Children[0].Name && secondType.Children[0].Children[0].Name == firstType.Children[0].Children[0].Name)
                        {
                            resultTree.Add(second);
                            break;
                        }
                    }
                }
            }
            else if (!isObjectSearch)
            {
                if (firstTree.Count != 0 && secondTree.Count != 0)
                {
                    Dictionary<string, TreeNode> firstBranches = GetFullPathDictionary(firstTree);
                    Dictionary<string, TreeNode> secondBranches = GetFullPathDictionary(secondTree);
                    foreach (var firstFullPath in firstBranches.Keys)
                    {
                        foreach (var secondFullPath in secondBranches.Keys)
                        {
                            if (firstFullPath.Length >= secondFullPath.Length && IsContains(firstFullPath, secondFullPath))
                            {
                                resultTree.Add(firstBranches[firstFullPath]);
                            }
                            else if (IsContains(secondFullPath, firstFullPath))
                            {
                                resultTree.Add(secondBranches[secondFullPath]);
                            }
                        }
                    }
                }
            }
            return resultTree;
        }

        /// <summary>
        /// 判断两条branch的包含关系
        /// </summary>
        /// <param name="firstFullPath">路径长的branch</param>
        /// <param name="secondFullPath">路径短的branch</param>
        /// <returns>true firstFullPath包含secondFullPath，false 不包含</returns>
        private bool IsContains(string firstFullPath, string secondFullPath)
        {
            bool result;
            if (firstFullPath.Length >= secondFullPath.Length)
            {
                int i = 0;
                string[] subFirst = firstFullPath.Split(new char[] { '/', '\\', ServiceConstants.Delimiter });
                string[] subSecond = secondFullPath.Split(new char[] { '/', '\\', ServiceConstants.Delimiter });
                while (i < subSecond.Length && subFirst[i].EqualsIgnoreCase(subSecond[i]))
                { i++; }
                if (i == subSecond.Length) { result = true; }
                else result = false;
            }
            else result = false;
            return result;
        }

        /// <summary>
        /// 获取一组branches的叶节点的全路径
        /// </summary>
        /// <param name="branches">参数branches</param>
        /// <returns>key是branch的叶节点的全路径，value是对应的branch</returns>
        private Dictionary<string, TreeNode> GetFullPathDictionary(List<TreeNode> branches)
        {
            var resultBranches = new Dictionary<string, TreeNode>();
            foreach (var item in branches)
            {
                TreeNode tempNode = item;
                string depthPath = item.FullPath;
                while (tempNode.Children != null && tempNode.Children.Count != 0)
                {
                    depthPath = tempNode.Children[0].FullPath;
                    tempNode = tempNode.Children[0];
                }
                resultBranches[depthPath] = item;
            }
            return resultBranches;
        }

        /// <summary>
        /// 去除branches中重复的结点
        /// </summary>
        /// <param name="branches">经过and 或者 or关系处理过的全部的branches</param>
        /// <returns>删除完重复结点，可以显示在界面上的tree</returns>
        private List<TreeNode> CombineBranchesToTree(List<TreeNode> branches)
        {
            List<TreeNode> resultList = new List<TreeNode>();
            Dictionary<string, List<TreeNode>> groupBranches = DivideBranchesIntoGroups(branches);
            foreach (var item in groupBranches)
            {
                TreeNode standardBranch = item.Value[0];
                for (int index = 1; index < item.Value.Count; index++)
                {
                    Dictionary<int, TreeNode> compareBranch = ConvertBranchToTreeNodes(item.Value[index]);
                    if(isGoogle)
                    {
                        standardBranch = CombineTwoBranchesForGoogleDrive(standardBranch, compareBranch, 0);
                    }
                    else
                    {
                        standardBranch = CombineTwoBranches(standardBranch, compareBranch, 0);
                    }
                }
                resultList.Add(standardBranch);
            }
            return resultList;
        }

        /// <summary>
        /// 将所有的branches按照根结点分组
        /// </summary>
        /// <param name="branches">经过and和or处理之后所有的branches</param>
        /// <returns>key是根节点的name，value是这个根节点下的branches</returns>
        private Dictionary<string, List<TreeNode>> DivideBranchesIntoGroups(List<TreeNode> branches)
        {
            Dictionary<string, List<TreeNode>> groups = new Dictionary<string, List<TreeNode>>();
            //branches.Reverse();
            foreach (var branch in branches)
            {
                if (groups.ContainsKey(branch.Name))
                {
                    groups[branch.Name].Add(branch);
                }
                else
                {
                    groups.Add(branch.Name, new List<TreeNode>());
                    groups[branch.Name].Add(branch);
                }
            }
            return groups;
        }

        /// <summary>
        /// 核心方法，将同一根节点下的branch合并，递归算法，依次将每一个branch对应的
        /// Dictionary<int, TreeNode>对象，合并到基准tree上
        /// </summary>
        /// <param name="standardBranch">基准tree</param>
        /// <param name="compareBranch">待合并的branch</param>
        /// <param name="depth">当前处理的深度</param>
        /// <returns>合并之后的tree</returns>
        private TreeNode CombineTwoBranches(TreeNode standardBranch, Dictionary<int, TreeNode> compareBranch, int depth)
        {
            TreeNode resultTree = standardBranch;
            bool hasChanged = default(bool);
            if (resultTree.Children.Count != 0 && compareBranch.ContainsKey(depth + 1))
            {
                bool hasNode = default(bool);
                List<TreeNode> tempChildren = resultTree.Children;
                foreach (var node in tempChildren)
                {
                    if (node.FullPath == compareBranch[depth + 1].FullPath)
                    {
                        hasNode = true;
                        TreeNode tempNode = CombineTwoBranches(node, compareBranch, depth + 1);
                        if (tempNode != null)
                            node.Children.Add(tempNode);
                    }
                }
                if (!hasNode)
                {
                    hasChanged = true;
                    if (depth == 0)
                        resultTree.Children.Add(compareBranch[depth + 1]);
                    else
                        resultTree = compareBranch[depth + 1];
                }
            }
            else if (resultTree.Children.Count == 0 && compareBranch.ContainsKey(depth + 1))
            {
                hasChanged = true;
                if (depth == 0)
                    resultTree.Children.Add(compareBranch[depth + 1]);
                else
                    resultTree = compareBranch[depth + 1];
            }
            return (hasChanged || depth == 0) ? resultTree : null;
        }
        private TreeNode CombineTwoBranchesForGoogleDrive(TreeNode standardBranch, Dictionary<int, TreeNode> compareBranch, int depth)
        {
            TreeNode resultTree = standardBranch;
            bool hasChanged = default(bool);
            if (resultTree.Children.Count != 0 && compareBranch.ContainsKey(depth + 1))
            {
                bool hasNode = default(bool);
                List<TreeNode> tempChildren = resultTree.Children;
                foreach (var node in tempChildren)
                {
                    if (node.ID == compareBranch[depth + 1].ID)
                    {
                        hasNode = true;
                        TreeNode tempNode = CombineTwoBranchesForGoogleDrive(node, compareBranch, depth + 1);
                        if (tempNode != null)
                            node.Children.Add(tempNode);
                    }
                }
                if (!hasNode)
                {
                    hasChanged = true;
                    if (depth == 0)
                        resultTree.Children.Add(compareBranch[depth + 1]);
                    else
                        resultTree = compareBranch[depth + 1];
                }
            }
            else if (resultTree.Children.Count == 0 && compareBranch.ContainsKey(depth + 1))
            {
                hasChanged = true;
                if (depth == 0)
                    resultTree.Children.Add(compareBranch[depth + 1]);
                else
                    resultTree = compareBranch[depth + 1];
            }
            return (hasChanged || depth == 0) ? resultTree : null;
        }
        /// <summary>
        /// 将branch按照深度进行分组
        /// </summary>
        /// <param name="branch">待分组的branch</param>
        /// <returns>key是深度，value是对应深度的结点</returns>
        private Dictionary<int, TreeNode> ConvertBranchToTreeNodes(TreeNode branch)
        {
            Dictionary<int, TreeNode> treeNodes = new Dictionary<int, TreeNode>();
            TreeNode tempNode = branch;
            int depth = 0;
            treeNodes[depth] = tempNode;
            while (tempNode.Children != null && tempNode.Children.Count != 0)
            {
                depth++;
                treeNodes[depth] = tempNode.Children[0];
                tempNode = tempNode.Children[0];
            }
            return treeNodes;
        }

        /// <summary>
        /// 通过用户输入的高级搜索条件，获取有执行顺序的栈
        /// </summary>
        /// <param name="advancedConditions">用户输入的高级搜索条件，例如“1&（2|3）”</param>
        /// <returns>一个有执行顺序的栈</returns>
        private Stack<int> GetStackFromAdvancedFilters(string advancedConditions)
        {
            Stack<int> stack = new Stack<int>();
            if (!string.IsNullOrEmpty(advancedConditions))
            {
                string suffixString = GetSuffixString(advancedConditions);
                int temp = 0;
                int n = 1;
                for (int i = suffixString.Length - 1; i >= 0; i--)
                {
                    if (suffixString[i] == AndSymbol)
                    {
                        if (temp != 0)
                        {
                            stack.Push(temp);
                            temp = 0;
                            n = 1;
                        }
                        stack.Push(And);
                    }
                    else if (suffixString[i] == OrSymbol)
                    {
                        if (temp != 0)
                        {
                            stack.Push(temp);
                            temp = 0;
                            n = 1;
                        }
                        stack.Push(Or);
                    }
                    else if (suffixString[i] >= '0' && suffixString[i] <= '9')
                    {
                        temp = ((suffixString[i] - 48) * n) + temp;
                        n = n * 10;
                    }
                    else if (suffixString[i] == '#')
                    {
                        if (temp != 0)
                        {
                            stack.Push(temp);
                            temp = 0;
                            n = 1;
                        }
                    }
                }
                stack.Push(temp);
            }
            return stack;
        }

        /// <summary>
        /// 将中缀表达式转化为后缀表达式
        /// </summary>
        /// <param name="infixString">用户输入的中缀表达式</param>
        /// <returns>转化之后的后缀表达式</returns>
        private string GetSuffixString(string fixedString)
        {
            string suffixString = string.Empty;
            Stack<char> tempStack = new Stack<char>();
            tempStack.Push((char)127);
            foreach (char c in fixedString)
            {
                if (c == AndSymbol || c == OrSymbol)
                {
                    suffixString += "#";
                    tempStack.Push(c);
                }
                else if (c == ')')
                {
                    tempStack.Push(c);
                }
                else if (c == '(')
                {
                    while (tempStack.Peek() != ')')
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

        /// <summary>
        /// 此方法主要用来处理界面上的advanced condition字符串
        /// </summary>
        /// <param name="infixString"></param>
        /// <param name="paramDictionary"></param>
        /// <returns>返回的字符串中左右括号是反的</returns>
        private string HandleConditionString(string infixString)
        {
            //return infixString.ToLower().Replace("and", "&").Replace("or", "|");
            List<int> sequenceNoes = new List<int>(){1};
            sequenceNoes.ForEach(num =>
                {
                    char[] numArray = num.ToString().ToCharArray();
                    Array.Reverse(numArray);
                    infixString = infixString.Replace(num.ToString(), new string(numArray));
                });
            char[] tempArray = infixString.ToLower().Replace("and", "&").Replace("or", "|").ToCharArray();
            Array.Reverse(tempArray);
            return new string(tempArray);
        }
    }
}
