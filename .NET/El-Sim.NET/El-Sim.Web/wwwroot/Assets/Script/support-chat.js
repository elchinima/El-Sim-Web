const supportChat = document.querySelector("[data-support-chat]");

if (supportChat) {
    const form = supportChat.querySelector("[data-chat-form]");
    const input = supportChat.querySelector("[data-chat-input]");
    const messages = supportChat.querySelector("[data-chat-messages]");
    const sendButton = supportChat.querySelector(".support-chat-send");
    const sendButtonText = sendButton?.querySelector("span");
    const historyButton = supportChat.querySelector("[data-chat-history-toggle]");
    const aiBadge = supportChat.querySelector("[data-ai-badge]");
    const agentLabel = supportChat.querySelector("[data-agent-label]");
    let agentName = supportChat.dataset.agentName || "";
    const storageKey = "elsim-language";
    const history = [];
    let isSending = false;
    let isClosed = false;
    let isCooldown = false;
    let closeTimer = null;
    let inactivityTimer = null;
    let cooldownTimer = null;
    const defaultSendText = sendButtonText?.textContent || "Send";

    const getLanguage = () => localStorage.getItem(storageKey) || "en";
    const supportTranslations = {
        ru: {
            "AI assistant": "AI \u0430\u0441\u0441\u0438\u0441\u0442\u0435\u043d\u0442",
            "History": "\u0418\u0441\u0442\u043e\u0440\u0438\u044f",
            "Send": "\u041e\u0442\u043f\u0440\u0430\u0432\u0438\u0442\u044c",
            "Send message": "\u041e\u0442\u043f\u0440\u0430\u0432\u0438\u0442\u044c \u0441\u043e\u043e\u0431\u0449\u0435\u043d\u0438\u0435",
            "Type your message": "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u0441\u043e\u043e\u0431\u0449\u0435\u043d\u0438\u0435",
            "Support is typing": "\u041f\u043e\u0434\u0434\u0435\u0440\u0436\u043a\u0430 \u043f\u0435\u0447\u0430\u0442\u0430\u0435\u0442",
            "Chat closes in": "\u0427\u0430\u0442 \u0437\u0430\u043a\u0440\u043e\u0435\u0442\u0441\u044f \u0447\u0435\u0440\u0435\u0437",
            "Chat closed": "\u0427\u0430\u0442 \u0437\u0430\u043a\u0440\u044b\u0442",
            "Support is temporarily unavailable.": "\u041f\u043e\u0434\u0434\u0435\u0440\u0436\u043a\u0430 \u0432\u0440\u0435\u043c\u0435\u043d\u043d\u043e \u043d\u0435\u0434\u043e\u0441\u0442\u0443\u043f\u043d\u0430.",
            "No chat history yet.": "\u0418\u0441\u0442\u043e\u0440\u0438\u0438 \u0447\u0430\u0442\u0430 \u043f\u043e\u043a\u0430 \u043d\u0435\u0442.",
            "Chat history is temporarily unavailable.": "\u0418\u0441\u0442\u043e\u0440\u0438\u044f \u0447\u0430\u0442\u0430 \u0432\u0440\u0435\u043c\u0435\u043d\u043d\u043e \u043d\u0435\u0434\u043e\u0441\u0442\u0443\u043f\u043d\u0430."
        },
        az: {
            "AI assistant": "AI assistent",
            "History": "Tarixce",
            "Send": "Gonder",
            "Send message": "Mesaj gonder",
            "Type your message": "Mesajinizi yazin",
            "Support is typing": "Destek yazir",
            "Chat closes in": "Cat baglanacaq",
            "Chat closed": "Cat baglandi",
            "Support is temporarily unavailable.": "Destek muveqqeti olaraq elcatan deyil.",
            "No chat history yet.": "Hele cat tarixcesi yoxdur.",
            "Chat history is temporarily unavailable.": "Cat tarixcesi muveqqeti olaraq elcatan deyil."
        }
    };
    const translate = (text) => supportTranslations[getLanguage()]?.[text] || window.getElsimTranslation?.(text, getLanguage()) || text;
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

        if (historyButton) {
            historyButton.textContent = translate("History");
        }

        if (aiBadge) {
            aiBadge.textContent = translate("AI assistant");
        }

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

    const addMessage = (text, type, author = "") => {
        const message = document.createElement("article");
        message.className = `chat-message chat-message-${type}`;
        message.innerHTML = `
            <span class="chat-message-icon" aria-hidden="true">${icon(type)}</span>
            <div>
                ${author ? "<strong class=\"chat-message-author\"></strong>" : ""}
                <p></p>
                <time>${getTime()}</time>
            </div>
        `;
        const authorElement = message.querySelector(".chat-message-author");
        if (authorElement) {
            authorElement.textContent = author;
        }
        message.querySelector("p").textContent = text;
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

    const setSending = (value) => {
        isSending = value;
        input.disabled = isClosed;
        sendButton.disabled = value || isClosed || isCooldown;
    };

    const startCooldown = (seconds = 20) => {
        if (cooldownTimer) {
            window.clearInterval(cooldownTimer);
            cooldownTimer = null;
        }

        isCooldown = true;
        sendButton.disabled = true;

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

                if (!isClosed) {
                    sendButton.disabled = false;

                    if (sendButtonText) {
                        sendButtonText.textContent = translate(defaultSendText);
                    }
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
        input.disabled = true;
        sendButton.disabled = true;

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

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        if (isSending || isClosed || isCooldown) {
            return;
        }

        const text = input.value.trim();

        if (!text) {
            input.focus();
            return;
        }

        addMessage(text, "user");
        resetInactivityTimer();
        input.value = "";
        input.focus();

        const typing = showTyping();
        setSending(true);

        try {
            const response = await fetch("/support/send", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    message: text,
                    history: history.slice(-10),
                    language: getLanguage()
                })
            });

            if (!response.ok) {
                throw new Error("Support request failed");
            }

            const result = await response.json();
            const reply = result.reply || translate("Support is temporarily unavailable.");
            setAgentName(result.agentName);
            typing.remove();
            addMessage(reply, "bot", result.agentName || agentName);
            history.push({ role: "user", text });
            history.push({ role: "model", text: reply });

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
            typing.remove();
            addMessage(translate("Support is temporarily unavailable."), "bot", agentName);
            startCooldown(20);
            resetInactivityTimer();
        } finally {
            if (!isClosed) {
                setSending(false);
                input.focus();
            }
        }
    });

    historyButton?.addEventListener("click", async () => {
        if (historyButton.disabled) {
            return;
        }

        historyButton.disabled = true;

        try {
            const response = await fetch("/support/history", {
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("History request failed");
            }

            const result = await response.json();
            const savedHistory = Array.isArray(result.history) ? result.history : [];
            setAgentName(result.agentName);

            messages.replaceChildren();
            history.splice(0, history.length);

            if (savedHistory.length === 0) {
                addMessage(translate("No chat history yet."), "bot", result.agentName || agentName);
                return;
            }

            savedHistory.forEach((message) => {
                const role = message.role === "model" ? "model" : "user";
                const text = message.text || "";

                if (!text.trim()) {
                    return;
                }

                addMessage(text, role === "model" ? "bot" : "user", role === "model" ? result.agentName || agentName : "");
                history.push({ role, text });
            });
        } catch {
            addMessage(translate("Chat history is temporarily unavailable."), "bot", agentName);
        } finally {
            historyButton.disabled = false;
        }
    });

    document.querySelectorAll("[data-lang-button]").forEach((button) => {
        button.addEventListener("click", () => window.setTimeout(setInputText, 0));
    });

    setInputText();
    resetInactivityTimer();
}
