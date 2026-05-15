const slider = document.querySelector(".top-slider");
if (slider) {
    const sliderTrack = document.querySelector(".slider-track");
    const dots = [...document.querySelectorAll(".slider-dots span")];
    let activeSlide = 0;
    let sliderTimer;
    let swipeStartX = 0;
    let swipeDeltaX = 0;
    let isSwiping = false;

    const setSlide = (index) => {
      activeSlide = (index + dots.length) % dots.length;
      slider.dataset.active = activeSlide;
      dots.forEach((dot, dotIndex) => {
        dot.classList.remove("is-active");
        if (dotIndex === activeSlide) {
          void dot.offsetWidth;
          dot.classList.add("is-active");
        }
      });
    };

    const nextSlide = () => {
      setSlide((activeSlide + 1) % dots.length);
    };

    const restartSliderTimer = () => {
      clearInterval(sliderTimer);
      sliderTimer = setInterval(nextSlide, 5000);
    };

    const finishSwipe = (event) => {
      if (!isSwiping) {
        return;
      }

      if (event) {
        event.preventDefault();
      }

      slider.classList.remove("is-dragging");
      sliderTrack.style.transform = "";
      isSwiping = false;

      if (Math.abs(swipeDeltaX) > 60) {
        setSlide(activeSlide + (swipeDeltaX < 0 ? 1 : -1));
        restartSliderTimer();
      }
    };

    slider.addEventListener("pointerdown", (event) => {
      event.preventDefault();
      isSwiping = true;
      swipeStartX = event.clientX;
      swipeDeltaX = 0;
      slider.classList.add("is-dragging");
      slider.setPointerCapture(event.pointerId);
    });

    slider.addEventListener("pointermove", (event) => {
      if (!isSwiping) {
        return;
      }

      event.preventDefault();
      swipeDeltaX = event.clientX - swipeStartX;
      sliderTrack.style.transform = `translateX(calc(${-activeSlide * 33.3333}% + ${swipeDeltaX}px))`;
    });

    slider.addEventListener("pointerup", finishSwipe);
    slider.addEventListener("pointercancel", finishSwipe);
    slider.addEventListener("lostpointercapture", finishSwipe);
    slider.addEventListener("dragstart", (event) => event.preventDefault());

    restartSliderTimer();
}
