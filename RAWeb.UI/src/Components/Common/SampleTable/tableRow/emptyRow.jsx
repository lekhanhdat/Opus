import React from "react";
import PropTypes from "prop-types";
import "./index.less";

export const EmptyRow = ({emptyMessage}) => {
    return (
        <div tabIndex={0} className="opus-common-sample-table-row opus-common-sample-table-empty-row">
            {emptyMessage || RMResx.RM_ES_CompliantExport_ChildTable_EmptyState}
        </div>
    );
};

EmptyRow.propTypes = {
    emptyMessage: PropTypes.string,
};
