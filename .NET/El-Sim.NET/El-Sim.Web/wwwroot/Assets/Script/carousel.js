document.querySelectorAll(".top-slider").forEach((slider) => {
    const sliderTrack = slider.querySelector(".slider-track");
    const dots = [...slider.querySelectorAll(".slider-dots span")];
    const slides = [...slider.querySelectorAll(".slider-slide")];

    if (!sliderTrack || slides.length === 0) {
      return;
    }

    let activeSlide = 0;
    let sliderTimer;
    let swipeStartX = 0;
    let swipeDeltaX = 0;
    let isSwiping = false;
    let isSliderVisible = false;

    const setSlide = (index) => {
      activeSlide = (index + slides.length) % slides.length;
      slider.dataset.active = activeSlide;
      sliderTrack.style.transform = `translateX(${-activeSlide * 100}%)`;
      dots.forEach((dot, dotIndex) => {
        dot.classList.remove("is-active");
        if (dotIndex === activeSlide) {
          void dot.offsetWidth;
          dot.classList.add("is-active");
        }
      });
    };

    const nextSlide = () => {
      setSlide(activeSlide + 1);
    };

    const startSliderTimer = () => {
      if (slides.length < 2 || slider.hidden) {
        return;
      }

      clearInterval(sliderTimer);
      slider.classList.add("is-running");
      sliderTimer = setInterval(nextSlide, 5000);
    };

    const stopSliderTimer = () => {
      clearInterval(sliderTimer);
      slider.classList.remove("is-running");
    };

    const restartSliderTimer = () => {
      if (!isSliderVisible) {
        return;
      }

      startSliderTimer();
    };

    const finishSwipe = (event) => {
      if (!isSwiping) {
        return;
      }

      if (event) {
        event.preventDefault();
      }

      slider.classList.remove("is-dragging");
      isSwiping = false;

      if (Math.abs(swipeDeltaX) > 60) {
        setSlide(activeSlide + (swipeDeltaX < 0 ? 1 : -1));
        restartSliderTimer();
      } else {
        setSlide(activeSlide);
      }
    };

    setSlide(0);

    if (slides.length < 2) {
      return;
    }

    window.addEventListener("elsim:slider-visibility", () => {
      if (slider.hidden) {
        stopSliderTimer();
        return;
      }

      setSlide(activeSlide);
      restartSliderTimer();
    });

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
      sliderTrack.style.transform = `translateX(calc(${-activeSlide * 100}% + ${swipeDeltaX}px))`;
    });

    slider.addEventListener("pointerup", finishSwipe);
    slider.addEventListener("pointercancel", finishSwipe);
    slider.addEventListener("lostpointercapture", finishSwipe);
    slider.addEventListener("dragstart", (event) => event.preventDefault());

    if ("IntersectionObserver" in window) {
      const sliderObserver = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            isSliderVisible = entry.isIntersecting;

            if (isSliderVisible) {
              startSliderTimer();
            } else {
              stopSliderTimer();
            }
          });
        },
        {
          threshold: 0.35,
        }
      );

      sliderObserver.observe(slider);
    } else {
      isSliderVisible = true;
      startSliderTimer();
    }
});
