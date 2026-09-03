import _ from "lodash";
import "./index.less";
import { NumberUtil, TextUtil } from "../../Utils";

const ImmutableDataCard = ({ name, value, unit, tooltip, cardAction }) => {
    const getUnitWidth = (text) => {
        return TextUtil.calculateTextWidth(text, {
            size: 14,
            family: "Open Sans",
        });
    };

    return (
        <div className="reco-immutable-data-card">
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
                <div className="reco-value" data-tooltip="ifneed" aria-label={typeof value === "number" ? NumberUtil.internaltionalCounting(value) : value}>
                    {typeof value === "number" ? NumberUtil.internaltionalCounting(value) : value}
                </div>
                <div className="flex align-center gap-xs">
                    {!_.isNil(unit) && <div className="reco-unit">{unit}</div>}
                    {cardAction?.canShow && (
                        <R.Button
                            type="link"
                            icon={cardAction.icon}
                            text=""
                            tooltip={cardAction.iconTooltip}
                            classify="default"
                            onClick={cardAction.onClick}
                        />
                    )}
                </div>
            </div>
        </div>
    );
};

export default ImmutableDataCard;
