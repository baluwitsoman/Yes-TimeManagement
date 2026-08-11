// YES TM – Time Booking entry: header auto-fill, Add-Task modal, grid + totals.
(function () {
    "use strict";

    var lines = [];                       // in-memory line list
    var duty = window.TM_DUTY || { start: "08:00", end: "17:00", weekend: [5, 6] };

    // ---- Header: auto-fill from the selected Job ----
    var jobSelect = document.getElementById("jobSelect");
    if (jobSelect) {
        jobSelect.addEventListener("change", function () {
            var o = jobSelect.options[jobSelect.selectedIndex];
            setVal("customerName", o.getAttribute("data-customer-name"));
            setVal("customerCode", o.getAttribute("data-customer-code"));
            setVal("industryCode", o.getAttribute("data-industry"));
            setVal("brand", o.getAttribute("data-brand"));
            setVal("equipType", o.getAttribute("data-equip-type"));
            setVal("serviceType", o.getAttribute("data-service-type"));
            setVal("serialNo", o.getAttribute("data-serial"));
            setVal("openingDate", o.getAttribute("data-opening"));
            var loc = o.getAttribute("data-location");
            if (loc) setVal("location", loc);
            updateSelectedJobCode();
        });
    }

    function setVal(id, v) { var el = document.getElementById(id); if (el) el.value = v || ""; }

    function updateSelectedJobCode() { setVal("selectedJobCode", jobSelect ? jobSelect.value : ""); }

    function clearHeaderAutofill() {
        ["customerName", "customerCode", "industryCode", "brand", "equipType",
         "serviceType", "serialNo", "openingDate"].forEach(function (id) { setVal(id, ""); });
    }

    // ---- Filter the Job dropdown by Job Location (server-side via AJAX; "All" = no filter) ----
    // Disabled in edit mode (the select carries the `disabled` attribute), so no listener is wired then.
    var jobLocFilter = document.getElementById("jobLocFilter");
    if (jobLocFilter && jobSelect && !jobLocFilter.disabled) {
        jobLocFilter.addEventListener("change", function () {
            var loc = jobLocFilter.value;
            var base = window.TM_JOBS_BY_LOCATION_URL || (window.location.pathname + "?handler=JobsByLocation");
            var url = base + (base.indexOf("?") === -1 ? "?" : "&") + "location=" + encodeURIComponent(loc);
            jobLocFilter.disabled = true;
            fetch(url, { credentials: "same-origin", headers: { "X-Requested-With": "XMLHttpRequest" } })
                .then(function (r) { if (!r.ok) throw new Error("HTTP " + r.status); return r.json(); })
                .then(function (jobs) { rebuildJobs(jobs); })
                .catch(function () { /* leave the current list in place on failure */ })
                .finally(function () { jobLocFilter.disabled = false; });
        });
    }

    // Rebuild the Job / Work Order options from the AJAX payload, keeping the current
    // selection when the job survives the filter (else reset it and clear the auto-fill).
    function rebuildJobs(jobs) {
        var current = jobSelect.value;
        jobSelect.innerHTML = "";
        var ph = document.createElement("option");
        ph.value = ""; ph.textContent = "— Select Job —";
        jobSelect.appendChild(ph);
        (jobs || []).forEach(function (j) {
            var o = document.createElement("option");
            o.value = j.code;
            o.textContent = j.code + " — " + (j.desc || "");
            o.setAttribute("data-customer-code", j.customerCode || "");
            o.setAttribute("data-customer-name", j.customerName || "");
            o.setAttribute("data-industry", j.industry || "");
            o.setAttribute("data-location", j.location || "");
            o.setAttribute("data-brand", j.brand || "");
            o.setAttribute("data-equip-type", j.equipType || "");
            o.setAttribute("data-service-type", j.serviceType || "");
            o.setAttribute("data-serial", j.serial || "");
            o.setAttribute("data-opening", j.opening || "");
            jobSelect.appendChild(o);
        });
        jobSelect.value = current;
        if (jobSelect.value !== current) { jobSelect.value = ""; clearHeaderAutofill(); }
        updateSelectedJobCode();
    }

    // ---- Time helpers ----
    function parseHM(s) { var p = (s || "0:0").split(":"); return { h: +p[0] || 0, m: +p[1] || 0 }; }
    function toMinutes(d) { return d.getHours() * 60 + d.getMinutes(); }

    function computeNet(start, end, lunch) {
        var ms = end - start;
        if (isNaN(ms) || ms <= 0) return null;
        var hrs = ms / 3600000 - (parseFloat(lunch) || 0);
        return Math.max(0, Math.round(hrs * 100) / 100);
    }

    function classify(start, end) {
        if (duty.weekend.indexOf(start.getDay()) !== -1) return "Overtime";
        var ds = parseHM(duty.start), de = parseHM(duty.end);
        var dutyStart = ds.h * 60 + ds.m, dutyEnd = de.h * 60 + de.m;
        if (toMinutes(start) < dutyStart || toMinutes(end) > dutyEnd) return "Overtime";
        return "Normal";
    }

    function fmtDt(d) {
        var mon = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        function pad(n) { return (n < 10 ? "0" : "") + n; }
        return pad(d.getDate()) + "-" + mon[d.getMonth()] + " " + pad(d.getHours()) + ":" + pad(d.getMinutes());
    }

    // ---- Add line from modal ----
    var addBtn = document.getElementById("addLineBtn");
    if (addBtn) addBtn.addEventListener("click", onAddLine);

    function onAddLine() {
        var err = document.getElementById("modalError");
        var taskSel = document.getElementById("m_task");
        var techSel = document.getElementById("m_tech");
        var startV = document.getElementById("m_start").value;
        var endV = document.getElementById("m_end").value;
        var lunch = document.getElementById("m_lunch").value;

        var problems = [];
        if (!taskSel.value) problems.push("Select a task.");
        if (!techSel.value) problems.push("Select a technician.");
        if (!startV) problems.push("Enter a start date-time.");
        if (!endV) problems.push("Enter an end date-time.");

        var start = new Date(startV), end = new Date(endV);
        var net = (startV && endV) ? computeNet(start, end, lunch) : null;
        if (startV && endV && net === null) problems.push("End must be after start.");

        if (problems.length) {
            err.innerHTML = problems.join("<br>");
            err.classList.remove("d-none");
            return;
        }
        err.classList.add("d-none");

        var taskOpt = taskSel.options[taskSel.selectedIndex];
        var techOpt = techSel.options[techSel.selectedIndex];

        lines.push({
            taskCode: taskSel.value,
            taskName: taskOpt.getAttribute("data-name") || taskOpt.text,
            stdHours: parseFloat(taskOpt.getAttribute("data-std")) || 0,
            techCode: techSel.value,
            techName: techOpt.getAttribute("data-name") || "",
            skill: document.getElementById("m_skill").value,
            start: start,
            end: end,
            startRaw: startV,
            endRaw: endV,
            lunch: parseFloat(lunch) || 0,
            net: net,
            timeType: classify(start, end)
        });

        render();
        resetModal();
        bootstrap.Modal.getInstance(document.getElementById("addTaskModal")).hide();
    }

    function resetModal() {
        ["m_task", "m_tech", "m_start", "m_end"].forEach(function (id) { document.getElementById(id).value = ""; });
        document.getElementById("m_lunch").value = "0.5";
        document.getElementById("m_skill").selectedIndex = 0;
    }

    // ---- Render grid + hidden inputs + totals ----
    function render() {
        var body = document.getElementById("linesBody");
        body.innerHTML = "";
        var totalNet = 0, totalStd = 0, normal = 0, ot = 0;

        lines.forEach(function (l, i) {
            totalNet += l.net; totalStd += l.stdHours;
            if (l.timeType === "Overtime") ot += l.net; else normal += l.net;

            var badge = l.timeType === "Overtime"
                ? '<span class="badge-soft badge-ot">Overtime</span>'
                : '<span class="badge-soft badge-normal">Normal</span>';

            var tr = document.createElement("tr");
            tr.innerHTML =
                "<td>" + (i + 1) + "</td>" +
                "<td>" + esc(l.taskName) + "</td>" +
                "<td class='text-num'>" + l.stdHours.toFixed(2) + "</td>" +
                "<td>" + esc(l.techCode + " · " + l.techName) + "</td>" +
                "<td>" + esc(l.skill) + "</td>" +
                "<td>" + fmtDt(l.start) + "</td>" +
                "<td>" + fmtDt(l.end) + "</td>" +
                "<td class='text-num'>" + l.lunch.toFixed(2) + "</td>" +
                "<td class='text-num'>" + l.net.toFixed(2) + "</td>" +
                "<td>" + badge + "</td>" +
                "<td class='text-num text-muted'>auto</td>" +
                "<td class='text-num text-muted'>auto</td>" +
                "<td><button type='button' class='btn btn-sm btn-outline-danger' data-i='" + i + "'><i class='fa-solid fa-trash'></i></button></td>" +
                hiddenInputs(l, i);
            body.appendChild(tr);
        });

        body.querySelectorAll("button[data-i]").forEach(function (b) {
            b.addEventListener("click", function () { lines.splice(+b.getAttribute("data-i"), 1); render(); });
        });

        document.getElementById("emptyGrid").style.display = lines.length ? "none" : "block";
        document.getElementById("footNet").textContent = totalNet.toFixed(2);
        document.getElementById("sumNet").textContent = totalNet.toFixed(2);
        document.getElementById("sumStd").textContent = totalStd.toFixed(2);
        document.getElementById("sumSplit").textContent = normal.toFixed(1) + " / " + ot.toFixed(1);
        document.getElementById("sumLines").textContent = lines.length;
    }

    function hiddenInputs(l, i) {
        function h(name, val) { return "<input type='hidden' name='Lines[" + i + "]." + name + "' value=\"" + esc(val) + "\">"; }
        return "<td class='d-none'>" +
            h("TaskCode", l.taskCode) + h("TaskName", l.taskName) +
            h("TechCode", l.techCode) + h("TechName", l.techName) + h("Skill", l.skill) +
            h("StartDt", l.startRaw) + h("EndDt", l.endRaw) + h("LunchHours", l.lunch) +
            "</td>";
    }

    function esc(s) {
        return String(s == null ? "" : s)
            .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
    }

    // ---- Seed the grid from an existing sheet's lines (edit mode) ----
    if (Array.isArray(window.TM_EXISTING_LINES) && window.TM_EXISTING_LINES.length) {
        window.TM_EXISTING_LINES.forEach(function (x) {
            var start = new Date(x.startRaw), end = new Date(x.endRaw);
            lines.push({
                taskCode: x.taskCode,
                taskName: x.taskName,
                stdHours: parseFloat(x.stdHours) || 0,
                techCode: x.techCode,
                techName: x.techName,
                skill: x.skill,
                start: start,
                end: end,
                startRaw: x.startRaw,
                endRaw: x.endRaw,
                lunch: parseFloat(x.lunch) || 0,
                net: computeNet(start, end, x.lunch) || 0,
                timeType: classify(start, end)
            });
        });
        render();
    }

    // Initialise the "Selected Job Code" display for the pre-selected job (edit mode / postback).
    updateSelectedJobCode();
})();
