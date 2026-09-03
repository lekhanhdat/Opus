import React from "react";
import PropTypes from "prop-types";
import "./index.less";

const PieChart = ({datas, colors}) => {
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
                    >{datas.reduce((prev, cur) => prev + cur.value, 0)}</div>
                </div>
            </div>
        </div>
    );
};

PieChart.propTypes = {
    datas: PropTypes.arrayOf(PropTypes.object),
    colors: PropTypes.arrayOf(PropTypes.string),
};

export default PieChart;