import { useEffect } from "react";
import { DataSizeType, DataSizeTypeI18ns } from "../../../Constants";
import { NumberUtil, UnitConvertsionUtil } from "../../../Utils";
import { useState } from "react";
import { SourceFlag } from "../../../../../Common/Constants";

const currentDate = new Date();
const sixMonthsAgoDate = new Date();
sixMonthsAgoDate.setMonth(sixMonthsAgoDate.getMonth() - 6);

const carbonStringArr =
    RMResx.RM_FA_Progress_ProjectionContext_Carbon.split("==Ave==");

const percentStringArr = RMResx.RM_FA_Progress_ProjectionContext_Percent.split("==Ave==");

const moneyStringArr = RMResx.RM_FA_Progress_ProjectionContext_Money.split("==Ave==");

const OtherInfo = ({ configurationInfo, savingInfo }) => {
    const [innerConfigurationInfo, setInnerConfigurationInfo] = useState({
        latestYear: currentDate.getFullYear(),
        latestMonth: currentDate.getMonth() + 1,
        latestStorageSize: 0,
        oldestYear: sixMonthsAgoDate.getFullYear(),
        oldestMonth: sixMonthsAgoDate.getMonth() + 1,
        oldestStorageSize: 0,
        realityMonthlyGrowthRate: 0,
        monthlyGrowthRate: 0,
        realityDailyOptimizationSpeed: 0,
        dailyOptimizationSpeed: 0,
        dataSizeUnitType: DataSizeType.TB,
    });

    const [innerSavingInfo, setInnerSavingInfo] = useState({
        freeStorage: 0,
        storagePrice: 0,
        archivedDataStoragePrice: 0,
    });

    useEffect(() => {
        const contentSource = configurationInfo.contentSource;
        setInnerConfigurationInfo({
            latestYear:
                contentSource === SourceFlag.SharePoint
                    ? configurationInfo.latestYear
                    : configurationInfo.odLatestYear,
            latestMonth:
                contentSource === SourceFlag.SharePoint
                    ? configurationInfo.latestMonth
                    : configurationInfo.odLatestMonth,
            latestStorageSize:
                contentSource === SourceFlag.SharePoint
                    ? configurationInfo.latestStorageSize
                    : configurationInfo.odLatestStorageSize,
            oldestYear:
                contentSource === SourceFlag.SharePoint
                    ? configurationInfo.oldestYear
                    : configurationInfo.odOldestYear,
            oldestMonth:
                contentSource === SourceFlag.SharePoint
                    ? configurationInfo.oldestMonth
                    : configurationInfo.odOldestMonth,
            oldestStorageSize:
                contentSource === SourceFlag.SharePoint
                    ? configurationInfo.oldestStorageSize
                    : configurationInfo.odOldestStorageSize,
            realityMonthlyGrowthRate:
                contentSource === SourceFlag.SharePoint
                    ? configurationInfo.realityMonthlyGrowthRate
                    : configurationInfo.odRealityMonthlyGrowthRate,
            monthlyGrowthRate:
                contentSource === SourceFlag.SharePoint
                    ? configurationInfo.monthlyGrowthRate
                    : configurationInfo.odMonthlyGrowthRate,
            realityDailyOptimizationSpeed:
                configurationInfo.realityDailyOptimizationSpeed,
            dailyOptimizationSpeed: configurationInfo.dailyOptimizationSpeed,
            dataSizeUnitType: configurationInfo.dataSizeUnitType,
        });

        setInnerSavingInfo({
            freeStorage:
                contentSource === SourceFlag.SharePoint
                    ? savingInfo.spFreeStorage
                    : savingInfo.odFreeStorage,
            storagePrice:
                contentSource === SourceFlag.SharePoint
                    ? savingInfo.spStoragePrice
                    : savingInfo.odStoragePrice,
            archivedDataStoragePrice: savingInfo.archivedDataStoragePrice,
        });
    }, [configurationInfo, savingInfo]);

    return (
        <>
            {innerConfigurationInfo && innerSavingInfo && (
                <div className="reco-discovery-other-info">
                    <div className="reco-item-info">
                        <div className="reco-item-title" tabIndex={0}>
                            {
                                RMResx.RM_FA_Progress_ProjectionContext_PercentTitle
                            }
                        </div>
                        <div
                            className="reco-item-content"
                            tabIndex={0}
                            data-tooltip="ifneed"
                            aria-label={`${percentStringArr[0]}${NumberUtil.toPercentage(
                                (innerConfigurationInfo.latestStorageSize +
                                    innerConfigurationInfo.monthlyGrowthRate *
                                        12 *
                                        3 -
                                    innerConfigurationInfo.latestStorageSize) /
                                    innerConfigurationInfo.latestStorageSize
                            )}${percentStringArr[1]}`}
                        >
                            <$g.I18NProvider msg={RMResx.RM_FA_Progress_ProjectionContext_Percent_Update} className="ra-ellipsis" style={{ color: "#293037" }}>
                                <span className="reco-highlight-content">
                                    {NumberUtil.toPercentage(
                                        (innerConfigurationInfo.latestStorageSize +
                                            innerConfigurationInfo.monthlyGrowthRate *
                                                12 *
                                                3 -
                                            innerConfigurationInfo.latestStorageSize) /
                                            innerConfigurationInfo.latestStorageSize
                                    )}
                                </span>
                            </$g.I18NProvider>
                        </div>
                    </div>
                    <div className="reco-item-info">
                        <div className="reco-item-title" tabIndex={0}>
                            {RMResx.RM_FA_Progress_ProjectionContext_MoneyTitle}
                        </div>
                        <div
                            className="reco-item-content"
                            tabIndex={0}
                            data-tooltip="ifneed"
                            aria-label={`${moneyStringArr[0]}${NumberUtil.toGreaterThanZero(
                                (
                                    (UnitConvertsionUtil.Convert(
                                        innerConfigurationInfo.latestStorageSize +
                                            innerConfigurationInfo.monthlyGrowthRate *
                                                12 *
                                                3 -
                                            innerConfigurationInfo.latestStorageSize
                                    ) *
                                        innerSavingInfo.storagePrice) /
                                    1000
                                ).toFixed(2)
                            )}K ${
                                moneyStringArr[1]
                            }`}
                        >
                            <$g.I18NProvider msg={RMResx.RM_FA_Progress_ProjectionContext_Money_Update} className="ra-ellipsis" style={{ color: "#293037" }}>
                                <span className="reco-highlight-content">
                                    {NumberUtil.toGreaterThanZero(
                                        (
                                            (UnitConvertsionUtil.Convert(
                                                innerConfigurationInfo.latestStorageSize +
                                                    innerConfigurationInfo.monthlyGrowthRate *
                                                        12 *
                                                        3 -
                                                    innerConfigurationInfo.latestStorageSize
                                            ) *
                                                innerSavingInfo.storagePrice) /
                                            1000
                                        ).toFixed(2)
                                    )}
                                    K
                                </span>
                            </$g.I18NProvider>
                            {/* <div className="reco-usual-content">
                                {moneyStringArr[0]}
                            </div>
                            <span className="reco-highlight-content">
                                {NumberUtil.toGreaterThanZero(
                                    (
                                        (UnitConvertsionUtil.Convert(
                                            innerConfigurationInfo.latestStorageSize +
                                                innerConfigurationInfo.monthlyGrowthRate *
                                                    12 *
                                                    3 -
                                                innerConfigurationInfo.latestStorageSize
                                        ) *
                                            innerSavingInfo.storagePrice) /
                                        1000
                                    ).toFixed(2)
                                )}
                                K
                            </span>
                            <div className="reco-usual-content">
                            {moneyStringArr[1]}
                            </div> */}
                        </div>
                    </div>
                    <div className="reco-item-info">
                        <div className="reco-item-title" tabIndex={0}>
                            {
                                RMResx.RM_FA_Progress_ProjectionContext_CarbonTitle
                            }
                        </div>
                        <div
                            className="reco-item-content"
                            data-tooltip="ifneed"
                            aria-label={`${
                                carbonStringArr[0]
                            } ${NumberUtil.toGreaterThanZero(
                                UnitConvertsionUtil.Convert(
                                    innerConfigurationInfo.latestStorageSize +
                                        innerConfigurationInfo.monthlyGrowthRate *
                                            12 *
                                            3 -
                                        innerSavingInfo.freeStorage *
                                            1024 *
                                            1024 *
                                            1024,
                                    innerConfigurationInfo.dataSizeUnitType
                                )
                            )}${DataSizeTypeI18ns.get(
                                innerConfigurationInfo.dataSizeUnitType
                            )} ${
                                carbonStringArr[1]
                            } ${NumberUtil.toGreaterThanZero(
                                (
                                    UnitConvertsionUtil.Convert(
                                        innerConfigurationInfo.latestStorageSize +
                                            innerConfigurationInfo.monthlyGrowthRate *
                                                12 *
                                                3 -
                                            innerSavingInfo.freeStorage *
                                                1024 *
                                                1024 *
                                                1024
                                    ) * 0.028
                                ).toFixed(2)
                            )}${carbonStringArr[2]}`}
                        >
                            <div className="reco-usual-content" tabIndex={0}>
                                {`${
                                    carbonStringArr[0]
                                } ${NumberUtil.toGreaterThanZero(
                                    UnitConvertsionUtil.Convert(
                                        innerConfigurationInfo.latestStorageSize +
                                            innerConfigurationInfo.monthlyGrowthRate *
                                                12 *
                                                3 -
                                            innerSavingInfo.freeStorage *
                                                1024 *
                                                1024 *
                                                1024,
                                        innerConfigurationInfo.dataSizeUnitType
                                    )
                                )}${DataSizeTypeI18ns.get(
                                    innerConfigurationInfo.dataSizeUnitType
                                )} ${
                                    carbonStringArr[1]
                                } ${NumberUtil.toGreaterThanZero(
                                    (
                                        UnitConvertsionUtil.Convert(
                                            innerConfigurationInfo.latestStorageSize +
                                                innerConfigurationInfo.monthlyGrowthRate *
                                                    12 *
                                                    3 -
                                                innerSavingInfo.freeStorage *
                                                    1024 *
                                                    1024 *
                                                    1024
                                        ) * 0.028
                                    ).toFixed(2)
                                )}${carbonStringArr[2]}`}
                            </div>
                            <$g.Popover>
                                {RMResx.RM_FA_Progress_ProjectionContext_Co2_Desc.format(
                                    "https://www.iea.org/commentaries/the-carbon-footprint-of-streaming-video-fact-checking-the-headlines"
                                )}
                            </$g.Popover>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
};

export default OtherInfo;
