"use strict";

document.addEventListener("DOMContentLoaded", function () {

    const form =
        document.querySelector(".composer");

    if (!form) {
        return;
    }

    const contentInput =
        form.querySelector('[name="content"]');

    if (!contentInput) {
        return;
    }

    // ارسال با Enter
    contentInput.addEventListener("keydown", function (event) {

        if (event.key === "Enter" && !event.shiftKey) {

            event.preventDefault();

            form.requestSubmit();
        }
    });


    form.addEventListener("submit", async function (event) {

        event.preventDefault();

        const channelId =
            Number(
                form.querySelector(
                    '[name="channelId"]'
                ).value
            );

        const content =
            contentInput.value.trim();

        if (channelId <= 0 || !content) {
            return;
        }

        // جلوگیری از ارسال چندباره
        const submitButton =
            form.querySelector('button[type="submit"]');

        if (submitButton) {
            submitButton.disabled = true;
        }

        try {

            const response =
                await fetch(
                    "/User/Home/SendMessage",
                    {
                        method: "POST",

                        headers: {
                            "Content-Type":
                                "application/x-www-form-urlencoded"
                        },

                        body:
                            new URLSearchParams({
                                channelId: channelId,
                                content: content
                            })
                    }
                );


            if (!response.ok) {

                console.error(
                    "Send message failed:",
                    response.status
                );

                return;
            }


            // پاک کردن Composer
            contentInput.value = "";

            // برگشت Focus به Input
            contentInput.focus();

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

    });

});