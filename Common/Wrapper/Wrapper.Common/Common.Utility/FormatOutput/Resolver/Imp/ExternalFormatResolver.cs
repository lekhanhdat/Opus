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
namespace AvePoint.Wrapper.Common
{
    using AvePoint.GCommon.Contract.Tree.Object;
    using System;
    using System.Text;

    public static class AveTreeNodeExtension
    {
        /// <summary>
        /// 转换Tree型结构Text
        /// </summary>
        /// <param name="prefix">用于此节点之前的\t和连线</param>
        /// <param name="isLastChild">此节点是否是该层最后一个节点</param>
        /// <returns></returns>
        public static string TextNodeExtension(this IAveTreeNodeDto current,string prefix, bool isLastChild)
        {
            string expandChar = current.ChildrenLoaded ?(current.Expanded? "▽" : "▹" ): " ";
            StringBuilder textBuilder = new StringBuilder();
            textBuilder.Append(prefix + (isLastChild ? "└" : "├") + (current.CheckNumber == 1 ? "√" : " ") + expandChar + (IsContentLevel(current) ? current?.ID : current?.Name) + " " + (current.SelectAll == SelectAllState.Checked ? "SelectAll" : "") + " " + (current.IncludeNew == IncludeNewState.Checked ? "IncludeNew" : "") + " " + current.Offset + " " + current.StartIndex + " " + current.ChildrenCount + "\r\n");
            if (current.Children != null)
            {
                for (int i = 0; i < current.Children.Count - 1; i++)
                {
                    IAveTreeNodeDto child=null;
                    if (current.Children[i] is IAveTreeNodeDto)
                    {
                        child = current.Children[i] as IAveTreeNodeDto;
                    }
                    if (child != null)
                    {
                        if (isLastChild)
                        {
                            textBuilder.Append(child.TextNodeExtension(prefix + "" + "\t", false));
                        }
                        else
                        {
                            textBuilder.Append(child.TextNodeExtension(prefix + "│" + "\t", false));
                        }
                    }
                }
                if (current.Children.Count > 0)
                {
                    IAveTreeNodeDto child = null;
                    var last = current.Children[current.Children.Count - 1];
                    if (last is IAveTreeNodeDto)
                    {
                        child = last as IAveTreeNodeDto;
                    }
                    if (child != null)
                    {
                        if (isLastChild)
                        {
                            textBuilder.Append(child.TextNodeExtension(prefix + "" + "\t", true));
                        }
                        else
                        {
                            textBuilder.Append(child.TextNodeExtension(prefix + "│" + "\t", true));
                        }
                    }
                }
            }
            return textBuilder.ToString();
        }
        private static bool IsContentLevel(IAveTreeNodeDto current)
        {
            return current?.Level == NodeLevel.Item || current?.Level == NodeLevel.Document;
        }
    }

    public class AveTreeNodeDtoFormatResolver : IFormatResolver
    {
        public int Order { get { return 12; } }
        public void Invoke(StringBuilder builder, int level, object key, object value)
        {
            builder.AppendLineByLevel(level, key + ":");
            string prefix = Extension.GetPrefixByLevel(level, "");
            try
            {
                builder.AppendLineByLevel(level, (value as IAveTreeNodeDto).TextNodeExtension(prefix, false).Substring(prefix.Length));
            }
            catch
            {
                builder.AppendLineByLevel(level, (value as SPTreeNodeDto).TextNode(prefix, false).Substring(prefix.Length));
            }
        }

        public bool IsTypeQualified(object value)
        {
            if (value == null)
            {
                return false;
            }
            if (value is SPTreeNodeDto)
            {
                return true;
            }
            return false;
        }
    }

    public class AveBPOSAccountInfoFormatResolver : IFormatResolver
    {
        public int Order { get { return 11; } }
        public void Invoke(StringBuilder builder, int level, object key, object value)
        {
            builder.AppendLineByLevel(level, string.Format("<{0}:{1}>", key, value));
        }

        public bool IsTypeQualified(object value)
        {
            if (value == null)
            {
                return false;
            }
            if (value is AveBPOSAccountInfo)
            {
                return true;
            }
            return false;
        }
    }
    public class AveModernThemeInfoFormatResolver : IFormatResolver
    {
        public int Order { get { return 13; } }
        public void Invoke(StringBuilder builder, int level, object key, object value)
        {
            var fields = value.GetType().GetFields();
            foreach (var field in fields)
            {
                builder.AppendLineByLevel(level, string.Format("<{0}:{1}>", field.Name, field.GetValue(value)));
            }
        }

        public bool IsTypeQualified(object value)
        {
            if (value == null)
            {
                return false;
            }
            if (value is AveModernThemeInfo)
            {
                return true;
            }
            return false;
        }
    }
}
