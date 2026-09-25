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