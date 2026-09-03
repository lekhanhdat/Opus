import React, { Fragment } from "react";
import { useHistory } from 'react-router-dom';
import PropTypes from "prop-types";
import "./index.less";

import { SourceFlag } from "../../Common/Constants";

const ColorLabelValueLink = ({ color, label, value,link}) => {
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
                    <a className={"highlight"} tabIndex="0" href={link}>{value}</a>
                </div>
            </div>
        </div>
    );
};

ColorLabelValueLink.propTypes = {
    color: PropTypes.string,
    label: PropTypes.string,
    value: PropTypes.number,
    link: PropTypes.string,
};

const SourceFlagIcons = new Map([
    [SourceFlag.SharePoint, "fi-ms-sharepoint"],
    [SourceFlag.Exchange, "fi-ms-exchange"],
    [SourceFlag.OneDrive, "fi-ms-onedrive"],
    [SourceFlag.Physical, "fia-physical-record"],
    [SourceFlag.FileSystem, "fia-file-system-c"],
    [SourceFlag.SharePointOnPrem, "fia-sharepoint"],
    [SourceFlag.AzureFileShare, "fi-ms-azure-file-share"],
    [SourceFlag.Box, "fia-box-blue-b"],
    [SourceFlag.Google, "fia-google-drive-f"],
    [SourceFlag.Teams, "fi-ms-teams"],
]);

const IconLabelValueLink = ({ sourceFlag, label, value, hasBgcColor = false ,link, routeUrl, routeParam}) => {

    const history = useHistory();

    const routeTo = () => {
        if(routeParam){
            history.push({pathname: routeUrl, query: routeParam});
        }
    };

    return (
        <div className="reco-dashboard-icon-label-value-wrapper" style={{ backgroundColor: hasBgcColor ? "#F2F3F4" : "" }}>
            <div className="reco-dashboard-icon-label-value-inner">
                <div className="reco-dashboard-icon-label">
                    <div className={["reco-dashboard-icon", SourceFlagIcons.get(sourceFlag)].join(" ")}>
                        {
                            (sourceFlag === SourceFlag.FileSystem || sourceFlag === SourceFlag.Box || sourceFlag === SourceFlag.Google) &&
                            <Fragment>
                                <span className="path1"></span>
                                <span className="path2"></span>
                                <span className="path3"></span>
                                <span className="path4"></span>
                                <span className="path5"></span>
                                <span className="path6"></span>
                            </Fragment>
                        }
                    </div>
                    <div className="reco-dashboard-label" tabIndex="0">{label}</div>
                </div>
                <div className="reco-dashboard-value" tabIndex="0">
                    <a className="highlight" tabIndex="0" href={link} onClick={ routeTo }>{value}</a>
                </div>
            </div>
        </div>
    );
};

IconLabelValueLink.propTypes = {
    sourceFlag: PropTypes.number,
    label: PropTypes.string,
    value: PropTypes.number,
    hasBgcColor: PropTypes.bool,
    link:PropTypes.string,
    routeUrl: PropTypes.string,
    routeParam: PropTypes.any 
};

export { ColorLabelValueLink, IconLabelValueLink };