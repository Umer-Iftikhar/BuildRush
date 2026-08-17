document.addEventListener("DOMContentLoaded", function () {

    const deleteButton = document.querySelector(".delete-auction-btn");

    if (deleteButton) {

        deleteButton.addEventListener("click", async function () {

            const confirmed = confirm(
                "Are you sure you want to delete this auction?"
            );

            if (!confirmed)
                return;

            const auctionId = this.dataset.id;

            try {

                const response = await fetch(
                    `/Auction/Delete/${auctionId}`,
                    {
                        method: "POST",
                        headers: {
                            "RequestVerificationToken": getCsrfToken()
                        }
                    }
                );

                console.log("Delete response status:", response.status);
                console.log(
                    "Delete response content-type:",
                    response.headers.get("content-type")
                );

                const responseText = await response.text();

                console.log("Delete raw response:", responseText);

                let result;

                try {
                    result = JSON.parse(responseText);
                }
                catch (jsonError) {

                    console.error(
                        "Response is not valid JSON:",
                        jsonError
                    );

                    showToast(
                        "Server returned an invalid response. Check the browser console.",
                        "error"
                    );

                    return;
                }

                if (result.responseCode !== 200) {
                    showToast(
                        result.responseMessage,
                        "error"
                    );

                    return;
                }

                showToast(
                    result.responseMessage,
                    "success"
                );

                setTimeout(() => {
                    window.location.href = "/Auction/Index";
                }, 1000);
            }
            catch (error) {

                console.error(
                    "Delete auction error:",
                    error
                );

                showToast(
                    "An unexpected error occurred while deleting the auction.",
                    "error"
                );
            }
        });
    }


    const bidForm = document.getElementById("placeBidForm");

    if (bidForm) {

        bidForm.addEventListener("submit", async function (event) {

            event.preventDefault();

            const auctionId = bidForm.querySelector(
                'input[name="auctionId"]'
            ).value;

            const amount = bidForm.querySelector(
                'input[name="amount"]'
            ).value;

            try {

                const response = await fetch(
                    "/Bid/PlaceBid",
                    {
                        method: "POST",
                        headers: {
                            "Content-Type":
                                "application/x-www-form-urlencoded",
                            "RequestVerificationToken":
                                getCsrfToken()
                        },
                        body: new URLSearchParams({
                            auctionId: auctionId,
                            amount: amount
                        })
                    }
                );

                console.log("Bid response status:", response.status);
                console.log(
                    "Bid response content-type:",
                    response.headers.get("content-type")
                );

                const responseText = await response.text();

                console.log("Bid raw response:", responseText);

                let result;

                try {
                    result = JSON.parse(responseText);
                }
                catch (jsonError) {

                    console.error(
                        "Response is not valid JSON:",
                        jsonError
                    );

                    showToast(
                        "Server returned an invalid response. Check the browser console.",
                        "error"
                    );

                    return;
                }

                if (result.responseCode !== 200) {

                    showToast(
                        result.responseMessage,
                        "error"
                    );

                    return;
                }

                showToast(
                    result.responseMessage,
                    "success"
                );

                setTimeout(() => {
                    window.location.reload();
                }, 1000);

            }
            catch (error) {

                console.error(
                    "Place bid error:",
                    error
                );

                showToast(
                    "An unexpected error occurred while placing your bid.",
                    "error"
                );
            }
        });
    }

});