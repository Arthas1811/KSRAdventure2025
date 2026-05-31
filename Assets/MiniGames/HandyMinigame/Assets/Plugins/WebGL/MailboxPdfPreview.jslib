mergeInto(LibraryManager.library, {
  MailboxPdfPreview_Open: function (titlePtr, dataPtr, length, streamingAssetsPathPtr) {
    var title = UTF8ToString(titlePtr);
    var streamingAssetsPath = UTF8ToString(streamingAssetsPathPtr);
    var heapBytes = new Uint8Array(HEAPU8.buffer, dataPtr, length);
    var pdfBytes = new Uint8Array(length);
    pdfBytes.set(heapBytes);

    if (!window.KSRMailboxPdfPreview) {
      window.KSRMailboxPdfPreview = (function () {
        var pdfJsPromise = null;
        var overlay = null;
        var pdfDocument = null;
        var currentPage = 1;
        var zoom = 1;
        var renderToken = 0;

        function normalizePath(path) {
          return (path || "").replace(/\/$/, "") + "/HandyMinigame/PdfJs";
        }

        function textButton(label) {
          var button = document.createElement("button");
          button.textContent = label;
          button.style.border = "0";
          button.style.borderRadius = "6px";
          button.style.background = "#005cb8";
          button.style.color = "#fff";
          button.style.font = "700 14px Arial, sans-serif";
          button.style.height = "38px";
          button.style.minWidth = "42px";
          button.style.padding = "0 14px";
          button.style.cursor = "pointer";
          return button;
        }

        function setButtonEnabled(button, enabled) {
          button.disabled = !enabled;
          button.style.opacity = enabled ? "1" : "0.42";
          button.style.cursor = enabled ? "pointer" : "default";
        }

        function createOverlay(title) {
          close();

          overlay = document.createElement("div");
          overlay.style.position = "fixed";
          overlay.style.left = "0";
          overlay.style.top = "0";
          overlay.style.right = "0";
          overlay.style.bottom = "0";
          overlay.style.zIndex = "2147483000";
          overlay.style.background = "rgba(3,5,8,0.9)";
          overlay.style.display = "flex";
          overlay.style.alignItems = "center";
          overlay.style.justifyContent = "center";
          overlay.style.fontFamily = "Arial, sans-serif";

          var panel = document.createElement("div");
          panel.style.width = "min(1050px, 86vw)";
          panel.style.height = "min(720px, 86vh)";
          panel.style.background = "#f6f8fb";
          panel.style.borderRadius = "8px";
          panel.style.boxShadow = "0 22px 70px rgba(0,0,0,0.42)";
          panel.style.overflow = "hidden";
          panel.style.display = "flex";
          panel.style.flexDirection = "column";
          overlay.appendChild(panel);

          var header = document.createElement("div");
          header.style.height = "58px";
          header.style.flex = "0 0 58px";
          header.style.background = "#093d75";
          header.style.color = "#fff";
          header.style.display = "flex";
          header.style.alignItems = "center";
          header.style.justifyContent = "space-between";
          header.style.padding = "0 18px 0 22px";
          panel.appendChild(header);

          var heading = document.createElement("div");
          heading.textContent = title || "PDF attachment";
          heading.style.font = "700 20px Arial, sans-serif";
          heading.style.overflow = "hidden";
          heading.style.textOverflow = "ellipsis";
          heading.style.whiteSpace = "nowrap";
          heading.style.paddingRight = "18px";
          header.appendChild(heading);

          var closeButton = textButton("X");
          closeButton.style.background = "rgba(255,255,255,0.18)";
          closeButton.style.minWidth = "42px";
          closeButton.onclick = close;
          header.appendChild(closeButton);

          var scroller = document.createElement("div");
          scroller.style.flex = "1 1 auto";
          scroller.style.margin = "18px";
          scroller.style.background = "#d6dde6";
          scroller.style.overflow = "auto";
          scroller.style.display = "flex";
          scroller.style.justifyContent = "center";
          scroller.style.alignItems = "flex-start";
          scroller.style.padding = "18px";
          panel.appendChild(scroller);

          var status = document.createElement("div");
          status.textContent = "Loading PDF...";
          status.style.margin = "auto";
          status.style.color = "#5c6672";
          status.style.font = "700 18px Arial, sans-serif";
          scroller.appendChild(status);

          var canvas = document.createElement("canvas");
          canvas.style.background = "#fff";
          canvas.style.boxShadow = "0 4px 18px rgba(15,23,42,0.24)";
          canvas.style.display = "none";
          scroller.appendChild(canvas);

          var footer = document.createElement("div");
          footer.style.height = "58px";
          footer.style.flex = "0 0 58px";
          footer.style.display = "flex";
          footer.style.alignItems = "center";
          footer.style.justifyContent = "center";
          footer.style.gap = "10px";
          footer.style.borderTop = "1px solid #d8dee8";
          panel.appendChild(footer);

          var previous = textButton("Previous");
          var next = textButton("Next");
          var pageLabel = document.createElement("div");
          pageLabel.style.minWidth = "150px";
          pageLabel.style.textAlign = "center";
          pageLabel.style.font = "700 16px Arial, sans-serif";
          pageLabel.style.color = "#17202b";
          var zoomOut = textButton("-");
          var zoomLabel = document.createElement("div");
          zoomLabel.style.minWidth = "70px";
          zoomLabel.style.textAlign = "center";
          zoomLabel.style.font = "700 16px Arial, sans-serif";
          zoomLabel.style.color = "#17202b";
          var zoomIn = textButton("+");

          previous.onclick = function () {
            if (pdfDocument && currentPage > 1) {
              currentPage--;
              renderPage();
            }
          };
          next.onclick = function () {
            if (pdfDocument && currentPage < pdfDocument.numPages) {
              currentPage++;
              renderPage();
            }
          };
          zoomOut.onclick = function () {
            if (pdfDocument && zoom > 0.75) {
              zoom = Math.max(0.75, zoom - 0.25);
              renderPage();
            }
          };
          zoomIn.onclick = function () {
            if (pdfDocument && zoom < 2.5) {
              zoom = Math.min(2.5, zoom + 0.25);
              renderPage();
            }
          };

          footer.appendChild(previous);
          footer.appendChild(next);
          footer.appendChild(pageLabel);
          footer.appendChild(zoomOut);
          footer.appendChild(zoomLabel);
          footer.appendChild(zoomIn);
          document.body.appendChild(overlay);

          return {
            canvas: canvas,
            status: status,
            previous: previous,
            next: next,
            pageLabel: pageLabel,
            zoomOut: zoomOut,
            zoomIn: zoomIn,
            zoomLabel: zoomLabel
          };
        }

        async function createBlobUrl(url, mimeType) {
          var response = await fetch(url);
          if (!response.ok) {
            throw new Error("Could not load " + url + " (" + response.status + ")");
          }

          var blob = new Blob([await response.text()], { type: mimeType });
          return URL.createObjectURL(blob);
        }

        async function loadPdfJs(basePath) {
          if (!pdfJsPromise) {
            pdfJsPromise = (async function () {
              var moduleUrl = await createBlobUrl(basePath + "/build/pdf.min.mjs", "text/javascript");
              var workerUrl = await createBlobUrl(basePath + "/build/pdf.worker.min.mjs", "text/javascript");
              var pdfjs = await import(moduleUrl);
              pdfjs.GlobalWorkerOptions.workerSrc = workerUrl;
              return pdfjs;
            })();
          }

          return pdfJsPromise;
        }

        function updateControls(elements) {
          var hasDocument = !!pdfDocument;
          elements.pageLabel.textContent = hasDocument ? "Page " + currentPage + " / " + pdfDocument.numPages : "Page - / -";
          elements.zoomLabel.textContent = Math.round(zoom * 100) + "%";
          setButtonEnabled(elements.previous, hasDocument && currentPage > 1);
          setButtonEnabled(elements.next, hasDocument && currentPage < pdfDocument.numPages);
          setButtonEnabled(elements.zoomOut, hasDocument && zoom > 0.75);
          setButtonEnabled(elements.zoomIn, hasDocument && zoom < 2.5);
        }

        async function renderPage() {
          var elements = overlay && overlay._pdfPreviewElements;
          if (!pdfDocument || !elements) {
            return;
          }

          var token = ++renderToken;
          elements.status.textContent = "Rendering page...";
          elements.status.style.display = "block";
          elements.canvas.style.display = "none";
          updateControls(elements);

          try {
            var page = await pdfDocument.getPage(currentPage);
            if (token !== renderToken) {
              return;
            }

            var deviceScale = Math.min(2, window.devicePixelRatio || 1);
            var viewport = page.getViewport({ scale: zoom * deviceScale });
            var canvas = elements.canvas;
            var context = canvas.getContext("2d", { alpha: false });
            canvas.width = Math.floor(viewport.width);
            canvas.height = Math.floor(viewport.height);
            canvas.style.width = Math.floor(viewport.width / deviceScale) + "px";
            canvas.style.height = Math.floor(viewport.height / deviceScale) + "px";

            await page.render({ canvasContext: context, viewport: viewport }).promise;
            if (token !== renderToken) {
              return;
            }

            elements.status.style.display = "none";
            elements.canvas.style.display = "block";
            updateControls(elements);
          } catch (error) {
            showError(error && error.message ? error.message : "Could not render the PDF page.");
          }
        }

        function showError(message) {
          var elements = overlay && overlay._pdfPreviewElements;
          if (!elements) {
            return;
          }

          elements.canvas.style.display = "none";
          elements.status.style.display = "block";
          elements.status.textContent = message || "Could not preview the PDF.";
          updateControls(elements);
        }

        async function open(title, pdfBytes, streamingAssetsPath) {
          var basePath = normalizePath(streamingAssetsPath);
          var elements = createOverlay(title);
          overlay._pdfPreviewElements = elements;
          pdfDocument = null;
          currentPage = 1;
          zoom = 1;
          var token = ++renderToken;
          updateControls(elements);

          try {
            var pdfjs = await loadPdfJs(basePath);
            var loadedDocument = await pdfjs.getDocument({
              data: pdfBytes,
              cMapUrl: basePath + "/cmaps/",
              cMapPacked: true,
              standardFontDataUrl: basePath + "/standard_fonts/",
              wasmUrl: basePath + "/wasm/"
            }).promise;

            if (token !== renderToken || !overlay) {
              try {
                loadedDocument.destroy();
              } catch (error) {
              }

              return;
            }

            pdfDocument = loadedDocument;
            renderPage();
          } catch (error) {
            if (token !== renderToken || !overlay) {
              return;
            }

            showError(error && error.message ? error.message : "Could not load the PDF preview.");
          }
        }

        function close() {
          renderToken++;
          if (pdfDocument) {
            try {
              pdfDocument.destroy();
            } catch (error) {
            }
          }

          pdfDocument = null;
          if (overlay && overlay.parentNode) {
            overlay.parentNode.removeChild(overlay);
          }

          overlay = null;
        }

        return {
          open: open,
          close: close
        };
      })();
    }

    window.KSRMailboxPdfPreview.open(title, pdfBytes, streamingAssetsPath);
  }
});
