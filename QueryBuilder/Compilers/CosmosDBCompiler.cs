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
using System.Text;

namespace SqlKata.Compilers
{
    public class CosmosDBCompiler
     : Compiler
    {
        public string ErrMsg = "invalidate operations for cosmos db compliler";
        public override string EngineCode { get; } = EngineCodes.CosmosDB;
        //protected override string parameterPlaceholder { get; set; } = "?";
        //protected override string parameterPrefix { get; set; } = "@p";
        protected override string OpeningIdentifier { get; set; } = "";
        protected override string ClosingIdentifier { get; set; } = "";
        //protected override string LastId { get; set; } = "select last_insert_rowid() as id";
        
        /// <summary>
        /// not wrap with openingIdentifier and closing identitifier
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override string Wrap(string value)
        {
            return value;
        }

        protected override SqlResult CompileSelectQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query.Clone(),
            };

            // compile from first.
            var fromResult = this.CompileFrom(ctx);

            var results = new[] {
                    this.CompileColumns(ctx),
                    fromResult,
                    this.CompileJoins(ctx),
                    this.CompileWheres(ctx),
                    this.CompileGroups(ctx),
                    this.CompileHaving(ctx),
                    this.CompileOrders(ctx),
                    this.CompileLimit(ctx),
                    this.CompileUnion(ctx),
                }
               .Where(x => x != null)
               .Where(x => !string.IsNullOrEmpty(x))
               .ToList();

            string sql = string.Join(" ", results);

            ctx.RawSql = sql;

            return ctx;
        }
        protected override string CompileBooleanCondition(SqlResult ctx, BooleanCondition item)
        {
            var columnPrefix = GetTableOrAliasPrefix(ctx);
            var column = Wrap(item.Column);
            var value = item.Value ? CompileTrue() : CompileFalse();

            var op = item.IsNot ? "!=" : "=";

            return $"{columnPrefix}{column} {op} {value}";
        }

      

        public override string CompileTableExpression(SqlResult ctx, AbstractFrom from)
        {
            if (from is FromClause fromClause)
            {
                ctx.Query.TableName = fromClause.Table;

                if(!string.IsNullOrEmpty(ctx.Query.QueryAlias))
                {
                    return Wrap(fromClause.Table + " " + ctx.Query.QueryAlias);
                }

                return Wrap(fromClause.Table);
            }

            if(from is FromParentClause fromParentClause)
            {
                ctx.Query.TableName = fromParentClause.Table;

                return $"{ctx.Query.TableName} in {ctx.Query.ParentTable}{fromParentClause.Path}";
            }

            if (from is QueryFromClause queryFromClause)
            {
                var fromQuery = queryFromClause.Query;

                var alias = string.IsNullOrEmpty(fromQuery.QueryAlias) ? "" : " "+fromQuery.QueryAlias;

                var subCtx = CompileSelectQuery(fromQuery);

                ctx.Bindings.AddRange(subCtx.Bindings);

                return "(" + subCtx.RawSql + ")" + alias;
            }

            throw new CompilerException(ErrMsg);
        }

        protected override string CompileBasicCondition(SqlResult ctx, BasicCondition x)
        {
            var columnPrefix = GetTableOrAliasPrefix(ctx);

            var sql = $"{Wrap(columnPrefix + x.Column)} {checkOperator(x.Operator)} {Parameter(ctx, x.Value)}";

            if (x.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        public override string CompileColumn(SqlResult ctx, AbstractColumn column)
        {
            if (column is RawColumn raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            //for count
            if((column as Column).Name == "*")
            {
                return "1";
            }

            var columnPrefix = GetTableOrAliasPrefix(ctx);
            return Wrap(columnPrefix + (column as Column).Name);
        }

        public override string CompileOrders(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("order", EngineCode))
            {
                return null;
            }
            var columnPrefix = GetTableOrAliasPrefix(ctx);
            var columns = ctx.Query
                .GetComponents<AbstractOrderBy>("order", EngineCode)
                .Select(x =>
                {

                    if (x is RawOrderBy raw)
                    {
                        ctx.Bindings.AddRange(raw.Bindings);
                        return WrapIdentifiers(raw.Expression);
                    }

                    var direction = (x as OrderBy).Ascending ? "" : " DESC";

                    return Wrap(columnPrefix +(x as OrderBy).Column) + direction;
                });

            return "ORDER BY " + string.Join(", ", columns);
        }

        public override string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            if (offset < 0)
            {
                throw new CompilerException(ErrMsg);
            }

            if (limit < 0)
            {
                throw new CompilerException(ErrMsg);
            }

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return "OFFSET ? LIMIT ?";
        }

        protected override string CompileBetweenCondition<T>(SqlResult ctx, BetweenCondition<T> item)
        {
            var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";
            var lower = Parameter(ctx, item.Lower);
            var higher = Parameter(ctx, item.Higher);

            var columnPrefix = GetTableOrAliasPrefix(ctx);

            return Wrap(columnPrefix + item.Column) + $" {between} {lower} AND {higher}";
        }

        protected override string CompileInCondition<T>(SqlResult ctx, InCondition<T> item)
        {
            var columnPrefix = GetTableOrAliasPrefix(ctx);

            var column = Wrap(columnPrefix + item.Column);

            if (!item.Values.Any())
            {
                return item.IsNot ? $"1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */";
            }

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            var values = Parameterize(ctx, item.Values);

            return column + $" {inOperator} ({values})";
        }


        private string GetTableOrAliasPrefix(SqlResult ctx)
        {
            string columnPrefix;

            if(ctx.Query.IsSubQuery)
            {
                columnPrefix = ctx.Query.TableName; // + ".";
            }
            else if(ctx.Query.HasJoin)
            {
                columnPrefix = "";
            }
            else if (!string.IsNullOrEmpty(ctx.Query.QueryAlias))
            {
                columnPrefix = ctx.Query.QueryAlias; // + ".";
            }
            else if (!string.IsNullOrEmpty(ctx.Query.TableName))
            {
                columnPrefix = ctx.Query.TableName; // + ".";
            }
            else
            {
                throw new CompilerException(ErrMsg);
            }

            return columnPrefix;
        }

        protected override string CompileNullCondition(SqlResult ctx, NullCondition item)
        {
            var columnPrefix = GetTableOrAliasPrefix(ctx);
            var op = item.IsNot ? "!= null" : "= null";
            return columnPrefix + Wrap(item.Column) + " " + op;
        }

        protected override string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
        {
            if(x.Operator == "like")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                var value = Resolve(ctx, x.Value) as string;

                if (value == null)
                {
                    throw new ArgumentException("Expecting a non nullable string");
                }

                var method = x.Operator;

                string sql;

                if (x.Value is UnsafeLiteral)
                {
                    sql = $"{columnPrefix}{column} {checkOperator(method)} {value}";
                }
                else
                {
                    sql = $"{columnPrefix}{column} {checkOperator(method)} {Parameter(ctx, value)}";
                }

                return x.IsNot ? $"NOT ({sql})" : sql;

            }

            if(x.Operator == "contains")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                var value = Resolve(ctx, x.Value) as string;

                if (value == null)
                {
                    throw new ArgumentException("Expecting a non nullable string");
                }

                var method = x.Operator;

                string sql;
                //CONTAINS("abc", "ab", false)
                if (x.Value is UnsafeLiteral)
                {                   
                    sql = $"CONTAINS({columnPrefix}{column},{value},{(!x.CaseSensitive).ToString().ToLowerInvariant()})";
                }
                else
                {
                    sql = $"CONTAINS({columnPrefix}{column},{Parameter(ctx, value)},{(!x.CaseSensitive).ToString().ToLowerInvariant()})";
                }

                return x.IsNot ? $"NOT ({sql})" : sql;
            }

            if (x.Operator == "starts")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                var value = Resolve(ctx, x.Value) as string;

                if (value == null)
                {
                    throw new ArgumentException("Expecting a non nullable string");
                }

                string sql;
                //CONTAINS("abc", "ab", false)
                if (x.Value is UnsafeLiteral)
                {
                    sql = $"STARTSWITH({columnPrefix}{column},{value},{(!x.CaseSensitive).ToString().ToLowerInvariant()})";
                }
                else
                {
                    sql = $"STARTSWITH({columnPrefix}{column},{Parameter(ctx, value)},{(!x.CaseSensitive).ToString().ToLowerInvariant()})";
                }

                return x.IsNot ? $"NOT ({sql})" : sql;
            } 
            else if (x.Operator == "ends")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                var value = Resolve(ctx, x.Value) as string;

                if (value == null)
                {
                    throw new ArgumentException("Expecting a non nullable string");
                }

                string sql;
                //CONTAINS("abc", "ab", false)
                if (x.Value is UnsafeLiteral)
                {
                    sql = $"ENDSWITH({columnPrefix}{column},{value},{(!x.CaseSensitive).ToString().ToLowerInvariant()})";
                }
                else
                {
                    sql = $"ENDSWITH({columnPrefix}{column},{Parameter(ctx, value)},{(!x.CaseSensitive).ToString().ToLowerInvariant()})";
                }

                return x.IsNot ? $"NOT ({sql})" : sql;
            }
            else if (x.Operator == "regex")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                var value = Resolve(ctx, x.Value) as string;

                if (value == null)
                {
                    throw new ArgumentException("Expecting a non nullable string");
                }

                string sql;
                //CONTAINS("abc", "ab", false)
                if (x.Value is UnsafeLiteral)
                {

                    sql = x.CaseSensitive ? $"REGEXMATCH({columnPrefix}{column},{value},'')" : $"REGEXMATCH({columnPrefix}{column},{value},'i')";
                }
                else
                {
                    sql = x.CaseSensitive ? $"REGEXMATCH({columnPrefix}{column},{Parameter(ctx, value)},'')" : $"REGEXMATCH({columnPrefix}{column},{Parameter(ctx, value)},'i')" ;
                }

                return x.IsNot ? $"NOT ({sql})" : sql;
            }

            if (x.Operator == "stringEquals")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                var value = Resolve(ctx, x.Value) as string;

                if (value == null)
                {
                    throw new ArgumentException("Expecting a non nullable string");
                }

                string sql;
                //CONTAINS("abc", "ab", false)
                if (x.Value is UnsafeLiteral)
                {
                    sql = $"STRINGEQUALS({columnPrefix}{column},{value},{(!x.CaseSensitive).ToString().ToLowerInvariant()})";
                }
                else
                {
                    sql = $"STRINGEQUALS({columnPrefix}{column},{Parameter(ctx, value)},{(!x.CaseSensitive).ToString().ToLowerInvariant()})";
                }

                return x.IsNot ? $"NOT ({sql})" : sql;
            }

            if (x.Operator == "arrayContain")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                //if (string.IsNullOrEmpty(column))
                //{
                //    columnPrefix = columnPrefix.TrimEnd('.');
                //}

                var value = Resolve(ctx, x.Value) as string;

                if (value == null)
                {
                    throw new ArgumentException("Expecting a non nullable string");
                }

                string sql;
                //CONTAINS("abc", "ab", false)
                if (x.Value is UnsafeLiteral)
                {
                    sql = $"ARRAY_CONTAINS({columnPrefix}{column},{value},{(x.CaseSensitive).ToString().ToLowerInvariant()})";
                }
                else
                {
                    sql = $"ARRAY_CONTAINS({columnPrefix}{column},{Parameter(ctx, value)},{(x.CaseSensitive).ToString().ToLowerInvariant()})";
                }

                return x.IsNot ? $"NOT ({sql})" : sql;
            }

            if (x.Operator == "arrayContainV2")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                //if(string.IsNullOrEmpty(column))
                //{
                //    columnPrefix = columnPrefix.TrimEnd('.');
                //}

                var value = Resolve(ctx, x.Value);

                //if (value == null)
                //{
                //    throw new ArgumentException("Expecting a non nullable string");
                //}

                string sql;
                //CONTAINS("abc", "ab", false)
                if (x.Value is UnsafeLiteral)
                {
                    sql = $"ARRAY_CONTAINS({value},{columnPrefix}{column},{(x.CaseSensitive).ToString().ToLowerInvariant()})";
                }
                else
                {
                    sql = $"ARRAY_CONTAINS({Parameter(ctx, value)},{columnPrefix}{column},{(x.CaseSensitive).ToString().ToLowerInvariant()})";
                }

                return x.IsNot ? $"NOT ({sql})" : sql;
            }

            if (x.Operator == "arrayContainV3")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);
                var function = x.Func;
                var value = Resolve(ctx, x.Value);
                string sql;
                if (x.Value is UnsafeLiteral)
                {
                    sql = $"ARRAY_CONTAINS({value},{function}({columnPrefix}{column}),{(x.CaseSensitive).ToString().ToLowerInvariant()})";
                }
                else
                {
                    sql = $"ARRAY_CONTAINS({Parameter(ctx, value)},{function}({columnPrefix}{column}),{(x.CaseSensitive).ToString().ToLowerInvariant()})";
                }

                return x.IsNot ? $"NOT ({sql})" : sql;
            }


            if (x.Operator == "IS_DEFINED")
            {
                var columnPrefix = GetTableOrAliasPrefix(ctx);
                var column = Wrap(x.Column);

                string sql;
                //CONTAINS("abc", "ab", false)

                sql = $"IS_DEFINED({columnPrefix}{column})";
               

                return x.IsNot ? $"NOT ({sql})" : sql;
            }


            throw new Exception();
        }

    }

    [Serializable]
    public class CompilerException: Exception
    {
        public CompilerException(string message): base(message)
        {

        }
    }
}
