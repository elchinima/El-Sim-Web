document.querySelectorAll("button, a").forEach((item) => {
    item.addEventListener("pointerdown", () => {
        item.classList.add("is-pressed");
    });

    item.addEventListener("pointerup", () => {
        item.classList.remove("is-pressed");
    });

    item.addEventListener("pointerleave", () => {
        item.classList.remove("is-pressed");
    });
});

const siteHeader = document.querySelector(".site-header");
const menuToggle = document.querySelector(".menu-toggle");
const navLinks = document.querySelector(".nav-links");

if (siteHeader && menuToggle && navLinks) {
    const setMenuState = (isOpen) => {
        siteHeader.classList.toggle("is-menu-open", isOpen);
        menuToggle.setAttribute("aria-expanded", String(isOpen));
        menuToggle.setAttribute("aria-label", isOpen ? "Close menu" : "Open menu");
    };

    menuToggle.addEventListener("click", () => {
        setMenuState(!siteHeader.classList.contains("is-menu-open"));
    });

    navLinks.querySelectorAll("a").forEach((link) => {
        link.addEventListener("click", () => {
            setMenuState(false);
        });
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            setMenuState(false);
        }
    });
}

const heroVisual = document.querySelector(".hero-visual");

if (heroVisual) {
    const setHeroAnimationState = (isVisible) => {
        heroVisual.classList.toggle("is-in-view", isVisible);
    };

    if ("IntersectionObserver" in window) {
        const heroObserver = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    setHeroAnimationState(entry.isIntersecting);
                });
            },
            {
                threshold: 0.45,
            }
        );

        heroObserver.observe(heroVisual);
    } else {
        setHeroAnimationState(true);
    }
}

const authPanel = document.querySelector(".auth-panel");

if (authPanel) {
    const tabs = [...authPanel.querySelectorAll(".auth-tab")];
    const forms = [...authPanel.querySelectorAll(".auth-form")];
    const finInputs = [...authPanel.querySelectorAll("[data-fin-input]")];
    const finPattern = /^[A-Za-z0-9]{7}$/;

    const showForm = (target) => {
        tabs.forEach((tab) => {
            tab.classList.toggle("is-active", tab.dataset.authTarget === target);
        });

        forms.forEach((form) => {
            const isActive = form.dataset.authForm === target;
            form.classList.toggle("is-active", isActive);
            form.setAttribute("aria-hidden", String(!isActive));
            form.inert = !isActive;
        });
    };

    showForm(tabs.find((tab) => tab.classList.contains("is-active"))?.dataset.authTarget || "login");

    finInputs.forEach((input) => {
        input.addEventListener("input", () => {
            input.value = input.value.replace(/[^A-Za-z0-9]/g, "").slice(0, 7).toUpperCase();
        });
    });

    tabs.forEach((tab) => {
        tab.addEventListener("click", () => {
            showForm(tab.dataset.authTarget);
        });
    });

    forms.forEach((form) => {
        const message = form.querySelector(".auth-message");

        form.addEventListener("submit", (event) => {
            event.preventDefault();
            const requiredInput = [...form.querySelectorAll("[required]")].find((input) => !input.value.trim());
            const finInput = form.querySelector("[data-fin-input]");
            const passwordInput = form.querySelector("[data-password-input]");
            const isRegister = form.dataset.authForm === "register";

            message.textContent = "";
            message.classList.remove("is-error");

            if (requiredInput) {
                const label = form.querySelector(`label[for="${requiredInput.id}"]`);
                message.textContent = `${label ? label.textContent : "This field"} is required.`;
                message.classList.add("is-error");
                requiredInput.focus();
                return;
            }

            if (!finPattern.test(finInput.value)) {
                message.textContent = "FIN must contain exactly 7 English letters or digits.";
                message.classList.add("is-error");
                finInput.focus();
                return;
            }

            if (isRegister && passwordInput.value.length < 8) {
                message.textContent = "Password must be at least 8 characters.";
                message.classList.add("is-error");
                passwordInput.focus();
                return;
            }

            message.textContent = isRegister
                ? "Registration data looks correct."
                : "Login data looks correct.";
            form.reset();
        });
    });
}
