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


namespace AvePoint.Wrapper.Common
{
    using System;

    public class CabinetProgressEventArgs : EventArgs
    {
        private long currentCabinetBytesProcessed;
        private string currentCabinetName;
        private short currentCabinetNumber;
        private long currentCabinetTotalBytes;
        private long currentFileBytesProcessed;
        private string currentFileName;
        private int currentFileNumber;
        private long currentFileTotalBytes;
        private long currentFolderBytesProcessed;
        private short currentFolderNumber;
        private long currentFolderTotalBytes;
        private long fileBytesProcessed;
        private CabinetProgressType progressType;
        private short totalCabinets;
        private long totalFileBytes;
        private int totalFiles;

        internal CabinetProgressEventArgs()
        {
        }

        public CabinetProgressEventArgs(CabinetProgressType progressType, string currentFileName, int currentFileNumber, int totalFiles, long currentFileBytesProcessed, long currentFileTotalBytes, int currentFolderNumber, long currentFolderBytesProcessed, long currentFolderTotalBytes, string currentCabinetName, int currentCabinetNumber, int totalCabinets, long currentCabinetBytesProcessed, long currentCabinetTotalBytes, long fileBytesProcessed, long totalFileBytes) : this()
        {
            this.progressType = progressType;
            this.currentFileName = currentFileName;
            this.currentFileNumber = currentFileNumber;
            this.totalFiles = totalFiles;
            this.currentFileBytesProcessed = currentFileBytesProcessed;
            this.currentFileTotalBytes = currentFileTotalBytes;
            this.currentFolderNumber = (short) currentFolderNumber;
            this.currentFolderBytesProcessed = currentFolderBytesProcessed;
            this.currentFolderTotalBytes = currentFolderTotalBytes;
            this.currentCabinetName = currentCabinetName;
            this.currentCabinetNumber = (short) currentCabinetNumber;
            this.totalCabinets = (short) totalCabinets;
            this.currentCabinetBytesProcessed = currentCabinetBytesProcessed;
            this.currentCabinetTotalBytes = currentCabinetTotalBytes;
            this.fileBytesProcessed = fileBytesProcessed;
            this.totalFileBytes = totalFileBytes;
        }

        public long CurrentCabinetBytesProcessed
        {
            get
            {
                return this.currentCabinetBytesProcessed;
            }
            internal set
            {
                this.currentCabinetBytesProcessed = value;
            }
        }

        public string CurrentCabinetName
        {
            get
            {
                return this.currentCabinetName;
            }
            internal set
            {
                this.currentCabinetName = value;
            }
        }

        public int CurrentCabinetNumber
        {
            get
            {
                return this.currentCabinetNumber;
            }
            internal set
            {
                this.currentCabinetNumber = (short) value;
            }
        }

        public long CurrentCabinetTotalBytes
        {
            get
            {
                return this.currentCabinetTotalBytes;
            }
            internal set
            {
                this.currentCabinetTotalBytes = value;
            }
        }

        public long CurrentFileBytesProcessed
        {
            get
            {
                return this.currentFileBytesProcessed;
            }
            internal set
            {
                this.currentFileBytesProcessed = value;
            }
        }

        public string CurrentFileName
        {
            get
            {
                return this.currentFileName;
            }
            internal set
            {
                this.currentFileName = value;
            }
        }

        public int CurrentFileNumber
        {
            get
            {
                return this.currentFileNumber;
            }
            internal set
            {
                this.currentFileNumber = value;
            }
        }

        public long CurrentFileTotalBytes
        {
            get
            {
                return this.currentFileTotalBytes;
            }
            internal set
            {
                this.currentFileTotalBytes = value;
            }
        }

        public long CurrentFolderBytesProcessed
        {
            get
            {
                return this.currentFolderBytesProcessed;
            }
            internal set
            {
                this.currentFolderBytesProcessed = value;
            }
        }

        public int CurrentFolderNumber
        {
            get
            {
                return this.currentFolderNumber;
            }
            internal set
            {
                this.currentFolderNumber = (short) value;
            }
        }

        public long CurrentFolderTotalBytes
        {
            get
            {
                return this.currentFolderTotalBytes;
            }
            internal set
            {
                this.currentFolderTotalBytes = value;
            }
        }

        public long FileBytesProcessed
        {
            get
            {
                return this.fileBytesProcessed;
            }
            internal set
            {
                this.fileBytesProcessed = value;
            }
        }

        public CabinetProgressType ProgressType
        {
            get
            {
                return this.progressType;
            }
            internal set
            {
                this.progressType = value;
            }
        }

        public int TotalCabinets
        {
            get
            {
                return this.totalCabinets;
            }
            internal set
            {
                this.totalCabinets = (short) value;
            }
        }

        public long TotalFileBytes
        {
            get
            {
                return this.totalFileBytes;
            }
            internal set
            {
                this.totalFileBytes = value;
            }
        }

        public int TotalFiles
        {
            get
            {
                return this.totalFiles;
            }
            internal set
            {
                this.totalFiles = value;
            }
        }
    }
}

