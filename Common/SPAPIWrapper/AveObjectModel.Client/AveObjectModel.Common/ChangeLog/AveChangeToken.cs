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

using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveChangeToken : AveClientObject, IAveChangeToken
    {
        public string StringValue
        {
            get { return this.DataCache.GetProperty<string>("StringValue"); }
            set { this.DataCache.AddProperty("StringValue",value); }
        }

        public AveCollectionScope Scope
        {
            get { return (AveCollectionScope)this.DataCache.GetProperty<int>("Scope"); }
            set { this.DataCache.AddProperty("Scope",(int)value); }
        }

        public Guid ScopeId
        {
            get { return this.DataCache.GetProperty<Guid>("ScopeId"); }
            set { this.DataCache.AddProperty("ScopeId",value); }
        }

        public int Version
        {
            get { return this.DataCache.GetProperty<int>("Version"); }
            set { this.DataCache.AddProperty("Version",value); }
        }

        public long Number 
        {
            get { return this.DataCache.GetProperty<long>("Number"); }
            set { this.DataCache.AddProperty("Number",value); }            
        }

        public DateTime ChangeTime 
        {
            get { return this.DataCache.GetProperty<DateTime>("ChangeTime"); }
            set { this.DataCache.AddProperty("ChangeTime",value); }     
        }

        public AveChangeToken(string strChangeToken)
        {
            this.DataCache.AddProperty("StringValue",strChangeToken);
            ParseChangeToken(strChangeToken);
        }

        public AveChangeToken(AveCollectionScope scope, Guid scopeId, DateTime changeTime)
        {
            string mStringValue = string.Format("1;{0};{1};{2};-1", (int)scope, scopeId.ToString(), changeTime.Ticks.ToString());
            ParseChangeToken(mStringValue);
            this.DataCache.AddProperty("StringValue",mStringValue);
        }

        private void ParseChangeToken(string strChangeToken)
        {
            string[] changeTokenParameters = strChangeToken.Split(new char[] { ';' });
            int i = 0;
            for (; i < changeTokenParameters.Length; i++) 
            {
                switch (i)
                {
                    case 0:
                        this.Version = int.Parse(changeTokenParameters[i], CultureInfo.InvariantCulture);
                        break;

                    case 1:
                        this.Scope = (AveCollectionScope)int.Parse(changeTokenParameters[i], CultureInfo.InvariantCulture);
                        break;

                    case 2:
                        this.ScopeId = new Guid(changeTokenParameters[i]);
                        break;

                    case 3:
                        this.ChangeTime = new DateTime(long.Parse(changeTokenParameters[i], CultureInfo.InvariantCulture));
                        break;

                    case 4:
                        this.Number = long.Parse(changeTokenParameters[i], CultureInfo.InvariantCulture);
                        break;
                }
            }
            if (i != 5)
            {
                throw new InvalidOperationException();
            }
            if (this.Version != 1)
            {
                throw new InvalidOperationException();
            }
        }

        public override string ToString()
        {
            return StringValue;
        }
    }
}
