import _ from "lodash";

import { NumberUtil, TextUtil } from "../../../../Utils";
import "./index.less";

const ImmutableDataCard = ({ name, value, unit, tooltip }) => {
    const getUnitWidth = (text) => {
        return TextUtil.calculateTextWidth(text, {
            size: 14,
            family: "Open Sans",
        });
    };

    return (
        <div className="sf-reco-immutable-data-card">
            <div className="reco-name">
                <div tabIndex="0">{name}</div>
                {!_.isNil(tooltip) && (
                    <$g.Popover>{tooltip}</$g.Popover>
                )}
            </div>
            <div
                className="reco-value-unit"
                style={{
                    gridTemplateColumns: _.isNil(unit)
                        ? "minmax(0, 1fr)"
                        : `minmax(0, calc(100% - ${getUnitWidth(
                              unit
                          )}px)) ${getUnitWidth(unit)}px`,
                }}
                tabIndex="0"
            >
                <div className="reco-value" data-tooltip="ifneed" aria-label={typeof value === "number" ? NumberUtil.internationalCountingSF(value) : value}>
                    {typeof value === "number" ? NumberUtil.internationalCountingSF(value) : value}
                </div>
                {!_.isNil(unit) && <div className="reco-unit">{unit}</div>}
            </div>
        </div>
    );
};

export default ImmutableDataCard;
