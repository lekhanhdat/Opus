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
import * as React from "react";
import { Dialog, DialogType } from '@fluentui/react';
import * as strings from "OpusCustomizationStrings";

interface IAppUpdateDialogProps {
}

interface IAppUpdateDialogState {
  hide: boolean,
}

export default class AppUpdateDialog extends React.Component<IAppUpdateDialogProps, IAppUpdateDialogState> {
  constructor(props: IAppUpdateDialogProps) {
    super(props);
    this.state = {
      hide: false
    };
  }

  public render(): React.ReactElement<{}> {
    return (
      <Dialog
        hidden={this.state.hide}
        onDismiss={() => { this.setState({ hide: true }) }}
        dialogContentProps={{
          type: DialogType.normal,
          title: strings.JPMC_App_Classify,
          subText: strings.JPMC_App_Msg_Classify_NeedUpdateApp,
          showCloseButton: true
        }}
      />
    );
  }
}
