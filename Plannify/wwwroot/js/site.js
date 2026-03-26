// Confirm dialog wrapper
function confirmAction(message) {
    return confirm(message);
}

// Auto-dismiss flash alerts
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(alert => {
        setTimeout(function () {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 4000);
    });
});

// Initialize Select2 on all .select2 elements
document.addEventListener('DOMContentLoaded', function () {
    if (typeof $ !== 'undefined' && typeof $.fn.select2 !== 'undefined') {
        $('.select2').select2({
            placeholder: 'Select an option',
            allowClear: true,
            theme: 'bootstrap-5'
        });
    }
});

// Utility to get CSRF token for AJAX requests
function getCsrfToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
}

// Utility to enable/disable form on demand
function toggleFormDisabled(formId, disabled) {
    const form = document.getElementById(formId);
    if (form) {
        const inputs = form.querySelectorAll('input, select, textarea, button');
        inputs.forEach(input => {
            if (input.type !== 'submit' || input.name !== 'submit') {
                input.disabled = disabled;
            }
        });
    }
}

// Log activity (for debugging/audit)
function logActivity(action, details) {
    console.log(`[${new Date().toISOString()}] ${action}:`, details);
}
