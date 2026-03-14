// Clipboard helper for Blazor
window.erebusClipboard = {
    copyText: async function(text) {
        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(text);
                return true;
            } else {
                // Fallback for older browsers
                const textArea = document.createElement("textarea");
                textArea.value = text;
                textArea.style.position = "fixed";
                textArea.style.left = "-999999px";
                textArea.style.top = "-999999px";
                document.body.appendChild(textArea);
                textArea.focus();
                textArea.select();
                try {
                    document.execCommand('copy');
                    return true;
                } catch (err) {
                    console.error('Failed to copy text:', err);
                    return false;
                } finally {
                    document.body.removeChild(textArea);
                }
            }
        } catch (err) {
            console.error('Clipboard write failed:', err);
            return false;
        }
    }
};
