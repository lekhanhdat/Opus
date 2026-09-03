import './index.less';

export const NoDataAvailable = ({ hasPriceConfig }) => {
    return (
        <div className="no-data-available">
            <span className="no-data-available__icon fia-book-b">
                <span className="path1"></span>
                <span className="path2"></span>
            </span>
            
            <div className="no-data-available__text">
                {!hasPriceConfig
                    ? RMResx.RM_JS_DSB_HasNotConfiguration
                    : RMResx.RM_JS_DSB_NoDataAvailable
                }
            </div>
        </div>
    );
};