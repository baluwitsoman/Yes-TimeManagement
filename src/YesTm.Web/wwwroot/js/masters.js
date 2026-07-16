// YES TM – generic Add/Edit modal driver for master screens.
// Conventions: #masterModal, #masterModalTitle, #masterForm,
//   inputs named "Input.<COLUMN>", a hidden [name="IsEdit"], the code/key
//   input marked [data-key-field], add buttons [data-master-add],
//   edit buttons carrying data-row='<json of the row>'.
(function () {
    "use strict";
    var modalEl = document.getElementById("masterModal");
    if (!modalEl) return;
    var modal = new bootstrap.Modal(modalEl);
    var form = document.getElementById("masterForm");
    var title = document.getElementById("masterModalTitle");

    function field(name) { return form.querySelector('[name="Input.' + name + '"]'); }
    function setField(name, val) {
        var el = field(name);
        if (!el) return;
        var v = (val === null || val === undefined) ? "" : String(val);
        // date inputs need yyyy-MM-dd; JSON dates arrive as ISO (yyyy-MM-ddThh:mm:ss)
        if (el.type === "date" && v.length > 10) v = v.substring(0, 10);
        el.value = v;
    }
    function setEdit(isEdit) {
        var ie = form.querySelector('[name="IsEdit"]');
        if (ie) ie.value = isEdit ? "true" : "false";
        var kf = form.querySelector("[data-key-field]");
        if (kf) { kf.readOnly = isEdit; kf.classList.toggle("field-auto", isEdit); }
    }

    document.querySelectorAll("[data-master-add]").forEach(function (b) {
        b.addEventListener("click", function () {
            form.querySelectorAll('[name^="Input."]').forEach(function (i) {
                i.value = i.getAttribute("data-default") || "";
            });
            setEdit(false);
            title.textContent = b.getAttribute("data-title") || "Add";
            modal.show();
        });
    });

    document.querySelectorAll("[data-row]").forEach(function (b) {
        b.addEventListener("click", function () {
            var row = JSON.parse(b.getAttribute("data-row"));
            Object.keys(row).forEach(function (k) { setField(k, row[k]); });
            setEdit(true);
            title.textContent = "Edit";
            modal.show();
        });
    });
})();
