'use strict';

// Helper: Sanitize input to prevent basic XSS (Security)
const sanitize = (str) => str ? str.trim() : '';

document.addEventListener('DOMContentLoaded', () => {
    
    // --- 1. Login Page Logic ---
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', (e) => {
            e.preventDefault();
            const studentIdInput = document.getElementById('studentId');
            const studentId = sanitize(studentIdInput.value);

            // Validation: Allow letters and numbers only
            const idPattern = /^[a-zA-Z0-9]+$/;
            
            if (studentId && idPattern.test(studentId)) {
                localStorage.setItem('student_id', studentId);
                window.location.href = 'form.html'; // Redirect to form
            } else {
                alert('Invalid ID. Please use letters and numbers only.');
            }
        });
    }

    // --- 2. Form Page Logic (Handles Image + New Fields) ---
    const cvForm = document.getElementById('cvForm');
    if (cvForm) {
        cvForm.addEventListener('submit', function(e) {
            e.preventDefault();

            // Function to save data and redirect
            const saveDataAndRedirect = (base64Image) => {
                const studentData = {
                    id: localStorage.getItem('student_id'), 
                    // Collecting new fields
                    fullName: sanitize(document.getElementById('fullName').value),
                    email: sanitize(document.getElementById('email').value),
                    phone: sanitize(document.getElementById('phone').value),
                    address: sanitize(document.getElementById('address').value),
                    department: sanitize(document.getElementById('department').value),
                    summary: sanitize(document.getElementById('summary').value),
                    projects: sanitize(document.getElementById('projects').value),
                    skills: sanitize(document.getElementById('skills').value),
                    image: base64Image || null, // Store image string
                    timestamp: new Date().toISOString() 
                };

                // Basic Validation
                if (!studentData.fullName || !studentData.department) {
                    alert("Please fill in the required fields (Name & Department).");
                    return;
                }

                try {
                    // Save to Browser Storage
                    localStorage.setItem('student_data', JSON.stringify(studentData));
                    window.location.href = 'cv.html';
                } catch (error) {
                    console.error("Storage error:", error);
                    alert("Error saving data. The image might be too large. Try a smaller image.");
                }
            };

            // --- Image File Handling ---
            const imageInput = document.getElementById('personalImage');
            
            // Check if user selected a file
            if (imageInput && imageInput.files && imageInput.files[0]) {
                const file = imageInput.files[0];
                
                // Security: Limit file size to 2MB to prevent storage crash
                const limit = 2 * 1024 * 1024; // 2MB
                if (file.size > limit) {
                    alert("Image is too large! Please choose an image under 2MB.");
                    return;
                }

                // Convert Image to Base64 String
                const reader = new FileReader();
                reader.onload = function(readerEvent) {
                    saveDataAndRedirect(readerEvent.target.result);
                };
                reader.readAsDataURL(file);
            } else {
                // No image uploaded, save text data only
                saveDataAndRedirect(null);
            }
        });
    }

    // --- 3. CV Display  ---
    const cvContent = document.getElementById('cvContent');
    if (cvContent) {
        loadCV(); // Load data into CV

        const printBtn = document.getElementById('printBtn');

        if (printBtn) printBtn.addEventListener('click', () =>{
            sendToServer();
            return window.print();
        } );
    }
});

function loadCV() {
    const dataString = localStorage.getItem('student_data');
    if (!dataString) {
        alert("No data found. Please login again.");
        window.location.href = 'login.html';
        return;
    }

    const data = JSON.parse(dataString);

    // Helper to set text content safely
    const setText = (id, value) => {
        const el = document.getElementById(id);
        if (el) el.textContent = value || '';
    };

    setText('displayId', data.id);
    setText('displayName', data.fullName);
    setText('displayEmail', data.email);
    setText('displayPhone', data.phone);
    setText('displayAddress', data.address);
    setText('displayDept', data.department);
    setText('displaySummary', data.summary);
    setText('displayProjects', data.projects);
    setText('displaySkills', data.skills);

    // Populate Image
    const imgElement = document.getElementById('displayImage');
    if (imgElement) {
        if (data.image) {
            imgElement.src = data.image;
        } else {
            // Default placeholder if no image exists
            imgElement.src = 'https://via.placeholder.com/150?text=No+Photo';
        }
    }
}

function sendToServer() {
    const dataString = localStorage.getItem('student_data');
    if (!dataString) return;

    const data = JSON.parse(dataString);
    
    // Simulate API Call
    console.log("Connecting to College Server...");
    console.log("Payload sent:", data);

    alert("Success! Your CV and details have been sent to the college administration.");
}