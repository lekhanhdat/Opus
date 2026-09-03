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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    #endregion

    public class SolutionFile
    {
        public String Name { get; set; }
        public Boolean IsChecked { get; set; }
        public String Version { get; set; }
        public Int64 CreateTime { get; set; }
        public String Description { get; set; }
        public String Path { get; set; }
        public TreeNodeLevel Type { get; set; }

        public override String ToString()
        {
            return String.Format("Name: {0}, Description: {1}, Path: {2}.",
                this.Name,
                this.Description,
                this.Path);
        }
    }
}