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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public static class CombineExpressionsHelper
    {
        public static Expression<Func<T, TResult>> CombineExpressions<T, TResult>(
            Expression<Func<T, TResult>> expr1,
            Expression<Func<T, TResult>> expr2,
            Func<Expression, Expression, BinaryExpression> combiner)
        {
            var map = expr1.Parameters
                .Select((firstParam, index) => new { firstParam, secondParam = expr2.Parameters[index] })
                .ToDictionary(p => p.secondParam, p => p.firstParam);

            // Replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, expr2.Body);

            // Apply composition of lambda expression bodies to parameters from the first expression
            return Expression.Lambda<Func<T, TResult>>(combiner(expr1.Body, secondBody), expr1.Parameters);
        }

        public static string FormatExpressionToString(Expression expr)
        {
            var visitor = new FormatPrintExpressionVisitor();
            visitor.Visit(expr);
            return visitor.ToString();
        }
    }


    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_map.TryGetValue(node, out var replacement))
            {
                node = replacement;
            }
            return base.VisitParameter(node);
        }
    }

    class FormatPrintExpressionVisitor : ExpressionVisitor
    {
        private StringBuilder _builder = new StringBuilder();
        private int _currentPrecedence = 0;

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            int precedence = GetPrecedence(node);

            bool needsParentheses = precedence < _currentPrecedence;

            if (needsParentheses)
            {
                _builder.Append("(");
            }

            int oldPrecedence = _currentPrecedence;
            _currentPrecedence = precedence;

            base.Visit(node);

            _currentPrecedence = oldPrecedence;

            if (needsParentheses)
            {
                _builder.Append(")");
            }

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    _builder.Append(" && ");
                    break;
                case ExpressionType.OrElse:
                    _builder.Append(" || ");
                    break;
                case ExpressionType.Equal:
                    _builder.Append(" == ");
                    break;
                case ExpressionType.NotEqual:
                    _builder.Append(" != ");
                    break;
                case ExpressionType.GreaterThan:
                    _builder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _builder.Append(" >= ");
                    break;
                case ExpressionType.LessThan:
                    _builder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _builder.Append(" <= ");
                    break;
                default:
                    throw new NotSupportedException($"Unsupported binary operator: {node.NodeType}");
            }

            Visit(node.Right);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _builder.Append(node.Name);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is bool)
            {
                _builder.Append(node.Value.ToString().ToLower());
            }
            else
            {
                _builder.Append(node.Value);
            }
            return node;
        }

        private int GetPrecedence(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.OrElse:
                    return 0;
                case ExpressionType.AndAlso:
                    return 1;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    return 2;
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return 3;
                default:
                    return int.MaxValue;
            }
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
