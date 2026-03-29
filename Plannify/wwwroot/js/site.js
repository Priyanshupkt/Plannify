// Auto-dismiss flash alerts after 4 seconds
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.flash-alert').forEach(function (el) {
        setTimeout(function () {
            el.style.opacity = '0';
            setTimeout(function () { el.remove(); }, 500);
        }, 4000);
    });
});

// Delete modal helpers
function showDeleteModal(entityName, formId) {
    document.getElementById('deleteModalEntityName').textContent = entityName;
    document.getElementById('deleteModalForm').id = formId;
    document.getElementById('deleteModal').classList.remove('hidden');
}
function hideDeleteModal() {
    document.getElementById('deleteModal').classList.add('hidden');
}
