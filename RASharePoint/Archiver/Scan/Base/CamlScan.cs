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
using AvePoint.GCommon;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class CamlScan
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public const string SP_ID = "ID";
        public const int CamlInConditionArrayValuesMaxCount = 100;
        public CAMLManager InitCamlQuery(IAveList list, IAveFieldCollection listFields, RuleItemCollection checkerColl, DateTime timePoint, bool includeRecords)
        {
            CAMLManager cm = new CAMLManager(Types.ScopeTypes.FilesOnly);
            logger.Info("Begin to init caml query for list {0} , Time {1}", list.Title, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));

            QueryGroup group = null;
            var groupFactory = new QueryGroupFactory(
                QueryGroupFactoryType.ArchiverScan,
                checkerColl,
                listFields,
                timePoint
                );
            group = groupFactory.GetQueryGroupByRuleCheckerCollection(includeRecords);

            if (group != null && (group.Conditions.Count != 0 || group.Groups.Count != 0))
            {
                cm.QueryGroup.AddGroup(group);
            }

            logger.Info("Init caml query finished, Time {0}.", DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));

            if (cm.QueryGroup.Groups.Count > 0)
            {
                return cm;
            }
            else
            {
                return null;
            }
        }

        public List<IAveListItem> QueryAllItemsInFolder(CAMLManager cm, IAveList list, string folderUrl, int rowLimit, int maxId)
        {
            int startIndex = 0;
            int endIndex = 0;
            AveCamlQuery query = new AveCamlQuery();
            List<IAveListItem> tempAllItems = new List<IAveListItem>();
            cm.ScopeType = Types.ScopeTypes.FilesOnly;
            cm.RowLimit = rowLimit;
            query.DatesInUtc = true;
            query.FolderServerRelativeUrl = folderUrl;
            int executeCount = 0;
            logger.Info($"Start to query files in :{folderUrl}");
            do
            {
                endIndex = startIndex + rowLimit > maxId ? maxId : startIndex + rowLimit;
                cm.QueryGroup.Conditions.RemoveAll(g => g.Query.Field == SP_ID);
                cm.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Gt, startIndex.ToString()));
                cm.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Leq, endIndex.ToString()));
                string queryXml = cm.GetFullCAML();
                query.ViewXml = queryXml;
                IAveListItemCollection items = list.GetItems(query);
                executeCount++;
                tempAllItems.AddRange(items);
                if (startIndex + rowLimit < maxId)
                {
                    startIndex = startIndex + rowLimit;
                }
                else if (startIndex + rowLimit > maxId && endIndex < maxId)
                {
                    startIndex = maxId - endIndex;
                }
                else
                {
                    break;
                }
            }
            while (true);
            logger.Info("SPQuery xml {0}:{1}, query execute count:{2} item count:{3}", folderUrl, cm.GetFullCAML(), executeCount, tempAllItems.Count);
            //IAveListItemCollection items = list.GetItems(query);
            //tempAllItems.AddRange(items);
            //IAveListItemCollectionPosition position = items.ListItemCollectionPosition;

            //while (position != null)
            //{
            //    query.ListItemCollectionPosition.PagingInfo = position.PagingInfo;
            //    IAveListItemCollection tempItems = list.GetItems(query);
            //    position = tempItems.ListItemCollectionPosition;
            //    tempAllItems.AddRange(tempItems);
            //}
            return tempAllItems;
        }

        public CAMLManager InitCamlQuery(IAveList list, DateTime timePoint, List<KeyValuePair<RuleItemCollection, List<int>>> termRuleQuerys)
        {
            IAveTimeZone spWebTimeZone = list.ParentWeb.RegionalSettings.TimeZone;
            IAveFieldCollection listFields = list.Fields;
            IAveTaxonomyField taxonomyField = list.Fields.GetFieldById(ScanDataCache.Instance.SiteLevelCache.BCSColumnID, false) as IAveTaxonomyField;

            logger.Info("Begin to init caml query for list {0} , Time {1}", list.Title, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));

            List<CAMLManager> cms = new();
            CAMLManager cm = new();
            foreach (var termRuleQuery in termRuleQuerys)
            {
                logger.Info($"[CamlQuery4TermRule]Query CAML terms wssid is: {string.Join(",", termRuleQuery.Value)}");
                var groupFactory = new QueryGroupFactory(
                                   QueryGroupFactoryType.DisposalScan,
                                   termRuleQuery.Key,
                                   listFields,
                                   spWebTimeZone,
                                   null,//SP Source，Rule中时间条件和BeforeReportTime都是UTC，不需要传RegionSetting
                                   timePoint,
                                   taxonomyField.InternalName,
                                   termRuleQuery.Value);
                var group = groupFactory.GetQueryGroupByRuleCheckerCollection();
                if (group != null && (group.Conditions.Count != 0 || group.Groups.Count != 0))
                {
                    cm.QueryGroup.AddGroup(group);
                }

            }
            logger.Info("Init caml query finished, Time {0}.", DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
            if (cm.QueryGroup.Groups.Count > 0)
            {
                return cm;
            }
            else
            {
                return null;
            }
        }

        public CAMLManager InitCamlQuery4Unclassification(IAveList list, DateTime timePoint, RuleItemCollection termRuleQuerys)
        {
            IAveTimeZone spWebTimeZone = list.ParentWeb.RegionalSettings.TimeZone;
            IAveFieldCollection listFields = list.Fields;
            IAveTaxonomyField taxonomyField = list.Fields.GetFieldById(ScanDataCache.Instance.SiteLevelCache.BCSColumnID, false) as IAveTaxonomyField;

            logger.Info("Begin to init caml query for unclassification for list {0} , Time {1}", list.Title, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
            CAMLManager cm = new();
            var groupFactory = new QueryGroupFactory(
                               QueryGroupFactoryType.DisposalScan,
                               termRuleQuerys,
                               listFields,
                               spWebTimeZone,
                               null,//SP Source，Rule中时间条件和BeforeReportTime都是UTC，不需要传RegionSetting
                               timePoint,
                               taxonomyField.InternalName);
            var group = groupFactory.GetQueryGroupByRuleCheckerCollection4UnClassification();
            if (group != null && (group.Conditions.Count != 0 || group.Groups.Count != 0))
            {
                cm.QueryGroup.AddGroup(group);
            }

            logger.Info("Init caml query for unclassification finished, Time {0}.", DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
            if (cm.QueryGroup.Groups.Count > 0) return cm;
            else return null;
        }
    }
}
