import { useEffect, useRef } from "react";

const useDidUpdateEffect = (effect, deps) => {
    const initRef = useRef(false);

    useEffect(() => {
        if(!initRef.current) {
            initRef.current = true;
            return;
        }
        effect();
    }, deps);
};

export default useDidUpdateEffect;