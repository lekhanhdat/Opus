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



namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    #endregion

    internal abstract partial class ExportServiceBase
        : IExportServiceEvents
    {
        ExportAction openingAction;
        ExportAction openedAction;
        ExportAction closingAction;
        ExportAction closedAction;
        ExportAction exportingAction;
        ExportAction exportedAction;

        public event ExportAction Opening
        {
            add { this.openingAction += value; }
            remove { this.openingAction -= value; }
        }
        public event ExportAction Opened
        {
            add { this.openedAction += value; }
            remove { this.openedAction -= value; }
        }
        public event ExportAction Closing
        {
            add { this.closingAction += value; }
            remove { this.closingAction -= value; }
        }
        public event ExportAction Closed
        {
            add { this.closedAction += value; }
            remove { this.closedAction -= value; }
        }
        public event ExportAction Exporting
        {
            add { this.exportingAction += value; }
            remove { this.exportingAction -= value; }
        }
        public event ExportAction Exported
        {
            add { this.exportedAction += value; }
            remove { this.exportedAction -= value; }
        }

        protected virtual void OnClosing(ExportEventArgs eventArgs)
        {
            this.FireEvent(this.closingAction, eventArgs);
        }

        protected virtual void OnClosed(ExportEventArgs eventArgs)
        {
            this.FireEvent(this.closedAction, eventArgs);
        }

        protected virtual void OnOpening(ExportEventArgs eventArgs)
        {
            this.FireEvent(this.openingAction, eventArgs);
        }

        protected virtual void OnOpened(ExportEventArgs eventArgs)
        {
            this.FireEvent(this.openedAction, eventArgs);
        }

        protected virtual void OnExporting(ExportEventArgs eventArgs)
        {
            this.FireEvent(this.exportingAction, eventArgs);
        }

        protected virtual void OnExported(ExportEventArgs eventArgs)
        {
            this.FireEvent(this.exportedAction, eventArgs);
        }

        void FireEvent(ExportAction action, ExportEventArgs eventArgs)
        {
            var temp = action;
            if (temp != null) temp(this, eventArgs);
        }

        void FireEvent(ExportEventType type)
        {
            var eventArgs = new ExportEventArgs();
            switch (type)
            {
                case ExportEventType.Opening:
                    this.OnOpening(eventArgs);
                    break;
                case ExportEventType.Opened:
                    this.OnOpened(eventArgs);
                    break;
                case ExportEventType.Closing:
                    this.OnClosing(eventArgs);
                    break;
                case ExportEventType.Closed:
                    this.OnClosed(eventArgs);
                    break;
                case ExportEventType.Exporting:
                    this.OnExporting(eventArgs);
                    break;
                case ExportEventType.Exported:
                    this.OnExported(eventArgs);
                    break;
                default:
                    break;
            }
        }
    }
}