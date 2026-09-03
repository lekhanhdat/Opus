import { useEffect } from "react";
import { useState } from "react";
import Chart from "react-apexcharts";
import ApexCharts from "apexcharts";
import _ from "lodash";
import { UnitConvertsionUtil } from "../../../Utils";
import { DataSizeType } from "../../../Constants";
import { SourceFlag } from "../../../../../Common/Constants";
import { formatDatePosition } from "../../../../../../Utilities/CommonUtil";

const currentDate = new Date();
const sixMonthsAgoDate = new Date();
sixMonthsAgoDate.setMonth(sixMonthsAgoDate.getMonth() - 6);

const ColumnChart = ({ configurationInfo }) => {
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
    }, [configurationInfo]);

    const [chartInfo, setChartInfo] = useState({
        options: {
            chart: {
                id: "reco-discovery-projection-column-chart",
                toolbar: {
                    show: false,
                },
            },
            plotOptions: {
                bar: {
                    borderRadius: 4,
                    columnWidth: 18,
                    borderRadiusApplication: "end",
                },
            },
            dataLabels: {
                enabled: false,
            },
        },
        series: [
            {
                name: RMResx.RM_FA_Discovery_Chart_Tooltip,
                data: [],
            },
        ],
    });

    useEffect(() => {
        if (innerConfigurationInfo === null) {
            return;
        }
        const {
            latestYear,
            latestMonth,
            latestStorageSize,
            monthlyGrowthRate,
            dataSizeUnitType,
        } = innerConfigurationInfo;
        const items = [];
        for (let i = 0; i < 4; i++) {
            items.push({
                x: formatDatePosition(latestMonth, latestYear + i),
                y:
                    latestStorageSize + monthlyGrowthRate * 12 * i > 0
                        ? UnitConvertsionUtil.Convert(
                              latestStorageSize + monthlyGrowthRate * 12 * i,
                              dataSizeUnitType
                          )
                        : 0,
            });
        }
        const clonedInfo = _.cloneDeep(chartInfo);
        clonedInfo.series = [{ data: items }];
        setChartInfo(clonedInfo);
    }, [innerConfigurationInfo]);

    useEffect(() => {
        ApexCharts.exec(
            "reco-discovery-projection-column-chart",
            "updateOptions",
            chartInfo.options,
            false,
            false,
            true
        );
    }, [chartInfo]);

    return (
        <div className="reco-column-chart">
            <Chart
                options={chartInfo.options}
                series={chartInfo.series}
                type="bar"
                width={"100%"}
                height={400}
            />
        </div>
    );
};

export default ColumnChart;
