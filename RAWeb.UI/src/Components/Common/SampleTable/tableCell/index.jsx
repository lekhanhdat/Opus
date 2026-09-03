import React from "react";
import PropTypes from "prop-types";
import "./index.less";

export const TableCell = ({ column, item, width }) => {
	const { key, fieldName, onRender, minWidth } = column;

	return (
		<div
			className="opus-common-sample-table-cell"
			key={key}
			style={{ width: width, minWidth: minWidth }}
		>
			{onRender?.(item) ?? item[fieldName || ""]}
		</div>
	);
};

TableCell.propTypes = {
	column: PropTypes.shape({
		key: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
		fieldName: PropTypes.string,
		width: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
		onRender: PropTypes.func,
	}).isRequired,
	item: PropTypes.any.isRequired,
};
