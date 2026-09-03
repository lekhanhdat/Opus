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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveNavigationNodeCollection : IEnumerable<IAveNavigationNode>, ICollection
    {
        IAveNavigationNode Parent { get; }
        IAveNavigationNode this[int index] { get; }

        IAveNavigationNode Add(AveNavigationNodeCreationInformation parameters);
        IAveNavigationNode Add(IAveNavigationNode node, IAveNavigationNode previousNode);
        IAveNavigationNode AddAsLast(IAveNavigationNode node);
        void Delete(IAveNavigationNode navNode);
        IAveNavigation Navigation { get; }
    }    

    public sealed class AveNavigationNodeCreationInformation
    {
        private bool masLastNode;
        private bool misExternal;
        private IAveNavigationNode mpreviousNode;
        private string mtitle;
        private string murl;     

        public bool AsLastNode
        {
            get
            {
                return this.masLastNode;
            }
            set
            {
                this.masLastNode = value;
            }
        }

        public bool IsExternal
        {
            get
            {
                return this.misExternal;
            }
            set
            {
                this.misExternal = value;
            }
        }

        public IAveNavigationNode PreviousNode
        {
            get
            {
                return this.mpreviousNode;
            }
            set
            {
                this.mpreviousNode = value;
            }
        }

        public string Title
        {
            get
            {
                return this.mtitle;
            }
            set
            {
                this.mtitle = value;
            }
        }

        public string Url
        {
            get
            {
                return this.murl;
            }
            set
            {
                this.murl = value;
            }
        }

    }
}
