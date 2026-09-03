import React, { Fragment } from "react";
import PropTypes from "prop-types";
import "./index.less";

import { SourceFlag } from "../../Common/Constants";

const ColorLabelValue = ({ color, label, value}) => {
    return (
        <div className="reco-dashboard-color-label-value-wrapper">
            <div className="reco-dashboard-color-label-value-inner">
                <div className="reco-dashboard-color-label">
                    <div className="reco-dashboard-color" style={{ backgroundColor: color }}></div>
                    <div className="reco-dashboard-label"
                        data-tooltip="ifneed"
                        aria-label={label}
                        tabIndex="0"
                    >{label}</div>
                </div>
                <div className="reco-dashboard-value"
                    data-tooltip="ifneed"
                    aria-label={value}
                    tabIndex="0"
                >
                    {value}
                </div>
            </div>
        </div>
    );
};

ColorLabelValue.propTypes = {
    color: PropTypes.string,
    label: PropTypes.string,
    value: PropTypes.number,
};

const SourceFlagIcons = new Map([
    [SourceFlag.SharePoint, "fi-ms-sharepoint"],
    [SourceFlag.Exchange, "fi-ms-exchange"],
    [SourceFlag.OneDrive, "fi-ms-onedrive"],
    [SourceFlag.Physical, "fia-physical-record"],
    [SourceFlag.FileSystem, "fia-file-system-c"],
    [SourceFlag.SharePointOnPrem, "fia-sharepoint"],
    [SourceFlag.Box,"fia-box-blue-b"],
    [SourceFlag.Teams,"fi-ms-teams"],
]);

const IconLabelValue = ({ sourceFlag, label, value, hasBgcColor = false}) => {
    return (
        <div className="reco-dashboard-icon-label-value-wrapper" style={{ backgroundColor: hasBgcColor ? "#F2F3F4" : "" }}>
            <div className="reco-dashboard-icon-label-value-inner">
                <div className="reco-dashboard-icon-label">
                    <div className={["reco-dashboard-icon", SourceFlagIcons.get(sourceFlag)].join(" ")}>
                        {
                            sourceFlag === SourceFlag.FileSystem &&
                            <Fragment>
                                <span className="path1"></span>
                                <span className="path2"></span>
                                <span className="path3"></span>
                            </Fragment>
                        }
                    </div>
                    <div className="reco-dashboard-label" tabIndex="0">{label}</div>
                </div>
                <div className="reco-dashboard-value" tabIndex="0">
                    {value}
                </div>
            </div>
        </div>
    );
};

IconLabelValue.propTypes = {
    sourceFlag: PropTypes.number,
    label: PropTypes.string,
    value: PropTypes.number,
    hasBgcColor: PropTypes.bool,
};

export { ColorLabelValue, IconLabelValue };