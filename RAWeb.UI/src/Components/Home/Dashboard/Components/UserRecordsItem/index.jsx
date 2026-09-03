import React from "react";
import PropTypes from "prop-types";
import "./index.less";

const Top3Colors = [
    "#FF5551",
    "#FDC607",
    "#00bbad"
];

const Colors = [
    "#0078D4",
    "#498205",
    "#8E562E",
    "#881798",
    "#5C2E91",
    "#393939",
    "#750B1C",
    "#CA5010",
    "#005B70",
    "#004E8C"
];

const UserRecordsItem = ({ index, name, email, count }) => {

    const getUserNameAbbr = (userName) => {
        const nameArr = userName.split(" ");
        const nameArrLen = nameArr.length;
        if (nameArrLen == 1) {
            return nameArr[0].substr(0, 1);
        }
        return nameArr[0].substr(0, 1) + nameArr[1].substr(0, 1);
    };

    return (
        <div className="reco-dashboard-uri-wrapper">
            <div
                className="reco-dashboard-user-header"
                data-tooltip
                aria-label={email}
                style={{ backgroundColor: Colors[index] }}
            >{getUserNameAbbr(name)}</div>
            <div className="reco-dashboard-user-record">
                <div
                    className="reco-dashboard-user-name"
                    tabIndex="0"
                    data-tooltip="ifneed"
                    aria-label={name}
                >
                    {name}
                </div>
                <div className="reco-dashboard-record-count" tabIndex="0">
                    <span style={{ marginRight: "4px" }}>{count}</span>
                    <span>{RMResx.RM_DSB_Records}</span>
                </div>
            </div>
            {index < 3 && <div className="reco-dashboard-user-top" style={{ backgroundColor: Top3Colors[index] }}>{index + 1}</div>}
        </div>
    );
};

UserRecordsItem.propTypes = {
    index: PropTypes.number,
    name: PropTypes.string,
    email: PropTypes.string,
    count: PropTypes.number
};

export default UserRecordsItem;