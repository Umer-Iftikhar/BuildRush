document.addEventListener("DOMContentLoaded", function () {

    const deleteButton = document.querySelector(".delete-auction-btn");

    if (!deleteButton)
        return;

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

            const result = await response.json();

            if (result.responseCode !== 200) {
                showToast(result.responseMessage, "error");
                return;
            }

            showToast(result.responseMessage, "success");

            setTimeout(() => {
                window.location.href = "/Auction/Index";
            }, 1000);

        }
        catch (error) {

            console.error("Delete auction error:", error);

            showToast(
                "An unexpected error occurred while deleting the auction.",
                "error"
            );
        }
    });

});