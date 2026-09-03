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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Explorer.Model;

namespace AvePoint.RA.SharePoint.RMExplorer.RMReclassifier
{
    public class RMSPReclassifierCache
    {
        readonly static object locker = new object();
        static RMSPReclassifierCache _instance;
        public static RMSPReclassifierCache Instance
        {
            get
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new RMSPReclassifierCache();
                    }
                }
                return _instance;
            }
        }

        public RMTerm Term { get; set; }
        public List<string> FolderDirPaths { get; internal set; }
        public void Init(ChangeTermDto dto)
        {
            _instance.Initialize(dto);
        }
        private void Initialize(ChangeTermDto dto)
        {
            AssembleTerm(dto);
        }

        private void AssembleTerm(ChangeTermDto dto)
        {
            ITermDao termDao = new TermDao();
            Term = termDao.GetRMTermByGuId(dto.TermInfo.UniqueId);
        }

        public bool ExistsParentFolderDirPath(string dirPath)
        {
            if (FolderDirPaths.Any(o => dirPath.StartsWith($"{o}/", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }
    }
}
