mergeInto(LibraryManager.library, {
    CopyToClipboard: function (text) {
        var str = UTF8ToString(text);
        var tempInput = document.createElement("textarea");
        tempInput.style = "position: absolute; left: -1000px; top: -1000px;";
        tempInput.value = str;
        document.body.appendChild(tempInput);
        tempInput.select();
        try {
            document.execCommand("copy");
        } catch (err) {
            console.error("Copy to clipboard failed", err);
        }
        document.body.removeChild(tempInput);
    }
});