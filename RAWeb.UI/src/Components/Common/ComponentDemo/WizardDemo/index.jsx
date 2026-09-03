import React, { useState } from "react";
import Wizard from "../../Wizard";

export const WizardDemo = () => {
    const [activeStep, setActiveStep] = useState(1);
    const handleNext = () => {
        if (activeStep <= 3) {
            setActiveStep((prev) => prev + 1);
        }
    };

    const handleBack = () => {
        setActiveStep((prev) => (prev > 1 ? prev - 1 : 1));
    };

  	const handleChangeStep = (stepIndex) => {
		if (activeStep > stepIndex) {
			setActiveStep(stepIndex + 1);
		}
	};

    return (
        <div style={{ background: "#fff" }}>
            <Wizard
                headerName={"Progress"}
                activeStep={activeStep}
                items={[
                    { text: "Format selection" },
                    { text: "Metadata mapping" },
                    { text: "Export location" },
                    { text: "ASS location" },
                ]}
                onChange={handleChangeStep}
            >
                this is content
            </Wizard>
            <div style={{ textAlign: "right", margin: "24px 16px 0 0" }}>
                <R.Button onClick={handleNext} text={"next"} />
                <R.Button onClick={handleBack} text={"back"} />
            </div>
        </div>
    );
};
