
function showLoader(element, message = "Loading...") {

    element.innerHTML = `
        <div class="loader-container">

            <div class="loader"></div>

            <p class="mt-3 mb-0 text-muted">
                ${message}
            </p>

        </div>
    `;

}


function getCsrfToken() {
    const token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
    );

    return token?.value;
}


function showToast(message, type = "success") {

    const toastElement = document.getElementById("appToast");

    const toastTitle = document.getElementById("toastTitle");
    const toastMessage = document.getElementById("toastMessage");
    const toastIcon = document.getElementById("toastIcon");

    const config = {
        success: {
            title: "Success",
            icon: "✓",
            cssClass: "toast-success"
        },
        warning: {
            title: "Warning",
            icon: "!",
            cssClass: "toast-warning"
        },
        error: {
            title: "Error",
            icon: "×",
            cssClass: "toast-error"
        }
    };

    const current = config[type] ?? config.success;

    toastElement.classList.remove(
        "toast-success",
        "toast-warning",
        "toast-error"
    );

    toastElement.classList.add(current.cssClass);

    toastTitle.textContent = current.title;
    toastMessage.textContent = message;
    toastIcon.textContent = current.icon;
    toastIcon.setAttribute("aria-label", current.title);

    const toast = bootstrap.Toast.getOrCreateInstance(toastElement);

    toast.show();
}