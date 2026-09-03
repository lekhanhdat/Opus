import React from "react";
import PropTypes from "prop-types";
import { TableCell } from "../tableCell";
import { getTableCellWith } from "../utils";
import "./index.less";

export const TableRow = ({ columns, item, flexible }) => {
	return (
		<div className="opus-common-sample-table-row">
			{columns.map((column) => {
				const width = getTableCellWith(columns, column, flexible);
				return <TableCell key={column.key} column={column} item={item} width={width}/>
			})}
		</div>
	);
};

TableRow.propTypes = {
	columns: PropTypes.arrayOf(
		PropTypes.shape({
			key: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
			fieldName: PropTypes.string,
			width: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
			onRender: PropTypes.func,
		})
	).isRequired,
	item: PropTypes.any.isRequired,
	flexible: PropTypes.boolean
};
