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

namespace AvePoint.RA.DB.Dao
{
    public interface IBaseDao<TModel> 
    {
        TModel Create(TModel entity);

        int BatchCreate(List<TModel> entities);

        Task<int> BatchCreateAsync(IEnumerable<TModel> entities);

        Task<bool> UpdateAsync(TModel entity);

        bool Update<TProperty>(TModel entity, params Expression<Func<TModel, TProperty>>[] properties);

        int BatchUpdate(List<TModel> entities);

        int BatchUpdate<TProperty>(List<TModel> entities,params Expression<Func<TModel, TProperty>>[] properties);

        bool Delete(TModel entity);

        int BatchDelete(List<TModel> entities);

        Task<int> BatchDeleteAsync(List<TModel> entities);

        Task<int> BatchDeleteAsync(Expression<Func<TModel, bool>> whereLambda);

        bool DeleteByKey(object keyValue);

        int CountAll();

        int Count(Expression<Func<TModel, bool>> whereLambda);
        
        Task<int> CountAsync(Expression<Func<TModel, bool>> whereLambda);
         
        /// <summary>
        /// 按条件查询，返回单条记录
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// Find(w => w.Stage == 1, o => o.Video);
        /// </summary>
        /// <param name="whereLambda">where条件</param>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        TModel Find(Expression<Func<TModel, bool>> whereLambda, params Expression<Func<TModel, object>>[] includeProperties);

        TModel FindWithNewContext(Expression<Func<TModel, bool>> whereLambda, params Expression<Func<TModel, object>>[] includeProperties);
        /// <summary>
        /// 获取全部记录
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// FindAll(o => o.Form, o => o.Video);
        /// </summary>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        List<TModel> FindAll(params Expression<Func<TModel, object>>[] includeProperties);

        bool Exist(Expression<Func<TModel, bool>> anyLambda);

        /// <summary>
        /// 按条件查询列表
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// FindList(w => w.Stage == 1, o => o.Video);
        /// </summary>
        /// <param name="whereLambda">where条件</param>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        Task<List<TModel>> FindListAsync(Expression<Func<TModel, bool>> whereLambda, params Expression<Func<TModel, object>>[] includeProperties);

        /// <summary>
        /// 按条件查询列表,返回指定字段
        /// 查询部分字段,可指定关联属性
        /// example:
        /// FindListWithColumns(c => new { c.Id, VideoName = c.Video.Name, Form = c.Video });
        /// </summary>
        /// <param name="columnsLambda">返回哪些字段,类型须匿名类或DTO</param>
        /// <param name="whereLambda">可选参数,where条件</param>
        /// <returns></returns>
        Task<List<TModel>> FindListWithColumnsAsync(Expression<Func<TModel, object>> columnsLambda, Expression<Func<TModel, bool>> whereLambda = null);

        /// <summary>
        /// 按条件查询列表,以指定字段排序
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// FindListWithOrder(true, o => o.CreatedTime, null, o => o.Video);
        /// </summary>
        /// <typeparam name="TO">排序字段类型</typeparam>
        /// <param name="isAsc">true:ASC,false:DESC</param>
        /// <param name="orderLambda">排序字段</param>
        /// <param name="whereLambda">可选参数,where条件</param>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        Task<List<TModel>> FindListWithOrderAsync<TO>(bool isAsc, Expression<Func<TModel, TO>> orderLambda, Expression<Func<TModel, bool>> whereLambda = null,
            params Expression<Func<TModel, object>>[] includeProperties);

        /// <summary>
        /// 按条件查询列表,以指定字段排序,返回指定字段
        /// 查询部分字段,可指定关联属性
        /// example:
        /// FindListWithOrderColumns(true, o => o.CreatedTime, c => new { c.Id, c.Title }, c => c.Stage == 0);
        /// </summary>
        /// <typeparam name="TO">排序字段类型</typeparam>
        /// <param name="isAsc">true:ASC,false:DESC</param>
        /// <param name="orderLambda">排序字段</param>
        /// <param name="columnsLambda">返回哪些字段,类型须匿名类或DTO</param>
        /// <param name="whereLambda">where条件</param>
        /// <returns></returns>
        Task<List<TModel>> FindListWithOrderColumnsAsync<TO>(bool isAsc, Expression<Func<TModel, TO>> orderLambda,
            Expression<Func<TModel, object>> columnsLambda, Expression<Func<TModel, bool>> whereLambda = null);

        /// <summary>
        /// 按条件分页查询列表,以指定字段排序
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// FindPageListWithOrder(1, 10, out total, true, o => o.CreatedTime, w => w.Stage == 0, o => o.Video);
        /// </summary>
        /// <typeparam name="S">排序字段类型</typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="totalRecord">out参数,返回总记录数</param>
        /// <param name="isAsc">true:ASC,false:DESC</param>
        /// <param name="orderLambda">排序字段</param>
        /// <param name="whereLambda">可选参数,where条件</param>
        /// <param name="includeProperties">可选参数,需加载关联属性</param>
        /// <returns></returns>
        Task<(List<TModel>, int)> FindPageListWithOrderAsync<S>(int pageIndex, int pageSize, bool isAsc, Expression<Func<TModel, S>> orderLambda,
            Expression<Func<TModel, bool>> whereLambda = null, params Expression<Func<TModel, object>>[] includeProperties);

        /// <summary>
        /// 按条件分页查询列表,以指定字段排序,返回指定字段
        /// 查询部分字段,可指定关联属性
        /// example:
        /// FindPageListWithOrderColumns(1, 10, out total, true, o => o.CreatedTime, c => new { c.Id, c.Title, c.Video });
        /// </summary>
        /// <typeparam name="S">排序字段类型</typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="totalRecord">out参数,返回总记录数</param>
        /// <param name="isAsc">true:ASC,false:DESC</param>
        /// <param name="orderLambda">排序字段</param>
        /// <param name="columnsLambda">返回哪些字段,类型须匿名类或DTO</param>
        /// <param name="whereLambda">可选参数,where条件</param>
        /// <returns></returns>
        Task<(List<TModel>, int)> FindPageListWithOrderColumnsAsync<S>(int pageIndex, int pageSize, bool isAsc, Expression<Func<TModel, S>> orderLambda,
            Expression<Func<TModel, object>> columnsLambda, Expression<Func<TModel, bool>> whereLambda = null);

        //Dictionary<TKey, int> CountOfGroupBy<TKey>(Expression<Func<TModel, bool>> whereLambda, Expression<Func<TModel, TKey>> groupByLambda);

    }
}
