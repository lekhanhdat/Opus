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




namespace AvePoint.RA.Contract.Global.Object
{
    #region using directives
    using System.Runtime.Serialization;
    #endregion

   
    public class PolicyValue
    {
        private string value1;
        private PolicyValueUnit value1Unit;
        private string value2;
        private PolicyValueUnit value2Unit;

        [DataMember]
        public string Value1
        {
            get { return value1; }
            set { value1 = value; }
        }

        [DataMember]
        public PolicyValueUnit Value1Unit
        {
            get { return value1Unit; }
            set { value1Unit = value; }
        }

        [DataMember]
        public string Value2
        {
            get { return value2; }
            set { value2 = value; }
        }

        [DataMember]
        public PolicyValueUnit Value2Unit
        {
            get { return value2Unit; }
            set { value2Unit = value; }
        }

        /// <summary>
        /// this extension is used for storing value extension, like CA search user and group permission settings, filter's timezoneid
        /// </summary>
        [DataMember]
        public Extention Extension { get; set; }

        public PolicyValue()
            : this(string.Empty)
        {
        }

        public PolicyValue(string value1)
            : this(value1, string.Empty)
        {
        }

        public PolicyValue(string value1, string value2)
            : this(value1, PolicyValueUnit.None, value2, PolicyValueUnit.None)
        {
        }

        public PolicyValue(string value1, PolicyValueUnit unit1)
            : this(value1, unit1, string.Empty, PolicyValueUnit.None)
        {
        }

        public PolicyValue(string value1, PolicyValueUnit unit1, string value2, PolicyValueUnit unit2)
        {
            this.value1 = value1;
            this.value1Unit = unit1;
            this.value2 = value2;
            this.value2Unit = unit2;

        }

    }


    public class Extention
    {
        /// <summary>
        /// CA用于存储时间类型的Filter的时区ID
        /// </summary>
        [DataMember]
        public string TimeZoneId { get; set; }
        /// <summary>
        /// 保存夏令时
        /// </summary>
        [DataMember]
        public bool isDST { get; set; }
    }
}
