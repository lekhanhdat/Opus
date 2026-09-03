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

namespace SqlKata.Compilers
{
    public class CteFinder
    {
        private readonly Query query;
        private readonly string engineCode;
        private HashSet<string> namesOfPreviousCtes;
        private List<AbstractFrom> orderedCteList;

        public CteFinder(Query query, string engineCode)
        {
            this.query = query;
            this.engineCode = engineCode;
        }

        public List<AbstractFrom> Find()
        {
            if (null != orderedCteList)
                return orderedCteList;

            namesOfPreviousCtes = new HashSet<string>();

            orderedCteList = findInternal(query);

            namesOfPreviousCtes.Clear();
            namesOfPreviousCtes = null;

            return orderedCteList;
        }

        private List<AbstractFrom> findInternal(Query queryToSearch)
        {
            var cteList = queryToSearch.GetComponents<AbstractFrom>("cte", engineCode);

            var resultList = new List<AbstractFrom>();

            foreach (var cte in cteList)
            {
                if (namesOfPreviousCtes.Contains(cte.Alias))
                    continue;

                namesOfPreviousCtes.Add(cte.Alias);
                resultList.Add(cte);

                if (cte is QueryFromClause queryFromClause)
                {
                    resultList.InsertRange(0, findInternal(queryFromClause.Query));
                }
            }

            return resultList;
        }
    }
}
