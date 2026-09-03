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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public interface IAveViewCollection : ICollection, IEnumerable<IAveView>
    {
        IAveView Add(AveViewCreationInformation parameters);
        IAveView Add(string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, AveViewType type, bool bPersonalView);
        IAveView GetById(Guid guidId);
        IAveView GetByTitle(string strTitle);
        XmlNode GetViewCollection(string listName);

        IAveView DefaultView { get; }
        IAveView this[string strTitle] { get; }
        IAveView this[Guid guid] { get; }
        IAveView this[int index] { get; }
        IAveList List { get; }

        IAveView Add(string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault);
    }

    public sealed class AveViewCreationInformation
    {     
        private bool mpaged;
        private bool mpersonalView;
        private string mquery;
        private uint mrowLimit;
        private bool msetAsDefaultView;
        private string mtitle;
        private string[] mviewFields;
        private string mtypeId;
        private AveViewType mviewTypeKind;
     
        public AveViewCreationInformation()
        {
        }
        
        public bool Paged 
        {
            get
            {
                return mpaged;
            }
            set
            {
                mpaged = value;
            }
        }

        public bool PersonalView 
        {
            get
            {
                return mpersonalView;
            }
            set
            {
                mpersonalView = value;
            }
        }

        public string Query
        {
            get
            {
                return mquery;
            }
            set
            {
                mquery = value;
            }
        }

        public uint RowLimit 
        {
            get
            {
                return mrowLimit;
            }
            set
            {
                mrowLimit = value;
            }
        }

        public bool SetAsDefaultView 
        {
            get
            {
                return msetAsDefaultView;
            }
            set
            {
                msetAsDefaultView = value;
            }
        }

        public string Title 
        {
            get
            {
                return mtitle;
            }
            set
            {
                mtitle = value;
            }
        }

        public string TypeId 
        { 
            get 
            { 
                return mtypeId; 
            } 
        }

        public string[] ViewFields 
        {
            get
            {
                return mviewFields;
            }
            set
            {
                mviewFields = value;
            }
        }

        public AveViewType ViewTypeKind 
        {
            get
            {
                return mviewTypeKind;
            }
            set
            {
                mviewTypeKind = value;
            }
        }
    }

    public enum AveViewType
    {
        Calendar = 0x80000,
        Chart = 0x20000,
        Gantt = 0x4000000,
        Grid = 0x800,
        Html = 1,
        None = 0,
        Recurrence = 0x2000,
        LockWeb = 0x10
    }
}
