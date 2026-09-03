import { SourceFlag, SourceFlagI18Ns } from "../../../../Common/Constants";
import { DataSizeTypeI18ns } from "../../Constants";
import ColumnChart from "./ColumnChart";
import LineChart from "./LineChart";
import OtherInfo from "./OtherInfo";
import "./index.less";

const Projection = ({
    configurationInfo,
    savingInfo,
    onContentSourceChange,
}) => {
    const getContentSourceOptions = (contentSource) => {
        return [
            {
                name: SourceFlagI18Ns.get(SourceFlag.SharePoint),
                value: SourceFlag.SharePoint,
            },
            {
                name: SourceFlagI18Ns.get(SourceFlag.OneDrive),
                value: SourceFlag.OneDrive,
            },
        ].map((item) => {
            item.checked = item.value === contentSource;
            return item;
        });
    };

    return (
        <div className="reco-discovery-projection">
            <div className="reco-header">
                <div className="reco-title" tabIndex={0}>{`${
                    RMResx.RM_FA_Progress_ProjectionTab_ChartTitle
                } (${DataSizeTypeI18ns.get(
                    configurationInfo.dataSizeUnitType
                )})`}</div>
                <div>
                    <div className="reco-content">
                        <R.Combobox
                            width={"200px"}
                            popupMaxHeight={400}
                            searchable={false}
                            items={getContentSourceOptions(
                                configurationInfo.contentSource
                            )}
                            textField="name"
                            valueField="value"
                            onChange={(args) => {
                                onContentSourceChange(args.newValue.value);
                            }}
                            aria="#ariaUnit"
                        />
                    </div>
                </div>
            </div>

            <div className="reco-storage-growth">
                <div className="reco-column-chart">
                    <ColumnChart configurationInfo={configurationInfo} />
                </div>
                <div className="reco-direction-split-line"></div>
                <div className="reco-storage-others">
                    <OtherInfo
                        configurationInfo={configurationInfo}
                        savingInfo={savingInfo}
                    />
                </div>
            </div>
            <div className="reco-title">
                {RMResx.RM_FA_Progress_ProjectionTab_ChartTitle}
            </div>
            <LineChart
                configurationInfo={configurationInfo}
                savingInfo={savingInfo}
            />
        </div>
    );
};

export default Projection;
