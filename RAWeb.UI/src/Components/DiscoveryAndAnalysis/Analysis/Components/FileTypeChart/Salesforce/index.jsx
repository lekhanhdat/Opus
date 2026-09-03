import { useEffect, useState } from "react";
import Chart from "react-apexcharts";
import useStableCallback from "../../../../../Common/Hooks/useStableCallback";
import _ from "lodash";
import { DiscoverySizeRangeQueryMode } from "../../../Constants";
import { UnitConvertsionUtil } from "../../../Utils";

export const fontSizeLabel = {
    fontSize: 14,
    fontWeight: 400,
    fontFamily: "Open Sans",
    color: '#293037',
}

function SFFileTypeChart({ id, height, queryParameter, queryData, onChange }) {

    const [chartInfo, setChartInfo] = useState({
        options: {
            chart: {
                id,
                events: {
                    dataPointSelection: onColumnSelect,
                },
                toolbar: {
                    show: false,
                },
            },
            title: {
                text: RMResx.RM_FA_Progress_Unit_GB,
                align: 'left',
                offsetX: 6.5,
                offsetY: 12,
                style: fontSizeLabel,
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
            xaxis: {
                labels: {
                    style: fontSizeLabel,
                },
            },
            yaxis: {
                labels: {
                    style: fontSizeLabel,
                },
            },
            series: [
                {
                    name: RMResx.RM_FA_SF_Discovery_SizeRangeChart_Tooltip,
                    data: [],
                },
            ],
        },
        series: [
            {
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
            const { sizeRanges, unit } = await queryData(queryParameter);
            const data = sizeRanges.map((item) => ({
                x: item.name,
                y: item.fileTotalSize,
                meta: item.id,
            }));

            const clonedInfo = _.cloneDeep(chartInfo);
            clonedInfo.options.chart.events.dataPointSelection = onColumnSelect;
            clonedInfo.options.title.text = `(${UnitConvertsionUtil.GetUnitI18N(unit)})`;
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
        ApexCharts.exec(id, "updateSeries", chartInfo.series, false);
    }, [chartInfo]);


    const onColumnSelect = useStableCallback((e, chart, options) => {
        const clonedParameter = _.cloneDeep(queryParameter);
        const selectedIndex = options.selectedDataPoints[0][0];
        if (_.isNil(selectedIndex)) {
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
        <Chart
            options={chartInfo.options}
            series={chartInfo.series}
            type="bar"
            width="100%"
            height={height}
        />
    )
}

export default SFFileTypeChart