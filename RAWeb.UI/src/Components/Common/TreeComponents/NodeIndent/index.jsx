import React from "react";
import PropTypes from "prop-types";
import "./index.less";

const NodeIndent = ({ distance }) => {
    return (
        <div className="reco-node-indent-wrapper" aria-hidden="true">
            {
                new Array(distance).fill(0).map((item, index) => {
                    return <span key={index} className="reco-node-indent"></span>;
                })
            }
        </div>
    );
};

NodeIndent.propTypes = {
    distance: PropTypes.number.isRequired
};

export default NodeIndent;