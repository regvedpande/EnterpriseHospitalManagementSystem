// site.js - EnterpriseHospitalManagement
// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        var alerts = document.querySelectorAll('.alert.alert-success, .alert.alert-danger');
        alerts.forEach(function (alert) {
            if (alert.classList.contains('fade')) {
                alert.classList.remove('show');
            }
        });
    }, 5000);
});
