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
using SqlKata;
using SqlKata.Compilers;
using System;

namespace QueryBuilder.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string tempResult;
            //tempResult = BasicFrom();
            tempResult = BasicWhere();
            //tempResult = Alias();
            //tempResult = Select();
            //tempResult = Distinct();
            //tempResult = Top();
            //tempResult = WhereCombine();
            //tempResult = Order();
            //tempResult = LimitAndOffset();
            //tempResult = Between();
            //tempResult = In();
            //tempResult = Like();
            tempResult = Contains();
            tempResult = StartWith();
            tempResult = RegexMatch();
            //tempResult = StringEquals();
            //tempResult = WhereArrayContain();
            //tempResult = WhereArrayContainV2();
            //tempResult = WorkingWithDate();
            //tempResult = WhereBool();
            //tempResult = WhereNull();
            //tempResult = WhereExsit();
            //tempResult = JoinSubQuery();
            //tempResult = Join();
            //tempResult = WhereExistInSubQuery();
            //tempResult = Defined();
            //tempResult = Count();
            //tempResult = CountWithWhereExistInSubQuery();
            Console.WriteLine(tempResult);
            Console.ReadLine();
         }

        static string BasicFrom()
        {
            Query testQuery = new Query();
            testQuery.From("Records");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string BasicWhere()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .Where(".sub.title", "=", "book");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string WhereBool()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .WhereTrue(".boolColumn")
                .WhereFalse(".anotherBoolColumn");
                

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string WhereNull()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .WhereNull(".columnA")
                .WhereNotNull(".columnB");


            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Alias()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(".title", "=", ".book");
           
            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Select()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(".title", "=", ".book")
                .Select(".id", ".internalId", ".sub.group");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Distinct()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(".title", "=", ".book")
                .Select(".id", ".internalId", ".group")
                .Distinct();

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Top()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(".title", "=", ".book")
                .Select(".id", ".internalId", ".group")
                .Distinct()
                .Top(5);

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Count()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .Where(".sub.title", "=", "book")
                .AsCount();

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string WhereCombine()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(
                q=>q.Where(".title", "=", ".book")
                .OrWhere(".name", "=", ".allen")
                )                
                .Where(".pic", "=", true)
                .WhereNot(".condition","<", 5)
                .Select(".id", ".internalId", ".group")
                .Distinct()
                .Top(5);
                
            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Order()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(".title", "=", ".book")
                .OrderByDesc(".order1", ".order2")
                .OrderBy(".order3")
                .OrderByDesc(".order4");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string LimitAndOffset()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(".title", "=", ".book")
                .OrderBy(".order3")
                .Limit(5)
                .Offset(10);

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Between()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .WhereBetween(".sub.no", -5,100);

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string In()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .WhereIn<Guid>(".ContainerId",new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() });

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Like()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereLike(".name", "%allen")
                .WhereNotLike(".name", "%guy%");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Contains()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereContains(".name", "allen",true)
                .WhereNotContains(".name", "avepoint",false);

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Defined()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereDefined(".columnA")
                .OrWhereNotDefined(".columnB");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }
        static string RegexMatch()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereRegex(".name", "a*b*c", true)
                .OrWhereRegex(".name", "avepoint*", false);

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string StartWith()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereStarts(".name", "allen", true)
                .WhereNotStarts(".name", "avepoint", false);

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string StringEquals()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereStringEquals(".name", "allen", true)
                .WhereNotStringEquals(".name", "avepoint", false);

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string WhereArrayContain()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereArrayContain(".name", "allen", true)
                .WhereNotArrayContain(".name", "avepoint", false);
            

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string WhereArrayContainV2()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereArrayContainV2(new int[] { 1, 2, 3, 4 }, ".flag", true)
                .WhereNotArrayContainV2(new string[] {"file1", "file2" },"", false);


            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/azure/cosmos-db/working-with-dates
        /// </summary>
        /// <returns></returns>
        static string WorkingWithDate()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .Where(".created", ">=", DateTime.UtcNow);

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string WhereExsit()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .WhereExists(q => { 
                    q.FromParent("sub", ".subRecords")
                    .WhereArrayContain(".value_Array","box1");
                    return q; })
                .Select(".c1",".c2",".c3");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string WhereExistInSubQuery()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(query => {
                    query
  .WhereExists(q =>
  {
      q.FromParent("sub", ".subRecords")
                    .WhereArrayContain(".value_Array", "box1");
      return q;
  })
  .OrWhereExists(q =>
  {
      q.FromParent("sub", ".subRecordsV2")
      .WhereArrayContain(".value", "box2");
      return q;
  });
                return query;
                }
                )
                .Select(".c1", ".c2", ".c3");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string CountWithWhereExistInSubQuery()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Where(query => {
                    query
  .WhereExists(q =>
  {
      q.FromParent("sub", ".subRecords")
                    .WhereArrayContain(".value_Array", "box1");
      return q;
  })
  .OrWhereExists(q =>
  {
      q.FromParent("sub", ".subRecordsV2")
      .WhereArrayContain(".value", "box2");
      return q;
  });
                    return query;
                }
                )
                .AsCount();

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string JoinSubQuery()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Join(q => {
                    q.FromParent("sub", ".subRecords")
                    .As("s1")
                    .WhereArrayContain(".value_Array", "box1");
                    return q;
                })
                .Join(q =>{
                    return q.FromParent("t", ".timestamps")
                    .As("t1")
                    .Where(".created", ">", DateTime.UtcNow)
                    .OrWhere(".modified", "<", DateTime.UtcNow);
                })
                //notice: supply the alias here in column name
                .Where("s1.condition", ">", 0)
                .Where("t1.condition", "=", "condition")
                .Select(".c1", ".c2", ".c3");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }

        static string Join()
        {
            Query testQuery = new Query();
            testQuery
                .From("Records")
                .As("r")
                .Join(q => {
                    q.FromParent(".sub", ".subRecords")
                    .As("s1")
                    .WhereArrayContain(".value_Array", "box1");
                    return q;
                })
                .Join(q => {
                    return q.FromParent("t", "timestamps")
                    .As("t1")
                    .Where(".created", ">", DateTime.UtcNow)
                    .OrWhere(".modified", "<", DateTime.UtcNow);
                })
                //notice: supply the alias here in column name
                .Where(".s1.condition", ">", 0)
                .Where(".t1.condition", "=", "condition")
                .Select(".c1", ".c2", ".c3");

            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(testQuery);
            return result.Sql;
        }





    }
}
