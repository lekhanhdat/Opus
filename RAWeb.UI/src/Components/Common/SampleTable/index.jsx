import React from "react";
import PropTypes from "prop-types";
import { TableColumns } from "./tableColumns";
import { TableRow } from "./tableRow";
import { EmptyRow } from "./tableRow/emptyRow";
import "./index.less";

export const SampleTable = ({ columns, items, flexible }) => {
    const isEmpty = items.length === 0;

    const renderTableRow = () => {
        if (isEmpty) {
            return <EmptyRow />;
        }

        return (
            <div className="opus-common-sample-table-body">
                {items.map((item) => {
                    return (
                        <TableRow
                            key={item.key}
                            columns={columns}
                            item={item}
                            flexible={flexible}
                        />
                    );
                })}
            </div>
        );
    };

    return (
        <div className="opus-common-sample-table">
            <TableColumns columns={columns} flexible={flexible} />
            {renderTableRow()}
        </div>
    );
};

SampleTable.propTypes = {
    columns: PropTypes.arrayOf(
        PropTypes.shape({
            key: PropTypes.string.isRequired,
            name: PropTypes.string.isRequired,
            width: PropTypes.oneOfType([PropTypes.string, PropTypes.number])
                .isRequired,
            fieldName: PropTypes.string,
            onRender: PropTypes.func,
        })
    ).isRequired,
    items: PropTypes.arrayOf(PropTypes.object).isRequired,
    flexible: PropTypes.boolean,
};
