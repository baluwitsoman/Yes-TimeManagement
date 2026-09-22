// YES TM – Time Booking entry: header auto-fill, one-sheet-per-job guard, Add-Task modal
// (date/time pickers + live server-side calculation with user adjustments), grid + totals.
(function () {
    "use strict";

    var lines = [];                       // in-memory line list
    var duty = window.TM_DUTY || { start: "08:00", end: "17:00", lunch: 0.5, weekend: [5, 6] };
    var urls = window.TM_URLS || {};
    var isEdit = !!window.TM_IS_EDIT;
    var tsId = window.TM_TS_ID || null;

    // ---- Header: auto-fill from the selected Job ----
    var jobSelect = document.getElementById("jobSelect");
    var lastJob = jobSelect ? jobSelect.value : "";
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
            checkExistingSheet(jobSelect.value);
        });
    }

    function setVal(id, v) { var el = document.getElementById(id); if (el) el.value = v || ""; }
    function getVal(id) { var el = document.getElementById(id); return el ? el.value : ""; }

    function updateSelectedJobCode() { setVal("selectedJobCode", jobSelect ? jobSelect.value : ""); }

    function clearHeaderAutofill() {
        ["customerName", "customerCode", "industryCode", "brand", "equipType",
         "serviceType", "serialNo", "openingDate"].forEach(function (id) { setVal(id, ""); });
    }

    // ---- One time sheet per job: offer to open the existing sheet as soon as the job is picked ----
    var esModalEl = document.getElementById("existingSheetModal");
    var esReverted = false;
    function checkExistingSheet(jobCode) {
        if (!jobCode || !urls.sheetByJob) { lastJob = jobCode; return; }
        var url = urls.sheetByJob + (urls.sheetByJob.indexOf("?") === -1 ? "?" : "&")
                + "jobCode=" + encodeURIComponent(jobCode)
                + (isEdit && tsId ? "&excludeTsId=" + encodeURIComponent(tsId) : "");
        fetch(url, { credentials: "same-origin", headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(function (r) { if (!r.ok) throw new Error("HTTP " + r.status); return r.json(); })
            .then(function (res) {
                if (res && res.exists) showExistingSheet(jobCode, res);
                else lastJob = jobCode;
            })
            .catch(function () { lastJob = jobCode; /* server-side check still applies on post */ });
    }

    function showExistingSheet(jobCode, res) {
        if (!esModalEl) return;
        document.getElementById("es_job").textContent = jobCode;
        document.getElementById("es_sheet").textContent = res.sheetNo || "";
        var open = document.getElementById("es_open");
        open.href = urls.entry + (urls.entry.indexOf("?") === -1 ? "?" : "&") + "tsId=" + encodeURIComponent(res.tsId);
        esReverted = false;
        bootstrap.Modal.getOrCreateInstance(esModalEl).show();
    }
    if (esModalEl) {
        // Any way of closing the prompt (Cancel, X, backdrop) puts the previous job back.
        esModalEl.addEventListener("hidden.bs.modal", function () {
            if (esReverted) return;
            esReverted = true;
            jobSelect.value = lastJob || "";
            if (jobSelect.value !== (lastJob || "")) jobSelect.value = "";
            if (!jobSelect.value) clearHeaderAutofill();
            else jobSelect.dispatchEvent(new Event("change"));
            updateSelectedJobCode();
        });
        document.getElementById("es_open").addEventListener("click", function () { esReverted = true; });
    }

    // ---- Filter the Job dropdown by Job Location (server-side via AJAX; "All" = no filter) ----
    // Disabled in edit mode (the select carries the `disabled` attribute), so no listener is wired then.
    var jobLocFilter = document.getElementById("jobLocFilter");
    if (jobLocFilter && jobSelect && !jobLocFilter.disabled) {
        jobLocFilter.addEventListener("change", function () {
            var loc = jobLocFilter.value;
            var base = urls.jobsByLocation || (window.location.pathname + "?handler=JobsByLocation");
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
            o.textContent = j.code + " — " + (j.desc || "") + (j.sheetNo ? "  ✓ " + j.sheetNo : "");
            o.setAttribute("data-customer-code", j.customerCode || "");
            o.setAttribute("data-customer-name", j.customerName || "");
            o.setAttribute("data-industry", j.industry || "");
            o.setAttribute("data-location", j.location || "");
            o.setAttribute("data-brand", j.brand || "");
            o.setAttribute("data-equip-type", j.equipType || "");
            o.setAttribute("data-service-type", j.serviceType || "");
            o.setAttribute("data-serial", j.serial || "");
            o.setAttribute("data-opening", j.opening || "");
            o.setAttribute("data-sheet-no", j.sheetNo || "");
            jobSelect.appendChild(o);
        });
        jobSelect.value = current;
        if (jobSelect.value !== current) { jobSelect.value = ""; clearHeaderAutofill(); lastJob = ""; }
        updateSelectedJobCode();
    }

    // ---- Number / time helpers ----
    function num(v) { var n = parseFloat(v); return isNaN(n) ? 0 : n; }
    function r2(n) { return Math.round((num(n) + Number.EPSILON) * 100) / 100; }
    function n2(n) { return r2(n).toFixed(2); }
    function parseHM(s) {
        var m = /^(\d{1,2}):(\d{2})/.exec(String(s || "").trim());
        return m ? { h: +m[1], m: +m[2] } : null;
    }
    function hmToHours(s) { var t = parseHM(s); return t ? r2(t.h + t.m / 60) : 0; }
    function hoursToHM(h) {
        var total = Math.round(num(h) * 60), hh = Math.floor(total / 60), mm = total % 60;
        return (hh < 10 ? "0" : "") + hh + ":" + (mm < 10 ? "0" : "") + mm;
    }
    function combine(dateStr, hm) {
        var t = parseHM(hm), p = String(dateStr || "").split("-");
        if (!t || p.length !== 3) return null;
        return new Date(+p[0], +p[1] - 1, +p[2], t.h, t.m, 0, 0);
    }
    function fmtDate(dateStr) {
        var p = String(dateStr || "").split("-");
        if (p.length !== 3) return dateStr || "";
        var mon = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        return p[2] + "-" + mon[+p[1] - 1] + "-" + p[0];
    }

    // Local fallback used only when the calc endpoint is unreachable: whole line Normal or OT.
    function localCalc(dateStr, startHm, endHm, lunchHours) {
        var start = combine(dateStr, startHm), end = combine(dateStr, endHm);
        if (!start || !end) return null;
        if (end <= start) end = new Date(end.getTime() + 86400000);
        var elapse = (end - start) / 3600000;
        var net = Math.max(0, r2(elapse - lunchHours));
        var ds = parseHM(duty.start), de = parseHM(duty.end);
        var ot = duty.weekend.indexOf(start.getDay()) !== -1
              || (start.getHours() * 60 + start.getMinutes()) < (ds.h * 60 + ds.m)
              || (end.getHours() * 60 + end.getMinutes()) > (de.h * 60 + de.m) || end.getDate() !== start.getDate();
        return { netHours: net, normalHours: ot ? 0 : net, otHours: ot ? net : 0, timeType: ot ? "OVERTIME" : "NORMAL",
                 rate: 0, otRate: 0, labourCost: 0, foodAllowance: 0, totalCost: 0, travelHours: 0, rateFound: false };
    }

    // ---- Pickers (flatpickr) ----
    var pickers = {};
    function timePicker(id, opts) {
        var el = document.getElementById(id);
        if (!el) return null;
        if (typeof flatpickr !== "function") { el.placeholder = "hh:mm"; el.addEventListener("change", onTimingChanged); return null; }
        var p = flatpickr(el, Object.assign({
            enableTime: true, noCalendar: true, dateFormat: "H:i", time_24hr: true,
            minuteIncrement: 5, allowInput: true,
            onClose: onTimingChanged, onChange: onTimingChanged
        }, opts || {}));
        pickers[id] = p;
        return p;
    }
    function setPicker(id, value) {
        var p = pickers[id], el = document.getElementById(id);
        if (p) p.setDate(value || null, false, id === "m_date" ? "Y-m-d" : "H:i");
        else if (el) el.value = value || "";
    }

    var dateEl = document.getElementById("m_date");
    if (dateEl && typeof flatpickr === "function") {
        pickers.m_date = flatpickr(dateEl, {
            dateFormat: "Y-m-d", altInput: true, altFormat: "d-M-Y", allowInput: true,
            onClose: onTimingChanged, onChange: onTimingChanged
        });
    } else if (dateEl) { dateEl.type = "date"; dateEl.addEventListener("change", onTimingChanged); }
    timePicker("m_start");
    timePicker("m_end");
    timePicker("m_lunch", { minuteIncrement: 15, defaultHour: 0, defaultMinute: 30 });
    timePicker("m_travel_start");
    timePicker("m_travel_end");
    var locSel = document.getElementById("m_location");
    if (locSel) locSel.addEventListener("change", onTimingChanged);

    // ---- Live calculation (server-side engine via AJAX) ----
    var auto = null;                       // last values returned by the calc endpoint
    var manual = {};                       // key -> true when the user typed over the auto value
    var calcTimer = null, calcSeq = 0;
    var KEYS = { normal: "m_normal", ot: "m_ot", rate: "m_rate", otRate: "m_otrate", cost: "m_cost", food: "m_food", travel: "m_travel_hours" };
    var AUTO_KEY = { normal: "normalHours", ot: "otHours", rate: "rate", otRate: "otRate", cost: "labourCost", food: "foodAllowance", travel: "travelHours" };

    function resetManual() { manual = {}; }

    function onTimingChanged() { scheduleCalc(false); }

    function scheduleCalc(preserveValues) {
        clearTimeout(calcTimer);
        calcTimer = setTimeout(function () { runCalc(preserveValues); }, 250);
    }

    function calcParams() {
        return {
            workDate: getVal("m_date"), start: getVal("m_start"), end: getVal("m_end"),
            lunch: hmToHours(getVal("m_lunch")), location: getVal("m_location"),
            industry: getVal("industryCode"), travelStart: getVal("m_travel_start"), travelEnd: getVal("m_travel_end")
        };
    }

    // preserveValues: keep whatever is in the editable fields and only work out which of them
    // differ from the engine (used when re-opening a line for edit).
    function runCalc(preserveValues) {
        var p = calcParams();
        var status = document.getElementById("m_calcStatus");
        showOvernight(p.workDate, p.start, p.end);
        if (!p.start || !p.end) { return; }

        var seq = ++calcSeq;
        if (status) status.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>calculating…';
        var q = Object.keys(p).map(function (k) { return k + "=" + encodeURIComponent(p[k] == null ? "" : p[k]); }).join("&");
        var url = urls.calcLine + (urls.calcLine.indexOf("?") === -1 ? "?" : "&") + q;

        fetch(url, { credentials: "same-origin", headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(function (r) { return r.json().then(function (j) { return { ok: r.ok, body: j }; }); })
            .then(function (res) {
                if (seq !== calcSeq) return;
                if (!res.ok) { if (status) status.innerHTML = '<span class="text-danger">' + esc(res.body && res.body.error || "cannot calculate") + "</span>"; return; }
                applyCalc(res.body, preserveValues);
                if (status) status.innerHTML = res.body.rateFound
                    ? '<i class="fa-solid fa-circle-check text-success me-1"></i>calculated'
                    : '<i class="fa-solid fa-triangle-exclamation text-warning me-1"></i>no labour rate for this location — enter rate';
            })
            .catch(function () {
                if (seq !== calcSeq) return;
                var fb = localCalc(p.workDate, p.start, p.end, p.lunch);
                if (fb) applyCalc(fb, preserveValues);
                if (status) status.innerHTML = '<i class="fa-solid fa-plug-circle-xmark text-warning me-1"></i>offline — hours only, enter rates';
            });
    }

    function applyCalc(r, preserveValues) {
        var prevNet = auto ? auto.netHours : null;
        auto = r;
        setVal("m_net", n2(r.netHours));
        setTimeType(r.timeType);

        if (preserveValues) {
            // Re-opened line: keep its figures, flag the ones that are not what the engine gives.
            Object.keys(KEYS).forEach(function (k) { manual[k] = isAdjusted(k); });
        } else {
            // Times changed → the hour split (and a cost typed against the old split) no longer applies.
            if (prevNet !== null && r2(prevNet) !== r2(r.netHours)) { manual.normal = manual.ot = manual.cost = false; }
            Object.keys(KEYS).forEach(function (k) { if (!manual[k]) setVal(KEYS[k], n2(r[AUTO_KEY[k]])); });
            if (!manual.cost) setVal("m_cost", n2(costFromFields()));
        }
        updateTotal();
        updateAdjBadges();
    }

    function costFromFields() {
        return r2(num(getVal("m_normal")) * num(getVal("m_rate")) + num(getVal("m_ot")) * num(getVal("m_otrate")));
    }
    function updateTotal() { setVal("m_total", n2(num(getVal("m_cost")) + num(getVal("m_food")))); }

    function setTimeType(t) {
        var el = document.getElementById("m_timetype");
        if (!el) return;
        el.innerHTML = badgeFor(t);
    }
    function badgeFor(t) {
        if (t === "OVERTIME") return '<span class="badge-soft badge-ot">Overtime</span>';
        if (t === "MIXED") return '<span class="badge-soft badge-mixed">Normal + OT</span>';
        return '<span class="badge-soft badge-normal">Normal</span>';
    }
    function timeTypeFrom(normal, ot) { return ot <= 0 ? "NORMAL" : normal <= 0 ? "OVERTIME" : "MIXED"; }

    function showOvernight(dateStr, s, e) {
        var el = document.getElementById("m_overnight");
        if (!el) return;
        var a = combine(dateStr, s), b = combine(dateStr, e);
        el.style.display = (a && b && b <= a) ? "inline" : "none";
    }

    // Editable auto-fields: typing marks the field as adjusted; Normal/OT move hours between each other.
    document.querySelectorAll(".tm-editable").forEach(function (inp) {
        inp.addEventListener("input", function () {
            var key = inp.getAttribute("data-key");
            manual[key] = true;
            var net = num(getVal("m_net"));
            if (key === "normal") { setVal("m_ot", n2(Math.max(0, net - num(inp.value)))); manual.ot = true; }
            if (key === "ot") { setVal("m_normal", n2(Math.max(0, net - num(inp.value)))); manual.normal = true; }
            if (key !== "cost" && key !== "food" && key !== "travel" && !manual.cost) setVal("m_cost", n2(costFromFields()));
            setTimeType(timeTypeFrom(num(getVal("m_normal")), num(getVal("m_ot"))));
            updateTotal();
            updateAdjBadges();
        });
    });

    // A field is "adjusted" when what is on screen is not what the engine would put there
    // (cost is judged against the formula so a changed hour split alone does not flag it;
    // travel hours only when the engine could work them out from travel start/end).
    function isAdjusted(k) {
        if (!auto) return !!manual[k];
        var v = r2(getVal(KEYS[k]));
        if (k === "cost") return v !== r2(costFromFields());
        if (k === "travel") return r2(auto.travelHours) > 0 && v !== r2(auto.travelHours);
        return v !== r2(auto[AUTO_KEY[k]]);
    }
    function updateAdjBadges() {
        document.querySelectorAll(".tm-adj").forEach(function (b) {
            b.innerHTML = isAdjusted(b.getAttribute("data-for"))
                ? '<span class="badge-soft badge-adj" title="Adjusted from the calculated value">adjusted</span>' : "";
        });
    }
    function isOverridden() { return Object.keys(KEYS).some(isAdjusted); }

    var recalcBtn = document.getElementById("m_recalc");
    if (recalcBtn) recalcBtn.addEventListener("click", function () { resetManual(); runCalc(false); });

    // ---- Add / edit a line via the modal ----
    var editIndex = null;                                   // null = add new; number = edit that row
    var headerLoc = document.getElementById("location");    // header Job Location (default for new lines)

    var addBtn = document.getElementById("addLineBtn");
    if (addBtn) addBtn.addEventListener("click", onSaveLine);

    // The "Add Task" toolbar button opens the modal in add mode.
    var addTaskBtn = document.getElementById("addTaskBtn");
    if (addTaskBtn) addTaskBtn.addEventListener("click", prepareModalForAdd);

    function onSaveLine() {
        var err = document.getElementById("modalError");
        var taskSel = document.getElementById("m_task");
        var techSel = document.getElementById("m_tech");
        var p = calcParams();

        var problems = [];
        if (!taskSel.value) problems.push("Select a task.");
        if (!techSel.value) problems.push("Select a technician.");
        if (locSel && !locSel.value) problems.push("Select a job location.");
        if (!p.workDate) problems.push("Enter the task date.");
        if (!parseHM(p.start)) problems.push("Enter a start time (hh:mm).");
        if (!parseHM(p.end)) problems.push("Enter an end time (hh:mm).");
        var start = combine(p.workDate, p.start), end = combine(p.workDate, p.end);
        if (start && end && start.getTime() === end.getTime()) problems.push("End time must differ from start time.");

        var normal = r2(getVal("m_normal")), ot = r2(getVal("m_ot"));
        var net = r2(getVal("m_net"));
        if (!net && start && end) { var fb = localCalc(p.workDate, p.start, p.end, p.lunch); if (fb) { net = fb.netHours; if (!normal && !ot) { normal = fb.normalHours; ot = fb.otHours; } } }
        if (net <= 0) problems.push("Net hours must be greater than zero (check times and lunch break).");
        if (r2(normal + ot) !== r2(net)) problems.push("Normal + OT hours (" + n2(normal + ot) + ") must equal Net hours (" + n2(net) + ").");

        if (problems.length) {
            err.innerHTML = problems.join("<br>");
            err.classList.remove("d-none");
            return;
        }
        err.classList.add("d-none");

        var taskOpt = taskSel.options[taskSel.selectedIndex];
        var techOpt = techSel.options[techSel.selectedIndex];
        var cost = r2(getVal("m_cost")), food = r2(getVal("m_food"));

        var line = {
            taskCode: taskSel.value,
            taskName: taskOpt.getAttribute("data-name") || taskOpt.text,
            stdHours: num(taskOpt.getAttribute("data-std")),
            techCode: techSel.value,
            techName: techOpt.getAttribute("data-name") || "",
            skill: getVal("m_skill"),
            location: locSel ? locSel.value : "",
            workDate: p.workDate,
            startTime: p.start, endTime: p.end,
            overnight: !!(start && end && end <= start),
            lunch: p.lunch,
            net: net, normal: normal, ot: ot,
            rate: r2(getVal("m_rate")), otRate: r2(getVal("m_otrate")),
            cost: cost, food: food, total: r2(cost + food),
            travelSite: getVal("m_travel_site").trim(),
            travelStart: parseHM(p.travelStart) ? p.travelStart : "",
            travelEnd: parseHM(p.travelEnd) ? p.travelEnd : "",
            travelHours: r2(getVal("m_travel_hours")),
            timeType: timeTypeFrom(normal, ot),
            overridden: isOverridden()
        };

        if (editIndex === null) lines.push(line); else lines[editIndex] = line;

        render();
        modal().hide();
    }

    function modal() { return bootstrap.Modal.getOrCreateInstance(document.getElementById("addTaskModal")); }

    // Reset the modal for a new line; default its location to the header Job Location.
    function prepareModalForAdd() {
        editIndex = null;
        auto = null; resetManual(); calcSeq++;
        document.getElementById("modalError").classList.add("d-none");
        ["m_task", "m_tech", "m_travel_site"].forEach(function (id) { setVal(id, ""); });
        document.getElementById("m_skill").selectedIndex = 0;
        setSelect("m_location", headerLoc ? headerLoc.value : "");
        var lastLine = lines.length ? lines[lines.length - 1] : null;
        setPicker("m_date", lastLine ? lastLine.workDate : (window.TM_POSTING_DATE || ""));
        setPicker("m_start", duty.start || "08:00");
        setPicker("m_end", duty.end || "17:00");
        setPicker("m_lunch", hoursToHM(duty.lunch != null ? duty.lunch : 0.5));
        setPicker("m_travel_start", ""); setPicker("m_travel_end", "");
        Object.keys(KEYS).forEach(function (k) { setVal(KEYS[k], ""); });
        setVal("m_net", ""); setVal("m_total", ""); setTimeType("");
        var status = document.getElementById("m_calcStatus"); if (status) status.textContent = "";
        updateAdjBadges();
        setModalMode(false);
        scheduleCalc(false);
    }

    // Pre-fill the modal to edit an existing grid line, then show it.
    function prepareModalForEdit(i) {
        var l = lines[i];
        if (!l) return;
        editIndex = i;
        auto = null; resetManual(); calcSeq++;
        document.getElementById("modalError").classList.add("d-none");
        setSelect("m_task", l.taskCode);
        setSelect("m_tech", l.techCode);
        setSelect("m_skill", l.skill);
        setSelect("m_location", l.location || (headerLoc ? headerLoc.value : ""));
        setPicker("m_date", l.workDate);
        setPicker("m_start", l.startTime);
        setPicker("m_end", l.endTime);
        setPicker("m_lunch", hoursToHM(l.lunch));
        setVal("m_net", n2(l.net)); setTimeType(l.timeType);
        setVal("m_normal", n2(l.normal)); setVal("m_ot", n2(l.ot));
        setVal("m_rate", n2(l.rate)); setVal("m_otrate", n2(l.otRate));
        setVal("m_cost", n2(l.cost)); setVal("m_food", n2(l.food)); setVal("m_total", n2(l.total));
        setVal("m_travel_site", l.travelSite || "");
        setPicker("m_travel_start", l.travelStart || ""); setPicker("m_travel_end", l.travelEnd || "");
        setVal("m_travel_hours", n2(l.travelHours));
        setModalMode(true);
        modal().show();
        scheduleCalc(true);      // fetch the engine values to show which figures were adjusted
    }

    function setModalMode(isEdit) {
        var title = document.getElementById("taskModalTitle");
        var btnText = document.getElementById("addLineBtnText");
        if (title) title.innerHTML = isEdit
            ? '<i class="fa-solid fa-pen me-2 text-primary"></i>Edit Task Line'
            : '<i class="fa-solid fa-plus me-2 text-primary"></i>Add Task Line';
        if (btnText) btnText.textContent = isEdit ? "Save Changes" : "Add to Grid";
    }

    function setSelect(id, val) { var el = document.getElementById(id); if (el) el.value = (val == null ? "" : val); }

    // ---- Render grid + hidden inputs + totals ----
    function render() {
        var body = document.getElementById("linesBody");
        body.innerHTML = "";
        var t = { net: 0, std: 0, normal: 0, ot: 0, cost: 0, food: 0, total: 0, travel: 0 };

        lines.forEach(function (l, i) {
            t.net += l.net; t.std += l.stdHours; t.normal += l.normal; t.ot += l.ot;
            t.cost += l.cost; t.food += l.food; t.total += l.total; t.travel += l.travelHours;

            var travel = l.travelHours > 0 || l.travelSite
                ? esc(l.travelSite || "") + (l.travelHours > 0 ? (l.travelSite ? " · " : "") + n2(l.travelHours) + " h" : "")
                : "<span class='text-muted'>—</span>";
            var adj = l.overridden ? " <span class='badge-soft badge-adj' title='Contains user-adjusted values'>adj</span>" : "";

            var tr = document.createElement("tr");
            tr.innerHTML =
                "<td>" + (i + 1) + "</td>" +
                "<td class='text-nowrap'>" + fmtDate(l.workDate) + "</td>" +
                "<td>" + esc(l.taskName) + "<div class='small text-muted'>Std " + n2(l.stdHours) + " h · " + esc(l.skill) + "</div></td>" +
                "<td>" + esc(l.techCode) + "<div class='small text-muted'>" + esc(l.techName) + "</div></td>" +
                "<td>" + esc(l.location || "") + "</td>" +
                "<td class='text-nowrap'>" + esc(l.startTime) + " – " + esc(l.endTime) + (l.overnight ? " <i class='fa-solid fa-moon text-muted' title='Ends next day'></i>" : "") +
                    "<div class='small'>" + badgeFor(l.timeType) + adj + "</div></td>" +
                "<td class='text-num'>" + hoursToHM(l.lunch) + "</td>" +
                "<td class='text-num fw-semibold'>" + n2(l.net) + "</td>" +
                "<td class='text-num'>" + n2(l.normal) + "</td>" +
                "<td class='text-num'>" + n2(l.ot) + "</td>" +
                "<td class='text-num text-nowrap'>" + n2(l.rate) + " / " + n2(l.otRate) + "</td>" +
                "<td class='text-num'>" + n2(l.cost) + "</td>" +
                "<td class='text-num'>" + n2(l.food) + "</td>" +
                "<td class='text-num fw-semibold'>" + n2(l.total) + "</td>" +
                "<td class='small'>" + travel + "</td>" +
                "<td class='text-nowrap'>" +
                    "<button type='button' class='btn btn-sm btn-outline-primary me-1' data-edit='" + i + "' title='Edit line'><i class='fa-solid fa-pen'></i></button>" +
                    "<button type='button' class='btn btn-sm btn-outline-danger' data-i='" + i + "' title='Remove line'><i class='fa-solid fa-trash'></i></button>" +
                "</td>" +
                hiddenInputs(l, i);
            body.appendChild(tr);
        });

        body.querySelectorAll("button[data-i]").forEach(function (b) {
            b.addEventListener("click", function () { lines.splice(+b.getAttribute("data-i"), 1); render(); });
        });
        body.querySelectorAll("button[data-edit]").forEach(function (b) {
            b.addEventListener("click", function () { prepareModalForEdit(+b.getAttribute("data-edit")); });
        });

        document.getElementById("emptyGrid").style.display = lines.length ? "none" : "block";
        setText("footNet", n2(t.net)); setText("footNormal", n2(t.normal)); setText("footOt", n2(t.ot));
        setText("footCost", n2(t.cost)); setText("footFood", n2(t.food)); setText("footTotal", n2(t.total));
        setText("footTravel", n2(t.travel) + " h");
        setText("sumNet", n2(t.net)); setText("sumStd", n2(t.std));
        setText("sumSplit", t.normal.toFixed(1) + " / " + t.ot.toFixed(1));
        setText("sumCost", n2(t.cost)); setText("sumFood", n2(t.food)); setText("sumTotal", n2(t.total));
        setText("sumTravel", n2(t.travel)); setText("sumLines", lines.length);
    }

    function setText(id, v) { var el = document.getElementById(id); if (el) el.textContent = v; }

    function hiddenInputs(l, i) {
        function h(name, val) { return "<input type='hidden' name='Lines[" + i + "]." + name + "' value=\"" + esc(val) + "\">"; }
        return "<td class='d-none'>" +
            h("TaskCode", l.taskCode) + h("TaskName", l.taskName) +
            h("TechCode", l.techCode) + h("TechName", l.techName) + h("Skill", l.skill) +
            h("Location", l.location) +
            h("WorkDate", l.workDate) + h("StartTime", l.startTime) + h("EndTime", l.endTime) + h("LunchHours", l.lunch) +
            h("NetHours", l.net) + h("NormalHours", l.normal) + h("OtHours", l.ot) +
            h("Rate", l.rate) + h("OtRate", l.otRate) + h("LabourCost", l.cost) +
            h("FoodAllowance", l.food) + h("TotalCost", l.total) +
            h("TravelSite", l.travelSite) + h("TravelStart", l.travelStart) + h("TravelEnd", l.travelEnd) + h("TravelHours", l.travelHours) +
            "</td>";
    }

    function esc(s) {
        return String(s == null ? "" : s)
            .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
    }

    // ---- Seed the grid from an existing sheet's lines (edit mode) ----
    if (Array.isArray(window.TM_EXISTING_LINES) && window.TM_EXISTING_LINES.length) {
        window.TM_EXISTING_LINES.forEach(function (x) {
            var s = combine(x.workDate, x.startTime), e = combine(x.workDate, x.endTime);
            lines.push({
                taskCode: x.taskCode, taskName: x.taskName, stdHours: num(x.stdHours),
                techCode: x.techCode, techName: x.techName, skill: x.skill, location: x.location || "",
                workDate: x.workDate, startTime: x.startTime, endTime: x.endTime,
                overnight: !!(s && e && e <= s),
                lunch: num(x.lunch), net: num(x.net), normal: num(x.normal), ot: num(x.ot),
                rate: num(x.rate), otRate: num(x.otRate), cost: num(x.cost), food: num(x.food), total: num(x.total),
                travelSite: x.travelSite || "", travelStart: x.travelStart || "", travelEnd: x.travelEnd || "",
                travelHours: num(x.travelHours),
                timeType: x.timeType || timeTypeFrom(num(x.normal), num(x.ot)),
                overridden: !!x.overridden
            });
        });
        render();
    }

    // Initialise the "Selected Job Code" display for the pre-selected job (edit mode / postback).
    updateSelectedJobCode();
})();
