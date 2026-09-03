import { SalesforceMutableDataCard } from "../index";
import "./index.less";

const TotalMutableData = ({ data }) => {
    return(
        <div className="reco-total-immutable-data">
            {data.map((item, index) => (
                <div key={index} tabIndex="0">
                    <SalesforceMutableDataCard text={item.text} value={item.value} />
                </div>
            ))}
        </div>
    );
};

export default TotalMutableData;
