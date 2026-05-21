const supportChat = document.querySelector("[data-support-chat]");

if (supportChat) {
    const form = supportChat.querySelector("[data-chat-form]");
    const input = supportChat.querySelector("[data-chat-input]");
    const imageInput = supportChat.querySelector("[data-chat-image]");
    const messages = supportChat.querySelector("[data-chat-messages]");
    const sendButton = supportChat.querySelector(".support-chat-send");
    const sendButtonText = sendButton?.querySelector("span");
    const attachmentPreview = supportChat.querySelector("[data-attachment-preview]");
    const attachmentPreviewImage = attachmentPreview?.querySelector("img");
    const attachmentOpen = supportChat.querySelector("[data-attachment-open]");
    const attachmentRemove = supportChat.querySelector("[data-attachment-remove]");
    const attachmentDialog = supportChat.querySelector("[data-attachment-dialog]");
    const attachmentDialogImage = attachmentDialog?.querySelector("img");
    const attachmentClose = supportChat.querySelector("[data-attachment-close]");
    const historyLink = supportChat.querySelector(".support-chat-history");
    const statusBadge = document.querySelector("[data-support-status]");
    const statusText = document.querySelector("[data-support-status-text]");
    const waitingNotice = supportChat.querySelector("[data-chat-waiting]");
    const waitingText = supportChat.querySelector("[data-chat-waiting-text]");
    const agentLabel = supportChat.querySelector("[data-agent-label]");
    let agentName = supportChat.dataset.agentName || "";
    const storageKey = "elsim-language";
    const history = [];
    let isSending = false;
    let isClosed = false;
    let isCooldown = false;
    let isUploading = false;
    let closeTimer = null;
    let inactivityTimer = null;
    let cooldownTimer = null;
    let pendingImage = null;
    let hasConnectedOperator = false;
    const defaultSendText = sendButtonText?.textContent || "Send";

    const getLanguage = () => localStorage.getItem(storageKey) || "en";
    const supportTranslations = {
        en: {
            "Connection error": "Connection error"
        },
        ru: {
            "History": "\u0418\u0441\u0442\u043e\u0440\u0438\u044f",
            "No request": "\u041d\u0435\u0442 \u0437\u0430\u043f\u0440\u043e\u0441\u0430",
            "Waiting for operator": "\u041e\u0436\u0438\u0434\u0430\u043d\u0438\u0435 \u043e\u043f\u0435\u0440\u0430\u0442\u043e\u0440\u0430",
            "Online": "\u041e\u043d\u043b\u0430\u0439\u043d",
            "Please wait for operator connection": "\u041e\u0436\u0438\u0434\u0430\u0439\u0442\u0435 \u043f\u043e\u0434\u043a\u043b\u044e\u0447\u0435\u043d\u0438\u044f \u043e\u043f\u0435\u0440\u0430\u0442\u043e\u0440\u0430",
            "Send": "\u041e\u0442\u043f\u0440\u0430\u0432\u0438\u0442\u044c",
            "Send message": "\u041e\u0442\u043f\u0440\u0430\u0432\u0438\u0442\u044c \u0441\u043e\u043e\u0431\u0449\u0435\u043d\u0438\u0435",
            "Type your message": "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u0441\u043e\u043e\u0431\u0449\u0435\u043d\u0438\u0435",
            "Support is typing": "\u041f\u043e\u0434\u0434\u0435\u0440\u0436\u043a\u0430 \u043f\u0435\u0447\u0430\u0442\u0430\u0435\u0442",
            "Chat closes in": "\u0427\u0430\u0442 \u0437\u0430\u043a\u0440\u043e\u0435\u0442\u0441\u044f \u0447\u0435\u0440\u0435\u0437",
            "Chat closed": "\u0427\u0430\u0442 \u0437\u0430\u043a\u0440\u044b\u0442",
            "Connection error": "\u041e\u0448\u0438\u0431\u043a\u0430 \u043f\u043e\u0434\u043a\u043b\u044e\u0447\u0435\u043d\u0438\u044f",
            "Image must be up to 3 MB.": "\u0418\u0437\u043e\u0431\u0440\u0430\u0436\u0435\u043d\u0438\u0435 \u0434\u043e\u043b\u0436\u043d\u043e \u0431\u044b\u0442\u044c \u0434\u043e 3 \u041c\u0411.",
            "Only jpeg, jpg and png images are allowed.": "\u0414\u043e\u0441\u0442\u0443\u043f\u043d\u044b \u0442\u043e\u043b\u044c\u043a\u043e jpeg, jpg \u0438 png.",
            "Image could not be processed.": "\u041d\u0435 \u0443\u0434\u0430\u043b\u043e\u0441\u044c \u043e\u0431\u0440\u0430\u0431\u043e\u0442\u0430\u0442\u044c \u0438\u0437\u043e\u0431\u0440\u0430\u0436\u0435\u043d\u0438\u0435."
        },
        az: {
            "History": "Tarix\u00e7\u0259",
            "No request": "Sor\u011fu yoxdur",
            "Waiting for operator": "Operator g\u00f6zl\u0259nilir",
            "Online": "Onlayn",
            "Please wait for operator connection": "Operatorun qo\u015fulmas\u0131n\u0131 g\u00f6zl\u0259yin",
            "Send": "Gonder",
            "Send message": "Mesaj gonder",
            "Type your message": "Mesajinizi yazin",
            "Support is typing": "Destek yazir",
            "Chat closes in": "Cat baglanacaq",
            "Chat closed": "Cat baglandi",
            "Connection error": "Ba\u011flant\u0131 x\u0259tas\u0131",
            "Image must be up to 3 MB.": "Sekil 3 MB-a qeder olmalidir.",
            "Only jpeg, jpg and png images are allowed.": "Yalniz jpeg, jpg ve png sekillerine icaze verilir.",
            "Image could not be processed.": "Sekli emal etmek mumkun olmadi."
        }
    };
    const translate = (text) => supportTranslations[getLanguage()]?.[text] || window.getElsimTranslation?.(text, getLanguage()) || text;
    const wait = (milliseconds) => new Promise((resolve) => window.setTimeout(resolve, milliseconds));
    const statusLabels = {
        idle: "No request",
        waiting: "Waiting for operator",
        online: "Online"
    };

    const setSupportStatus = (state) => {
        if (!statusBadge || !statusText) {
            return;
        }

        statusBadge.classList.remove("is-idle", "is-waiting", "is-online");
        statusBadge.classList.add(`is-${state}`);
        statusText.textContent = translate(statusLabels[state] || statusLabels.idle);
    };

    const setWaitingNotice = (visible) => {
        if (!waitingNotice) {
            return;
        }

        waitingNotice.hidden = !visible;

        if (waitingText) {
            waitingText.textContent = translate("Please wait for operator connection");
        }

        if (visible) {
            input.placeholder = translate("Please wait for operator connection");
        } else {
            input.placeholder = translate("Type your message");
        }
    };

    const agentText = () => {
        const language = getLanguage();

        if (language === "ru") {
            return `\u0421 \u0432\u0430\u043c\u0438 \u0440\u0430\u0437\u0433\u043e\u0432\u0430\u0440\u0438\u0432\u0430\u0435\u0442: ${agentName}`;
        }

        if (language === "az") {
            return `Sizinle danisir: ${agentName}`;
        }

        return `You are chatting with: ${agentName}`;
    };

    const setAgentName = (name) => {
        if (!name) {
            return;
        }

        agentName = name;

        if (agentLabel) {
            agentLabel.hidden = false;
            agentLabel.textContent = agentText();
        }
    };

    const setInputText = () => {
        input.placeholder = translate("Type your message");
        sendButton?.setAttribute("title", translate("Send message"));
        sendButton?.setAttribute("aria-label", translate("Send message"));

        if (agentLabel && agentName) {
            agentLabel.hidden = false;
            agentLabel.textContent = agentText();
        }

        if (historyLink) {
            historyLink.textContent = translate("History");
        }

        if (waitingNotice && !waitingNotice.hidden) {
            setWaitingNotice(true);
        }

        setSupportStatus(hasConnectedOperator ? "online" : isSending ? "waiting" : "idle");

        if (!isCooldown && sendButtonText) {
            sendButtonText.textContent = translate(defaultSendText);
        }
    };

    const scrollToLatest = () => {
        messages.scrollTo({
            top: messages.scrollHeight,
            behavior: "smooth"
        });
    };

    const getTime = () => new Intl.DateTimeFormat(getLanguage(), {
        hour: "2-digit",
        minute: "2-digit"
    }).format(new Date());

    const icon = (type) => type === "user"
        ? `<svg viewBox="0 0 24 24"><path d="M20 21a8 8 0 0 0-16 0"></path><circle cx="12" cy="8" r="4"></circle></svg>`
        : `<svg viewBox="0 0 24 24"><path d="M4 12a8 8 0 0 1 16 0v4a2 2 0 0 1-2 2h-2"></path><path d="M9 19h6"></path><path d="M8 13h.01"></path><path d="M12 13h.01"></path><path d="M16 13h.01"></path></svg>`;

    const addMessage = (text, type, author = "", imageUrl = "") => {
        const message = document.createElement("article");
        message.className = `chat-message chat-message-${type}`;
        message.innerHTML = `
            <span class="chat-message-icon" aria-hidden="true">${icon(type)}</span>
            <div>
                ${author ? "<strong class=\"chat-message-author\"></strong>" : ""}
                ${imageUrl ? "<button class=\"chat-message-image\" type=\"button\"><img alt=\"Attached image\"></button>" : ""}
                ${text ? "<p></p>" : ""}
                <time>${getTime()}</time>
            </div>
        `;
        const authorElement = message.querySelector(".chat-message-author");
        if (authorElement) {
            authorElement.textContent = author;
        }
        const textElement = message.querySelector("p");
        if (textElement) {
            textElement.textContent = text;
        }
        const imageButton = message.querySelector(".chat-message-image");
        const image = imageButton?.querySelector("img");
        if (image && imageUrl) {
            image.src = imageUrl;
            imageButton.addEventListener("click", () => openAttachment(imageUrl));
        }
        messages.append(message);
        scrollToLatest();
    };

    const showTyping = () => {
        const typing = document.createElement("article");
        typing.className = "chat-message chat-message-bot chat-typing";
        typing.innerHTML = `
            <span class="chat-message-icon" aria-hidden="true">${icon("bot")}</span>
            <div aria-label="${translate("Support is typing")}">
                <i></i><i></i><i></i>
            </div>
        `;
        messages.append(typing);
        scrollToLatest();
        return typing;
    };

    const removeTyping = () => {
        messages.querySelectorAll(".chat-typing").forEach((typing) => typing.remove());
    };

    const updateSendState = () => {
        input.disabled = isSending || isClosed;
        sendButton.disabled = isSending || isClosed || isCooldown || isUploading;
        if (imageInput) {
            imageInput.disabled = isSending || isClosed || isCooldown || isUploading;
        }
    };

    const setSending = (value) => {
        isSending = value;
        updateSendState();
    };

    const startCooldown = (seconds = 20) => {
        if (cooldownTimer) {
            window.clearInterval(cooldownTimer);
            cooldownTimer = null;
        }

        isCooldown = true;
        updateSendState();

        let remaining = Math.max(1, Number(seconds) || 20);

        if (sendButtonText) {
            sendButtonText.textContent = `${remaining}s`;
        }

        cooldownTimer = window.setInterval(() => {
            remaining -= 1;

            if (remaining <= 0) {
                window.clearInterval(cooldownTimer);
                cooldownTimer = null;
                isCooldown = false;
                updateSendState();

                if (!isClosed && sendButtonText) {
                    sendButtonText.textContent = translate(defaultSendText);
                }

                return;
            }

            if (sendButtonText) {
                sendButtonText.textContent = `${remaining}s`;
            }
        }, 1000);
    };

    const closeChat = (delaySeconds = 60) => {
        if (closeTimer) {
            window.clearInterval(closeTimer);
            closeTimer = null;
        }

        if (cooldownTimer) {
            window.clearInterval(cooldownTimer);
            cooldownTimer = null;
        }

        if (inactivityTimer) {
            window.clearTimeout(inactivityTimer);
            inactivityTimer = null;
        }

        isClosed = true;
        isCooldown = false;
        setSending(false);
        fetch("/support/close", { method: "POST" }).catch(() => {});

        let seconds = Math.max(1, Number(delaySeconds) || 60);
        input.placeholder = `${translate("Chat closes in")} ${seconds}`;

        closeTimer = window.setInterval(() => {
            seconds -= 1;
            input.placeholder = seconds > 0 ? `${translate("Chat closes in")} ${seconds}` : translate("Chat closed");

            if (seconds <= 0) {
                window.clearInterval(closeTimer);
                closeTimer = null;
                form.hidden = true;
            }
        }, 1000);
    };

    const resetInactivityTimer = () => {
        if (isClosed) {
            return;
        }

        if (inactivityTimer) {
            window.clearTimeout(inactivityTimer);
        }

        inactivityTimer = window.setTimeout(() => closeChat(1), 5 * 60 * 1000);
    };

    const restoreActiveChat = async () => {
        try {
            const response = await fetch("/support/history", {
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                return;
            }

            const result = await response.json();
            const savedHistory = Array.isArray(result.history) ? result.history : [];

            if (!result.isActive || savedHistory.length === 0) {
                return;
            }

            setAgentName(result.agentName);
            hasConnectedOperator = true;
            messages.replaceChildren();
            history.splice(0, history.length);

            savedHistory.forEach((message) => {
                const role = message.role === "model" ? "model" : "user";
                const text = message.text || "";
                const imageUrl = message.imageUrl || "";

                if (!text.trim() && !imageUrl) {
                    return;
                }

                addMessage(text, role === "model" ? "bot" : "user", role === "model" ? result.agentName || agentName : "", imageUrl);
                history.push({ role, text, imageUrl });
            });

            while (history.length > 10) {
                history.shift();
            }

            setWaitingNotice(false);
            setSupportStatus("online");
        } catch {
        }
    };

    const openAttachment = (url) => {
        if (!attachmentDialog || !attachmentDialogImage || !url) {
            return;
        }

        attachmentDialogImage.src = url;
        attachmentDialog.showModal();
    };

    const setPendingImage = (image) => {
        pendingImage = image;

        if (!attachmentPreview || !attachmentPreviewImage) {
            return;
        }

        if (!image) {
            attachmentPreview.hidden = true;
            attachmentPreviewImage.removeAttribute("src");
            return;
        }

        attachmentPreviewImage.src = image.url;
        attachmentPreview.hidden = false;
    };

    const deletePendingImage = async () => {
        if (!pendingImage) {
            return;
        }

        const image = pendingImage;
        setPendingImage(null);

        try {
            await fetch(`/support/image/${image.id}`, {
                method: "DELETE"
            });
        } catch {
        }
    };

    imageInput?.addEventListener("change", async () => {
        const file = imageInput.files?.[0];

        if (!file) {
            return;
        }

        if (file.size > 3 * 1024 * 1024) {
            addMessage(translate("Image must be up to 3 MB."), "bot", agentName);
            imageInput.value = "";
            return;
        }

        if (!["image/jpeg", "image/jpg", "image/png"].includes(file.type)) {
            addMessage(translate("Only jpeg, jpg and png images are allowed."), "bot", agentName);
            imageInput.value = "";
            return;
        }

        await deletePendingImage();
        isUploading = true;
        updateSendState();

        try {
            const formData = new FormData();
            formData.append("image", file);

            const response = await fetch("/support/image", {
                method: "POST",
                body: formData
            });

            if (!response.ok) {
                const error = await response.json().catch(() => ({}));
                throw new Error(error.message || "Image could not be processed.");
            }

            const result = await response.json();
            setPendingImage({ id: result.id, url: result.url });
        } catch (error) {
            addMessage(translate(error.message || "Image could not be processed."), "bot", agentName);
        } finally {
            isUploading = false;
            imageInput.value = "";
            updateSendState();
        }
    });

    attachmentOpen?.addEventListener("click", () => openAttachment(pendingImage?.url));
    attachmentRemove?.addEventListener("click", deletePendingImage);
    attachmentClose?.addEventListener("click", () => attachmentDialog?.close());
    attachmentDialog?.addEventListener("click", (event) => {
        if (event.target === attachmentDialog) {
            attachmentDialog.close();
        }
    });

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        if (isSending || isClosed || isCooldown || isUploading) {
            return;
        }

        const text = input.value.trim();
        const image = pendingImage;

        if (!text && !image) {
            input.focus();
            return;
        }

        const isFirstRequest = !hasConnectedOperator;
        addMessage(text, "user", "", image?.url || "");
        resetInactivityTimer();
        input.value = "";
        setPendingImage(null);
        setSending(true);

        if (isFirstRequest) {
            setWaitingNotice(true);
            setSupportStatus("waiting");
        }

        let typing = null;

        try {
            await wait(20000);
            typing = showTyping();
            const response = await fetch("/support/send", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    message: text,
                    imageId: image?.id || null,
                    history: history.slice(-10),
                    language: getLanguage()
                })
            });

            if (!response.ok) {
                throw new Error("Support request failed");
            }

            const result = await response.json();
            const reply = result.hasConnectionError ? translate("Connection error") : result.reply;
            setAgentName(result.agentName);
            hasConnectedOperator = true;
            setWaitingNotice(false);
            setSupportStatus("online");
            typing.remove();
            if (reply) {
                addMessage(reply, "bot", result.agentName || agentName);
            }
            history.push({ role: "user", text, imageUrl: image?.url || "" });
            if (reply && !result.hasConnectionError) {
                history.push({ role: "model", text: reply });
            }

            while (history.length > 10) {
                history.shift();
            }

            if (result.closeChat) {
                closeChat(result.closeAfterSeconds || 60);
                return;
            }

            startCooldown(20);
            resetInactivityTimer();
        } catch {
            typing?.remove();
            setWaitingNotice(false);
            setSupportStatus(hasConnectedOperator ? "online" : "idle");
            addMessage(translate("Connection error"), "bot", agentName);
            startCooldown(20);
            resetInactivityTimer();
        } finally {
            if (!isClosed) {
                setSending(false);
                input.focus();
            }
        }
    });

    document.querySelectorAll("[data-lang-button]").forEach((button) => {
        button.addEventListener("click", () => window.setTimeout(setInputText, 0));
    });

    setInputText();
    setWaitingNotice(false);
    setSupportStatus("idle");
    updateSendState();
    resetInactivityTimer();
    restoreActiveChat();
}
