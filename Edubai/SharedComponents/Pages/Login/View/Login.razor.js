export function registerLoginCardEffects() {
    ///* -- Glow effect -- */

    //const blob = document.getElementById("blob");

    //window.onpointermove = event => {
    //    const { clientX, clientY } = event;
    //    let blobRect = blob.getBoundingClientRect();

    //    blob.animate({
    //        left: `${clientX - blobRect.width/2}px`,
    //        top: `${clientY - blobRect.height/2}px`
    //    }, { duration: 3000, fill: "forwards" });
    //}

    /* -- Text effect -- */

    const letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    let interval = null;

    const screen = document.querySelector(".screen"),
        name = document.querySelector(".screen__title-name");

    screen.onmouseenter = event => {
        let iteration = 0;

        clearInterval(interval);

        interval = setInterval(() => {
            name.innerText = name.innerText
                .split("")
                .map((letter, index) => {
                    if (index < iteration) {
                        return name.dataset.value[index];
                    }

                    return letters[Math.floor(Math.random() * 26)]
                })
                .join("");

            if (iteration >= name.dataset.value.length) {
                clearInterval(interval);
            }

            iteration += 1 / 3;
        }, 30);
    }
}