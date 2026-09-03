import React from "react";
import PropTypes from "prop-types";
import "./index.less";

const PieChartLink = ({datas, colors, link}) => {
    return (
        <div className="reco-dashboard-piechart-wrapper">
            <div className="reco-dashboard-piechart-inner">
                <R.Charts>
                    <R.Charts.Pie
                        type="donut"
                        items={datas}
                        color={colors}
                        thickness="15"
                    />
                </R.Charts>
                <div className="reco-dashboard-piechart-inner-content">
                    <div className="reco-dashboard-piechart-inner-name" tabIndex="0">{RMResx.RM_DSB_Total}</div>
                    <div className="reco-dashboard-piechart-inner-value"
                        data-tooltip="ifneed"
                        aria-label={datas.reduce((prev, cur) => prev + cur.value, 0)}
                        tabIndex="0"
                    ><a className={link?"highlight":"highlight reco-dashboard-value-link-noClick"} tabIndex="0" href={link}>{datas.reduce((prev, cur) => prev + cur.value, 0)}</a></div>
                </div>
            </div>
        </div>
    );
};

PieChartLink.propTypes = {
    datas: PropTypes.arrayOf(PropTypes.object),
    colors: PropTypes.arrayOf(PropTypes.string),
    link: PropTypes.string
};

export default PieChartLink;