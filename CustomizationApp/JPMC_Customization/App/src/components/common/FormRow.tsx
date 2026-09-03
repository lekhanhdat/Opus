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
import * as React from 'react';
import { TooltipHost, ITooltipHostStyles } from '@fluentui/react/lib/Tooltip';
import { Label } from '@fluentui/react';
export interface IFormRowProps {
  required?: boolean;
  labelName: string;
  value?: string;
}

let rowItemMaxId = 0;
export class FormRow extends React.Component<IFormRowProps, {}> {
  constructor(props: IFormRowProps) {
    super(props);
  }

  private renderRowValue = () => {
    if(!this.props.children && !!this.props.value) {
      const calloutProps = { gapSpace: 0 };
      const hostStyles: Partial<ITooltipHostStyles> = { root: { display: 'inline-block' } };
      let tooltipId = "rowVal_" + rowItemMaxId++;
      return <TooltipHost
        content={this.props.value}
        id={tooltipId}
        calloutProps={calloutProps}
        styles={hostStyles}
      >
        <div aria-describedby={tooltipId}>{this.props.value}</div>
      </TooltipHost>;
    } else {
      return <div title={this.props.value}>{!this.props.children ? this.props.value : this.props.children}</div>;
    }
  }

  public render(): React.ReactElement<{}> {
    return (
      <div className={"styles.FormRow"}>
        <Label className={this.props.required ? "styles.FormRequired" : ""}>{this.props.labelName}</Label>
        {this.renderRowValue()}
      </div>
    );
  }
}
