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
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB.Query.Linq
{
    public interface IRMAzureCosmosDBLinqOrderBy
    {
        RMAzureCosmosDBLinqSorter OrderBy(Expression<Func<Record, dynamic>> keySelector);

        RMAzureCosmosDBLinqSorter OrderByDescending(Expression<Func<Record, dynamic>> keySelector);
    }

    public interface IRMAzureCosmosDBLinqSelect
    {
        RMAzureCosmosDBLinqSelector<TResult> Select<TResult>(Expression<Func<Record, TResult>> selector);
    }

    public interface IRMAzureCosmosDBLinqWhere
    {
        RMAzureCosmosDBLinqPredicateor Where(Expression<Func<Record, bool>> predicate);
    }

    public interface IRMAzureCosmosDBLinqResultSet<TResult>
    {
        RMAzureCosmosDBLinqResultSet<TResult> AsResultSet();
    }

    public interface IRMAzureCosmosDBLinqQueryableBuilder<TResult>
    {
        IQueryable<TResult> BuildQueryableLinq();

        IQueryable<TResult> BuildQueryableLinq(string continuationToken, int pageSize);
    }

    public class RMAzureCosmosDBLinqQuerier :
        IRMAzureCosmosDBLinqOrderBy,
        IRMAzureCosmosDBLinqSelect,
        IRMAzureCosmosDBLinqWhere,
        IRMAzureCosmosDBLinqResultSet<Record>,
        IRMAzureCosmosDBLinqQueryableBuilder<Record>
    {
        private readonly Container CosmosContainer;

        internal RMAzureCosmosDBLinqQuerier(Container container)
        {
            CosmosContainer = container;
        }

        public RMAzureCosmosDBLinqPredicateor Where(Expression<Func<Record, bool>> predicate)
        {
            return new RMAzureCosmosDBLinqPredicateor(CosmosContainer)
                .SetPredicate(predicate);
        }

        public RMAzureCosmosDBLinqSelector<TResult> Select<TResult>(Expression<Func<Record, TResult>> selector)
        {
            return new RMAzureCosmosDBLinqSelector<TResult>(CosmosContainer)
                .SetSelector(selector);
        }

        public RMAzureCosmosDBLinqSorter OrderBy(Expression<Func<Record, dynamic>> keySelector)
        {
            return new RMAzureCosmosDBLinqSorter(CosmosContainer)
                .SetOrderBy(keySelector);
        }

        public RMAzureCosmosDBLinqSorter OrderByDescending(Expression<Func<Record, dynamic>> keySelector)
        {
            return new RMAzureCosmosDBLinqSorter(CosmosContainer)
                .SetOrderByDescending(keySelector);
        }

        public RMAzureCosmosDBLinqResultSet<Record> AsResultSet()
        {
            return new RMAzureCosmosDBLinqResultSet<Record>(this);
        }

        public IQueryable<Record> BuildQueryableLinq()
        {
            return CosmosContainer.GetItemLinqQueryable<Record>(true, requestOptions: new QueryRequestOptions
            {
                MaxBufferedItemCount = -1,
                MaxConcurrency = -1,
            });
        }

        public IQueryable<Record> BuildQueryableLinq(string continuationToken, int pageSize)
        {
            return CosmosContainer.GetItemLinqQueryable<Record>(true, continuationToken, new QueryRequestOptions
            {
                MaxItemCount = pageSize,
                MaxBufferedItemCount = -1,
                MaxConcurrency = -1,
            });
        }
    }

    public class RMAzureCosmosDBLinqPredicateor :
        IRMAzureCosmosDBLinqOrderBy,
        IRMAzureCosmosDBLinqSelect,
        IRMAzureCosmosDBLinqResultSet<Record>,
        IRMAzureCosmosDBLinqQueryableBuilder<Record>
    {
        private readonly Container CosmosContainer;

        private Expression<Func<Record, bool>> Predicate { get; set; }

        internal RMAzureCosmosDBLinqPredicateor(Container container)
        {
            CosmosContainer = container;
        }

        internal RMAzureCosmosDBLinqPredicateor SetPredicate(Expression<Func<Record, bool>> predicate)
        {
            Predicate = predicate;
            return this;
        }

        public RMAzureCosmosDBLinqSelector<TResult> Select<TResult>(Expression<Func<Record, TResult>> selector)
        {
            return new RMAzureCosmosDBLinqSelector<TResult>(CosmosContainer)
                .SetPredicate(Predicate)
                .SetSelector(selector);
        }

        public RMAzureCosmosDBLinqSorter OrderBy(Expression<Func<Record, dynamic>> keySelector)
        {
            return new RMAzureCosmosDBLinqSorter(CosmosContainer)
                .SetPredicate(Predicate)
                .SetOrderBy(keySelector);
        }

        public RMAzureCosmosDBLinqSorter OrderByDescending(Expression<Func<Record, dynamic>> keySelector)
        {
            return new RMAzureCosmosDBLinqSorter(CosmosContainer)
                .SetPredicate(Predicate)
                .SetOrderByDescending(keySelector);
        }

        public RMAzureCosmosDBLinqResultSet<Record> AsResultSet()
        {
            return new RMAzureCosmosDBLinqResultSet<Record>(this);
        }

        public IQueryable<Record> BuildQueryableLinq()
        {
            return CosmosContainer.GetItemLinqQueryable<Record>(true, requestOptions: new QueryRequestOptions
            {
                MaxBufferedItemCount = -1,
                MaxConcurrency = -1
            }).Where(Predicate);
        }

        public IQueryable<Record> BuildQueryableLinq(string continuationToken, int pageSize)
        {
            return CosmosContainer.GetItemLinqQueryable<Record>(true, continuationToken, new QueryRequestOptions
            {
                MaxItemCount = pageSize,
                MaxBufferedItemCount = -1,
                MaxConcurrency = -1,
            }).Where(Predicate);
        }
    }

    public class RMAzureCosmosDBLinqSorter :
     IRMAzureCosmosDBLinqSelect,
     IRMAzureCosmosDBLinqResultSet<Record>,
     IRMAzureCosmosDBLinqQueryableBuilder<Record>
    {
        private readonly Container CosmosContainer;

        private Expression<Func<Record, bool>> Predicate { get; set; }

        private Expression<Func<Record, dynamic>> KeySelector { get; set; }

        private bool IsDescending { get; set; }

        internal RMAzureCosmosDBLinqSorter(Container container)
        {
            CosmosContainer = container;
        }

        internal RMAzureCosmosDBLinqSorter SetPredicate(Expression<Func<Record, bool>> predicate)
        {
            Predicate = predicate;
            return this;
        }

        internal RMAzureCosmosDBLinqSorter SetOrderBy(Expression<Func<Record, dynamic>> keySelector)
        {
            IsDescending = false;
            KeySelector = keySelector;
            return this;
        }

        internal RMAzureCosmosDBLinqSorter SetOrderByDescending(Expression<Func<Record, dynamic>> keySelector)
        {
            IsDescending = true;
            KeySelector = keySelector;
            return this;
        }

        public RMAzureCosmosDBLinqSelector<TResult> Select<TResult>(Expression<Func<Record, TResult>> selector)
        {
            return new RMAzureCosmosDBLinqSelector<TResult>(CosmosContainer)
                .SetPredicate(Predicate)
                .SetKeySelector(KeySelector)
                .SetIsDescending(IsDescending)
                .SetSelector(selector);
        }

        public RMAzureCosmosDBLinqResultSet<Record> AsResultSet()
        {
            return new RMAzureCosmosDBLinqResultSet<Record>(this);
        }

        public IQueryable<Record> BuildQueryableLinq()
        {
            var queryable = CosmosContainer.GetItemLinqQueryable<Record>(true, requestOptions: new QueryRequestOptions
            {
                MaxBufferedItemCount = -1,
                MaxConcurrency = -1
            }).AsQueryable();
            if (Predicate != null)
            {
                queryable = queryable.Where(Predicate);
            }

            if (IsDescending)
            {
                queryable = queryable.OrderByDescending(KeySelector);
            }
            else
            {
                queryable = queryable.OrderBy(KeySelector);
            }

            return queryable;
        }

        public IQueryable<Record> BuildQueryableLinq(string continuationToken, int pageSize)
        {
            var queryable = CosmosContainer.GetItemLinqQueryable<Record>(true, continuationToken, new QueryRequestOptions
            {
                MaxItemCount = pageSize,
                MaxBufferedItemCount = -1,
                MaxConcurrency = -1,
            }).AsQueryable();
            if (Predicate != null)
            {
                queryable = queryable.Where(Predicate);
            }

            if (IsDescending)
            {
                queryable = queryable.OrderByDescending(KeySelector);
            }
            else
            {
                queryable = queryable.OrderBy(KeySelector);
            }

            return queryable;
        }
    }

    public class RMAzureCosmosDBLinqSelector<TResult> :
        IRMAzureCosmosDBLinqResultSet<TResult>,
        IRMAzureCosmosDBLinqQueryableBuilder<TResult>
    {
        private readonly Container CosmosContainer;

        private Expression<Func<Record, bool>> Predicate { get; set; }

        private Expression<Func<Record, TResult>> Selector { get; set; }

        private Expression<Func<Record, dynamic>> KeySelector { get; set; }

        private bool IsDescending { get; set; }

        internal RMAzureCosmosDBLinqSelector(Container container)
        {
            CosmosContainer = container;
        }

        internal RMAzureCosmosDBLinqSelector<TResult> SetPredicate(Expression<Func<Record, bool>> predicate)
        {
            Predicate = predicate;
            return this;
        }

        internal RMAzureCosmosDBLinqSelector<TResult> SetKeySelector(Expression<Func<Record, dynamic>> keySelector)
        {
            KeySelector = keySelector;
            return this;
        }

        internal RMAzureCosmosDBLinqSelector<TResult> SetIsDescending(bool isDescending)
        {
            IsDescending = isDescending;
            return this;
        }

        internal RMAzureCosmosDBLinqSelector<TResult> SetSelector(Expression<Func<Record, TResult>> selector)
        {
            Selector = selector;
            return this;
        }

        public RMAzureCosmosDBLinqResultSet<TResult> AsResultSet()
        {
            return new RMAzureCosmosDBLinqResultSet<TResult>(this);
        }

        public IQueryable<TResult> BuildQueryableLinq()
        {
            var queryable = CosmosContainer.GetItemLinqQueryable<Record>(true).AsQueryable();
            if (Predicate != null)
            {
                queryable = queryable.Where(Predicate);
            }

            if (KeySelector != null)
            {
                if (IsDescending)
                {
                    queryable = queryable.OrderByDescending(KeySelector);
                }
                else
                {
                    queryable = queryable.OrderBy(KeySelector);
                }
            }

            var selecteQueryable = queryable.Select(Selector);
            return selecteQueryable;
        }

        public IQueryable<TResult> BuildQueryableLinq(string continuationToken, int pageSize)
        {
            var queryable = CosmosContainer.GetItemLinqQueryable<Record>(true, continuationToken, new QueryRequestOptions
            {
                MaxItemCount = pageSize
            }).AsQueryable();
            if (Predicate != null)
            {
                queryable = queryable.Where(Predicate);
            }

            if (KeySelector != null)
            {
                if (IsDescending)
                {
                    queryable = queryable.OrderByDescending(KeySelector);
                }
                else
                {
                    queryable = queryable.OrderBy(KeySelector);
                }
            }

            var selecteQueryable = queryable.Select(Selector);
            return selecteQueryable;
        }
    }

    public class RMAzureCosmosDBLinqResultSet<TResult>
    {
        private readonly IRMAzureCosmosDBLinqQueryableBuilder<TResult> QueryableBuilder;

        internal RMAzureCosmosDBLinqResultSet(IRMAzureCosmosDBLinqQueryableBuilder<TResult> queryableBuilder)
        {
            QueryableBuilder = queryableBuilder;
        }

        public async Task<TResult> FirstOrDefault()
        {
            return await QueryableBuilder.BuildQueryableLinq()
                .Take(1)
                .ToAsyncEnumerable()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<TResult>> TopAsync(int count)
        {
            return await QueryableBuilder.BuildQueryableLinq()
                .Take(count)
                .ToAsyncEnumerable()
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async IAsyncEnumerable<TResult> AllAsync()
        {
            using var iterator = QueryableBuilder.BuildQueryableLinq().ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync().ConfigureAwait(false))
                {
                    yield return item;
                }
            }
        }

        public async Task<bool> ExistAsync()
        {
            var res = await FirstOrDefault().ConfigureAwait(false);
            if(res == null)
            {
                return false;
            }
            return true;
        }

        public async Task<int> CountAsync()
        {
            return await QueryableBuilder.BuildQueryableLinq()
                .CountAsync()
                .ConfigureAwait(false);
        }

        public async Task<RMAzureCosmosDBQueryPagniationResult<TResult>> PaginateAsync(string continuationToken, int pageSize)
        {
            var items = new List<TResult>(pageSize);
            do
            {
                using var iterator = QueryableBuilder.BuildQueryableLinq(continuationToken, pageSize).ToFeedIterator();

                if (!iterator.HasMoreResults) break;

                var response = await iterator.ReadNextAsync().ConfigureAwait(false);
                continuationToken = response.ContinuationToken;

                foreach (var item in response)
                {
                    items.Add(item);
                }

                pageSize -= response.Count;

            } while (pageSize > 0 && !string.IsNullOrEmpty(continuationToken));

            return new RMAzureCosmosDBQueryPagniationResult<TResult>(continuationToken, items);
        }
    }
}
