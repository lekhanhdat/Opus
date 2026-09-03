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
using System.IO;
namespace AvePoint.RA.SharePoint.Object
{
    public class PhysicalItemInfo
    {
        public PhysicalItemType Type
        {
            get
            {
                if (string.IsNullOrEmpty(PhysicalRecordName))
                {
                    return PhysicalItemType.PhysicalFile;
                }
                else
                {
                    return PhysicalItemType.PhysicalRecord;
                }
            }
        }

        public string PhysicalFileName { get; set; }

        public string PhysicalRecordName
        {
            get
            {
                return physicalRecordName;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || string.Equals(Path.GetExtension(value), RECORD_EXTENSION, StringComparison.OrdinalIgnoreCase))
                {
                    physicalRecordName = value;
                }
                else
                {
                    physicalRecordName = value + RECORD_EXTENSION;
                }
            }
        }

        public string LifecycleStatus { get; set; }

        public string HomeLocation { get; set; }

        public string BoxName { get; set; }

        public string BusinessClassification { get; set; }

        public DateTime DateOpened { get; set; }

        public DateTime DateClosed { get; set; }

        public Availability Availability { get; set; }

        public string CurrentlyHeldBy { get; set; }

        private string physicalRecordName = string.Empty;
        private const string RECORD_EXTENSION = ".txt";

        public override string ToString()
        {
            return string.Format("Type:[{0}],PhysicalFileName:[{1}],PhysicalRecordName:[{2}],LifecycleStatus:[{3}],HomeLocation:[{4}],BoxName:[{5}],BusinessClassification:[{6}],DateOpened:[{7}],DateClosed:[{8}],Availability:[{9}],CurrentlyHeldBy:[{10}]"
                , Type.ToString(), PhysicalFileName, PhysicalRecordName, LifecycleStatus.ToString(), HomeLocation, BoxName, BusinessClassification, DateOpened.ToString(), DateClosed.ToString(), Availability.ToString(), CurrentlyHeldBy);
        }
    }

    public enum PhysicalItemType
    {
        None,
        //Folder
        PhysicalFile,
        //File
        PhysicalRecord
    }

    public enum LifecycleStatus
    {
        None,
        Open,
        Closed,
        PendingDestruction,
        Destroyed
    }

    public enum Availability
    {
        None,
        Available,
        Loaned,
        Missing
    }
}
