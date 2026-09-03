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



using Microsoft.Office.InfoPath.Server.Administration;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOConverterMessageCollection : AveAbstractCommonCollection<IAveOConverterMessage>, IAveOConverterMessageCollection
    {
        private ConverterMessageCollection mConverterMessageCollection;

        public AveOConverterMessageCollection(ConverterMessageCollection converterMessageCollection)
            : base(converterMessageCollection)
        {
            mConverterMessageCollection = converterMessageCollection;
        }

        public override IAveOConverterMessage this[int index]
        {
            get
            {
                ConverterMessage converterMessage = mConverterMessageCollection[index];
                if (converterMessage == null)
                {
                    return null;
                }
                return new AveOConverterMessage(converterMessage);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveOConverterMessage(t as ConverterMessage);
        }

        public override int Count
        {
            get { return mConverterMessageCollection.Count; }
        }


        public bool ContainsFailure
        {
            get { return mConverterMessageCollection.ContainsFailure; }
        }
    }
}
