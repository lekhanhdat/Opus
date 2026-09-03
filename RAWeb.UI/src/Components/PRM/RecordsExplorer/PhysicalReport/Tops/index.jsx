import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import "./index.less";

import EmptyContent from "../EmptyContent";

const LableValue = ({ label, value, tooltip, index }) => {
    return (
        <div className="reco-phy-lablevalue-wrapper"
            style={{ backgroundColor: index % 2 === 0 ? "#F2F3F4" : "#FFF" }}>
            <div tabIndex="0"
                data-tooltip
                aria-label={tooltip}
            >
                {label}
            </div>
            <div tabIndex="0"
                data-tooltip="ifneed"
                aria-label={value}
            >
                {value}
            </div>
        </div>
    );
};

LableValue.propTypes = {
    label: PropTypes.string,
    value: PropTypes.number,
    tooltip: PropTypes.string,
    index: PropTypes.number,
};

const LocationDatasRequestOption = {
    url: "/api/Dashboard/GetTop10MostUsedSites",
    data: 4,
};

const TopLocations = () => {

    const [datas, setDatas] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const responseDatas = await fetchUtility(LocationDatasRequestOption);
            setDatas(responseDatas.slice(0, 5));
        };
        fetchData();
    }, []);

    const checkIsEmpty = () => {
        return datas.reduce((prev, cur) => prev + cur.Active, 0) === 0;
    };

    return (
        <div className="reco-phy-top-locations-wrapper">
            <section className="reco-phy-report-section-title" tabIndex="0">
                {RMResx.RM_Phy_DSB_Title_TopLocations}
            </section>
            <EmptyContent isEmpty={checkIsEmpty()}>
                {
                    datas.map((item, index) =>
                        <LableValue
                            key={index}
                            label={item.Title}
                            value={item.Active}
                            tooltip={item.Path}
                            index={index}
                        />
                    )
                }
            </EmptyContent>
        </div>
    );
};

const TermsDatasRequestOption = {
    url: "/api/Dashboard/GetTop10TermUsages",
    data: 4,
};

const TopTerms = () => {

    const [datas, setDatas] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const responseDatas = await fetchUtility(TermsDatasRequestOption);
            setDatas(responseDatas.slice(0, 5));
        };
        fetchData();
    }, []);

    const checkIsEmpty = () => {
        return datas.reduce((prev, cur) => prev + cur.Active, 0) === 0;
    };

    return (
        <div className="reco-phy-top-locations-wrapper">
            <section className="reco-phy-report-section-title" tabIndex="0">
                {RMResx.RM_Phy_DSB_Title_TopTerms}
            </section>
            <EmptyContent isEmpty={checkIsEmpty()}>
                {
                    datas.map((item, index) =>
                        <LableValue
                            key={index}
                            label={item.TermName}
                            value={item.Active}
                            tooltip={item.TermFullPath}
                            index={index}
                        />
                    )
                }
            </EmptyContent>
        </div>
    );
};

export { TopLocations, TopTerms };