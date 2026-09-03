import React from "react";
import PropTypes from "prop-types";
import { getTableCellWith } from "../utils";
import "./index.less";

export const TableColumns = ({ columns, flexible }) => {
	function render() {
		return (
			<div className="opus-common-sample-table-header">
				{columns.map((column) => {
					const width = getTableCellWith(columns, column, flexible);
					return (
						<div tabIndex={column.name ? 0 : -1} key={column.key} style={{ width: width, minWidth: column.minWidth }}>
							{column.name}
						</div>
					);
				})}
			</div>
		);
	}

	return render();
};

TableColumns.propTypes = {
	columns: PropTypes.arrayOf(
		PropTypes.shape({
			key: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
			width: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
			name: PropTypes.string,
		})
	).isRequired,
	flexible: PropTypes.boolean
};
