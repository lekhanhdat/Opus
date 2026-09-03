import "./index.less";
import MutableDataCard from "../../../../Components/MutableDataCard";

const TotalMutableData = ({ data, isSalesforce }) => {
    return(
        <div className="reco-total-immutable-data">
            {data.map((item, index) => (
                <div key={index} tabIndex="0">
                    <MutableDataCard text={item.text} value={item.value} isSalesforce={isSalesforce} />
                </div>
            ))}
        </div>
    );
};

export default TotalMutableData;
