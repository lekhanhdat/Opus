const ColumnTemplate = ({
    columnName, 
    popoverIcon="fia-status-info", 
    popoverContent
}) => {
    return <div className="ra-flex-align-center">
        <span className="margin-right-s">{columnName}</span>
        <R.Popover>
            <div className={popoverIcon} tabIndex="0" role="button"></div>
            <span>{popoverContent}</span>
        </R.Popover>
    </div>;
};

export default ColumnTemplate;