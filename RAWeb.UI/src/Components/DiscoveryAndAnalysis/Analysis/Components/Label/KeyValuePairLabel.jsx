import "./index.less";

const KeyValuePairLabel = ({ maxWidth, height, keyText, valueText }) => {

    return (
        <div
            tabIndex="0"
            data-tooltip="ifneed"
            className="reco-keyvaluepair-label"
            style={{ height: height, maxWidth: maxWidth }}
        >
            <div className="reco-key">{`${keyText}:`}</div>
            <div className="reco-value">{`${valueText}`}</div>
        </div>
    );
};

export default KeyValuePairLabel;
