import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import "./index.less";

import EmptyContent from "../EmptyContent";

const Colors = ["#5ACEBB", "#F7A100", "#F57367"];

const QueryParam = [
    "/Root/PRM/MyRequest?source=2",
    "/Root/PRM/MyRequest?source=3",
    "/Root/PRM/MyRequest?source=4",
];

const LabelValue = ({ color, label, value }) => {


    return (
        <div className="reco-phy-request-lv-wrapper">
            <div className="reco-phy-request-lv-color" style={{ backgroundColor: color }}></div>
            <div className="reco-phy-request-lv">
                <div className="reco-phy-request-label" tabIndex="0">
                    {label}
                </div>
                <div className="reco-phy-request-value" data-tooltip="ifneed"
                    aria-label={value}
                    tabIndex="0">
                    {value}
                </div>
            </div>
        </div>
    );
};

LabelValue.propTypes = {
    color: PropTypes.string,
    label: PropTypes.string,
    value: PropTypes.number,
};
const LabelValueLink = ({ color, label, value, Link }) => {


    return (
        <div className="reco-phy-request-lv-wrapper">
            <div className="reco-phy-request-lv-color" style={{ backgroundColor: color }}></div>
            <div className="reco-phy-request-lv">
                <div className="reco-phy-request-label" tabIndex="0">
                    {label}
                </div>
                <div className="reco-phy-request-value" data-tooltip="ifneed">
                    <a className={"highlight"} aria-label={value} href={Link}>{value}</a>
                </div>
            </div>
        </div>
    );
};

LabelValueLink.propTypes = {
    color: PropTypes.string,
    label: PropTypes.string,
    value: PropTypes.number,
    Link: PropTypes.string,
};

const RequestOption = {
    url: "/api/Dashboard/GetPhysicalRequestsByPhysicalExplorer",
    data: 4
};

const Request = () => {

    const [datas, setDatas] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const responseDatas = await fetchUtility(RequestOption);
            setDatas(responseDatas);
        };
        fetchData();
    }, []);

    const checkIsEmpty = () => {
        return datas.reduce((prev, cur) => prev + cur.value, 0) === 0;
    };

    return (
        <div className="reco-phy-request-wrapper">
            <section className="reco-phy-report-section-title" tabIndex="0">
                {RMResx.RM_Phy_DSB_Title_Request}
            </section>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-phy-request-chart">
                    <div className="reco-phy-request-inner">
                        <R.Charts height={200}>
                            <R.Charts.Pie
                                type="donut"
                                items={datas}
                                color={Colors}
                                thickness="20"
                            />
                        </R.Charts>
                        <div className="reco-phy-request-inner-content">
                            <div className="reco-phy-request-inner-name" tabIndex="0">{RMResx.RM_Phy_DSB_Total_Request}</div>
                            <div className="reco-phy-request-inner-value"
                                data-tooltip="ifneed"
                                aria-label={datas.reduce((prev, cur) => prev + cur.value, 0)}
                                tabIndex="0"
                            >{datas.reduce((prev, cur) => prev + cur.value, 0)}</div>
                        </div>
                    </div>
                    <div className="reco-phy-request-lable-values">
                        {
                            datas.map((item, index) =>
                                <LabelValueLink
                                    key={index}
                                    color={Colors[index]}
                                    label={item.name}
                                    value={item.value}
                                    Link={QueryParam[index]}
                                />
                            )
                        }
                    </div>
                </div>
            </EmptyContent>
        </div>
    );
};

export default Request;