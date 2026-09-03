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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveUserSolutionCollection : AveAbstractCommonCollection<IAveUserSolution>, IAveUserSolutionCollection
    {
        private IAveRequest mRequest;

        public AveUserSolutionCollection(IAveRequest request, Dictionary<string, object> solutionColProperties)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(solutionColProperties);
            InitSolutionCollection();
        }

        internal void InitSolutionCollection()
        {
            var solutionPropertiesList = base.DataCache.GetChildren();
            if (solutionPropertiesList != null)
            {
                mListData = new List<IAveUserSolution>(solutionPropertiesList.Count);
                foreach (var solutionProperties in solutionPropertiesList)
                {
                    AveUserSolution userSolution = new AveUserSolution(mRequest, solutionProperties);
                    mListData.Add(userSolution);
                }
            }
            else
            {
                mListData = new List<IAveUserSolution>();
            }
        }

        #region IAveUserSolutionCollection Members

        public IAveUserSolution this[Guid solutionId]
        {
            get 
            {
                return mListData.Find(s => s.SolutionId == solutionId);
            }
        }

        public void Remove(IAveUserSolution solution)
        {
            var package = new AveDesignPackageInfo()
            {
                MajorVersion = 1,
                MinorVersion = 1,
                PackageGuid = solution.SolutionId,
                PackageName = solution.Name
            };
            mRequest.UnInstallDesignPackage(package);
            mListData.Remove(solution);
        }

        #endregion


        //public IAveUserSolution Add(int p)
        //{
        //    var solutionProperties = mRequest.OperateOnSolution("ACT", p);
        //    return new AveUserSolution(mRequest, solutionProperties);
        //}


        public IAveUserSolution Add(IAveListItem solutionItem, AveDesignPackageInfo package)
        {
            //if (mRequest.TokenProvider.TokenType == Office365.Api.TokenType.Bearer)
            //{
            mRequest.InstallDesignPackage(package, solutionItem.File.ServerRelativeUrl);
            //}
            //var solutionProperties = mRequest.OperateOnSolution("ACT", solutionItem.ID);
            var solutionProperties = mRequest.LoadSolution(solutionItem.ID);
            return new AveUserSolution(mRequest, solutionProperties);
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
