export const getAllWidth = (columns) => {
	return columns.reduce((sum, col) => sum + (col.width || 0), 0);
};

export const getTableCellWith = (columns, column, flexible) => {
    return flexible ? column.width / getAllWidth(columns) * 100 + "%" : column.width;
}
