import { useEffect, useState } from "react";
import "./index.less";
import Chart from "react-apexcharts";
import ApexCharts from "apexcharts";
import _ from "lodash";
import { DiscoverySizeRangeQueryMode } from "../../Constants";
import useStableCallback from "../../../../Common/Hooks/useStableCallback";
import { NumberUtil } from "../../Utils";

const SizeRangeChart = ({ id, height, queryParameter, onChange, queryData }) => {
    const [chartInfo, setChartInfo] = useState({
        options: {
            chart: {
                id: id,
                events: {
                    dataPointSelection: onColumnSelect,
                },
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
            tooltip: {
                enabled: true,
                y: {
                    formatter: (val) => NumberUtil.internaltionalCounting(val)
                }
            },
            yaxis: {
                decimalsInFloat: 0,
            }
        },
        series: [
            {
                name: RMResx.RM_FA_Discovery_Chart_Tooltip,
                data: [],
            },
        ],
    });

    useEffect(() => {
        const handler = async () => {
            const filteredSizeRange =
                queryParameter.sizeRangeQueryParameter.sizeRange;

            if (!_.isNil(filteredSizeRange) && filteredSizeRange > 0) {
                return;
            }

            ApexCharts.exec(id, "resetSeries");

            const sizeRanges = await queryData(queryParameter);
            const data = sizeRanges.map((item) => ({
                x: item.name,
                y: item.fileTotalSize,
                meta: item.id,
            }));

            const clonedInfo = _.cloneDeep(chartInfo);
            clonedInfo.options.chart.events.dataPointSelection = onColumnSelect;
            clonedInfo.series = [{ data: data }];
            setChartInfo(clonedInfo);
        };
        handler();
    }, [queryParameter]);

    useEffect(() => {
        ApexCharts.exec(
            id,
            "updateOptions",
            chartInfo.options,
            false,
            false,
            true
        );
    }, [chartInfo]);

    const onColumnSelect = useStableCallback((e, chart, options) => {
        const clonedParameter = _.cloneDeep(queryParameter);
        const selectedIndex = options.selectedDataPoints[0][0];
        if(_.isNil(selectedIndex)) {
            clonedParameter.sizeRangeQueryParameter = {};
        }
        else {
            const item = chartInfo.series[0].data[selectedIndex];
            clonedParameter.sizeRangeQueryParameter = {
                queryMode: DiscoverySizeRangeQueryMode.Range,
                sizeRange: item.meta
            };
        }
        onChange(clonedParameter);
    });

    return (
        <div className="reco-column-chart">
            <Chart
                options={chartInfo.options}
                series={chartInfo.series}
                type="bar"
                width={"100%"}
                height={height}
            />
        </div>
    );
};

export default SizeRangeChart;
