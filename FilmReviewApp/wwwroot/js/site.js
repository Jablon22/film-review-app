(function () {
    "use strict";

    function debounce(fn, delay) {
        let timer = null;
        return function (...args) {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), delay);
        };
    }

    function escapeHtml(str) {
        const div = document.createElement("div");
        div.textContent = str ?? "";
        return div.innerHTML;
    }

    // ---------------------------------------------------------------
    // Live movie search (navbar + catalog)
    // ---------------------------------------------------------------
    function setupLiveSearch(inputId, resultsId) {
        const input = document.getElementById(inputId);
        const results = document.getElementById(resultsId);
        if (!input || !results) {
            return;
        }

        const render = (items) => {
            results.innerHTML = "";
            if (!items || items.length === 0) {
                const li = document.createElement("li");
                li.className = "list-group-item text-muted small";
                li.textContent = "Brak wyników";
                results.appendChild(li);
                return;
            }
            items.forEach((m) => {
                const a = document.createElement("a");
                a.className = "list-group-item list-group-item-action d-flex align-items-center gap-2";
                a.href = "/Movies/Details/" + m.id;
                const poster = m.posterUrl
                    ? `<img src="${escapeHtml(m.posterUrl)}" alt="" width="32" height="48" style="object-fit:cover;border-radius:3px;">`
                    : `<span class="text-muted"><i class="fa-solid fa-film"></i></span>`;
                a.innerHTML = `${poster}<span><strong>${escapeHtml(m.title)}</strong> <small class="text-muted">(${m.year})</small></span>`;
                results.appendChild(a);
            });
        };

        const doSearch = debounce(function () {
            const q = input.value.trim();
            if (q.length < 2) {
                results.innerHTML = "";
                return;
            }
            results.innerHTML = '<li class="list-group-item text-center"><span class="ajax-spinner text-warning"></span></li>';
            fetch("/Movies/Search?q=" + encodeURIComponent(q))
                .then((r) => r.json())
                .then(render)
                .catch(() => { results.innerHTML = ""; });
        }, 300);

        input.addEventListener("input", doSearch);

        document.addEventListener("click", function (e) {
            if (!results.contains(e.target) && e.target !== input) {
                results.innerHTML = "";
            }
        });
    }

    // ---------------------------------------------------------------
    // Delete confirmation modal
    // ---------------------------------------------------------------
    function setupDeleteConfirm() {
        const modalEl = document.getElementById("confirmDeleteModal");
        if (!modalEl || typeof bootstrap === "undefined") {
            return;
        }
        const modal = new bootstrap.Modal(modalEl);
        const bodyEl = document.getElementById("confirmDeleteBody");
        const confirmBtn = document.getElementById("confirmDeleteBtn");
        let pendingForm = null;

        document.querySelectorAll("form.confirm-delete-form").forEach((form) => {
            form.addEventListener("submit", function (e) {
                if (form.dataset.confirmed === "true") {
                    return; // allow normal submit
                }
                e.preventDefault();
                pendingForm = form;
                bodyEl.textContent = form.dataset.confirmMessage
                    || "Czy na pewno chcesz usunąć ten element? Tej operacji nie można cofnąć.";
                modal.show();
            });
        });

        confirmBtn?.addEventListener("click", function () {
            if (pendingForm) {
                pendingForm.dataset.confirmed = "true";
                pendingForm.submit();
            }
            modal.hide();
        });
    }

    // ---------------------------------------------------------------
    // TMDB import (admin)
    // ---------------------------------------------------------------
    function setupTmdbImport() {
        const btn = document.getElementById("tmdbSearchBtn");
        const input = document.getElementById("tmdbQuery");
        const results = document.getElementById("tmdbResults");
        const token = document.querySelector('#tmdbImportForm input[name="__RequestVerificationToken"]');
        if (!btn || !input || !results) {
            return;
        }

        const renderResults = (items) => {
            results.innerHTML = "";
            if (!items || items.length === 0) {
                results.innerHTML = '<div class="alert alert-warning">Brak wyników w TMDB.</div>';
                return;
            }
            const row = document.createElement("div");
            row.className = "row row-cols-1 row-cols-md-2 row-cols-lg-3 g-3";
            items.forEach((m) => {
                const col = document.createElement("div");
                col.className = "col";
                const poster = m.posterUrl
                    ? `<img src="${escapeHtml(m.posterUrl)}" class="card-img-top" alt="Plakat ${escapeHtml(m.title)}" style="height:260px;object-fit:cover;">`
                    : `<div class="poster-placeholder" style="height:260px;"><i class="fa-solid fa-film"></i></div>`;
                col.innerHTML = `
                    <article class="card h-100 shadow-sm">
                        ${poster}
                        <div class="card-body d-flex flex-column">
                            <h3 class="h6">${escapeHtml(m.title)} <span class="text-muted">(${m.year || "—"})</span></h3>
                            <p class="small text-muted flex-grow-1">${escapeHtml((m.overview || "").substring(0, 120))}${(m.overview || "").length > 120 ? "…" : ""}</p>
                            <button type="button" class="btn btn-sm btn-success js-tmdb-import" data-tmdb-id="${m.id}">
                                <i class="fa-solid fa-download me-1"></i>Importuj
                            </button>
                        </div>
                    </article>`;
                row.appendChild(col);
            });
            results.appendChild(row);

            results.querySelectorAll(".js-tmdb-import").forEach((el) => {
                el.addEventListener("click", function () {
                    importMovie(this.dataset.tmdbId, this);
                });
            });
        };

        const doSearch = function () {
            const q = input.value.trim();
            if (q.length < 2) {
                results.innerHTML = '<div class="alert alert-info">Wpisz co najmniej 2 znaki.</div>';
                return;
            }
            results.innerHTML = '<div class="text-center py-3"><span class="ajax-spinner text-warning"></span> Szukam w TMDB...</div>';
            fetch("/Admin/Movies/TmdbSearch?q=" + encodeURIComponent(q))
                .then((r) => r.json())
                .then(renderResults)
                .catch(() => { results.innerHTML = '<div class="alert alert-danger">Błąd połączenia z TMDB.</div>'; });
        };

        const importMovie = function (tmdbId, button) {
            button.disabled = true;
            button.innerHTML = '<span class="ajax-spinner"></span> Importuję...';
            const fd = new FormData();
            fd.append("tmdbId", tmdbId);
            if (token) {
                fd.append("__RequestVerificationToken", token.value);
            }
            fetch("/Admin/Movies/ImportTmdb", { method: "POST", body: fd })
                .then((r) => r.json())
                .then((res) => {
                    if (res.success) {
                        button.className = "btn btn-sm btn-secondary";
                        button.innerHTML = '<i class="fa-solid fa-check me-1"></i>Zaimportowano';
                    } else {
                        button.disabled = false;
                        button.className = "btn btn-sm btn-danger";
                        button.innerHTML = res.message || "Błąd";
                    }
                })
                .catch(() => {
                    button.disabled = false;
                    button.innerHTML = "Błąd";
                });
        };

        btn.addEventListener("click", doSearch);
        input.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                doSearch();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        setupLiveSearch("navSearchInput", "navSearchResults");
        setupLiveSearch("catalogSearch", "catalogSearchResults");
        setupDeleteConfirm();
        setupTmdbImport();
    });
})();
