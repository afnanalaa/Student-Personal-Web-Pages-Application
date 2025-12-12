'use strict';

document.addEventListener('DOMContentLoaded', () => {

    const cvForm = document.getElementById('cvForm');
    if (!cvForm) return;

    cvForm.addEventListener('submit', async function (e) {
        e.preventDefault(); // „‰⁄ «·›Ê—„ «·«› —«÷Ì ·√‰‰« ”‰” Œœ„ fetch

        const formData = new FormData(cvForm);

        try {
            const response = await fetch('/Student/CreateForm', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });

            if (response.ok) {
                const data = await response.json().catch(() => null);
                if (data && data.slug) {
                    window.location.href = `/Student/MyProfile?slug=${data.slug}`;
                } else {
                    window.location.href = '/Student/MyProfile';
                }
            } else {
                const text = await response.text();
                console.error("Server Error:", text);
                alert("ÕœÀ Œÿ√ √À‰«¡ Õ›Ÿ «·»Ì«‰« . Õ«Ê· „—… √Œ—Ï.");
            }

        } catch (error) {
            console.error("Submit Error:", error);
            alert("›‘· «·« ’«· »«·Œ«œ„.  Õﬁﬁ „‰ «·‘»ﬂ… ÊÕ«Ê· „—… √Œ—Ï.");
        }
    });
});
