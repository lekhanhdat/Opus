import "./index.less";
import SFMutableDataCard from "../../../../../Analysis/Components/MutableDataCard/Salesforce";

const TotalMutableData = ({ data }) => {
    return(
        <div className="reco-total-immutable-data">
            {data.map((item, index) => (
                <div key={index} tabIndex="0">
                    <SFMutableDataCard text={item.text} value={item.value} />
                </div>
            ))}
        </div>
    );
};

export default TotalMutableData;
