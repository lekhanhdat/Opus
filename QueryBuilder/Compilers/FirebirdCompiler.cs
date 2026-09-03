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
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlKata.Compilers
{
    public class FirebirdCompiler : Compiler
    {
        public FirebirdCompiler()
        {
        }

        public override string EngineCode { get; } = EngineCodes.Firebird;

        protected override SqlResult CompileInsertQuery(Query query)
        {
            var ctx = base.CompileInsertQuery(query);

            var inserts = ctx.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);

            if (inserts.Count > 1)
            {
                ctx.RawSql = Regex.Replace(ctx.RawSql, @"\)\s+VALUES\s+\(", ") SELECT ");
                ctx.RawSql = Regex.Replace(ctx.RawSql, @"\),\s*\(", " FROM RDB$DATABASE UNION ALL SELECT ");
                ctx.RawSql = Regex.Replace(ctx.RawSql, @"\)$", " FROM RDB$DATABASE");
            }

            return ctx;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                ctx.Bindings.Add(offset + 1);
                ctx.Bindings.Add(limit + offset);

                return "ROWS ? TO ?";
            }

            return null;
        }


        protected override string CompileColumns(SqlResult ctx)
        {
            var compiled = base.CompileColumns(ctx);

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                ctx.Bindings.Insert(0, limit);

                ctx.Query.ClearComponent("limit");

                return "SELECT FIRST ?" + compiled.Substring(6);
            }
            else if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Insert(0, offset);

                ctx.Query.ClearComponent("offset");

                return "SELECT SKIP ?" + compiled.Substring(6);
            }

            return compiled;
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
            {
                left = $"CAST({column} as TIME)";
            }
            else if (condition.Part == "date")
            {
                left = $"CAST({column} as DATE)";
            }
            else
            {
                left = $"EXTRACT({condition.Part.ToUpperInvariant()} FROM {column})";
            }

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        public override string WrapValue(string value)
        {
            return base.WrapValue(value).ToUpperInvariant();
        }

        public override string CompileTrue()
        {
            return "1";
        }

        public override string CompileFalse()
        {
            return "0";
        }
    }
}
