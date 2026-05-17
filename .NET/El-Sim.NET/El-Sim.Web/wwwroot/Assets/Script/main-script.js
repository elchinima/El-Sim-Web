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
            const requiredInput = [...form.querySelectorAll("[required]")].find((input) => !input.value.trim());
            const finInput = form.querySelector("[data-fin-input]");
            const passwordInput = form.querySelector("[data-password-input]");
            const isRegister = form.dataset.authForm === "register";

            message.textContent = "";
            message.classList.remove("is-error");

            if (requiredInput) {
                event.preventDefault();
                const label = form.querySelector(`label[for="${requiredInput.id}"]`);
                message.textContent = `${label ? label.textContent : "This field"} is required.`;
                message.classList.add("is-error");
                requiredInput.focus();
                return;
            }

            if (!finPattern.test(finInput.value)) {
                event.preventDefault();
                message.textContent = "FIN must contain exactly 7 English letters or digits.";
                message.classList.add("is-error");
                finInput.focus();
                return;
            }

            if (isRegister && passwordInput.value.length < 8) {
                event.preventDefault();
                message.textContent = "Password must be at least 8 characters.";
                message.classList.add("is-error");
                passwordInput.focus();
                return;
            }
        });
    });
}

document.querySelectorAll(".file-drop-zone").forEach((dropZone) => {
    const input = dropZone.querySelector('input[type="file"]');
    const title = dropZone.querySelector(".file-drop-title");
    const text = dropZone.querySelector(".file-drop-text");
    const preview = dropZone.querySelector(".file-drop-preview");
    const clearButton = dropZone.closest("form")?.querySelector("[data-clear-file]");
    const defaultTitle = title?.textContent || "";
    const defaultText = text?.textContent || "";
    const defaultPreview = preview?.style.backgroundImage || "";
    const maxFileSize = 2 * 1024 * 1024;
    let previewUrl = "";

    if (!input) {
        return;
    }

    const setFileName = () => {
        if (input.files?.length && input.files[0].size > maxFileSize) {
            clearFile();
            if (title) {
                title.textContent = "File is larger than 2 MB";
            }
            return;
        }

        if (title && input.files?.length) {
            title.textContent = input.files[0].name;
        }

        if (text && input.files?.length) {
            text.textContent = "Hover to preview";
        }

        if (preview && input.files?.length) {
            if (previewUrl) {
                URL.revokeObjectURL(previewUrl);
            }

            previewUrl = URL.createObjectURL(input.files[0]);
            preview.style.backgroundImage = `url("${previewUrl}")`;
        }
    };

    const clearFile = () => {
        input.value = "";

        if (previewUrl) {
            URL.revokeObjectURL(previewUrl);
            previewUrl = "";
        }

        if (title) {
            title.textContent = defaultTitle;
        }

        if (text) {
            text.textContent = defaultText;
        }

        if (preview) {
            preview.style.backgroundImage = defaultPreview;
        }
    };

    ["dragenter", "dragover"].forEach((eventName) => {
        dropZone.addEventListener(eventName, (event) => {
            event.preventDefault();
            dropZone.classList.add("is-dragover");
        });
    });

    ["dragleave", "drop"].forEach((eventName) => {
        dropZone.addEventListener(eventName, () => {
            dropZone.classList.remove("is-dragover");
        });
    });

    dropZone.addEventListener("drop", (event) => {
        event.preventDefault();

        if (event.dataTransfer?.files?.length) {
            input.files = event.dataTransfer.files;
            setFileName();
        }
    });

    input.addEventListener("change", setFileName);

    clearButton?.addEventListener("click", clearFile);
});

const getConfirmDialog = () => {
    let dialog = document.querySelector("[data-confirm-modal]");

    if (dialog) {
        return dialog;
    }

    dialog = document.createElement("dialog");
    dialog.className = "profile-confirm-modal";
    dialog.dataset.confirmModal = "";
    dialog.innerHTML = `
        <form method="dialog" class="profile-confirm-box">
            <h2 data-confirm-title>Confirm action?</h2>
            <p data-confirm-text></p>
            <div class="profile-confirm-actions">
                <button class="button button-secondary" type="button" value="cancel" data-confirm-cancel>Cancel</button>
                <button class="button button-primary" type="button" value="confirm" data-confirm-submit>Confirm</button>
            </div>
        </form>
    `;
    document.body.append(dialog);

    return dialog;
};

const showConfirmModal = ({ title = "Confirm action?", message, button = "Confirm" }) => {
    const dialog = getConfirmDialog();
    const titleElement = dialog.querySelector("[data-confirm-title]");
    const textElement = dialog.querySelector("[data-confirm-text]");
    const cancelButton = dialog.querySelector("[data-confirm-cancel]");
    const submitButton = dialog.querySelector("[data-confirm-submit]");

    titleElement.textContent = title;
    textElement.textContent = message;
    submitButton.textContent = button;

    return new Promise((resolve) => {
        const cleanup = () => {
            cancelButton.removeEventListener("click", cancel);
            submitButton.removeEventListener("click", confirm);
            dialog.removeEventListener("cancel", cancel);
            dialog.removeEventListener("close", close);
        };
        const close = () => {
            cleanup();
            resolve(dialog.returnValue === "confirm");
        };
        const cancel = (event) => {
            event?.preventDefault();
            dialog.returnValue = "cancel";
            dialog.close("cancel");
        };
        const confirm = () => {
            dialog.returnValue = "confirm";
            dialog.close("confirm");
        };

        cancelButton.addEventListener("click", cancel);
        submitButton.addEventListener("click", confirm);
        dialog.addEventListener("cancel", cancel);
        dialog.addEventListener("close", close);
        dialog.showModal();
    });
};

window.showConfirmModal = showConfirmModal;

document.querySelectorAll("[data-open-confirm-form]").forEach((button) => {
    button.addEventListener("click", async () => {
        const form = document.getElementById(button.dataset.openConfirmForm);

        if (!form) {
            return;
        }

        const confirmed = await showConfirmModal({
            title: button.dataset.confirmTitle || "Confirm action?",
            message: button.dataset.confirmMessage,
            button: button.dataset.confirmButton || "Confirm"
        });

        if (confirmed) {
            form.requestSubmit();
        }
    });
});

document.querySelectorAll("[data-confirm-message]").forEach((form) => {
    form.addEventListener("submit", async (event) => {
        if (form.dataset.confirmed === "true") {
            delete form.dataset.confirmed;
            return;
        }

        event.preventDefault();

        const confirmed = await showConfirmModal({
            title: form.dataset.confirmTitle || "Confirm action?",
            message: form.dataset.confirmMessage,
            button: form.dataset.confirmButton || "Confirm"
        });

        if (confirmed) {
            form.dataset.confirmed = "true";
            form.requestSubmit();
        }
    });
});

document.querySelectorAll("[data-confirm-two-factor]").forEach((form) => {
    const checkbox = form.querySelector('input[name="IsTwoFactorEnabled"][type="checkbox"]');

    if (!checkbox) {
        return;
    }

    form.addEventListener("submit", async (event) => {
        if (form.dataset.confirmed === "true") {
            delete form.dataset.confirmed;
            return;
        }

        const original = form.dataset.twoFactorOriginal === "true";

        if (checkbox.checked === original) {
            return;
        }

        const message = checkbox.checked ? "Enable 2FA for your account?" : "Disable 2FA for your account?";
        const button = checkbox.checked ? "Enable 2FA" : "Disable 2FA";

        event.preventDefault();

        const confirmed = await showConfirmModal({
            message,
            button
        });

        if (confirmed) {
            form.dataset.confirmed = "true";
            form.requestSubmit();
        }
    });
});
