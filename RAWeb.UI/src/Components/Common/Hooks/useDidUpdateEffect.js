import { useEffect, useRef } from "react";

export default (effect, dependencyList) => {

    const initRef = useRef(false);

    useEffect(() => {
        if(!initRef.current) {
            initRef.current = true;
            return;
        }

        effect();
    }, dependencyList);
};