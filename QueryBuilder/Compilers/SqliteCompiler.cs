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
using System.Collections.Generic;
using SqlKata;
using SqlKata.Compilers;

namespace SqlKata.Compilers
{
    public class SqliteCompiler : Compiler
    {
        public override string EngineCode { get; } = EngineCodes.Sqlite;
        protected override string parameterPlaceholder { get; set; } = "?";
        protected override string parameterPrefix { get; set; } = "@p";
        protected override string OpeningIdentifier { get; set; } = "\"";
        protected override string ClosingIdentifier { get; set; } = "\"";
        protected override string LastId { get; set; } = "select last_insert_rowid() as id";

        public override string CompileTrue()
        {
            return "1";
        }

        public override string CompileFalse()
        {
            return "0";
        }

        public override string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Add(offset);
                return "LIMIT -1 OFFSET ?";
            }

            return base.CompileLimit(ctx);
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);
            var value = Parameter(ctx, condition.Value);

            var formatMap = new Dictionary<string, string> {
                { "date", "%Y-%m-%d" },
                { "time", "%H:%M:%S" },
                { "year", "%Y" },
                { "month", "%m" },
                { "day", "%d" },
                { "hour", "%H" },
                { "minute", "%M" },
            };

            if (!formatMap.ContainsKey(condition.Part))
            {
                return $"{column} {condition.Operator} {value}";
            }

            var sql = $"strftime('{formatMap[condition.Part]}', {column}) {condition.Operator} cast({value} as text)";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }
    }
}
