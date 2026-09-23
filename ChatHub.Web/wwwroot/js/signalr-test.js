"use strict";

/* =========================================================
   ChatHub SignalR
   ========================================================= */

const currentUserId = 1;

let connection = null;
let currentSignalRChannelId = null;


/* =========================================================
   Helpers
   ========================================================= */

function getCurrentChannelId() {

    const element =
        document.querySelector(
            ".chat-messages[data-channel-id]"
        );

    if (element) {

        const value =
            parseInt(
                element.getAttribute(
                    "data-channel-id"
                ),
                10
            );

        if (!isNaN(value) && value > 0) {
            return value;
        }
    }


    const input =
        document.querySelector(
            'input[name="channelId"]'
        );

    if (input) {

        const value =
            parseInt(
                input.value,
                10
            );

        if (!isNaN(value) && value > 0) {
            return value;
        }
    }


    const urlParams =
        new URLSearchParams(
            window.location.search
        );


    const queryChannelId =
        parseInt(
            urlParams.get("channelId"),
            10
        );


    if (
        !isNaN(queryChannelId) &&
        queryChannelId > 0
    ) {
        return queryChannelId;
    }


    return null;
}


/* =========================================================
   Message Container
   ========================================================= */

function getMessageContainer() {

    return document.querySelector(
        ".chat-messages"
    );
}


/* =========================================================
   Find Message
   ========================================================= */

function findMessageElement(
    messageId,
    root = document
) {

    const messages =
        root.querySelectorAll(
            ".message"
        );


    for (const message of messages) {

        const id =
            parseInt(
                message.getAttribute(
                    "data-message-id"
                ),
                10
            );


        if (
            !isNaN(id) &&
            id === Number(messageId)
        ) {

            return message;
        }
    }


    return null;
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


    /*
     * اول سعی می‌کنیم یک پیام موجود که منوی
     * عملیات دارد پیدا کنیم تا دقیقاً همان HTML
     * و همان CSS را Clone کنیم.
     */

    const template =
        container.querySelector(
            ".message .message-actions"
        )?.closest(".message")
        ||
        container.querySelector(
            ".message"
        );


    let wrapper;


    /* =====================================================
       Clone Existing Message
       ===================================================== */

    if (template) {

        wrapper =
            template.cloneNode(true);

    } else {

        /*
         * اگر کانال کاملاً خالی باشد،
         * fallback با ساختار واقعی Index.cshtml.
         */

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

            </div>
        `;
    }


    /* =====================================================
       Message Attributes
       ===================================================== */

    wrapper.setAttribute(
        "data-message-id",
        String(message.id)
    );


    wrapper.setAttribute(
        "data-channel-id",
        String(message.channelId)
    );


    wrapper.setAttribute(
        "data-sender-id",
        String(message.senderId)
    );


    /* =====================================================
       Own Message
       ===================================================== */

    const isOwnMessage =
        Number(message.senderId) ===
        Number(currentUserId);


    wrapper.classList.toggle(
        "own-message",
        isOwnMessage
    );


    /* =====================================================
       Sender
       ===================================================== */

    const author =
        wrapper.querySelector(
            ".message-author"
        );


    if (author) {

        author.textContent =
            message.senderName || "";
    }


    /* =====================================================
       Content
       ===================================================== */

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


    /* =====================================================
       Time
       ===================================================== */

    const createdAt =
        message.createdAt
            ? new Date(message.createdAt)
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


    /* =====================================================
       Avatar
       ===================================================== */

    const avatar =
        wrapper.querySelector(
            ".message-avatar"
        );


    if (avatar) {

        if (message.senderAvatarUrl) {

            avatar.innerHTML = "";

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

            /*
             * در HTML فعلی avatar به صورت
             * متن مستقیم است، پس همان ساختار را
             * حفظ می‌کنیم.
             */

            avatar.innerHTML =
                "";


            avatar.textContent =
                (
                    message.senderName ||
                    "?"
                ).charAt(0);
        }
    }


    /* =====================================================
       Edited State
       ===================================================== */

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


    /* =====================================================
       Message Actions
       ===================================================== */

    const actions =
        wrapper.querySelector(
            ".message-actions"
        );


    if (isOwnMessage) {

        /*
         * پیام خود کاربر باید دکمه سه‌نقطه داشته باشد.
         */

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

            /*
             * پیام جدید همیشه با منوی بسته وارد شود.
             */

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

        /*
         * پیام دیگران نباید منوی ویرایش/حذف داشته باشد.
         */

        const ownActions =
            wrapper.querySelector(
                ".message-actions"
            );


        if (ownActions) {
            ownActions.remove();
        }
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


    scrollMessagesToBottom();


    console.log(
        "MESSAGE ADDED TO DOM:",
        message.id
    );
}


/* =========================================================
   SignalR Events
   ========================================================= */

function registerSignalREvents() {

    /* -----------------------------------------------------
       ReceiveMessage
       ----------------------------------------------------- */

    connection.on(
        "ReceiveMessage",
        function (message) {

            console.log(
                "REAL-TIME MESSAGE RECEIVED:",
                message
            );


            const currentChannel =
                getCurrentChannelId();


            console.log(
                "CURRENT CHANNEL:",
                currentChannel
            );


            console.log(
                "MESSAGE CHANNEL:",
                message.channelId
            );


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


    /* -----------------------------------------------------
       MessageEdited
       ----------------------------------------------------- */

    connection.on(
        "MessageEdited",
        function (message) {

            console.log(
                "MESSAGE EDITED:",
                message
            );


            const currentChannel =
                getCurrentChannelId();


            if (
                currentChannel &&
                Number(message.channelId) !==
                Number(currentChannel)
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
        }
    );


    /* -----------------------------------------------------
       MessageDeleted
       ----------------------------------------------------- */

    connection.on(
        "MessageDeleted",
        function (message) {

            console.log(
                "MESSAGE DELETED:",
                message
            );


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
   Start SignalR
   ========================================================= */

async function startSignalR() {

    if (connection) {
        return;
    }


    connection =
        new signalR.HubConnectionBuilder()
            .withUrl(
                "http://localhost:5070/hubs/chat"
            )
            .withAutomaticReconnect()
            .configureLogging(
                signalR.LogLevel.Information
            )
            .build();


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


            await joinCurrentChannel();
        }
    );


    connection.onclose(
        function (error) {

            console.warn(
                "SignalR connection closed.",
                error
            );


            connection = null;

            currentSignalRChannelId =
                null;
        }
    );


    try {

        await connection.start();


        console.log(
            "SignalR connected."
        );


        await joinCurrentChannel();

    } catch (error) {

        console.error(
            "SignalR connection failed:",
            error
        );


        connection = null;
    }
}


/* =========================================================
   Message Actions
   ========================================================= */

document.addEventListener(
    "click",
    async function (event) {

        /* =================================================
           THREE DOT BUTTON
           ================================================= */

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


            /*
             * Close every other menu.
             */

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

                /*
                 * تنها چیزی که اینجا تغییر می‌دهیم
                 * display است.
                 *
                 * background / border / padding /
                 * position / colors از CSS اصلی
                 * خود پروژه می‌آیند.
                 */

                menu.style.display =
                    "block";


                moreButton.setAttribute(
                    "aria-expanded",
                    "true"
                );
            }


            return;
        }


        /* =================================================
           EDIT
           ================================================= */

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
                    )
                    ||
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


            /* -------------------------------------------------
               Close Menu
               ------------------------------------------------- */

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


            /* -------------------------------------------------
               Inline Editor
               ------------------------------------------------- */

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


            /* -------------------------------------------------
               Cancel
               ------------------------------------------------- */

            cancelButton.addEventListener(
                "click",
                function () {

                    editor.remove();


                    textElement.style.display =
                        "";
                }
            );


            /* -------------------------------------------------
               Save
               ------------------------------------------------- */

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
                                    method: "POST",

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


        /* =================================================
           DELETE
           ================================================= */

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
                    )
                    ||
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
                        `/User/Home/DeleteMessage?messageId=${encodeURIComponent(messageId)}`,
                        {
                            method: "POST"
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


                    return;
                }

            } catch (error) {

                console.error(
                    "Delete request failed:",
                    error
                );
            }


            return;
        }


        /* =================================================
           CLICK OUTSIDE
           ================================================= */

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
   Initialize
   ========================================================= */

document.addEventListener(
    "DOMContentLoaded",
    function () {

        console.log(
            "ChatHub SignalR initializing..."
        );


        startSignalR();
    }
);