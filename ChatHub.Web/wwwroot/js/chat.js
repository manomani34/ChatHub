"use strict";

document.addEventListener(
    "DOMContentLoaded",
    function () {

        const form =
            document.querySelector(
                ".composer"
            );

        if (!form) {
            return;
        }


        const contentInput =
            form.querySelector(
                '[name="content"]'
            );

        if (!contentInput) {
            return;
        }


        // =====================================================
        // ارسال با Enter
        // =====================================================

        contentInput.addEventListener(
            "keydown",
            function (event) {

                if (
                    event.key === "Enter" &&
                    !event.shiftKey
                ) {

                    event.preventDefault();

                    form.requestSubmit();
                }
            }
        );


        // =====================================================
        // Submit
        // =====================================================

        form.addEventListener(
            "submit",
            async function (event) {

                event.preventDefault();


                const content =
                    contentInput.value.trim();


                if (!content) {
                    return;
                }


                /*
                 * وضعیت فرم را هر بار هنگام Submit
                 * مستقیماً از DOM می‌خوانیم.
                 */

                const conversationIdInput =
                    form.querySelector(
                        '[name="conversationId"]'
                    );


                const channelIdInput =
                    form.querySelector(
                        '[name="channelId"]'
                    );


                const userNameInput =
                    form.querySelector(
                        '[name="userName"]'
                    );


                // =================================================
                // Reply Parent Message
                // =================================================

                const parentMessageInput =
                    form.querySelector(
                        '[name="parentMessageId"]'
                    );


                const parentMessageId =
                    parentMessageInput
                        ? parentMessageInput.value.trim()
                        : "";


                const isDirectMessage =
                    conversationIdInput !== null;


                let channelId = 0;
                let conversationId = 0;
                let userName = "";


                // =================================================
                // Channel
                // =================================================

                if (!isDirectMessage) {

                    if (!channelIdInput) {

                        console.error(
                            "Channel ID input not found."
                        );

                        return;
                    }


                    channelId =
                        Number(
                            channelIdInput.value
                        );


                    if (channelId <= 0) {

                        console.error(
                            "Invalid channel id."
                        );

                        return;
                    }
                }


                // =================================================
                // Direct Message
                // =================================================

                else {

                    conversationId =
                        Number(
                            conversationIdInput.value
                        );


                    if (conversationId <= 0) {

                        console.error(
                            "Invalid conversation id."
                        );

                        return;
                    }


                    if (userNameInput) {

                        userName =
                            userNameInput.value.trim();
                    }
                }


                // =================================================
                // Prevent Double Submit
                // =================================================

                const submitButton =
                    form.querySelector(
                        'button[type="submit"]'
                    );


                if (submitButton) {
                    submitButton.disabled = true;
                }


                try {

                    let url;
                    let body;


                    // =================================================
                    // Channel
                    // =================================================

                    if (!isDirectMessage) {

                        url =
                            "/User/Home/SendMessage";


                        body =
                            new URLSearchParams({
                                channelId:
                                    channelId,

                                content:
                                    content,

                                parentMessageId:
                                    parentMessageId
                            });
                    }


                    // =================================================
                    // Direct Message
                    // =================================================

                    else {

                        url =
                            "/User/Home/SendDirectMessage";


                        body =
                            new URLSearchParams({
                                conversationId:
                                    conversationId,

                                content:
                                    content,

                                userName:
                                    userName,

                                parentMessageId:
                                    parentMessageId
                            });
                    }


                    console.log(
                        "SEND MESSAGE:",
                        {
                            isDirectMessage,
                            channelId,
                            conversationId,
                            userName,
                            parentMessageId,
                            content
                        }
                    );


                    const response =
                        await fetch(
                            url,
                            {
                                method: "POST",

                                headers: {
                                    "Content-Type":
                                        "application/x-www-form-urlencoded"
                                },

                                body:
                                    body
                            }
                        );


                    if (!response.ok) {

                        const errorText =
                            await response.text();

                        console.error(
                            "Send message failed:",
                            response.status,
                            errorText
                        );

                        return;
                    }


                    contentInput.value = "";

                    contentInput.focus();


                    /*
                     * بعد از ارسال موفق Reply،
                     * حالت Reply را پاک می‌کنیم.
                     */

                    if (
                        typeof clearReplyTarget ===
                        "function"
                    ) {

                        clearReplyTarget();
                    }


                    console.log(
                        isDirectMessage
                            ? "DIRECT MESSAGE SENT."
                            : "CHANNEL MESSAGE SENT."
                    );

                }
                catch (error) {

                    console.error(
                        "Send message error:",
                        error
                    );

                }
                finally {

                    if (submitButton) {
                        submitButton.disabled = false;
                    }

                }

            }
        );

    }
);

// =====================================================
// Message Search
// =====================================================

document.addEventListener(
    "DOMContentLoaded",
    function () {

        const searchInput =
            document.getElementById(
                "messageSearchInput"
            );

        const searchButton =
            document.getElementById(
                "messageSearchButton"
            );

        const chatMessages =
            document.getElementById(
                "chatMessages"
            );

        const searchContainer =
            document.querySelector(
                ".chat-search"
            );


        if (
            !searchInput ||
            !searchButton ||
            !chatMessages ||
            !searchContainer
        ) {
            return;
        }


        // =================================================
        // Search Results Container
        // =================================================

        const resultsContainer =
            document.createElement("div");

        resultsContainer.id =
            "messageSearchResults";

        resultsContainer.className =
            "chat-search-results";

        searchContainer.appendChild(
            resultsContainer
        );


        // =================================================
        // Close Results
        // =================================================

        function closeSearchResults() {

            resultsContainer.innerHTML = "";

            resultsContainer.classList.remove(
                "show"
            );
        }


        // =================================================
        // Highlight Message
        // =================================================

        function highlightMessage(
            messageId
        ) {

            const message =
                document.querySelector(
                    `.message[data-message-id="${messageId}"]`
                );

            if (!message) {
                return;
            }


            message.scrollIntoView({
                behavior: "smooth",
                block: "center"
            });


            message.classList.remove(
                "message-search-highlight"
            );


            // Force reflow so the animation
            // can run again for the same message.

            void message.offsetWidth;


            message.classList.add(
                "message-search-highlight"
            );


            setTimeout(
                function () {

                    message.classList.remove(
                        "message-search-highlight"
                    );

                },
                1800
            );
        }


        // =================================================
        // Render Search Results
        // =================================================

        function renderSearchResults(
            results
        ) {

            resultsContainer.innerHTML = "";

            if (!results.length) {

                const empty =
                    document.createElement("div");

                empty.className =
                    "chat-search-empty";

                empty.textContent =
                    "پیامی پیدا نشد.";

                resultsContainer.appendChild(
                    empty
                );

                resultsContainer.classList.add(
                    "show"
                );

                return;
            }


            results.forEach(
                function (message) {

                    const item =
                        document.createElement("button");

                    item.type = "button";

                    item.className =
                        "chat-search-result";


                    const top =
                        document.createElement("div");

                    top.className =
                        "chat-search-result-top";


                    const author =
                        document.createElement("span");

                    author.className =
                        "chat-search-result-author";

                    author.textContent =
                        message.senderName || "کاربر";


                    const time =
                        document.createElement("span");

                    time.className =
                        "chat-search-result-time";


                    if (message.createdAt) {

                        const date =
                            new Date(
                                message.createdAt
                            );

                        if (!Number.isNaN(
                            date.getTime()
                        )) {

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


                    top.appendChild(
                        author
                    );

                    top.appendChild(
                        time
                    );


                    const content =
                        document.createElement("div");

                    content.className =
                        "chat-search-result-content";

                    content.textContent =
                        message.content || "";


                    item.appendChild(
                        top
                    );

                    item.appendChild(
                        content
                    );


                    item.addEventListener(
                        "click",
                        function () {

                            highlightMessage(
                                message.id
                            );

                            closeSearchResults();

                        }
                    );


                    resultsContainer.appendChild(
                        item
                    );

                }
            );


            resultsContainer.classList.add(
                "show"
            );
        }


        // =================================================
        // Execute Search
        // =================================================

        async function searchMessages() {

            const query =
                searchInput.value.trim();


            if (!query) {

                closeSearchResults();

                return;
            }


            const channelId =
                chatMessages.dataset.channelId;

            const conversationId =
                chatMessages.dataset.conversationId;


            const params =
                new URLSearchParams();

            params.set(
                "query",
                query
            );


            if (channelId) {

                params.set(
                    "channelId",
                    channelId
                );
            }
            else if (conversationId) {

                params.set(
                    "conversationId",
                    conversationId
                );
            }
            else {

                closeSearchResults();

                return;
            }


            params.set(
                "take",
                "50"
            );


            searchButton.disabled = true;


            try {

                resultsContainer.innerHTML = "";

                const loading =
                    document.createElement("div");

                loading.className =
                    "chat-search-empty";

                loading.textContent =
                    "در حال جستجو...";

                resultsContainer.appendChild(
                    loading
                );

                resultsContainer.classList.add(
                    "show"
                );


                const response =
                    await fetch(
                        `/User/Home/SearchMessages?${params.toString()}`,
                        {
                            method: "GET",
                            headers: {
                                "Accept":
                                    "application/json"
                            }
                        }
                    );


                if (!response.ok) {

                    throw new Error(
                        `Search failed: ${response.status}`
                    );
                }


                const results =
                    await response.json();


                renderSearchResults(
                    Array.isArray(results)
                        ? results
                        : []
                );

            }
            catch (error) {

                console.error(
                    "Message search error:",
                    error
                );


                resultsContainer.innerHTML = "";

                const errorElement =
                    document.createElement("div");

                errorElement.className =
                    "chat-search-empty";

                errorElement.textContent =
                    "خطا در جستجوی پیام.";

                resultsContainer.appendChild(
                    errorElement
                );

                resultsContainer.classList.add(
                    "show"
                );

            }
            finally {

                searchButton.disabled = false;

            }
        }


        // =================================================
        // Search Button
        // =================================================

        searchButton.addEventListener(
            "click",
            searchMessages
        );


        // =================================================
        // Enter
        // =================================================

        searchInput.addEventListener(
            "keydown",
            function (event) {

                if (
                    event.key === "Enter"
                ) {

                    event.preventDefault();

                    searchMessages();
                }
            }
        );


        // =================================================
        // Close on Outside Click
        // =================================================

        document.addEventListener(
            "click",
            function (event) {

                if (
                    !searchContainer.contains(
                        event.target
                    )
                ) {

                    closeSearchResults();
                }
            }
        );

    }
);