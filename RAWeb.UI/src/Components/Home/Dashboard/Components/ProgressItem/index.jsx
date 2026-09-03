import React from "react";
import PropTypes from "prop-types";
import "./index.less";

const MostUsedTermsColor = {
    progressColor: "#209EE1",
    progressBgcColor: "#209EE129",
};

const MostSiteCollectColor = {
    progressColor: "#24BCA4",
    progressBgcColor: "#24BCA429",
};

const ProgressItem = ({ isMostTermComponent, title, tooltip, percentage, count}) => {
    return (
        <div className="reco-dashboard-progress-item-wrapper">
            <div className="reco-dashboard-progress-item-title"
                tabIndex="0"
                data-tooltip="ifneed"
                aria-label={title}
            >{title}</div>
            <div className="reco-dashboard-progress">
                <div className="reco-dashboard-progress-bar"
                    data-tooltip
                    aria-label={tooltip}
                    style={{ backgroundColor: isMostTermComponent ? MostUsedTermsColor.progressBgcColor : MostSiteCollectColor.progressBgcColor }}
                >
                    <div className="reco-dashboard-progress-inner-bar-wrapper">
                        <div className="reco-dashboard-progress-inner-bar"
                            style={{
                                backgroundColor: isMostTermComponent ? MostUsedTermsColor.progressColor : MostSiteCollectColor.progressColor,
                                width: percentage
                            }}
                        ></div>
                    </div>
                </div>
                <div
                    className="reco-dashboard-progress-counter"
                    data-tooltip="ifneed"
                    aria-label={count}
                    tabIndex="0"
                >{count}</div>
            </div>
        </div>
    );
};

ProgressItem.propTypes = {
    isMostTermComponent: PropTypes.bool,
    title: PropTypes.string,
    tooltip: PropTypes.string,
    percentage: PropTypes.string,
    count: PropTypes.number,
};

export default ProgressItem;