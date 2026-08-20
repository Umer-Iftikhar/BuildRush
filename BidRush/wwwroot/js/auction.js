async function initializeAuctionSignalR() {

    const auctionContainer = document.querySelector(
        ".auction-details-container"
    );

    if (!auctionContainer) {
        return;
    }

    const auctionId = auctionContainer.dataset.auctionId;

    if (!auctionId) {
        console.error("Auction ID not found.");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/auction")
        .withAutomaticReconnect()
        .build();


    connection.on("BidPlaced", function (data) {

        console.log("BidPlaced:", data);

        const bidForm =
            document.getElementById("placeBidForm");

        const currentHighestBid =
            document.getElementById("currentHighestBid");

        const minimumNextBid =
            document.getElementById("minimumNextBid");

        const auctionEndTime =
            document.getElementById("auctionEndTime");


        // Everyone watching the auction.

        if (currentHighestBid) {

            currentHighestBid.textContent =
                `Rs. ${Number(data.amount).toLocaleString(
                    "en-US",
                    {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    }
                )}`;
        }


        if (auctionEndTime) {

            const endTime = new Date(data.endTime);

            auctionEndTime.textContent =
                endTime.toLocaleString(
                    "en-US",
                    {
                        day: "2-digit",
                        month: "short",
                        year: "numeric",
                        hour: "2-digit",
                        minute: "2-digit"
                    }
                );
        }


        // Only bidders have a bid form.

        if (bidForm) {

            const minimumBidIncrement =
                parseFloat(
                    bidForm.dataset.minimumBidIncrement
                );

            const minimumNextBidValue =
                Number(data.amount) + minimumBidIncrement;

            const bidAmount =
                document.getElementById("bidAmount");


            if (minimumNextBid) {

                minimumNextBid.textContent =
                    `Rs. ${minimumNextBidValue.toLocaleString(
                        "en-US",
                        {
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        }
                    )}`;
            }


            if (bidAmount) {

                bidAmount.min = minimumNextBidValue;
                bidAmount.value = minimumNextBidValue;
            }
        }

    });


    connection.on("AuctionEnded", function (data) {

        console.log("AuctionEnded:", data);

    });


    try {

        await connection.start();

        console.log("SignalR connected.");

        await connection.invoke(
            "JoinAuction",
            parseInt(auctionId)
        );

        console.log(`Joined auction_${auctionId}`);

    }
    catch (error) {

        console.error(
            "SignalR initialization error:",
            error
        );

    }
}

document.addEventListener("DOMContentLoaded", async function () {

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

                const responseText = await response.text();


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

                const responseText = await response.text();

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

    await initializeAuctionSignalR();

});