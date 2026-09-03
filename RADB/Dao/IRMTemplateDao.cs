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
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMTemplateDao : IBaseDao<RMTemplate>
    {
        Task<int> ResetDefaultDataAsync();
        Task<int> InitDefaultDataAsync();
        List<RMTemplateCategory> LoadCategories(Guid templateId);
        RMTemplate GetTemplateByTemplateType(TemplateType type);
        RMTemplate GetTemplateById(int id);
        RMTemplate GetTemplateByUniqueId(Guid uniquIid);
        List<RMTemplate> GetChildrenTemplateByParentID(Guid parentID);
        bool SaveTemplateWithColumns(TemplateDto dto);
        List<RMTemplate> GetTemplate();
        /// <summary>
        /// get templates with some fields, e.g, name,id,uniqueId, type
        /// </summary>
        /// <returns></returns>
        List<SimplifyTemplateDto> GetAllSimplifyTemplates();
        List<RMTemplate> GetTemplateByType(TemplateType type);
        List<RMTemplate> GetTemplateByIds(List<int> ids);
        List<RMTemplate> GetTemplateByUniqueIds(List<Guid> uinqueIds);
        RMTemplate GetTemplateByName(string templateName);
        void DeleteSuite(Guid suiteId);
        //List<RMTemplate> GetTemplatesByParent(SuiteTemplateQueryDto queryDto, out int totalCount);
        //List<RMTemplate> GetChildTemplatesByParent(SuiteTemplateQueryDto queryDto,bool isBrowseFold = false);
        List<RMTemplate> GetAllSubTemplateBySuiteId(Guid suiteId);
        /// <summary>
        /// 在删除时，如果此template没有被add到其他地方，那么会真正删除，否则只是解除此template与其父idPathList中的templates之间的关系，不会真正删除
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="idPathList">first one is suite unique id, others are template id(not unique id)</param>
        void DeleteTemplate(Guid templateId, List<string> idPathList);
        /// <summary>
        /// build the template relation with its ancestors
        /// </summary>
        /// <param name="ancestorTemplateIdList">template id list starting from suite. first one is suite unique id, others are template id(not unique id)</param>
        /// <param name="templateId"></param>
        void AddTemplateRelatonship(List<string> ancestorTemplateIdList, int templateId);
    }
}
