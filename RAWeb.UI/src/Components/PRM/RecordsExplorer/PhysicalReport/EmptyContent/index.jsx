import React, { Fragment } from "react";
import PropTypes from "prop-types";
import "./index.less";


const EmptyContent = ({ children, isEmpty }) => {
    return (
        isEmpty ?
            <Fragment>
                <div className="reco-phy-empty-content-wrapper">
                    <span className="reco-phy-empty-content-icon fia-book-b">
                        <span className="path1"></span>
                        <span className="path2"></span>
                    </span>
                    <span className="reco-phy-empty-content-text" tabIndex="0">{RMResx.RM_DSB_NoItem}</span>
                </div>
            </Fragment> :
            children
    );
};

EmptyContent.propTypes = {
    children: PropTypes.oneOfType([PropTypes.element, PropTypes.arrayOf(PropTypes.element)]),
    isEmpty: PropTypes.bool,
};

export default EmptyContent;