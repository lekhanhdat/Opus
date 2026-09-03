import Chart from "react-apexcharts";
import ApexCharts from "apexcharts";
import { useEffect, useState } from "react";
import { NumberUtil, UnitConvertsionUtil } from "../../../Utils";
import { DataSizeType, DataSizeTypeI18ns } from "../../../Constants";
import { SourceFlag } from "../../../../../Common/Constants";
import { formatDatePosition } from "../../../../../../Utilities/CommonUtil";

const currentDate = new Date();
const sixMonthsAgoDate = new Date();
sixMonthsAgoDate.setMonth(sixMonthsAgoDate.getMonth() - 6);

const LineChart = ({ configurationInfo, savingInfo }) => {

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
            latestYear:  contentSource === SourceFlag.SharePoint ? configurationInfo.latestYear : configurationInfo.odLatestYear,
            latestMonth: contentSource === SourceFlag.SharePoint ? configurationInfo.latestMonth : configurationInfo.odLatestMonth,
            latestStorageSize: contentSource === SourceFlag.SharePoint ? configurationInfo.latestStorageSize : configurationInfo.odLatestStorageSize,
            oldestYear: contentSource === SourceFlag.SharePoint ? configurationInfo.oldestYear : configurationInfo.odOldestYear,
            oldestMonth: contentSource === SourceFlag.SharePoint ? configurationInfo.oldestMonth : configurationInfo.odOldestMonth,
            oldestStorageSize: contentSource === SourceFlag.SharePoint ? configurationInfo.oldestStorageSize : configurationInfo.odOldestStorageSize,
            realityMonthlyGrowthRate: contentSource === SourceFlag.SharePoint ? configurationInfo.realityMonthlyGrowthRate : configurationInfo.odRealityMonthlyGrowthRate,
            monthlyGrowthRate: contentSource === SourceFlag.SharePoint ? configurationInfo.monthlyGrowthRate : configurationInfo.odMonthlyGrowthRate,
            realityDailyOptimizationSpeed: configurationInfo.realityDailyOptimizationSpeed,
            dailyOptimizationSpeed: configurationInfo.dailyOptimizationSpeed,
            dataSizeUnitType: configurationInfo.dataSizeUnitType,
        });
        setInnerSavingInfo({
            freeStorage: contentSource === SourceFlag.SharePoint ? savingInfo.spFreeStorage : savingInfo.odFreeStorage,
            storagePrice: contentSource === SourceFlag.SharePoint ? savingInfo.spStoragePrice : savingInfo.odStoragePrice,
            archivedDataStoragePrice: savingInfo.archivedDataStoragePrice
        });
    }, [configurationInfo, savingInfo]);

    const [chartInfo, setChartInfo] = useState({
        options: {
            chart: {
                id: "reco-discovery-projection-line-chart",
                toolbar: {
                    show: false,
                },
                selection: {
                    enabled: false
                },
                brush: {
                    enabled: false
                },
                zoom: {
                    enabled: false
                }
            },
            forecastDataPoints: {
                count: 0,
            },
            dataLabels: {
                enabled: true,
                enabledOnSeries: [2],
            },
            colors: ["#F5BD24", "#45CF86"],
            labels: [],
            annotations: {
                xaxis: [
                    {
                        x: "05 Jan 2001",
                        strokeDashArray: 0,
                        borderColor: "#45CF86",
                        label: {
                            borderColor: "#45CF86",
                            style: {
                                color: "#fff",
                                background: "#45CF86",
                            },
                            text: "7/1/2024",
                            position: "top",
                        },
                    },
                ],
            },
            xaxis: {},
            yaxis: [
                {
                    title: {
                        text: RMResx.RM_FA_Progress_ProjectionTab_YearStorage,
                    },
                    axisBorder: {
                        show: true,
                        color: "#F5BD24",
                        width: 2,
                    },
                },
                {
                    opposite: true,
                    title: {
                        text: RMResx.RM_FA_Progress_ProjectionTab_StorageUsage,
                    },
                    axisBorder: {
                        show: true,
                        color: "#45CF86",
                        width: 2,
                    },
                },
            ],
            stroke: {
                curve: "straight",
                width: 2,
            },
            markers: {
                size: 5,
            },
            legend: {
                show: false,
            },
        },
        series: [
            {
                name: RMResx.RM_FA_Progress_ProjectionTab_YearStorage,
                data: [],
            },
            {
                name: RMResx.RM_FA_Progress_ProjectionTab_StorageUsage,
                data: [],
            },
        ],
    });

    const [flagInfo, setFlagInfo] = useState({
        storageSize: 0,
        saving: 0,
    })

    const [archivedDate, setArchivedDate] = useState(new Date());

    const onArchiveDateChange = (args) => {
        setArchivedDate(new Date(args.newValue));
    };

    useEffect(() => {
        if (innerConfigurationInfo === null || innerSavingInfo === null) {
            return;
        }
        const {
            latestYear,
            latestMonth,
            latestStorageSize,
            oldestYear,
            oldestMonth,
            oldestStorageSize,
            monthlyGrowthRate,
            dailyOptimizationSpeed,
            dataSizeUnitType,
        } = innerConfigurationInfo;
        let dateList = [];
        let savingList = [];
        let storageList = [];

        dateList.push([oldestYear, oldestMonth]);
        let firstPointSaving = (
            UnitConvertsionUtil.Convert(
                oldestStorageSize -
                    innerSavingInfo.freeStorage * 1024 * 1024 * 1024
            ) * innerSavingInfo.storagePrice
        ).toFixed(0);
        savingList.push(
            firstPointSaving > 0 ? firstPointSaving : 0
        );
        storageList.push(oldestStorageSize);

        dateList.push([latestYear, latestMonth]);
        let secondPointSaving = (
            UnitConvertsionUtil.Convert(
                latestStorageSize -
                innerSavingInfo.freeStorage * 1024 * 1024 * 1024
            ) * innerSavingInfo.storagePrice
        ).toFixed(0);
        savingList.push(
            secondPointSaving > 0 ? secondPointSaving : 0
        );
        storageList.push(latestStorageSize);

        const archivedMonth = new Date(archivedDate.getFullYear(), archivedDate.getMonth());

        const monthlyGrowthRateWithArchived =
            monthlyGrowthRate - dailyOptimizationSpeed * 30;
        let flagDate = null;

        for (let i = 1; i < 14; i++) {
            const latestDate = new Date(dateList[i][0], dateList[i][1] - 1);
            const latestStorageSize = storageList[i];
            const latestSaving = savingList[i];

            if (latestStorageSize <= 0 && latestDate > archivedMonth) {
                break;
            }

            let intervalMonth = 6;

            if (
                latestDate.getFullYear() === archivedMonth.getFullYear() &&
                latestDate.getMonth() === archivedMonth.getMonth()
            ) {
                flagDate = dateList[i];
                intervalMonth = ((latestDate.getFullYear() - new Date(dateList[i - 1][0], dateList[i - 1][1] - 1).getFullYear()) * 12
                + latestDate.getMonth() - new Date(dateList[i - 1][0], dateList[i - 1][1] - 1).getMonth());
                intervalMonth = intervalMonth === 6 ? intervalMonth : 6 - intervalMonth;
                setFlagInfo({
                    saving: latestSaving,
                    storageSize: latestStorageSize
                });
            }

            if (latestDate < archivedMonth) {
                const nextIntervalMonths =
                    (archivedMonth.getFullYear() - latestDate.getFullYear()) *
                        12 +
                    archivedMonth.getMonth() -
                    latestDate.getMonth();
                if (nextIntervalMonths < 6 && nextIntervalMonths > 0) {
                    intervalMonth = nextIntervalMonths;
                    flagDate = [archivedDate.getMonth() - 1, archivedDate.getFullYear()];
                }
            }

            if (latestDate >= archivedMonth) {
                let newlyStorageSize =
                    latestStorageSize +
                    monthlyGrowthRateWithArchived * intervalMonth;
                newlyStorageSize = newlyStorageSize > 0 ? newlyStorageSize : 0;
                let newlySaving =
                    UnitConvertsionUtil.Convert(
                        newlyStorageSize -
                            innerSavingInfo.freeStorage * 1024 * 1024 * 1024
                    ) * innerSavingInfo.storagePrice;
                newlySaving = newlySaving > 0 ? newlySaving : 0;
                storageList.push(newlyStorageSize);
                savingList.push(newlySaving.toFixed(0));
            } else {
                let newlyStorageSize =
                    latestStorageSize + monthlyGrowthRate * intervalMonth;
                newlyStorageSize = newlyStorageSize > 0 ? newlyStorageSize : 0;
                let newlySaving =
                    UnitConvertsionUtil.Convert(
                        newlyStorageSize -
                        innerSavingInfo.freeStorage * 1024 * 1024 * 1024
                    ) * innerSavingInfo.storagePrice;
                newlySaving = newlySaving > 0 ? newlySaving : 0;
                storageList.push(newlyStorageSize);
                savingList.push(newlySaving.toFixed(0));
            }

            const newlyMonth = latestDate.getMonth() + intervalMonth;
            const newlyYear = latestDate.getFullYear() + (newlyMonth >= 12 ? 1 : 0);
            dateList.push([newlyYear, (newlyMonth >= 12 ? newlyMonth - 12 : newlyMonth) + 1]);
        }

        storageList = storageList.map((item) =>
            UnitConvertsionUtil.Convert(item, dataSizeUnitType)
        );
        const clonedChartInfo = _.cloneDeep(chartInfo);
        const dateListLabels = dateList.map(item => formatDatePosition(item[1], item[0]));
        clonedChartInfo.options.labels = dateListLabels;
        clonedChartInfo.options.yaxis[1].title.text = `${RMResx.RM_FA_Progress_ProjectionTab_Storage} (${DataSizeTypeI18ns.get(
            dataSizeUnitType
        )})`;
        clonedChartInfo.series[0].data = savingList;
        clonedChartInfo.series[1].name = `${RMResx.RM_FA_Progress_ProjectionTab_Storage} (${DataSizeTypeI18ns.get(
            dataSizeUnitType
        )})`;
        clonedChartInfo.series[1].data = storageList;
        if (flagDate !== null) {
            clonedChartInfo.options.annotations.xaxis[0].x = formatDatePosition(flagDate[1], flagDate[0]);
            clonedChartInfo.options.annotations.xaxis[0].label.text = formatDatePosition(flagDate[1], flagDate[0]);
            clonedChartInfo.options.forecastDataPoints.count = dateList.length - 2;
        }

        setChartInfo(clonedChartInfo);
    }, [innerConfigurationInfo, archivedDate]);

    useEffect(() => {
        ApexCharts.exec(
            "reco-discovery-projection-line-chart",
            "updateOptions",
            chartInfo.options,
            false,
            false,
            true
        );
    }, [chartInfo]);
    const titleDescArr = RMResx.RM_FA_Progress_ProjectionTab_TitleDesc.split('==Ave==');
    return (
        <div className="reco-projection-line-chart">
            <div className="reco-date-picker">
                <R.Monthpicker
                    id="reco-projection-date-picker"
                    width="220px"
                    selectedDate={archivedDate}
                    enableDates={{ start: new Date(), end: null }}
                    onChange={onArchiveDateChange}
                />
            </div>
            <div className="reco-description" tabIndex={0}>
                <span className="reco-desc-paragraph">
                    {titleDescArr[0]}
                </span>
                <span className="reco-desc-paragraph-highlight">
                    {formatDatePosition(archivedDate.getMonth() + 1, archivedDate.getFullYear())}
                </span>
                <span className="reco-desc-paragraph">
                    {titleDescArr[1].format(configurationInfo.contentSource === SourceFlag.SharePoint ? RMResx.RM_JS_SPS_TabLabel_SP : RMResx.RM_JS_SPS_TabLabel_OneDrive)}
                </span>
                <span className="reco-desc-paragraph-highlight" style={{marginRight: 0}}>
                    {`${NumberUtil.internaltionalCounting(UnitConvertsionUtil.Convert(flagInfo.storageSize, innerConfigurationInfo ? innerConfigurationInfo.dataSizeUnitType : 5))} ${DataSizeTypeI18ns.get(innerConfigurationInfo ? innerConfigurationInfo.dataSizeUnitType : 5)}`}
                </span>
                <span className="reco-desc-paragraph">
                    {titleDescArr[2]}
                </span>
                <span className="reco-desc-paragraph-highlight" style={{marginRight: 0}}>
                    {`${NumberUtil.internaltionalCounting(flagInfo.saving)}${titleDescArr[3]}`}
                </span>
            </div>
            <Chart
                options={chartInfo.options}
                series={chartInfo.series}
                type="line"
                width={"100%"}
                height={500}
            />
        </div>
    );
};

export default LineChart;
