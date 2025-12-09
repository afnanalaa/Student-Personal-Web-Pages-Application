
function setPrivacy(level) {
    let message = '';
    let apiEndpoint = '/api/profile/update-visibility';
    
    // 1. تحديد الرسالة بناءً على مستوى الخصوصية
    if (level === 'public') {
        message = 'Your profile is now public and visible to everyone.';
    } else if (level === 'university') {
        message = 'Your profile is now restricted to registered university students only.';
    } else if (level === 'private') {
        message = 'Your profile is now private (Admin only). It will not appear in the directory.';
    } else {
        return; // لا تفعل شيئاً إذا كان المستوى غير معروف
    }

    // 2. إرسال البيانات إلى السيرفر (Back-End)
    // (يجب على فريق الباك إند بناء هذا الـ Endpoint)
    fetch(apiEndpoint, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ visibility: level }),
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert("Success! " + message);
        } else {
            alert("Error: Could not update visibility. Please try again.");
        }
    })
    .catch(error => {
        console.error('API Error:', error);
        alert("An error occurred during update.");
    });
}