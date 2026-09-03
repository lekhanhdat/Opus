import MutableDataCard from "../../../../Components/MutableDataCard";

import "./index.less";

function TotalMutableData({ data }) {
    return (
        <div className="reco-total-immutable-data">
            {data.map((item, index) => (
                <div key={index} tabIndex="0">
                    <MutableDataCard text={item.text} value={item.value} />
                </div>
            ))}
        </div>
    );
}

export default TotalMutableData;
