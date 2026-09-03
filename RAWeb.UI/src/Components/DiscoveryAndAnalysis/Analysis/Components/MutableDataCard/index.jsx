import { NumberUtil } from "../../Utils";
import "./index.less";

const MutableDataCard = ({ text, value }) => {
    return (
        <div className="reco-mutable-data-card">
            <div className="reco-value" data-tooltip="ifneed">
                {NumberUtil.internaltionalCounting(value)}
            </div>
            <div className="reco-text" data-tooltip="ifneed">{text}</div>
        </div>
    );
};

export default MutableDataCard;
