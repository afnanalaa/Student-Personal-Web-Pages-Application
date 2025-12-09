function setupPasswordToggle(inputId, toggleBtnId, iconId) {
    const input = document.getElementById(inputId);
    const toggleButton = document.getElementById(toggleBtnId);
    const eyeIcon = document.getElementById(iconId);

    if (input && toggleButton && eyeIcon) {
        toggleButton.addEventListener('click', function() {
            const type = input.getAttribute('type') === 'password' ? 'text' : 'password';
            
            input.setAttribute('type', type);
            
            if (type === 'text') {
                eyeIcon.classList.remove('bi-eye');
                eyeIcon.classList.add('bi-eye-slash'); 
            } else {
                eyeIcon.classList.remove('bi-eye-slash');
                eyeIcon.classList.add('bi-eye');
            }
            
            input.focus();
        });
    }
}

document.addEventListener('DOMContentLoaded', function() {
    setupPasswordToggle('password', 'togglePassword', 'eyeIcon');
    
    setupPasswordToggle('confirmPassword', 'toggleConfirmPassword', 'eyeIconConfirm');
    
});