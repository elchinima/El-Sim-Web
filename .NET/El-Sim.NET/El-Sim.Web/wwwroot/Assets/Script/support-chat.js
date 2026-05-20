const supportChat = document.querySelector("[data-support-chat]");

if (supportChat) {
    const form = supportChat.querySelector("[data-chat-form]");
    const input = supportChat.querySelector("[data-chat-input]");
    const messages = supportChat.querySelector("[data-chat-messages]");
    const storageKey = "elsim-language";
    const replies = [
        "Thanks, we are checking this in the simulator. Please keep your phone near a stable internet connection.",
        "If this is about a QR code, open your phone eSIM settings and make sure the code has not been used before.",
        "For balance or plan questions, we can help you compare the current package with the best next option.",
        "We will stay here while you try it. Send another message if something still does not work."
    ];

    let replyIndex = 0;

    const getLanguage = () => localStorage.getItem(storageKey) || "en";
    const translate = (text) => window.getElsimTranslation?.(text, getLanguage()) || text;

    const setInputText = () => {
        input.placeholder = translate("Type your message");
        supportChat.querySelector(".support-chat-send")?.setAttribute("title", translate("Send message"));
        supportChat.querySelector(".support-chat-send")?.setAttribute("aria-label", translate("Send message"));
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

    const addMessage = (text, type) => {
        const message = document.createElement("article");
        message.className = `chat-message chat-message-${type}`;
        message.innerHTML = `
            <span class="chat-message-icon" aria-hidden="true">${icon(type)}</span>
            <div>
                <p></p>
                <time>${getTime()}</time>
            </div>
        `;
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

    form.addEventListener("submit", (event) => {
        event.preventDefault();

        const text = input.value.trim();

        if (!text) {
            input.focus();
            return;
        }

        addMessage(text, "user");
        input.value = "";
        input.focus();

        const typing = showTyping();

        window.setTimeout(() => {
            typing.remove();
            addMessage(translate(replies[replyIndex % replies.length]), "bot");
            replyIndex += 1;
        }, 900 + Math.min(text.length * 12, 900));
    });

    document.querySelectorAll("[data-lang-button]").forEach((button) => {
        button.addEventListener("click", () => window.setTimeout(setInputText, 0));
    });

    setInputText();
}
