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
import { ExtensionContext } from '@microsoft/sp-extension-base';
import { Logger } from "../common/Logger";
import * as strings from 'OpusCustomizationStrings';


export interface IDeclareUtilProps {
  context: ExtensionContext;
  listId: string;
  itemId: number;
}

export interface IDeclaredResult {
  success: boolean;
  message?: string;
}

const iframeIdFixedPart = new Date().getTime();
let iframeIdIndex = 0;
export class DeclareUtil {
  private props: IDeclareUtilProps;
  private refIframe: any;
  private iframeId: any;
  private toggleRecordStatus: any;
  private inProcessing: boolean;
  private isDeclare: boolean;
  private onInitializedCallback: any;
  private onToggledCallback: (result: IDeclaredResult) => void;

  constructor(props: IDeclareUtilProps) {
    this.props = props;

  }

  public init(): Promise<void> {

    iframeIdIndex++;
    this.iframeId = `opusIframe${iframeIdFixedPart}${iframeIdIndex}`;

    this.refIframe = document.createElement("iframe");
    this.refIframe.setAttribute("src", this.getFrameUrl());
    this.refIframe.setAttribute("id", this.iframeId);
    this.refIframe.style.width = "0";
    this.refIframe.style.height = "0";
    this.refIframe.onload = this.onIframeLoad;
    document.body.appendChild(this.refIframe);

    return new Promise((resolve) => {
      this.onInitializedCallback = resolve;
    });
  }

  private getFrameUrl() {
    let webUrl = this.props.context.pageContext.web.absoluteUrl;
    return `${webUrl}/_layouts/15/itemexpiration.aspx?ID=${this.props.itemId}&List=%7b${this.props.listId}%7d`;
  }

  private onIframeLoad = () => {
    let iframeWindow = this.refIframe.contentWindow;

    if (iframeWindow && iframeWindow.ToggleRecordStatus) {
      this.toggleRecordStatus = iframeWindow.ToggleRecordStatus.bind(iframeWindow);
    }
    iframeWindow.ToggleRecordStatus = this.onToggleRecordStatusSuccess;
    iframeWindow.ToggleRecordStatusErr = this.onToggleRecordStatusFailed;

    this.onInitializedCallback();
  }

  private onToggleRecordStatusSuccess = (result: any, context: any) => {
    let bIsRecd = (result == "true");
    this.inProcessing = false;
    this.toggleRecordStatus(result, context);

    let retObj: IDeclaredResult = { success: false };
    if (bIsRecd && !this.isDeclare) {
      retObj.message = strings.JPMC_App_Msg_Error_Undeclared;
    } else if (!bIsRecd && this.isDeclare) {
      retObj.message = strings.JPMC_App_Msg_Error_Declared;
    } else {
      retObj.success = true;
    }

    this.onToggledCallback(retObj);
  }

  private onToggleRecordStatusFailed = (result: any, context: any) => {
    this.inProcessing = false;
    this.onToggledCallback({
      success: false,
      message: this.refIframe.contentWindow.bIsRecd ? strings.JPMC_App_Msg_Error_Undeclared : strings.JPMC_App_Msg_Error_Declared
    });
  }

  public allowDeclareAsRecord = () => {
    return this.refIframe.contentWindow.hasDeclareRight;
  }

  public declareAsRecord = (): Promise<IDeclaredResult> => {
    if (this.inProcessing) {
      throw new Error("Previous operation is in processing.");
    }

    let iframeWindow = this.refIframe.contentWindow;
    if (iframeWindow.bIsRecd) {
      Logger.warn("Already declared as record. " + this.props.itemId);
      return Promise.resolve({success: true});
    }
    if (!iframeWindow.hasDeclareRight) {
      Logger.warn("No permission declared as record. " + this.props.itemId);
      return Promise.resolve({success: true, message: strings.JPMC_App_Msg_Action_NoPermission_Declared});
    }

    this.inProcessing = true;
    this.isDeclare = true;
    return new Promise((resolve) => {
      this.onToggledCallback = resolve;
      this.refIframe.contentWindow.ChangeRecordStatus();
    });
  }

  public undeclaredRecord = () : Promise<IDeclaredResult> => {
    if (this.inProcessing) {
      throw new Error("Previous operation is in processing.");
    }

    if (this.refIframe?.contentDocument?.location?.href?.indexOf("AccessDenied.aspx") > -1) {
      Logger.warn("No permission undeclared record. " + this.props.itemId);
      return Promise.resolve({ success: false, message: strings.JPMC_App_Msg_Action_NoPermission_Undeclared });
    } else {
      let iframeWindow = this.refIframe.contentWindow;
      if (!iframeWindow.bIsRecd) {
        Logger.warn("Already undeclared. " + this.props.itemId);
        return Promise.resolve({ success: true });
      }
      if (!iframeWindow.hasUndeclareRight) {
        Logger.warn("No permission undeclared record. " + this.props.itemId);
        return Promise.resolve({ success: false, message: strings.JPMC_App_Msg_Action_NoPermission_Undeclared });
      }
    }

    this.inProcessing = true;
    this.isDeclare = false;
    return new Promise((resolve) => {
      this.onToggledCallback = resolve;
      this.refIframe.contentWindow.ChangeRecordStatus();
    });
  }

  public dispose() {
    this.refIframe.remove();
    this.refIframe = null;
  }
}
