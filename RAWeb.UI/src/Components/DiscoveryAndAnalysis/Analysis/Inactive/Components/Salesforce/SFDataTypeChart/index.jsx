import { useEffect, useState } from "react";
import Chart from "react-apexcharts";

import { fontSizeLabel } from "../../../../Components/FileTypeChart/Salesforce";
import { UnitConvertsionUtil } from "../../../../Utils";

function SFDataTypeChart({ id, height, queryParameter, queryData }) {
   
    const [chartInfo, setChartInfo] = useState({
        options: {
            chart: {
                id,
                toolbar: {
                    show: false,
                },
                zoom: {
                    enabled: false
                },
            },
            grid: {
                borderColor: "#E5EBEF",
                padding: {
                    top: 0,
                    right: 36,
                    bottom: 0,
                    left: 36,
                },
            },
            forecastDataPoints: {
                count: 0,
            },
            title: {
                text: RMResx.RM_FA_SF_Discovery_GrowthChart_CountUnit,
                align: 'left',
                offsetX: 32,
                offsetY: 32,
                style: fontSizeLabel,
            },
            subtitle: {
                text:RMResx.RM_FA_SF_Discovery_GrowthChart_StorageUnit,
                align: 'right',
                offsetX: -41.5,
                offsetY: 32,
                style: fontSizeLabel
            },
            stroke: {
                width: [3, 3],
                curve: 'smooth',
            },
            dataLabels: {
                enabled: false,
            },
            xaxis: {
                labels: {
                    style: fontSizeLabel,
                },
                categories: [],
                axisBorder: {
                    show: false,  // Disable the x-axis line
                },
                axisTicks: {
                    show: false,
                },
            },
            yaxis: [
                {
                    title: {
                        text: RMResx.RM_FA_SF_Inactive_SummaryTab_Data_Chart_Count,
                        offsetX: -20,
                        style: fontSizeLabel,
                    },
                    labels: {
                        style: fontSizeLabel,
                        formatter: function(val) {
                            return val.toFixed(0);
                        }
                    },
                    axisBorder: {
                        show: true,
                        width: 2,
                        color: "#F5BD24",
                        offsetX: 0,
                        offsetY: 0
                    },
                    axisTicks: {
                        show: true,
                        width: 6,
                        color: "#F5BD24",
                    },
                },
                {
                    opposite: true,
                    title: {
                        text: RMResx.RM_FA_SF_Inactive_SummaryTab_Data_Chart_StorageUsed,
                        offsetX: 20,
                        style: fontSizeLabel,
                    },
                    labels: {
                        style: fontSizeLabel,
                    },
                    axisBorder: {
                        show: true,
                        width: 2,
                        color: "#24BCA4",
                        offsetX: 0,
                        offsetY: 0
                    },
                    axisTicks: {
                        show: true,
                        width: 6,
                        color: "#24BCA4",
                    },
                },
            ],
            legend: {
                show: false,
            },
            colors: ['#F5BD24', '#24BCA4'],
            markers: {
                size: 4,
                colors: ["#fff", "#fff"],
                strokeColors: ['#F5BD24', '#24BCA4'],
                strokeWidth: 2,
                hover: {
                    size: 4,
                }
            },
        },
        series:[
            {
                name: RMResx.RM_FA_SF_Inactive_SummaryTab_Data_Chart_Count,
                data: [],
            },
            {
                name: RMResx.RM_FA_SF_Inactive_SummaryTab_Data_Chart_StorageUsed,
                data: [],
            },
        ]
    });

    const [categories, setCategories] =useState([])

    useEffect(() => {
        const fetchData = async () => {
            ApexCharts.exec(id, "resetSeries");
            const {items: figures, unit} = await queryData(queryParameter);
            const clonedFigures = _.cloneDeep(figures);
            const clonedInfo = _.cloneDeep(chartInfo);
            
            const recordCountLine = clonedFigures.map((item)=>item?.DataCreatedCount);
            const storageCountLine = clonedFigures.map((item)=>item?.TotalStorageUsed);
            const data = [
                {
                    name: RMResx.RM_FA_SF_Inactive_SummaryTab_Data_Chart_Count,
                    data: recordCountLine,
                },
                {
                    name: RMResx.RM_FA_SF_Inactive_SummaryTab_Data_Chart_StorageUsed,
                    data: storageCountLine,
                },
            ]

            clonedInfo.series = data;
            const categories = clonedFigures.map((item)=>item?.Year);
            const countForecast = clonedFigures.filter((item)=>!!item?.IsDashLine)?.length;

            clonedInfo.options.forecastDataPoints.count = countForecast;
            clonedInfo.options.subtitle.text = `(${UnitConvertsionUtil.GetUnitI18N(unit)})`;

            setCategories(categories);
            setChartInfo(clonedInfo);
        };

        fetchData();
    }, [queryParameter]);

    useEffect(() => {
        const cloneOptions = _.cloneDeep(chartInfo.options);
        cloneOptions.xaxis.categories = categories;
        ApexCharts.exec(
            id,
            "updateOptions",
            cloneOptions,
            false,
            false,
            true
        );
    }, [chartInfo.options,categories]);

    return (
        <Chart
            options={chartInfo.options}
            series={chartInfo?.series}
            type="line"
            width="100%"
            height={height}
        />
    );
}

export default SFDataTypeChart;