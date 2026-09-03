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
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMTemplateRelationshipDao : IBaseDao<RMTemplateRelationship>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">unique id</param>
        /// <param name="idPathList">ancestor list of parent, include parent itself. first one sould be suite unique id, others are templates id(not unique id)</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        List<Guid> GetByParent(Guid parent, List<string> idPathList, int pageIndex, int pageCount, out int total);

        /// <summary>
        /// get the template number of certain template type based on the id path list
        /// </summary>
        /// <param name="idPathList">first one sould be suite unique id, others are templates id(not unique id)</param>
        /// <param name="templateType"></param>
        /// <returns></returns>
        int GetAncesstorCount(List<string> idPathList, TemplateType templateType);

        /// <summary>
        /// get the sub templates by parent, if subTypes has value, then will only return children of those types.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="idPathList"></param>
        /// <param name="subTypes"></param>
        /// <returns></returns>
        List<Guid> GetAllByParent(Guid parent, List<string> idPathList, List<TemplateType> subTypes = null);

        /// <summary>
        /// return the total children count
        /// </summary>
        /// <param name="parent">unique id</param>
        /// <param name="ancestorList">ancestor list of parent, include parent itself. first one sould be suite unique id, others are templates id(not unique id)</param>
        /// <returns></returns>
        int GetChildrenCount(Guid parent,List<string> ancestorList);

        bool AddRelationships(List<RMTemplateRelationship> relationships);

        /// <summary>
        /// return the suite unique id if template is the start from template of one suite.
        /// </summary>
        /// <param name="rootTemplateUniqueId">start template unique id</param>
        /// <returns>suite unique id or an empty guid</returns>
        Guid GetSuiteUniqueId(Guid rootTemplateUniqueId);

        /// <summary>
        /// get the start template unique id for a suite
        /// </summary>
        /// <param name="suiteUniqueId"></param>
        /// <returns>template unique id or an empty guid</returns>
        Guid GetStartTemplateUniqueId(Guid suiteUniqueId);

        /// <summary>
        /// check if the template is used as start template for a suite
        /// </summary>
        /// <param name="templateUniqueId"></param>
        /// <returns></returns>
        bool UsedAsStartTemplate(Guid templateUniqueId);

        /// <summary>
        /// check if the suite already has a start template of certain template type
        /// </summary>
        /// <param name="suiteUniqueId"></param>
        /// <param name="templateType"></param>
        /// <returns></returns>
        bool HasStartTemplate(Guid suiteUniqueId, TemplateType templateType);

        /// <summary>
        /// check if the template is under ancestorIdPath
        /// </summary>
        /// <param name="ancestorIdPathList">ancestor id path list, path is something like '6feecea2-2076-4557-ae9c-a90f9eb91617/1/', first part is suite id, the others are template id.</param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        bool Exists(string ancestorIdPath, int templateId);

        List<string> GetAllPathBySuite(Guid suiteId);

        /// <summary>
        /// check if the template with idpath is exist.
        /// </summary>
        /// <param name="idPath">path is something like '6feecea2-2076-4557-ae9c-a90f9eb91617/1/', first part is suite id, the others are template id.</param>
        /// <returns></returns>
        bool Exists(string idPath);
    }
}
