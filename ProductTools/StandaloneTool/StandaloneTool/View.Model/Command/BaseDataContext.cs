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
using StandaloneTool.View.Model.Binding;

namespace StandaloneTool.View.Model.Command
{
    public class BaseDataContext
    {
        private readonly Lazy<ModelCommonInfo> modelCommonInfo = new();
        private static readonly Lazy<BaseDataContext> _instance = new();
        private readonly Lazy<NavigationOperator> _navigationOperator = new();
        private readonly Lazy<NextOperator> _nextOperator = new(() => new NextOperator(_instance.Value));
        private readonly Lazy<BackOperator> _backOperator = new(() => new BackOperator(_instance.Value));
        public BaseDataContext() { }

        public ModelCommonInfo ModelCommonInfo => modelCommonInfo.Value;
        public static BaseDataContext Instance => _instance.Value;
        public NavigationOperator NavigationOperator => _navigationOperator.Value;
        public NextOperator NextOperator => _nextOperator.Value;
        public BackOperator BackOperator => _backOperator.Value;

    }
}
