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

namespace AvePoint.RA.RAPhysical.Common
{
    public class PhysicalExpressionVisitor : ExpressionVisitor
    {
        public ParameterExpression _Parameter { get; set; }

        public PhysicalExpressionVisitor(ParameterExpression Parameter)
        {
            _Parameter = Parameter;
        }
        protected override Expression VisitParameter(ParameterExpression p)
        {
            return _Parameter;
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);//Visit会根据VisitParameter()方法返回的Expression修改这里的node变量
        }
    }
}
