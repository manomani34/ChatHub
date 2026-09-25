"use strict";

/* =========================================================
   ChatHub SignalR
   ========================================================= */

let currentUserId = null;
let connection = null;
let currentSignalRChannelId = null;
let isTyping = false;
let typingStopTimer = null;
let replyToMessageId = null;
let currentSignalRConversationId = null;

/* =========================================================
   Helpers
   ========================================================= */

function getCurrentChannelId() {
    const element = document.querySelector(
        ".chat-messages[data-channel-id]"
    );

    if (element) {
        const value = parseInt(
            element.getAttribute("data-channel-id"),
            10
        );

        if (!isNaN(value) && value > 0) {
            return value;
        }
    }

    const input = document.querySelector('input[name="channelId"]');

    if (input) {
        const value = parseInt(input.value, 10);

        if (!isNaN(value) && value > 0) {
            return value;
        }
    }

    const urlParams = new URLSearchParams(window.location.search);
    const queryChannelId = parseInt(urlParams.get("channelId"), 10);

    if (!isNaN(queryChannelId) && queryChannelId > 0) {
        return queryChannelId;
    }

    return null;
}

function getCurrentUserId() {
    const element = document.querySelector(
        ".chat-messages[data-current-user-id]"
    );

    if (!element) {
        console.warn("Current user id element not found.");
        return null;
    }

    const value = parseInt(
        element.getAttribute("data-current-user-id"),
        10
    );

    if (isNaN(value) || value <= 0) {
        console.warn("Invalid current user id.");
        return null;
    }

    return value;
}

function getMessageContainer() {
    return document.querySelector(".chat-messages");
}

function findMessageElement(messageId, root = document) {
    const messages = root.querySelectorAll(".message");

    for (const message of messages) {
        const id = parseInt(
            message.getAttribute("data-message-id"),
            10
        );

        if (!isNaN(id) && id === Number(messageId)) {
            return message;
        }
    }

    return null;
}

function escapeHtml(value) {
    if (value == null) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

/* =========================================================
   Reply Reference
   ========================================================= */

function addReplyReference(messageElement) {
    if (!messageElement) {
        return;
    }

    const parentMessageId = parseInt(
        messageElement.getAttribute("data-parent-message-id"),
        10
    );

    if (isNaN(parentMessageId) || parentMessageId <= 0) {
        return;
    }

    if (
        messageElement.querySelector(
            ".message-reply-reference"
        )
    ) {
        return;
    }

    const parentMessage =
        findMessageElement(parentMessageId);

    if (!parentMessage) {
        return;
    }

    const parentAuthor =
        parentMessage.querySelector(
            ".message-author"
        );

    const parentText =
        parentMessage.querySelector(
            ".message-text"
        );

    const author =
        parentAuthor
            ? parentAuthor.textContent.trim()
            : "کاربر";

    const content =
        parentText
            ? parentText.textContent.trim()
            : "";

    const reference =
        document.createElement("div");

    reference.className =
        "message-reply-reference";

    reference.setAttribute(
        "data-target-message-id",
        String(parentMessageId)
    );

    reference.innerHTML = `
        <div class="message-reply-reference-title">
            ↩ در پاسخ به ${escapeHtml(author)}
        </div>

        <div class="message-reply-reference-text">
            ${escapeHtml(content)}
        </div>
    `;

    const messageText =
        messageElement.querySelector(
            ".message-text"
        );

    if (messageText) {
        messageText.insertAdjacentElement(
            "beforebegin",
            reference
        );
    }
}

function initializeReplyReferences() {
    document
        .querySelectorAll(
            ".message[data-parent-message-id]"
        )
        .forEach(function (messageElement) {

            addReplyReference(
                messageElement
            );
        });
}

/* =========================================================
   Reply Target
   ========================================================= */

function setReplyTarget(messageElement) {

    if (!messageElement) {
        return;
    }

    const messageId =
        parseInt(
            messageElement.getAttribute(
                "data-message-id"
            ),
            10
        );

    if (!messageId) {
        return;
    }

    const authorElement =
        messageElement.querySelector(
            ".message-author"
        );

    const textElement =
        messageElement.querySelector(
            ".message-text"
        );

    const author =
        authorElement
            ? authorElement.textContent.trim()
            : "کاربر";

    const content =
        textElement
            ? textElement.textContent.trim()
            : "";

    replyToMessageId =
        messageId;

    const preview =
        document.querySelector(
            "#replyPreview"
        );

    const previewAuthor =
        document.querySelector(
            "#replyPreviewAuthor"
        );

    const previewText =
        document.querySelector(
            "#replyPreviewText"
        );

    if (preview) {
        preview.style.display = "flex";
    }

    if (previewAuthor) {
        previewAuthor.textContent =
            author;
    }

    if (previewText) {
        previewText.textContent =
            content;
    }

    const form =
        document.querySelector(
            ".composer"
        );

    if (form) {

        let parentInput =
            form.querySelector(
                '[name="parentMessageId"]'
            );

        if (!parentInput) {

            parentInput =
                document.createElement(
                    "input"
                );

            parentInput.type =
                "hidden";

            parentInput.name =
                "parentMessageId";

            form.appendChild(
                parentInput
            );
        }

        parentInput.value =
            String(messageId);
    }

    console.log(
        "REPLY TARGET SET:",
        {
            messageId,
            author,
            content
        }
    );

    const textarea =
        document.querySelector(
            ".composer textarea[name='content']"
        );

    if (textarea) {
        textarea.focus();
    }
}

/* =========================================================
   Clear Reply Target
   ========================================================= */

function clearReplyTarget() {

    replyToMessageId =
        null;

    const preview =
        document.querySelector(
            "#replyPreview"
        );

    const previewAuthor =
        document.querySelector(
            "#replyPreviewAuthor"
        );

    const previewText =
        document.querySelector(
            "#replyPreviewText"
        );

    if (preview) {
        preview.style.display =
            "none";
    }

    if (previewAuthor) {
        previewAuthor.textContent =
            "";
    }

    if (previewText) {
        previewText.textContent =
            "";
    }

    const form =
        document.querySelector(
            ".composer"
        );

    if (form) {

        const parentInput =
            form.querySelector(
                '[name="parentMessageId"]'
            );

        if (parentInput) {
            parentInput.value =
                "";
        }
    }

    console.log(
        "REPLY TARGET CLEARED."
    );
}

/* =========================================================
   Reply Button
   ========================================================= */

function addReplyButton(messageElement) {

    if (!messageElement) {
        return;
    }

    if (
        messageElement.querySelector(
            ".message-reply-button"
        )
    ) {
        return;
    }

    const messageId =
        messageElement.getAttribute(
            "data-message-id"
        );

    if (!messageId) {
        return;
    }

    const button =
        document.createElement(
            "button"
        );

    button.type =
        "button";

    button.className =
        "message-reply-button";

    button.setAttribute(
        "data-message-id",
        messageId
    );

    button.textContent =
        "↩️ پاسخ";

    const reactions =
        messageElement.querySelector(
            ".message-reactions"
        );

    if (reactions) {

        reactions.insertAdjacentElement(
            "beforebegin",
            button
        );

    } else {

        const body =
            messageElement.querySelector(
                ".message-body"
            );

        if (body) {
            body.appendChild(
                button
            );
        }
    }
}

function initializeReplyButtons() {

    document
        .querySelectorAll(
            ".message[data-message-id]"
        )
        .forEach(function (messageElement) {

            addReplyButton(
                messageElement
            );
        });
}

/* =========================================================
   Scroll
   ========================================================= */

function scrollMessagesToBottom() {

    const container =
        getMessageContainer();

    if (!container) {
        return;
    }

    container.scrollTop =
        container.scrollHeight;
}

/* =========================================================
   Create Message
   ========================================================= */

function createMessageElement(message) {

    const container =
        getMessageContainer();

    if (!container) {

        console.error(
            "Message container not found."
        );

        return null;
    }

    const template =
        container.querySelector(
            ".message .message-actions"
        )?.closest(".message")
        ||
        container.querySelector(
            ".message"
        );

    let wrapper;

    if (template) {

        wrapper =
            template.cloneNode(
                true
            );

    } else {

        wrapper =
            document.createElement(
                "article"
            );

        wrapper.className =
            "message";

        wrapper.innerHTML = `
            <div class="message-avatar"></div>

            <div class="message-body">

                <div class="message-meta">

                    <span class="message-author"></span>

                    <span class="message-time"></span>

                </div>

                <div class="message-text"></div>

                <div class="message-reactions"
                     data-message-id=""></div>

            </div>
        `;
    }

    /* -----------------------------------------------------
       Remove cloned reply reference
       ----------------------------------------------------- */

    const clonedReplyReference =
        wrapper.querySelector(
            ".message-reply-reference"
        );

    if (clonedReplyReference) {
        clonedReplyReference.remove();
    }

    /* -----------------------------------------------------
       Message Attributes
       ----------------------------------------------------- */

    wrapper.setAttribute(
        "data-message-id",
        String(message.id)
    );

    wrapper.setAttribute(
        "data-channel-id",
        message.channelId == null
            ? ""
            : String(message.channelId)
    );

    wrapper.setAttribute(
        "data-conversation-id",
        message.conversationId == null
            ? ""
            : String(message.conversationId)
    );

    wrapper.setAttribute(
        "data-parent-message-id",
        message.parentMessageId == null
            ? ""
            : String(message.parentMessageId)
    );

    wrapper.setAttribute(
        "data-sender-id",
        String(message.senderId)
    );

    /* -----------------------------------------------------
       Own Message
       ----------------------------------------------------- */

    const isOwnMessage =
        Number(message.senderId) ===
        Number(currentUserId);

    wrapper.classList.toggle(
        "own-message",
        isOwnMessage
    );

    /* -----------------------------------------------------
       Sender
       ----------------------------------------------------- */

    const author =
        wrapper.querySelector(
            ".message-author"
        );

    if (author) {
        author.textContent =
            message.senderName || "";
    }

    /* -----------------------------------------------------
       Content
       ----------------------------------------------------- */

    const text =
        wrapper.querySelector(
            ".message-text"
        );

    if (text) {

        text.textContent =
            message.content || "";

        text.style.removeProperty(
            "display"
        );
    }

    /* -----------------------------------------------------
       Time
       ----------------------------------------------------- */

    const createdAt =
        message.createdAt
            ? new Date(
                message.createdAt
            )
            : new Date();

    const timeText =
        createdAt.toLocaleTimeString(
            "fa-IR",
            {
                hour: "2-digit",
                minute: "2-digit"
            }
        );

    const time =
        wrapper.querySelector(
            ".message-time"
        );

    if (time) {
        time.textContent =
            timeText;
    }

    /* -----------------------------------------------------
       Avatar
       ----------------------------------------------------- */

    const avatar =
        wrapper.querySelector(
            ".message-avatar"
        );

    if (avatar) {

        if (message.senderAvatarUrl) {

            avatar.innerHTML =
                "";

            const image =
                document.createElement(
                    "img"
                );

            image.src =
                message.senderAvatarUrl;

            image.alt =
                "";

            avatar.appendChild(
                image
            );

        } else {

            avatar.innerHTML =
                "";

            avatar.textContent =
                (
                    message.senderName ||
                    "?"
                ).charAt(0);
        }
    }

    /* -----------------------------------------------------
       Edited State
       ----------------------------------------------------- */

    const existingEdited =
        wrapper.querySelector(
            ".edited-label"
        );

    if (existingEdited) {
        existingEdited.remove();
    }

    if (message.isEdited) {

        const meta =
            wrapper.querySelector(
                ".message-meta"
            );

        if (meta) {

            const edited =
                document.createElement(
                    "span"
                );

            edited.className =
                "edited-label";

            edited.textContent =
                "ویرایش شد";

            meta.appendChild(
                edited
            );
        }
    }

    /* -----------------------------------------------------
       Message Actions
       ----------------------------------------------------- */

    if (isOwnMessage) {

        let messageActions =
            wrapper.querySelector(
                ".message-actions"
            );

        if (!messageActions) {

            messageActions =
                document.createElement(
                    "div"
                );

            messageActions.className =
                "message-actions";

            messageActions.innerHTML = `
                <button
                    type="button"
                    class="message-actions-button"
                    aria-label="عملیات پیام">
                    ⋮
                </button>

                <div class="message-actions-menu">

                    <button
                        type="button"
                        class="message-action-edit">

                        <span>✏️</span>
                        <span>ویرایش</span>

                    </button>

                    <button
                        type="button"
                        class="message-action-delete">

                        <span>🗑️</span>
                        <span>حذف</span>

                    </button>

                </div>
            `;

            wrapper.appendChild(
                messageActions
            );
        }

        const editButton =
            wrapper.querySelector(
                ".message-action-edit"
            );

        if (editButton) {

            editButton.setAttribute(
                "data-message-id",
                String(message.id)
            );
        }

        const deleteButton =
            wrapper.querySelector(
                ".message-action-delete"
            );

        if (deleteButton) {

            deleteButton.setAttribute(
                "data-message-id",
                String(message.id)
            );
        }

        const menu =
            wrapper.querySelector(
                ".message-actions-menu"
            );

        if (menu) {

            menu.style.display =
                "none";
        }

        const moreButton =
            wrapper.querySelector(
                ".message-actions-button"
            );

        if (moreButton) {

            moreButton.setAttribute(
                "aria-expanded",
                "false"
            );
        }

    } else {

        const ownActions =
            wrapper.querySelector(
                ".message-actions"
            );

        if (ownActions) {
            ownActions.remove();
        }
    }

    /* -----------------------------------------------------
       Message Reply Button
       ----------------------------------------------------- */

    addReplyButton(
        wrapper
    );

    /* -----------------------------------------------------
       Message Reactions
       ----------------------------------------------------- */

    const reactionsContainer =
        wrapper.querySelector(
            ".message-reactions"
        );

    if (reactionsContainer) {

        reactionsContainer.setAttribute(
            "data-message-id",
            String(message.id)
        );

        reactionsContainer.innerHTML =
            "";
    }

    return wrapper;
}

/* =========================================================
   Append Message
   ========================================================= */

function appendMessageToUI(message) {

    if (!message) {

        console.error(
            "appendMessageToUI: message is null."
        );

        return;
    }

    const container =
        getMessageContainer();

    if (!container) {

        console.error(
            "ChatHub: message container not found."
        );

        return;
    }

    const existing =
        findMessageElement(
            message.id,
            container
        );

    if (existing) {

        console.log(
            "Message already exists:",
            message.id
        );

        return;
    }

    const messageElement =
        createMessageElement(
            message
        );

    if (!messageElement) {
        return;
    }

    container.appendChild(
        messageElement
    );

    /* IMPORTANT:
       Reply reference is created only after
       the message is inside the DOM. */

    addReplyReference(
        messageElement
    );

    loadMessageReactions(
        Number(message.id)
    );

    scrollMessagesToBottom();

    console.log(
        "MESSAGE ADDED TO DOM:",
        message.id
    );
}
/* =========================================================
   Update Direct Conversation List Item
   ========================================================= */

function updateDirectConversationListItem(
    conversationId,
    content,
    createdAt,
    increaseUnread,
    moveToTop = true
) {

    if (!conversationId) {
        return;
    }

    const item =
        document.querySelector(
            `.direct-room-item[data-conversation-id="${conversationId}"]`
        );

    if (!item) {

        console.warn(
            "Direct conversation item not found:",
            conversationId
        );

        return;
    }

    const hasContent =
        content !== null &&
        content !== undefined &&
        String(content).trim() !== "";

    const hasTime =
        createdAt !== null &&
        createdAt !== undefined &&
        String(createdAt).trim() !== "";

    if (!hasContent && !hasTime) {

        const preview =
            item.querySelector(
                ".direct-room-preview"
            );

        if (preview) {
            preview.remove();
        }

        const time =
            item.querySelector(
                ".direct-room-time"
            );

        if (time) {
            time.remove();
        }

        const badge =
            item.querySelector(
                ".unread-badge"
            );

        if (badge) {
            badge.remove();
        }

        return;
    }

    if (hasContent) {

        let preview =
            item.querySelector(
                ".direct-room-preview"
            );

        if (!preview) {

            preview =
                document.createElement(
                    "span"
                );

            preview.className =
                "direct-room-preview";

            const contentContainer =
                item.querySelector(
                    ".direct-room-content"
                );

            if (contentContainer) {

                contentContainer.appendChild(
                    preview
                );
            }
        }

        preview.textContent =
            String(content);
    }

    if (hasTime) {

        let time =
            item.querySelector(
                ".direct-room-time"
            );

        if (!time) {

            time =
                document.createElement(
                    "span"
                );

            time.className =
                "direct-room-time";

            const top =
                item.querySelector(
                    ".direct-room-top"
                );

            if (top) {
                top.appendChild(
                    time
                );
            }
        }

        const date =
            new Date(
                createdAt
            );

        if (!isNaN(date.getTime())) {

            time.textContent =
                date.toLocaleTimeString(
                    "fa-IR",
                    {
                        hour: "2-digit",
                        minute: "2-digit"
                    }
                );
        }
    }

    const activeConversationId =
        Number(
            currentSignalRConversationId
        );

    const currentConversationId =
        Number(
            conversationId
        );

    const isActive =
        activeConversationId &&
        activeConversationId ===
        currentConversationId;

    if (
        increaseUnread &&
        !isActive
    ) {

        let badge =
            item.querySelector(
                ".unread-badge"
            );

        if (!badge) {

            badge =
                document.createElement(
                    "span"
                );

            badge.className =
                "unread-badge";

            badge.textContent =
                "1";

            item.appendChild(
                badge
            );

        } else {

            let count =
                parseInt(
                    badge.textContent,
                    10
                );

            if (isNaN(count)) {
                count = 0;
            }

            count++;

            badge.textContent =
                count > 99
                    ? "99+"
                    : String(count);
        }
    }

    if (!moveToTop) {
        return;
    }

    const parent =
        item.parentElement;

    if (!parent) {
        return;
    }

    const firstDirectItem =
        parent.querySelector(
            ".direct-room-item"
        );

    if (
        firstDirectItem &&
        firstDirectItem !== item
    ) {

        parent.insertBefore(
            item,
            firstDirectItem
        );
    }
}

/* =========================================================
   SignalR Events
   ========================================================= */

function registerSignalREvents() {

    connection.on(
        "ReceiveMessage",
        function (message) {

            console.log(
                "REAL-TIME MESSAGE RECEIVED:",
                message
            );

            if (!message) {
                return;
            }

            if (
                isCurrentDirectMessage()
            ) {

                console.log(
                    "Currently in Direct Message. Channel message ignored."
                );

                return;
            }

            const currentChannel =
                getCurrentChannelId();

            if (
                currentChannel &&
                Number(message.channelId) !==
                Number(currentChannel)
            ) {

                console.log(
                    "Message belongs to another channel. Ignored."
                );

                return;
            }

            appendMessageToUI(
                message
            );
        }
    );

    connection.on(
        "ReceiveDirectMessage",
        function (message) {

            console.log(
                "REAL-TIME DIRECT MESSAGE RECEIVED:",
                message
            );

            if (!message) {
                return;
            }

            updateDirectConversationListItem(
                message.conversationId,
                message.content,
                message.createdAt,
                false
            );

            if (
                !currentSignalRConversationId
            ) {
                return;
            }

            if (
                Number(message.conversationId) !==
                Number(currentSignalRConversationId)
            ) {

                return;
            }

            appendMessageToUI(
                message
            );
        }
    );

    connection.on(
        "DirectUnreadUpdated",
        function (data) {

            console.log(
                "DIRECT UNREAD UPDATED:",
                data
            );

            if (
                !data ||
                !data.conversationId
            ) {
                return;
            }

            const conversationId =
                Number(
                    data.conversationId
                );

            if (!conversationId) {
                return;
            }

            const activeConversationId =
                Number(
                    currentSignalRConversationId
                );

            const isActiveConversation =
                activeConversationId &&
                activeConversationId ===
                conversationId;

            updateDirectConversationListItem(
                conversationId,
                data.lastMessageContent,
                data.lastMessageAt,
                !isActiveConversation
            );

            if (
                isActiveConversation
            ) {

                clearUnreadBadge(
                    conversationId
                );
            }
        }
    );

    connection.on(
        "DirectConversationAvailable",
        async function () {

            await refreshDirectConversationList();
        }
    );

    connection.on(
        "MessageEdited",
        function (message) {

            console.log(
                "MESSAGE EDITED:",
                message
            );

            if (!message) {
                return;
            }

            if (
                isCurrentDirectMessage() &&
                Number(message.conversationId) !==
                Number(currentSignalRConversationId)
            ) {
                return;
            }

            if (
                !isCurrentDirectMessage() &&
                getCurrentChannelId() &&
                Number(message.channelId) !==
                Number(getCurrentChannelId())
            ) {
                return;
            }

            const messageElement =
                findMessageElement(
                    message.id
                );

            if (!messageElement) {

                console.warn(
                    "Edited message not found:",
                    message.id
                );

                return;
            }

            const text =
                messageElement.querySelector(
                    ".message-text"
                );

            if (text) {

                text.textContent =
                    message.content || "";

                text.style.removeProperty(
                    "display"
                );
            }

            const existingLabel =
                messageElement.querySelector(
                    ".edited-label"
                );

            if (!existingLabel) {

                const meta =
                    messageElement.querySelector(
                        ".message-meta"
                    );

                if (meta) {

                    const label =
                        document.createElement(
                            "span"
                        );

                    label.className =
                        "edited-label";

                    label.textContent =
                        "ویرایش شد";

                    meta.appendChild(
                        label
                    );
                }
            }

            const editor =
                messageElement.querySelector(
                    ".message-inline-editor"
                );

            if (editor) {
                editor.remove();
            }

            const replyReference =
                messageElement.querySelector(
                    ".message-reply-reference"
                );

            if (replyReference) {

                replyReference.remove();

                addReplyReference(
                    messageElement
                );
            }

            document
                .querySelectorAll(
                    `.message[data-parent-message-id="${message.id}"]`
                )
                .forEach(
                    function (replyElement) {

                        const reference =
                            replyElement.querySelector(
                                ".message-reply-reference"
                            );

                        if (reference) {
                            reference.remove();
                        }

                        addReplyReference(
                            replyElement
                        );
                    }
                );
        }
    );

    connection.on(
        "DirectConversationUpdated",
        function (data) {

            console.log(
                "DIRECT CONVERSATION UPDATED:",
                data
            );

            if (
                !data ||
                !data.conversationId
            ) {
                return;
            }

            const conversationId =
                Number(
                    data.conversationId
                );

            if (!conversationId) {
                return;
            }

            updateDirectConversationListItem(
                conversationId,
                data.lastMessageContent,
                data.lastMessageAt,
                false,
                data.moveToTop === true
            );

            if (
                !data.lastMessageContent &&
                !data.lastMessageAt
            ) {

                const item =
                    document.querySelector(
                        `.direct-room-item[data-conversation-id="${conversationId}"]`
                    );

                if (!item) {
                    return;
                }

                const preview =
                    item.querySelector(
                        ".direct-room-preview"
                    );

                if (preview) {
                    preview.remove();
                }

                const time =
                    item.querySelector(
                        ".direct-room-time"
                    );

                if (time) {
                    time.remove();
                }
            }
        }
    );

    function updateMemberPresence(
        userId,
        isOnline
    ) {

        if (!userId) {
            return;
        }

        const member =
            document.querySelector(
                `.direct-member[data-user-id="${userId}"]`
            );

        if (!member) {
            return;
        }

        const status =
            member.querySelector(
                ".member-status"
            );

        if (!status) {
            return;
        }

        status.classList.toggle(
            "online",
            isOnline === true
        );

        status.classList.toggle(
            "offline",
            isOnline !== true
        );
    }

    connection.on(
        "PresenceChanged",
        function (data) {

            console.log(
                "PRESENCE CHANGED:",
                data
            );

            if (
                !data ||
                !data.userId ||
                !data.conversationId
            ) {
                return;
            }

            updateMemberPresence(
                Number(data.userId),
                data.isOnline === true
            );

            if (
                !isCurrentDirectMessage()
            ) {
                return;
            }

            if (
                !currentSignalRConversationId
            ) {
                return;
            }

            if (
                Number(data.conversationId) !==
                Number(currentSignalRConversationId)
            ) {
                return;
            }

            updateChatRoomPresence(
                data.isOnline,
                data.lastSeenAt
            );
        }
    );

    connection.on(
        "UserTyping",
        function (data) {

            console.log(
                "USER TYPING:",
                data
            );

            if (
                !data ||
                !data.conversationId
            ) {
                return;
            }

            if (
                Number(data.conversationId) !==
                Number(currentSignalRConversationId)
            ) {
                return;
            }

            const indicator =
                document.querySelector(
                    "#typingIndicator"
                );

            if (!indicator) {
                return;
            }

            let displayName =
                "کاربر";

            const activeRoom =
                document.querySelector(
                    `.direct-room-item[data-conversation-id="${data.conversationId}"]`
                );

            if (activeRoom) {

                const roomName =
                    activeRoom.querySelector(
                        ".room-name"
                    );

                if (
                    roomName &&
                    roomName.textContent.trim()
                ) {

                    displayName =
                        roomName.textContent.trim();
                }
            }

            if (
                displayName ===
                "کاربر"
            ) {

                const title =
                    document.querySelector(
                        ".chat-room-title"
                    );

                if (title) {

                    const spans =
                        title.querySelectorAll(
                            "span"
                        );

                    if (spans.length > 0) {

                        const lastSpan =
                            spans[
                            spans.length - 1
                            ];

                        if (
                            lastSpan &&
                            lastSpan.textContent.trim()
                        ) {

                            displayName =
                                lastSpan.textContent.trim();
                        }
                    }
                }
            }

            indicator.textContent =
                `${displayName} در حال تایپ...`;

            indicator.style.display =
                "flex";
        }
    );

    connection.on(
        "UserStoppedTyping",
        function (data) {

            console.log(
                "USER STOPPED TYPING:",
                data
            );

            if (
                !data ||
                !data.conversationId
            ) {
                return;
            }

            if (
                Number(data.conversationId) !==
                Number(currentSignalRConversationId)
            ) {
                return;
            }

            const indicator =
                document.querySelector(
                    "#typingIndicator"
                );

            if (!indicator) {
                return;
            }

            indicator.textContent =
                "";

            indicator.style.display =
                "none";
        }
    );

    connection.on(
        "MessageDeleted",
        function (message) {

            console.log(
                "MESSAGE DELETED:",
                message
            );

            if (!message) {
                return;
            }

            const messageElement =
                findMessageElement(
                    message.id
                );

            if (!messageElement) {
                return;
            }

            messageElement.remove();
        }
    );

    connection.on(
        "MessageReactionUpdated",
        async function (data) {

            console.log(
                "MESSAGE REACTION UPDATED:",
                data
            );

            if (
                !data ||
                !data.messageId
            ) {
                return;
            }

            const messageElement =
                findMessageElement(
                    data.messageId
                );

            if (!messageElement) {
                return;
            }

            await loadMessageReactions(
                Number(
                    data.messageId
                )
            );
        }
    );
}

/* =========================================================
   Join / Leave
   ========================================================= */

async function joinCurrentChannel() {

    if (!connection) {
        return;
    }

    const channelId =
        getCurrentChannelId();

    if (!channelId) {

        console.warn(
            "No current channel found."
        );

        return;
    }

    if (
        currentSignalRChannelId &&
        Number(currentSignalRChannelId) ===
        Number(channelId)
    ) {
        return;
    }

    if (currentSignalRChannelId) {

        try {

            await connection.invoke(
                "LeaveChannel",
                Number(
                    currentSignalRChannelId
                )
            );

            console.log(
                "Left SignalR channel:",
                currentSignalRChannelId
            );

        } catch (error) {

            console.error(
                "LeaveChannel failed:",
                error
            );
        }
    }

    try {

        await connection.invoke(
            "JoinChannel",
            Number(channelId)
        );

        currentSignalRChannelId =
            Number(channelId);

        console.log(
            "Joined SignalR channel:",
            currentSignalRChannelId
        );

    } catch (error) {

        console.error(
            "JoinChannel failed:",
            error
        );
    }
}

/* =========================================================
   Join Conversation
   ========================================================= */

async function joinConversation(
    conversationId
) {

    if (!connection) {

        console.warn(
            "SignalR connection is not ready."
        );

        return false;
    }

    if (!conversationId) {

        console.warn(
            "Invalid conversation id."
        );

        return false;
    }

    if (
        currentSignalRConversationId &&
        Number(currentSignalRConversationId) ===
        Number(conversationId)
    ) {
        return true;
    }

    try {

        await connection.invoke(
            "JoinConversation",
            Number(conversationId)
        );

        currentSignalRConversationId =
            Number(conversationId);

        console.log(
            "Joined SignalR conversation:",
            currentSignalRConversationId
        );

        await loadConversationPresence(
            currentSignalRConversationId
        );

        return true;

    } catch (error) {

        console.error(
            "JoinConversation failed:",
            error
        );

        return false;
    }
}

async function joinUserGroup() {

    if (!connection) {
        return false;
    }

    try {

        await connection.invoke(
            "JoinUser"
        );

        console.log(
            "Joined SignalR user group."
        );

        return true;

    } catch (error) {

        console.error(
            "JoinUser failed:",
            error
        );

        return false;
    }
}

/* =========================================================
   Start SignalR
   ========================================================= */

async function startSignalR() {

    if (connection) {
        return;
    }

    connection =
        new signalR.HubConnectionBuilder()
            .withUrl(
                "http://localhost:5070/hubs/chat",
                {
                    accessTokenFactory:
                        async function () {

                            const response =
                                await fetch(
                                    "/Auth/Account/AccessToken",
                                    {
                                        method:
                                            "GET",
                                        credentials:
                                            "include"
                                    }
                                );

                            if (!response.ok) {

                                throw new Error(
                                    "Could not obtain SignalR access token."
                                );
                            }

                            const result =
                                await response.json();

                            if (
                                !result ||
                                !result.accessToken
                            ) {

                                throw new Error(
                                    "SignalR access token is empty."
                                );
                            }

                            return result.accessToken;
                        }
                }
            )
            .withAutomaticReconnect()
            .configureLogging(
                signalR.LogLevel.Information
            )
            .build();

    const typingIndicator =
        document.querySelector(
            "#typingIndicator"
        );

    if (typingIndicator) {

        typingIndicator.textContent =
            "";

        typingIndicator.style.display =
            "none";
    }

    registerSignalREvents();

    connection.onreconnecting(
        function (error) {

            console.warn(
                "SignalR reconnecting...",
                error
            );
        }
    );

    connection.onreconnected(
        async function (connectionId) {

            console.log(
                "SignalR reconnected:",
                connectionId
            );

            currentSignalRChannelId =
                null;

            currentSignalRConversationId =
                null;

            await joinUserGroup();

            await loadOnlineUsers();

            await joinCurrentTarget();
        }
    );

    connection.onclose(
        function (error) {

            console.warn(
                "SignalR connection closed.",
                error
            );

            connection =
                null;

            currentSignalRChannelId =
                null;

            currentSignalRConversationId =
                null;
        }
    );

    try {

        await connection.start();

        console.log(
            "SignalR connected."
        );

        await joinUserGroup();

        await loadOnlineUsers();

        await joinCurrentTarget();

    } catch (error) {

        console.error(
            "SignalR connection failed:",
            error
        );

        connection =
            null;
    }
}

/* =========================================================
   Message Actions
   ========================================================= */

document.addEventListener(
    "click",
    async function (event) {

        const replyButton =
            event.target.closest(
                ".message-reply-button"
            );

        if (replyButton) {

            event.preventDefault();
            event.stopPropagation();

            const messageElement =
                replyButton.closest(
                    ".message"
                );

            if (!messageElement) {
                return;
            }

            setReplyTarget(
                messageElement
            );

            return;
        }

        const reactionButton =
            event.target.closest(
                ".message-reaction-button, .message-reaction-chip"
            );

        if (reactionButton) {

            event.preventDefault();
            event.stopPropagation();

            const messageId =
                reactionButton.getAttribute(
                    "data-message-id"
                );

            const emoji =
                reactionButton.getAttribute(
                    "data-emoji"
                );

            if (
                messageId &&
                emoji
            ) {

                await toggleMessageReaction(
                    Number(messageId),
                    emoji
                );
            }

            return;
        }

        const moreButton =
            event.target.closest(
                ".message-actions-button"
            );

        if (moreButton) {

            event.preventDefault();
            event.stopPropagation();

            const actions =
                moreButton.closest(
                    ".message-actions"
                );

            if (!actions) {
                return;
            }

            const menu =
                actions.querySelector(
                    ".message-actions-menu"
                );

            if (!menu) {

                console.warn(
                    "Message actions menu not found."
                );

                return;
            }

            document
                .querySelectorAll(
                    ".message-actions-menu"
                )
                .forEach(
                    function (item) {

                        if (item !== menu) {
                            item.style.display =
                                "none";
                        }
                    }
                );

            document
                .querySelectorAll(
                    ".message-actions-button"
                )
                .forEach(
                    function (button) {

                        if (
                            button !==
                            moreButton
                        ) {

                            button.setAttribute(
                                "aria-expanded",
                                "false"
                            );
                        }
                    }
                );

            const isOpen =
                menu.style.display ===
                "block";

            if (isOpen) {

                menu.style.display =
                    "none";

                moreButton.setAttribute(
                    "aria-expanded",
                    "false"
                );

            } else {

                menu.style.display =
                    "block";

                moreButton.setAttribute(
                    "aria-expanded",
                    "true"
                );
            }

            return;
        }

        const editButton =
            event.target.closest(
                ".message-action-edit"
            );

        if (editButton) {

            event.preventDefault();
            event.stopPropagation();

            const messageElement =
                editButton.closest(
                    ".message"
                );

            if (!messageElement) {
                return;
            }

            const messageId =
                parseInt(
                    editButton.getAttribute(
                        "data-message-id"
                    ) ||
                    messageElement.getAttribute(
                        "data-message-id"
                    ),
                    10
                );

            if (!messageId) {
                return;
            }

            const textElement =
                messageElement.querySelector(
                    ".message-text"
                );

            if (!textElement) {
                return;
            }

            const existingEditor =
                messageElement.querySelector(
                    ".message-inline-editor"
                );

            if (existingEditor) {
                return;
            }

            const originalContent =
                textElement.textContent;

            const menu =
                editButton.closest(
                    ".message-actions-menu"
                );

            if (menu) {
                menu.style.display =
                    "none";
            }

            const moreButton =
                messageElement.querySelector(
                    ".message-actions-button"
                );

            if (moreButton) {

                moreButton.setAttribute(
                    "aria-expanded",
                    "false"
                );
            }

            const editor =
                document.createElement(
                    "div"
                );

            editor.className =
                "message-inline-editor";

            const textarea =
                document.createElement(
                    "textarea"
                );

            textarea.className =
                "message-inline-textarea";

            textarea.value =
                originalContent;

            const actions =
                document.createElement(
                    "div"
                );

            actions.className =
                "message-inline-actions";

            const saveButton =
                document.createElement(
                    "button"
                );

            saveButton.type =
                "button";

            saveButton.className =
                "message-inline-save";

            saveButton.textContent =
                "ذخیره";

            const cancelButton =
                document.createElement(
                    "button"
                );

            cancelButton.type =
                "button";

            cancelButton.className =
                "message-inline-cancel";

            cancelButton.textContent =
                "انصراف";

            actions.appendChild(
                saveButton
            );

            actions.appendChild(
                cancelButton
            );

            editor.appendChild(
                textarea
            );

            editor.appendChild(
                actions
            );

            textElement.style.display =
                "none";

            textElement.insertAdjacentElement(
                "afterend",
                editor
            );

            textarea.focus();
            textarea.select();

            cancelButton.addEventListener(
                "click",
                function () {

                    editor.remove();

                    textElement.style.display =
                        "";
                }
            );

            saveButton.addEventListener(
                "click",
                async function () {

                    const newContent =
                        textarea.value.trim();

                    if (!newContent) {

                        textarea.focus();

                        return;
                    }

                    if (
                        newContent ===
                        originalContent
                    ) {

                        editor.remove();

                        textElement.style.display =
                            "";

                        return;
                    }

                    saveButton.disabled =
                        true;

                    cancelButton.disabled =
                        true;

                    try {

                        const response =
                            await fetch(
                                "/User/Home/EditMessage",
                                {
                                    method:
                                        "POST",

                                    headers: {
                                        "Content-Type":
                                            "application/x-www-form-urlencoded"
                                    },

                                    body:
                                        new URLSearchParams({
                                            messageId:
                                                messageId,
                                            content:
                                                newContent
                                        })
                                }
                            );

                        if (!response.ok) {

                            const errorText =
                                await response.text();

                            console.error(
                                "Edit failed:",
                                response.status,
                                errorText
                            );

                            saveButton.disabled =
                                false;

                            cancelButton.disabled =
                                false;

                            return;
                        }

                        const updatedMessage =
                            await response.json();

                        textElement.textContent =
                            updatedMessage.content ||
                            newContent;

                        textElement.style.display =
                            "";

                        editor.remove();

                        if (
                            updatedMessage.isEdited !==
                            false
                        ) {

                            const meta =
                                messageElement.querySelector(
                                    ".message-meta"
                                );

                            if (
                                meta &&
                                !meta.querySelector(
                                    ".edited-label"
                                )
                            ) {

                                const editedLabel =
                                    document.createElement(
                                        "span"
                                    );

                                editedLabel.className =
                                    "edited-label";

                                editedLabel.textContent =
                                    "ویرایش شد";

                                meta.appendChild(
                                    editedLabel
                                );
                            }
                        }

                    } catch (error) {

                        console.error(
                            "Edit request failed:",
                            error
                        );

                        saveButton.disabled =
                            false;

                        cancelButton.disabled =
                            false;
                    }
                }
            );

            return;
        }

        const deleteButton =
            event.target.closest(
                ".message-action-delete"
            );

        if (deleteButton) {

            event.preventDefault();
            event.stopPropagation();

            const messageElement =
                deleteButton.closest(
                    ".message"
                );

            if (!messageElement) {
                return;
            }

            const messageId =
                parseInt(
                    deleteButton.getAttribute(
                        "data-message-id"
                    ) ||
                    messageElement.getAttribute(
                        "data-message-id"
                    ),
                    10
                );

            if (!messageId) {
                return;
            }

            const confirmed =
                window.confirm(
                    "آیا از حذف این پیام مطمئن هستید؟"
                );

            if (!confirmed) {
                return;
            }

            const menu =
                deleteButton.closest(
                    ".message-actions-menu"
                );

            if (menu) {

                menu.style.display =
                    "none";
            }

            const moreButton =
                messageElement.querySelector(
                    ".message-actions-button"
                );

            if (moreButton) {

                moreButton.setAttribute(
                    "aria-expanded",
                    "false"
                );
            }

            try {

                const response =
                    await fetch(
                        `/User/Home/DeleteMessage?messageId=${encodeURIComponent(
                            messageId
                        )}`,
                        {
                            method:
                                "POST"
                        }
                    );

                if (!response.ok) {

                    const errorText =
                        await response.text();

                    console.error(
                        "Delete failed:",
                        response.status,
                        errorText
                    );
                }

            } catch (error) {

                console.error(
                    "Delete request failed:",
                    error
                );
            }

            return;
        }

        if (
            !event.target.closest(
                ".message-actions-menu"
            ) &&
            !event.target.closest(
                ".message-actions-button"
            )
        ) {

            document
                .querySelectorAll(
                    ".message-actions-menu"
                )
                .forEach(
                    function (menu) {

                        menu.style.display =
                            "none";
                    }
                );

            document
                .querySelectorAll(
                    ".message-actions-button"
                )
                .forEach(
                    function (button) {

                        button.setAttribute(
                            "aria-expanded",
                            "false"
                        );
                    }
                );
        }
    }
);

/* =========================================================
   Cancel Reply
   ========================================================= */

document.addEventListener(
    "click",
    function (event) {

        const cancelButton =
            event.target.closest(
                "#cancelReplyButton"
            );

        if (!cancelButton) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        clearReplyTarget();
    }
);

/* =========================================================
   Reply Reference Click
   ========================================================= */

document.addEventListener(
    "click",
    function (event) {

        const reference =
            event.target.closest(
                ".message-reply-reference"
            );

        if (!reference) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        const targetMessageId =
            parseInt(
                reference.getAttribute(
                    "data-target-message-id"
                ),
                10
            );

        if (
            isNaN(targetMessageId) ||
            targetMessageId <= 0
        ) {
            return;
        }

        const target =
            findMessageElement(
                targetMessageId
            );

        if (!target) {

            console.warn(
                "Reply target message not found:",
                targetMessageId
            );

            return;
        }

        target.scrollIntoView({
            behavior:
                "smooth",
            block:
                "center"
        });

        target.classList.add(
            "message-reply-highlight"
        );

        setTimeout(
            function () {

                target.classList.remove(
                    "message-reply-highlight"
                );

            },
            1200
        );
    }
);
/* =========================================================
   Initialize
   ========================================================= */

document.addEventListener(
    "DOMContentLoaded",
    function () {

        currentUserId =
            getCurrentUserId();

        initializeReplyReferences();

        console.log(
            "CURRENT USER ID:",
            currentUserId
        );

        console.log(
            "ChatHub SignalR initializing..."
        );

        startSignalR();

        initializeMessageReactions();

        initializeReplyButtons();
    }
);

/* =========================================================
   Get / Create Direct Conversation
   ========================================================= */

async function getOrCreateDirectConversation(
    userName
) {

    if (!userName) {
        return null;
    }

    try {

        const tokenResponse =
            await fetch(
                "/Auth/Account/AccessToken",
                {
                    method: "GET",
                    credentials: "include"
                }
            );

        if (!tokenResponse.ok) {

            throw new Error(
                "Could not obtain access token."
            );
        }

        const tokenResult =
            await tokenResponse.json();

        if (
            !tokenResult ||
            !tokenResult.accessToken
        ) {

            throw new Error(
                "Access token is empty."
            );
        }

        const response =
            await fetch(
                `http://localhost:5070/api/user/conversations/direct/open?userName=${encodeURIComponent(userName)}`,
                {
                    method: "GET",

                    headers: {
                        Authorization:
                            `Bearer ${tokenResult.accessToken}`
                    }
                }
            );

        if (!response.ok) {

            console.error(
                "Get/Create conversation failed:",
                response.status
            );

            return null;
        }

        return await response.json();

    } catch (error) {

        console.error(
            "Direct conversation request failed:",
            error
        );

        return null;
    }
}

/* =========================================================
   Mark Direct Conversation As Read
   ========================================================= */

async function markConversationAsRead(
    conversationId,
    lastReadMessageId
) {

    if (!conversationId) {
        return false;
    }

    try {

        const tokenResponse =
            await fetch(
                "/Auth/Account/AccessToken",
                {
                    method: "GET",
                    credentials: "include"
                }
            );

        if (!tokenResponse.ok) {

            throw new Error(
                "Could not obtain access token."
            );
        }

        const tokenResult =
            await tokenResponse.json();

        if (
            !tokenResult ||
            !tokenResult.accessToken
        ) {

            throw new Error(
                "Access token is empty."
            );
        }

        let url =
            `http://localhost:5070/api/user/conversations/${conversationId}/read`;

        if (lastReadMessageId) {

            url +=
                `?lastReadMessageId=${encodeURIComponent(
                    lastReadMessageId
                )}`;
        }

        const response =
            await fetch(
                url,
                {
                    method: "POST",

                    headers: {
                        Authorization:
                            `Bearer ${tokenResult.accessToken}`
                    }
                }
            );

        if (!response.ok) {

            console.error(
                "Mark conversation as read failed:",
                response.status
            );

            return false;
        }

        console.log(
            "Conversation marked as read:",
            conversationId,
            lastReadMessageId
        );

        return true;

    } catch (error) {

        console.error(
            "Mark conversation as read error:",
            error
        );

        return false;
    }
}

/* =========================================================
   Open Direct Conversation
   ========================================================= */

async function openDirectConversation(
    userName
) {

    if (!userName) {
        return;
    }

    console.log(
        "Opening direct conversation with:",
        userName
    );

    const conversation =
        await getOrCreateDirectConversation(
            userName
        );

    if (!conversation) {

        console.error(
            "Could not get/create direct conversation."
        );

        return;
    }

    console.log(
        "DIRECT CONVERSATION:",
        conversation
    );

    const messages =
        await getDirectConversationMessages(
            conversation.conversationId
        );

    const container =
        getMessageContainer();

    if (container) {

        container.setAttribute(
            "data-is-direct",
            "true"
        );

        container.setAttribute(
            "data-conversation-id",
            String(
                conversation.conversationId
            )
        );

        container.setAttribute(
            "data-channel-id",
            ""
        );
    }

    currentSignalRChannelId =
        null;

    updateDirectChatHeader(
        conversation.otherDisplayName
    );

    updateDirectNavigationState(
        conversation.otherUserName
    );

    updateComposerForDirect(
        conversation,
        conversation.otherUserName
    );

    renderDirectConversationMessages(
        messages,
        conversation.conversationId
    );

    initializeReplyButtons();

    const lastMessage =
        messages &&
            messages.length > 0
            ? messages[messages.length - 1]
            : null;

    await markConversationAsRead(
        conversation.conversationId,
        lastMessage?.id ?? null
    );

    clearUnreadBadge(
        conversation.conversationId
    );

    const newUrl =
        `/User?userName=${encodeURIComponent(
            conversation.otherUserName
        )}`;

    window.history.pushState(
        {
            type: "direct",
            userName:
                conversation.otherUserName
        },
        "",
        newUrl
    );

    const joined =
        await joinConversation(
            conversation.conversationId
        );

    if (!joined) {

        console.warn(
            "Direct conversation opened, but SignalR join failed."
        );

        return;
    }

    console.log(
        "DIRECT CONVERSATION READY:",
        conversation.conversationId
    );

    initializeMessageReactions();
}

function isCurrentDirectMessage() {

    const element =
        document.querySelector(
            ".chat-messages[data-is-direct]"
        );

    if (!element) {
        return false;
    }

    return (
        element.getAttribute(
            "data-is-direct"
        ) === "true"
    );
}

function getCurrentConversationId() {

    const element =
        document.querySelector(
            ".chat-messages[data-conversation-id]"
        );

    if (!element) {
        return null;
    }

    const value =
        parseInt(
            element.getAttribute(
                "data-conversation-id"
            ),
            10
        );

    if (
        !isNaN(value) &&
        value > 0
    ) {

        return value;
    }

    return null;
}

/* =========================================================
   Load Online Users
   ========================================================= */

async function loadOnlineUsers() {

    if (!connection) {
        return;
    }

    try {

        const onlineUserIds =
            await connection.invoke(
                "GetOnlineUserIds"
            );

        if (
            !Array.isArray(
                onlineUserIds
            )
        ) {
            return;
        }

        const onlineSet =
            new Set(
                onlineUserIds.map(
                    id => Number(id)
                )
            );

        document
            .querySelectorAll(
                ".direct-member[data-user-id]"
            )
            .forEach(
                function (member) {

                    const userId =
                        Number(
                            member.getAttribute(
                                "data-user-id"
                            )
                        );

                    const status =
                        member.querySelector(
                            ".member-status"
                        );

                    if (!status) {
                        return;
                    }

                    if (
                        userId &&
                        onlineSet.has(userId)
                    ) {

                        member.classList.add(
                            "online"
                        );

                        status.classList.add(
                            "online"
                        );

                    } else {

                        member.classList.remove(
                            "online"
                        );

                        status.classList.remove(
                            "online"
                        );
                    }
                }
            );

    } catch (error) {

        console.error(
            "Load online users failed:",
            error
        );
    }
}

/* =========================================================
   Load Conversation Presence
   ========================================================= */

async function loadConversationPresence(
    conversationId
) {

    if (!connection) {
        return;
    }

    if (!conversationId) {
        return;
    }

    const presenceElement =
        document.querySelector(
            "#chatRoomPresence"
        );

    if (!presenceElement) {
        return;
    }

    try {

        const presence =
            await connection.invoke(
                "GetConversationPresence",
                Number(conversationId)
            );

        if (!presence) {

            presenceElement.textContent =
                "";

            return;
        }

        if (presence.isOnline) {

            presenceElement.textContent =
                "● آنلاین";

            presenceElement.classList.add(
                "online"
            );

            presenceElement.classList.remove(
                "offline"
            );

            return;
        }

        presenceElement.classList.remove(
            "online"
        );

        presenceElement.classList.add(
            "offline"
        );

        if (presence.lastSeenAt) {

            const lastSeen =
                new Date(
                    presence.lastSeenAt
                );

            if (
                !isNaN(
                    lastSeen.getTime()
                )
            ) {

                presenceElement.textContent =
                    `آخرین بازدید: ${lastSeen.toLocaleTimeString(
                        "fa-IR",
                        {
                            hour: "2-digit",
                            minute: "2-digit"
                        }
                    )}`;

                return;
            }
        }

        presenceElement.textContent =
            "آفلاین";

    } catch (error) {

        console.error(
            "Load conversation presence failed:",
            error
        );

        presenceElement.textContent =
            "";
    }
}

/* =========================================================
   Update Chat Room Presence
   ========================================================= */

function updateChatRoomPresence(
    isOnline,
    lastSeenAt
) {

    const presenceElement =
        document.querySelector(
            "#chatRoomPresence"
        );

    if (!presenceElement) {
        return;
    }

    presenceElement.classList.remove(
        "online",
        "offline"
    );

    if (isOnline) {

        presenceElement.textContent =
            "● آنلاین";

        presenceElement.classList.add(
            "online"
        );

        return;
    }

    presenceElement.classList.add(
        "offline"
    );

    if (lastSeenAt) {

        const lastSeen =
            new Date(
                lastSeenAt
            );

        if (
            !isNaN(
                lastSeen.getTime()
            )
        ) {

            presenceElement.textContent =
                `آخرین بازدید: ${lastSeen.toLocaleTimeString(
                    "fa-IR",
                    {
                        hour: "2-digit",
                        minute: "2-digit"
                    }
                )}`;

            return;
        }
    }

    presenceElement.textContent =
        "آفلاین";
}

async function joinCurrentTarget() {

    if (!connection) {
        return;
    }

    if (isCurrentDirectMessage()) {

        const conversationId =
            getCurrentConversationId();

        if (!conversationId) {

            console.warn(
                "No current conversation found."
            );

            return;
        }

        await joinConversation(
            conversationId
        );

        return;
    }

    await joinCurrentChannel();
}

/* =========================================================
   Refresh Direct Conversation List
   ========================================================= */

async function refreshDirectConversationList() {

    const list =
        document.querySelector(
            "#directMessagesList"
        );

    if (!list) {
        return;
    }

    try {

        const tokenResponse =
            await fetch(
                "/Auth/Account/AccessToken",
                {
                    method: "GET",
                    credentials: "include"
                }
            );

        if (!tokenResponse.ok) {
            return;
        }

        const tokenResult =
            await tokenResponse.json();

        if (
            !tokenResult ||
            !tokenResult.accessToken
        ) {
            return;
        }

        const response =
            await fetch(
                "http://localhost:5070/api/user/conversations/direct",
                {
                    method: "GET",

                    headers: {
                        Authorization:
                            `Bearer ${tokenResult.accessToken}`
                    }
                }
            );

        if (!response.ok) {
            return;
        }

        const conversations =
            await response.json();

        list.innerHTML =
            "";

        if (
            !conversations ||
            conversations.length === 0
        ) {

            list.innerHTML = `
                <div class="direct-empty">
                    هنوز گفتگوی مستقیمی ندارید.
                </div>
            `;

            return;
        }

        for (
            const conversation
            of conversations
        ) {

            const link =
                document.createElement(
                    "a"
                );

            link.href =
                `/User?userName=${encodeURIComponent(
                    conversation.otherUserName
                )}`;

            link.className =
                "room-item direct-room-item";

            link.setAttribute(
                "data-user-name",
                conversation.otherUserName || ""
            );

            link.setAttribute(
                "data-conversation-id",
                String(
                    conversation.conversationId
                )
            );

            const isActive =
                currentSignalRConversationId &&
                Number(
                    currentSignalRConversationId
                ) ===
                Number(
                    conversation.conversationId
                );

            if (isActive) {
                link.classList.add(
                    "active"
                );
            }

            const icon =
                document.createElement(
                    "span"
                );

            icon.className =
                "room-icon";

            icon.textContent =
                conversation.otherDisplayName
                    ? conversation.otherDisplayName.charAt(0)
                    : "?";

            const content =
                document.createElement(
                    "span"
                );

            content.className =
                "direct-room-content";

            const top =
                document.createElement(
                    "span"
                );

            top.className =
                "direct-room-top";

            const name =
                document.createElement(
                    "span"
                );

            name.className =
                "room-name";

            name.textContent =
                conversation.otherDisplayName ||
                conversation.otherUserName ||
                "";

            top.appendChild(
                name
            );

            if (
                conversation.lastMessageAt
            ) {

                const time =
                    document.createElement(
                        "span"
                    );

                time.className =
                    "direct-room-time";

                const date =
                    new Date(
                        conversation.lastMessageAt
                    );

                if (
                    !isNaN(
                        date.getTime()
                    )
                ) {

                    time.textContent =
                        date.toLocaleTimeString(
                            "fa-IR",
                            {
                                hour: "2-digit",
                                minute: "2-digit"
                            }
                        );

                    top.appendChild(
                        time
                    );
                }
            }

            content.appendChild(
                top
            );

            if (
                conversation.lastMessageContent
            ) {

                const preview =
                    document.createElement(
                        "span"
                    );

                preview.className =
                    "direct-room-preview";

                preview.textContent =
                    conversation.lastMessageContent;

                content.appendChild(
                    preview
                );
            }

            link.appendChild(
                icon
            );

            link.appendChild(
                content
            );

            const unreadCount =
                Number(
                    conversation.unreadCount || 0
                );

            if (
                unreadCount > 0
            ) {

                const badge =
                    document.createElement(
                        "span"
                    );

                badge.className =
                    "unread-badge";

                badge.textContent =
                    unreadCount > 99
                        ? "99+"
                        : String(unreadCount);

                link.appendChild(
                    badge
                );
            }

            list.appendChild(
                link
            );
        }

    } catch (error) {

        console.error(
            "Refresh direct conversations failed:",
            error
        );
    }
}

/* =========================================================
   Direct Message UI
   ========================================================= */

async function getDirectConversationMessages(
    conversationId
) {

    if (!conversationId) {
        return [];
    }

    try {

        const tokenResponse =
            await fetch(
                "/Auth/Account/AccessToken",
                {
                    method: "GET",
                    credentials: "include"
                }
            );

        if (!tokenResponse.ok) {

            throw new Error(
                "Could not obtain access token."
            );
        }

        const tokenResult =
            await tokenResponse.json();

        if (
            !tokenResult ||
            !tokenResult.accessToken
        ) {

            throw new Error(
                "Access token is empty."
            );
        }

        const response =
            await fetch(
                `http://localhost:5070/api/user/messages/conversation/${conversationId}`,
                {
                    method: "GET",

                    headers: {
                        Authorization:
                            `Bearer ${tokenResult.accessToken}`
                    }
                }
            );

        if (!response.ok) {

            console.error(
                "Get conversation messages failed:",
                response.status
            );

            return [];
        }

        return await response.json();

    } catch (error) {

        console.error(
            "Get conversation messages failed:",
            error
        );

        return [];
    }
}

function updateDirectChatHeader(
    displayName
) {

    const title =
        document.querySelector(
            ".chat-room-title"
        );

    const description =
        document.querySelector(
            ".chat-room-description"
        );

    if (title) {

        const icon =
            document.createElement(
                "span"
            );

        icon.textContent =
            "💬";

        const name =
            document.createElement(
                "span"
            );

        name.textContent =
            displayName || "کاربر";

        title.replaceChildren(
            icon,
            name
        );
    }

    if (description) {

        description.textContent =
            "پیام مستقیم";
    }

    let presence =
        document.querySelector(
            "#chatRoomPresence"
        );

    if (!presence) {

        presence =
            document.createElement(
                "div"
            );

        presence.id =
            "chatRoomPresence";

        presence.className =
            "chat-room-presence";

        presence.setAttribute(
            "aria-live",
            "polite"
        );

        if (description) {

            description.insertAdjacentElement(
                "afterend",
                presence
            );

        } else if (title) {

            title.insertAdjacentElement(
                "afterend",
                presence
            );
        }
    }

    presence.textContent =
        "";

    presence.classList.remove(
        "online",
        "offline"
    );

    presence.style.display =
        "block";
}

function updateDirectNavigationState(
    userName
) {

    if (!userName) {
        return;
    }

    const normalizedUserName =
        String(userName)
            .trim()
            .toLowerCase();

    document
        .querySelectorAll(
            ".channel-item"
        )
        .forEach(
            function (item) {

                item.classList.remove(
                    "active"
                );
            }
        );

    document
        .querySelectorAll(
            ".direct-room-item, .direct-member"
        )
        .forEach(
            function (item) {

                const itemUserName =
                    item.getAttribute(
                        "data-user-name"
                    );

                if (!itemUserName) {

                    item.classList.remove(
                        "active"
                    );

                    return;
                }

                const normalizedItemUserName =
                    String(itemUserName)
                        .trim()
                        .toLowerCase();

                item.classList.toggle(
                    "active",
                    normalizedItemUserName ===
                    normalizedUserName
                );
            }
        );
}

function updateComposerForDirect(
    conversation,
    userName
) {

    const form =
        document.querySelector(
            ".composer"
        );

    if (!form) {
        return;
    }

    let conversationInput =
        form.querySelector(
            '[name="conversationId"]'
        );

    if (!conversationInput) {

        conversationInput =
            document.createElement(
                "input"
            );

        conversationInput.type =
            "hidden";

        conversationInput.name =
            "conversationId";

        form.appendChild(
            conversationInput
        );
    }

    conversationInput.value =
        conversation.conversationId;

    let userNameInput =
        form.querySelector(
            '[name="userName"]'
        );

    if (!userNameInput) {

        userNameInput =
            document.createElement(
                "input"
            );

        userNameInput.type =
            "hidden";

        userNameInput.name =
            "userName";

        form.appendChild(
            userNameInput
        );
    }

    userNameInput.value =
        userName;

    const channelInput =
        form.querySelector(
            '[name="channelId"]'
        );

    if (channelInput) {
        channelInput.remove();
    }

    form.setAttribute(
        "data-chat-mode",
        "direct"
    );

    form.action =
        "/User/Home/SendDirectMessage";

    const contentInput =
        form.querySelector(
            '[name="content"]'
        );

    if (contentInput) {

        contentInput.placeholder =
            "پیام خود را بنویسید...";
    }
}

function renderDirectConversationMessages(
    messages,
    conversationId
) {

    const container =
        getMessageContainer();

    if (!container) {
        return;
    }

    container.setAttribute(
        "data-is-direct",
        "true"
    );

    container.setAttribute(
        "data-conversation-id",
        String(conversationId)
    );

    container.setAttribute(
        "data-channel-id",
        ""
    );

    container.innerHTML =
        "";

    if (
        !messages ||
        messages.length === 0
    ) {

        const empty =
            document.createElement(
                "div"
            );

        empty.className =
            "empty-chat";

        empty.textContent =
            "هنوز پیامی در این گفتگوی خصوصی وجود ندارد.";

        container.appendChild(
            empty
        );

        return;
    }

    messages.forEach(
        function (message) {

            const element =
                createMessageElement(
                    message
                );

            if (element) {

                container.appendChild(
                    element
                );
            }
        }
    );

    /* Reply references must be initialized
       after ALL messages are in the DOM. */

    initializeReplyReferences();

    scrollMessagesToBottom();
}

/* =========================================================
Open Direct Message Without Refresh
========================================================= */

document.addEventListener(
    "click",
    async function (event) {

        const member =
            event.target.closest(
                ".direct-member, .direct-room-item"
            );

        if (!member) {
            return;
        }

        event.preventDefault();

        const userName =
            member.getAttribute(
                "data-user-name"
            );

        if (!userName) {

            console.warn(
                "Direct message username not found."
            );

            return;
        }

        await openDirectConversation(
            userName
        );
    }
);

/* =========================================================
   Clear Unread Badge
   ========================================================= */

function clearUnreadBadge(
    conversationId
) {

    if (!conversationId) {
        return;
    }

    const item =
        document.querySelector(
            `.direct-room-item[data-conversation-id="${conversationId}"]`
        );

    if (!item) {
        return;
    }

    const badge =
        item.querySelector(
            ".unread-badge"
        );

    if (badge) {
        badge.remove();
    }
}

/* =========================================================
   Typing Indicator - Local User
   ========================================================= */

document.addEventListener(
    "input",
    async function (event) {

        const textarea =
            event.target.closest(
                ".composer textarea[name='content']"
            );

        if (!textarea) {
            return;
        }

        if (!isCurrentDirectMessage()) {
            return;
        }

        if (!connection) {
            return;
        }

        const conversationId =
            Number(
                currentSignalRConversationId
            );

        if (!conversationId) {
            return;
        }

        if (!textarea.value.trim()) {

            if (isTyping) {

                try {

                    await connection.invoke(
                        "StopTyping",
                        conversationId
                    );

                } catch (error) {

                    console.error(
                        "StopTyping failed:",
                        error
                    );
                }

                isTyping =
                    false;
            }

            if (typingStopTimer) {

                clearTimeout(
                    typingStopTimer
                );

                typingStopTimer =
                    null;
            }

            return;
        }

        if (!isTyping) {

            try {

                await connection.invoke(
                    "StartTyping",
                    conversationId
                );

                isTyping =
                    true;

                console.log(
                    "Typing started:",
                    conversationId
                );

            } catch (error) {

                console.error(
                    "StartTyping failed:",
                    error
                );
            }
        }

        if (typingStopTimer) {

            clearTimeout(
                typingStopTimer
            );
        }

        typingStopTimer =
            setTimeout(
                async function () {

                    if (
                        !connection ||
                        !isTyping
                    ) {
                        return;
                    }

                    try {

                        await connection.invoke(
                            "StopTyping",
                            conversationId
                        );

                        console.log(
                            "Typing stopped:",
                            conversationId
                        );

                    } catch (error) {

                        console.error(
                            "StopTyping failed:",
                            error
                        );
                    }

                    isTyping =
                        false;

                    typingStopTimer =
                        null;

                },
                1200
            );
    }
);

/* =========================================================
   Stop Typing On Send
   ========================================================= */

document.addEventListener(
    "submit",
    async function (event) {

        const form =
            event.target.closest(
                ".composer"
            );

        if (!form) {
            return;
        }

        if (!isCurrentDirectMessage()) {
            return;
        }

        const conversationId =
            Number(
                currentSignalRConversationId
            );

        if (
            connection &&
            conversationId &&
            isTyping
        ) {

            try {

                await connection.invoke(
                    "StopTyping",
                    conversationId
                );

            } catch (error) {

                console.error(
                    "StopTyping on submit failed:",
                    error
                );
            }
        }

        isTyping =
            false;

        if (typingStopTimer) {

            clearTimeout(
                typingStopTimer
            );

            typingStopTimer =
                null;
        }
    }
);

/* =========================================================
   Message Reactions
   ========================================================= */

async function getApiAccessToken() {

    const response =
        await fetch(
            "/Auth/Account/AccessToken",
            {
                method: "GET",
                credentials: "include"
            }
        );

    if (!response.ok) {

        throw new Error(
            "Could not obtain API access token."
        );
    }

    const result =
        await response.json();

    if (
        !result ||
        !result.accessToken
    ) {

        throw new Error(
            "API access token is empty."
        );
    }

    return result.accessToken;
}

async function loadMessageReactions(
    messageId
) {

    console.log(
        "LOADING REACTIONS FOR MESSAGE:",
        messageId
    );

    if (!messageId) {
        return;
    }

    const messageElement =
        findMessageElement(
            messageId
        );

    if (!messageElement) {
        return;
    }

    const reactionsContainer =
        messageElement.querySelector(
            ".message-reactions"
        );

    if (!reactionsContainer) {
        return;
    }

    try {

        const token =
            await getApiAccessToken();

        const response =
            await fetch(
                `http://localhost:5070/api/user/messages/${encodeURIComponent(messageId)}/reactions`,
                {
                    method: "GET",

                    headers: {
                        Authorization:
                            `Bearer ${token}`
                    }
                }
            );

        if (!response.ok) {

            console.error(
                "Load reactions failed:",
                response.status
            );

            return;
        }

        const reactions =
            await response.json();

        renderMessageReactions(
            messageElement,
            reactions
        );

    } catch (error) {

        console.error(
            "Load reactions request failed:",
            error
        );
    }
}

function renderMessageReactions(
    messageElement,
    reactions
) {

    if (!messageElement) {
        return;
    }

    const container =
        messageElement.querySelector(
            ".message-reactions"
        );

    if (!container) {
        return;
    }

    container.innerHTML =
        "";

    const actions =
        document.createElement(
            "div"
        );

    actions.className =
        "message-reaction-actions";

    const emojis = [
        "👍",
        "❤️",
        "😂"
    ];

    emojis.forEach(
        function (emoji) {

            const button =
                document.createElement(
                    "button"
                );

            button.type =
                "button";

            button.className =
                "message-reaction-button";

            button.setAttribute(
                "data-message-id",
                messageElement.getAttribute(
                    "data-message-id"
                )
            );

            button.setAttribute(
                "data-emoji",
                emoji
            );

            button.textContent =
                emoji;

            actions.appendChild(
                button
            );
        }
    );

    container.appendChild(
        actions
    );

    if (
        !Array.isArray(
            reactions
        ) ||
        reactions.length === 0
    ) {

        return;
    }

    const list =
        document.createElement(
            "div"
        );

    list.className =
        "message-reaction-list";

    reactions.forEach(
        function (reaction) {

            const button =
                document.createElement(
                    "button"
                );

            button.type =
                "button";

            button.className =
                "message-reaction-chip";

            button.setAttribute(
                "data-message-id",
                messageElement.getAttribute(
                    "data-message-id"
                )
            );

            button.setAttribute(
                "data-emoji",
                reaction.emoji
            );

            if (
                reaction.reactedByCurrentUser
            ) {

                button.classList.add(
                    "active"
                );
            }

            button.innerHTML =
                `<span>${reaction.emoji}</span>
                 <span>${reaction.count}</span>`;

            list.appendChild(
                button
            );
        }
    );

    container.appendChild(
        list
    );
}

async function toggleMessageReaction(
    messageId,
    emoji
) {

    if (
        !messageId ||
        !emoji
    ) {
        return;
    }

    try {

        const token =
            await getApiAccessToken();

        const response =
            await fetch(
                "http://localhost:5070/api/user/messages/reactions",
                {
                    method: "POST",

                    headers: {
                        "Content-Type":
                            "application/json",

                        Authorization:
                            `Bearer ${token}`
                    },

                    body:
                        JSON.stringify({
                            messageId:
                                Number(messageId),

                            emoji:
                                emoji
                        })
                }
            );

        if (!response.ok) {

            const errorText =
                await response.text();

            console.error(
                "Toggle reaction failed:",
                response.status,
                errorText
            );

            return;
        }

        const result =
            await response.json();

        const messageElement =
            findMessageElement(
                messageId
            );

        if (
            messageElement &&
            result &&
            Array.isArray(
                result.reactions
            )
        ) {

            renderMessageReactions(
                messageElement,
                result.reactions
            );
        }

    } catch (error) {

        console.error(
            "Toggle reaction request failed:",
            error
        );
    }
}

function initializeMessageReactions() {

    document
        .querySelectorAll(
            ".message[data-message-id]"
        )
        .forEach(
            function (messageElement) {

                const messageId =
                    Number(
                        messageElement.getAttribute(
                            "data-message-id"
                        )
                    );

                if (
                    messageId > 0
                ) {

                    loadMessageReactions(
                        messageId
                    );
                }
            }
        );
}