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
import { Log } from '@microsoft/sp-core-library';

const LOG_SOURCE_OpusActions: string = "OpusActions";
export const LOG_SOURCE_EndDateCustomizer: string = "EndDateCustomizer";
export const LOG_SOURCE_StartDateCustomizer: string = "StartDateCustomizer";

export class Logger {
  public static info(message: string, source?: string): void {
    Log.info(!source ? LOG_SOURCE_OpusActions : source, message);
  }

  public static warn(message: string, source?: string) {
    Log.warn(!source ? LOG_SOURCE_OpusActions : source, message);
  }

  public static error(error: Error, message?: string, source?: string): void {
    if (message && error) {
      error.message = `${message} ${error.message}
${error.stack}`;
    }
    Log.error(!source ? LOG_SOURCE_OpusActions : source, error);
  }
}

