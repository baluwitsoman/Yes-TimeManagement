// YES TM – Field Service Report entry: crew list, day-wise Travel/Work segments added through a
// modal (one entry → one line per ticked technician), server-side Normal/OT calculation, vehicle
// km cost, grid grouped by date with day subtotals, and hidden inputs for the post.
(function () {
    "use strict";

    var FS = window.FS || {};
    var duty = FS.duty || { start: "08:00", end: "17:00", lunch: 0.5 };
    var urls = FS.urls || {};
    var crew = (FS.crew || []).slice();
    var lines = [];

    // ---------- helpers ----------
    function $(id) { return document.getElementById(id); }
    function val(id) { var el = $(id); return el ? el.value : ""; }
    function setVal(id, v) { var el = $(id); if (el) el.value = v == null ? "" : v; }
    function setText(id, v) { var el = $(id); if (el) el.textContent = v; }
    function num(v) { var n = parseFloat(v); return isNaN(n) ? 0 : n; }
    function r2(n) { return Math.round((num(n) + Number.EPSILON) * 100) / 100; }
    function n2(n) { return r2(n).toFixed(2); }
    function esc(s) {
        return String(s == null ? "" : s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
    }
    function parseHM(s) { var m = /^(\d{1,2}):(\d{2})/.exec(String(s || "").trim()); return m ? { h: +m[1], m: +m[2] } : null; }
    function hmToHours(s) { var t = parseHM(s); return t ? r2(t.h + t.m / 60) : 0; }
    function hoursToHM(h) {
        var t = Math.round(num(h) * 60), hh = Math.floor(t / 60), mm = t % 60;
        return (hh < 10 ? "0" : "") + hh + ":" + (mm < 10 ? "0" : "") + mm;
    }
    function minutes(hm) { var t = parseHM(hm); return t ? t.h * 60 + t.m : -1; }
    function fmtDate(d) {
        var p = String(d || "").split("-"); if (p.length !== 3) return d || "";
        var mon = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        var dt = new Date(+p[0], +p[1] - 1, +p[2]);
        return ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"][dt.getDay()] + " " + p[2] + "-" + mon[+p[1] - 1] + "-" + p[0];
    }
    function timeTypeFrom(normal, ot) { return ot <= 0 ? "NORMAL" : normal <= 0 ? "OVERTIME" : "MIXED"; }

    // ---------- job pick (new report): reload with the job so the server fills details / opens an existing FSR ----------
    var jobSel = $("jobSelect");
    if (jobSel && !FS.isEdit) {
        jobSel.addEventListener("change", function () {
            if (!jobSel.value) return;
            window.location.href = urls.self + (urls.self.indexOf("?") === -1 ? "?" : "&") + "jobCode=" + encodeURIComponent(jobSel.value);
        });
    }

    // ---------- vehicle ----------
    function vehicle() {
        var s = val("kmStart"), e = val("kmEnd");
        var km = (s !== "" && e !== "" && num(e) > num(s)) ? num(e) - num(s) : 0;
        var cost = r2(km * num(val("kmRate")));
        setVal("kmTotal", km ? km.toFixed(1) : "");
        setVal("kmCost", n2(cost));
        return cost;
    }
    ["kmStart", "kmEnd", "kmRate"].forEach(function (id) { var el = $(id); if (el) el.addEventListener("input", render); });

    // ---------- crew ----------
    var crewAdd = $("crewAdd");
    if (crewAdd) crewAdd.addEventListener("click", function () {
        var sel = $("crewTech");
        if (!sel.value) return;
        if (crew.some(function (c) { return c.code.toLowerCase() === sel.value.toLowerCase(); })) { sel.value = ""; return; }
        var o = sel.options[sel.selectedIndex];
        crew.push({ code: sel.value, name: o.getAttribute("data-name") || "", fa: val("crewFa").trim() });
        sel.value = ""; setVal("crewFa", "");
        render();
    });

    function renderCrew(perTech) {
        var body = $("crewBody");
        body.innerHTML = "";
        crew.forEach(function (c, i) {
            var t = perTech[c.code] || { work: 0, travel: 0, idle: 0, food: 0 };
            var tr = document.createElement("tr");
            tr.innerHTML = "<td>" + (i + 1) + "</td><td><strong>" + esc(c.code) + "</strong> — " + esc(c.name) + "</td>" +
                "<td><input class='form-control form-control-sm' data-fa='" + i + "' maxlength='30' value=\"" + esc(c.fa) + "\"></td>" +
                "<td class='text-num'>" + n2(t.work) + "</td><td class='text-num'>" + n2(t.travel) + "</td>" +
                "<td class='text-num'>" + n2(t.idle) + "</td><td class='text-num'>" + n2(t.food) + "</td>" +
                "<td><button type='button' class='btn btn-sm btn-outline-danger' data-rm='" + i + "' title='Remove'><i class='fa-solid fa-trash'></i></button></td>";
            body.appendChild(tr);
        });
        body.querySelectorAll("input[data-fa]").forEach(function (inp) {
            inp.addEventListener("input", function () { crew[+inp.getAttribute("data-fa")].fa = inp.value; writeHidden(); });
        });
        body.querySelectorAll("button[data-rm]").forEach(function (b) {
            b.addEventListener("click", function () {
                var c = crew[+b.getAttribute("data-rm")];
                var used = lines.some(function (l) { return l.techCode === c.code; });
                if (used && !confirm(c.name + " has segment lines. Remove them as well?")) return;
                lines = lines.filter(function (l) { return l.techCode !== c.code; });
                crew.splice(+b.getAttribute("data-rm"), 1);
                render();
            });
        });
        $("crewEmpty").style.display = crew.length ? "none" : "block";
    }

    // ---------- pickers ----------
    var pickers = {};
    function picker(id, opts) {
        var el = $(id);
        if (!el) return;
        if (typeof flatpickr !== "function") { el.addEventListener("change", scheduleCalc); return; }
        pickers[id] = flatpickr(el, Object.assign({
            enableTime: true, noCalendar: true, dateFormat: "H:i", time_24hr: true, minuteIncrement: 5, allowInput: true,
            onClose: scheduleCalc, onChange: scheduleCalc
        }, opts || {}));
    }
    function setPicker(id, v) {
        if (pickers[id]) pickers[id].setDate(v || null, false, id === "g_date" ? "Y-m-d" : "H:i");
        else setVal(id, v);
    }
    if ($("g_date") && typeof flatpickr === "function") {
        pickers.g_date = flatpickr($("g_date"), { dateFormat: "Y-m-d", altInput: true, altFormat: "D d-M-Y", allowInput: true, onChange: scheduleCalc });
    } else if ($("g_date")) { $("g_date").type = "date"; $("g_date").addEventListener("change", scheduleCalc); }
    picker("g_start"); picker("g_end"); picker("g_lunch", { minuteIncrement: 15 });

    // ---------- calculation ----------
    var auto = null, manual = {}, calcTimer = null, calcSeq = 0;
    function scheduleCalc() { clearTimeout(calcTimer); calcTimer = setTimeout(runCalc, 250); }

    function runCalc() {
        var d = val("g_date"), s = val("g_start"), e = val("g_end");
        $("g_overnight").style.display = (parseHM(s) && parseHM(e) && minutes(e) <= minutes(s)) ? "inline" : "none";
        if (!d || !parseHM(s) || !parseHM(e)) return;
        var q = "workDate=" + encodeURIComponent(d) + "&start=" + encodeURIComponent(s) + "&end=" + encodeURIComponent(e) +
                "&lunch=" + hmToHours(val("g_lunch")) + "&location=Field&industry=" + encodeURIComponent(val("industryCode"));
        var seq = ++calcSeq, status = $("g_calcStatus");
        status.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>calculating…';
        fetch(urls.calcLine + (urls.calcLine.indexOf("?") === -1 ? "?" : "&") + q,
              { credentials: "same-origin", headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(function (r) { return r.json().then(function (j) { return { ok: r.ok, body: j }; }); })
            .then(function (res) {
                if (seq !== calcSeq) return;
                if (!res.ok) { status.innerHTML = '<span class="text-danger">' + esc(res.body && res.body.error || "cannot calculate") + "</span>"; return; }
                applyCalc(res.body);
                status.innerHTML = res.body.rateFound
                    ? '<i class="fa-solid fa-circle-check text-success me-1"></i>calculated'
                    : '<i class="fa-solid fa-triangle-exclamation text-warning me-1"></i>no Field labour rate — enter rate';
            })
            .catch(function () { if (seq === calcSeq) status.innerHTML = '<span class="text-warning">offline — enter hours and rates</span>'; });
    }

    function applyCalc(r) {
        var prevNet = auto ? auto.netHours : null;
        auto = r;
        setVal("g_net", n2(r.netHours));
        if (prevNet !== null && r2(prevNet) !== r2(r.netHours)) { manual.normal = manual.ot = manual.cost = false; }
        if (!manual.normal) setVal("g_normal", n2(r.normalHours));
        if (!manual.ot) setVal("g_ot", n2(r.otHours));
        if (!manual.rate) setVal("g_rate", n2(r.rate));
        if (!manual.otRate) setVal("g_otrate", n2(r.otRate));
        if (!manual.cost) setVal("g_cost", n2(costFromFields()));
    }
    function costFromFields() { return r2(num(val("g_normal")) * num(val("g_rate")) + num(val("g_ot")) * num(val("g_otrate"))); }

    document.querySelectorAll(".g-edit").forEach(function (inp) {
        inp.addEventListener("input", function () {
            var k = inp.getAttribute("data-key"), net = num(val("g_net"));
            manual[k] = true;
            if (k === "normal") { setVal("g_ot", n2(Math.max(0, net - num(inp.value)))); manual.ot = true; }
            if (k === "ot") { setVal("g_normal", n2(Math.max(0, net - num(inp.value)))); manual.normal = true; }
            if (k !== "cost" && !manual.cost) setVal("g_cost", n2(costFromFields()));
        });
    });

    // ---------- segment modal ----------
    var editIndex = null;
    var typeSel = $("g_type");
    function syncType() { $("g_taskWrap").style.visibility = typeSel.value === "WORK" ? "visible" : "hidden"; }
    typeSel.addEventListener("change", syncType);

    function modal() { return bootstrap.Modal.getOrCreateInstance($("segModal")); }

    function techBoxes(checkedCodes, single) {
        var box = $("g_techs");
        box.innerHTML = "";
        if (!crew.length) { box.innerHTML = "<span class='text-danger'>Add the crew first.</span>"; return; }
        crew.forEach(function (c) {
            var id = "gt_" + c.code.replace(/[^A-Za-z0-9_-]/g, "_");
            var on = checkedCodes.indexOf(c.code) !== -1;
            box.insertAdjacentHTML("beforeend",
                "<div class='form-check crew-chk'><input class='form-check-input' type='" + (single ? "radio" : "checkbox") +
                "' name='g_tech' id='" + id + "' value=\"" + esc(c.code) + "\"" + (on ? " checked" : "") + ">" +
                "<label class='form-check-label' for='" + id + "'>" + esc(c.code) + " — " + esc(c.name) + "</label></div>");
        });
        $("g_techHint").textContent = single ? "editing this technician's line" : "tick everyone this segment applies to — one line per technician";
    }

    $("addSegBtn").addEventListener("click", function () {
        if (!crew.length) { alert("Add the crew (technicians) first."); return; }
        editIndex = null; auto = null; manual = {}; calcSeq++;
        $("segError").classList.add("d-none");
        var last = lines.length ? lines[lines.length - 1] : null;
        setPicker("g_date", last ? last.workDate : FS.postingDate);
        typeSel.value = last ? "WORK" : "TRAVEL";
        setVal("g_task", last && last.taskCode ? last.taskCode : "");
        setPicker("g_start", last ? last.endTime : duty.start);
        setPicker("g_end", duty.end);
        setPicker("g_lunch", "00:00");
        ["g_idle", "g_idleReason", "g_net", "g_normal", "g_ot", "g_rate", "g_otrate", "g_cost", "g_food"].forEach(function (id) { setVal(id, ""); });
        $("g_calcStatus").textContent = "";
        techBoxes(crew.map(function (c) { return c.code; }), false);
        syncType();
        $("segTitle").innerHTML = '<i class="fa-solid fa-plus me-2 text-primary"></i>Add Segment';
        $("segSaveText").textContent = "Add to Grid";
        modal().show();
        scheduleCalc();
    });

    function openEdit(i) {
        var l = lines[i]; if (!l) return;
        editIndex = i; auto = null; manual = { normal: true, ot: true, rate: true, otRate: true, cost: true }; calcSeq++;
        $("segError").classList.add("d-none");
        setPicker("g_date", l.workDate);
        typeSel.value = l.type;
        setVal("g_task", l.taskCode || "");
        setPicker("g_start", l.startTime); setPicker("g_end", l.endTime); setPicker("g_lunch", hoursToHM(l.lunch));
        setVal("g_idle", l.idle ? n2(l.idle) : ""); setVal("g_idleReason", l.idleReason || "");
        setVal("g_net", n2(l.net)); setVal("g_normal", n2(l.normal)); setVal("g_ot", n2(l.ot));
        setVal("g_rate", n2(l.rate)); setVal("g_otrate", n2(l.otRate)); setVal("g_cost", n2(l.cost)); setVal("g_food", l.food ? n2(l.food) : "");
        $("g_calcStatus").textContent = "";
        techBoxes([l.techCode], true);
        syncType();
        $("segTitle").innerHTML = '<i class="fa-solid fa-pen me-2 text-primary"></i>Edit Segment';
        $("segSaveText").textContent = "Save Changes";
        modal().show();
    }

    $("segSave").addEventListener("click", function () {
        var d = val("g_date"), s = val("g_start"), e = val("g_end"), type = typeSel.value;
        var techs = Array.prototype.map.call(document.querySelectorAll("input[name='g_tech']:checked"), function (x) { return x.value; });
        var taskSel = $("g_task"), problems = [];
        var normal = r2(val("g_normal")), ot = r2(val("g_ot")), net = r2(normal + ot), idle = r2(val("g_idle"));
        if (!d) problems.push("Enter the date.");
        if (!parseHM(s) || !parseHM(e)) problems.push("Enter from and to times (hh:mm).");
        else if (minutes(s) === minutes(e)) problems.push("To time must differ from From time.");
        if (type === "WORK" && !taskSel.value) problems.push("Select the task for a Work segment.");
        if (!techs.length) problems.push("Tick at least one technician.");
        if (net <= 0) problems.push("Net hours must be greater than zero (check times and lunch).");
        if (auto && r2(net) !== r2(auto.netHours) && !manual.normal && !manual.ot) problems.push("Normal + OT must equal Net hours.");
        if (idle < 0 || idle > net) problems.push("Idle hours must be between 0 and the net hours.");
        // Same technician can't be in two places at once: overlapping segments on the same day.
        var st = minutes(s), en = minutes(e); if (en <= st) en += 1440;
        techs.forEach(function (code) {
            lines.forEach(function (o, idx) {
                if (idx === editIndex || o.techCode !== code || o.workDate !== d) return;
                var os = minutes(o.startTime), oe = minutes(o.endTime); if (oe <= os) oe += 1440;
                if (st < oe && os < en) problems.push(code + " already has a segment " + o.startTime + "–" + o.endTime + " on this date.");
            });
        });
        if (problems.length) { $("segError").innerHTML = problems.join("<br>"); $("segError").classList.remove("d-none"); return; }

        var taskOpt = taskSel.options[taskSel.selectedIndex];
        var cost = r2(val("g_cost")), food = r2(val("g_food"));
        var base = {
            type: type, workDate: d, startTime: s, endTime: e, lunch: hmToHours(val("g_lunch")),
            idle: idle, idleReason: idle > 0 ? val("g_idleReason").trim() : "",
            taskCode: type === "WORK" ? taskSel.value : "", taskName: type === "WORK" ? (taskOpt.getAttribute("data-name") || taskOpt.text) : "Travel",
            stdHours: type === "WORK" ? num(taskOpt.getAttribute("data-std")) : 0,
            skill: type === "WORK" ? (taskOpt.getAttribute("data-skill") || "") : "",
            net: net, normal: normal, ot: ot, rate: r2(val("g_rate")), otRate: r2(val("g_otrate")), cost: cost, food: food,
            travelSite: "", travelStart: "", travelEnd: "", travelHours: 0
        };
        function forTech(code, prev) {
            var c = crew.filter(function (x) { return x.code === code; })[0] || { name: "" };
            var l = Object.assign({}, base, { techCode: code, techName: c.name });
            if (prev && prev.type === "WORK" && type === "WORK") {   // keep legacy travel-on-line values of old lines
                l.travelSite = prev.travelSite; l.travelStart = prev.travelStart; l.travelEnd = prev.travelEnd; l.travelHours = prev.travelHours;
            }
            return l;
        }
        if (editIndex === null) techs.forEach(function (code) { lines.push(forTech(code)); });
        else lines[editIndex] = forTech(techs[0], lines[editIndex]);
        render();
        modal().hide();
    });

    // Re-run the engine for a line already in the grid (keeps its rates; refreshes the hour split + cost).
    function recalcLine(l) {
        var q = "workDate=" + encodeURIComponent(l.workDate) + "&start=" + encodeURIComponent(l.startTime) + "&end=" + encodeURIComponent(l.endTime) +
                "&lunch=" + l.lunch + "&location=Field&industry=" + encodeURIComponent(val("industryCode"));
        fetch(urls.calcLine + (urls.calcLine.indexOf("?") === -1 ? "?" : "&") + q,
              { credentials: "same-origin", headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
            .then(function (c) {
                l.net = r2(c.netHours); l.normal = r2(c.normalHours); l.ot = r2(c.otHours);
                l.cost = r2(l.normal * l.rate + l.ot * l.otRate);
                render();
            })
            .catch(function () { /* keep the copied figures */ });
    }

    // ---------- grid ----------
    function sortLines() {
        lines.sort(function (a, b) {
            if (a.workDate !== b.workDate) return a.workDate < b.workDate ? -1 : 1;
            if (a.startTime !== b.startTime) return minutes(a.startTime) - minutes(b.startTime);
            return String(a.techName).localeCompare(String(b.techName));
        });
    }

    function render() {
        sortLines();
        var body = $("segBody");
        body.innerHTML = "";
        var t = { work: 0, travel: 0, idle: 0, normal: 0, ot: 0, labour: 0, food: 0 }, perTech = {}, day = null, dayTot = null;

        function flushDay() {
            if (!day) return;
            var tr = document.createElement("tr");
            tr.className = "small text-muted";
            tr.innerHTML = "<td colspan='6' class='text-end'>Day total</td><td class='text-num'>" + n2(dayTot.net) + "</td><td class='text-num'>" + n2(dayTot.normal) +
                "</td><td class='text-num'>" + n2(dayTot.ot) + "</td><td class='text-num'>" + n2(dayTot.labour) + "</td><td class='text-num'>" + n2(dayTot.food) +
                "</td><td class='text-num'>" + n2(dayTot.labour + dayTot.food) + "</td><td></td>";
            body.appendChild(tr);
        }

        lines.forEach(function (l, i) {
            if (l.workDate !== day) {
                flushDay();
                day = l.workDate; dayTot = { net: 0, normal: 0, ot: 0, labour: 0, food: 0 };
                var h = document.createElement("tr");
                h.className = "fsr-day";
                h.innerHTML = "<td colspan='13'><i class='fa-regular fa-calendar me-2'></i>" + fmtDate(day) + "</td>";
                body.appendChild(h);
            }
            dayTot.net += l.net; dayTot.normal += l.normal; dayTot.ot += l.ot; dayTot.labour += l.cost; dayTot.food += l.food;
            var isTravel = l.type === "TRAVEL";
            if (isTravel) t.travel += l.net; else t.work += l.net;
            t.travel += num(l.travelHours); t.idle += l.idle; t.normal += l.normal; t.ot += l.ot; t.labour += l.cost; t.food += l.food;
            var p = perTech[l.techCode] || (perTech[l.techCode] = { work: 0, travel: 0, idle: 0, food: 0 });
            if (isTravel) p.travel += l.net; else p.work += l.net;
            p.travel += num(l.travelHours); p.idle += l.idle; p.food += l.food;

            var overnight = minutes(l.endTime) <= minutes(l.startTime);
            var tt = timeTypeFrom(l.normal, l.ot);
            var ttBadge = tt === "OVERTIME" ? "<span class='badge-soft badge-ot'>OT</span>" : tt === "MIXED" ? "<span class='badge-soft badge-mixed'>N+OT</span>" : "<span class='badge-soft badge-normal'>Normal</span>";
            var tr = document.createElement("tr");
            tr.innerHTML =
                "<td><span class='seg-badge seg-" + l.type + "'>" + (isTravel ? "Travel" : "Work") + "</span></td>" +
                "<td class='text-nowrap'>" + esc(l.startTime) + " – " + esc(l.endTime) + (overnight ? " <i class='fa-solid fa-moon text-muted' title='Ends next day'></i>" : "") + " " + ttBadge + "</td>" +
                "<td>" + (isTravel ? "<span class='text-muted'>—</span>" : esc(l.taskName)) +
                    (l.idle > 0 ? "<div class='small text-muted'>idle: " + esc(l.idleReason || "—") + "</div>" : "") + "</td>" +
                "<td>" + esc(l.techCode) + "<div class='small text-muted'>" + esc(l.techName) + "</div></td>" +
                "<td class='text-num'>" + hoursToHM(l.lunch) + "</td>" +
                "<td class='text-num'>" + (l.idle ? n2(l.idle) : "") + "</td>" +
                "<td class='text-num fw-semibold'>" + n2(l.net) + "</td>" +
                "<td class='text-num'>" + n2(l.normal) + "</td><td class='text-num'>" + n2(l.ot) + "</td>" +
                "<td class='text-num'>" + n2(l.cost) + "</td><td class='text-num'>" + (l.food ? n2(l.food) : "") + "</td>" +
                "<td class='text-num fw-semibold'>" + n2(l.cost + l.food) + "</td>" +
                "<td class='text-nowrap'>" +
                    "<button type='button' class='btn btn-sm btn-outline-primary me-1' data-edit='" + i + "' title='Edit'><i class='fa-solid fa-pen'></i></button>" +
                    "<button type='button' class='btn btn-sm btn-outline-secondary me-1' data-copy='" + i + "' title='Copy this segment (whole crew) to the next day'><i class='fa-regular fa-copy'></i></button>" +
                    "<button type='button' class='btn btn-sm btn-outline-danger' data-del='" + i + "' title='Remove'><i class='fa-solid fa-trash'></i></button>" +
                "</td>";
            body.appendChild(tr);
        });
        flushDay();

        body.querySelectorAll("button[data-edit]").forEach(function (b) { b.addEventListener("click", function () { openEdit(+b.getAttribute("data-edit")); }); });
        body.querySelectorAll("button[data-del]").forEach(function (b) { b.addEventListener("click", function () { lines.splice(+b.getAttribute("data-del"), 1); render(); }); });
        body.querySelectorAll("button[data-copy]").forEach(function (b) {
            b.addEventListener("click", function () {
                var src = lines[+b.getAttribute("data-copy")], p = src.workDate.split("-");
                var nd = new Date(+p[0], +p[1] - 1, +p[2] + 1);
                var ds = nd.getFullYear() + "-" + ("0" + (nd.getMonth() + 1)).slice(-2) + "-" + ("0" + nd.getDate()).slice(-2);
                // The crew shares segments: copy this segment for every technician on it (skipping clashes on the new day).
                var copies = lines.filter(function (o) {
                    return o.workDate === src.workDate && o.type === src.type && o.startTime === src.startTime && o.endTime === src.endTime;
                }).filter(function (o) {
                    return !lines.some(function (x) { return x.techCode === o.techCode && x.workDate === ds && x.startTime === o.startTime; });
                }).map(function (o) {
                    return Object.assign({}, o, { workDate: ds, food: 0, idle: 0, idleReason: "", travelSite: "", travelStart: "", travelEnd: "", travelHours: 0 });
                });
                Array.prototype.push.apply(lines, copies);
                render();
                copies.forEach(recalcLine);   // the new date may be a weekend → different Normal/OT split
            });
        });
        $("segEmpty").style.display = lines.length ? "none" : "block";

        var vcost = vehicle();
        setText("sWork", n2(t.work)); setText("sTravel", n2(t.travel)); setText("sIdle", n2(t.idle));
        setText("sSplit", t.normal.toFixed(1) + " / " + t.ot.toFixed(1));
        setText("sLabour", n2(t.labour)); setText("sFood", n2(t.food)); setText("sVehicle", n2(vcost));
        setText("sTotal", n2(t.labour + t.food + vcost));
        renderCrew(perTech);
        writeHidden();
    }

    // ---------- hidden inputs for the post ----------
    function writeHidden() {
        var html = [];
        function h(name, v) { html.push("<input type='hidden' name='" + name + "' value=\"" + esc(v) + "\">"); }
        crew.forEach(function (c, i) { h("Crew[" + i + "].EmpCode", c.code); h("Crew[" + i + "].EmpName", c.name); h("Crew[" + i + "].FaCode", c.fa); });
        lines.forEach(function (l, i) {
            var p = "Lines[" + i + "].";
            h(p + "SegmentType", l.type); h(p + "WorkDate", l.workDate); h(p + "StartTime", l.startTime); h(p + "EndTime", l.endTime);
            h(p + "LunchHours", l.lunch); h(p + "IdleHours", l.idle); h(p + "IdleReason", l.idleReason);
            h(p + "TaskCode", l.taskCode); h(p + "TaskName", l.taskName); h(p + "TechCode", l.techCode); h(p + "TechName", l.techName); h(p + "Skill", l.skill);
            h(p + "NetHours", l.net); h(p + "NormalHours", l.normal); h(p + "OtHours", l.ot);
            h(p + "Rate", l.rate); h(p + "OtRate", l.otRate); h(p + "LabourCost", l.cost); h(p + "FoodAllowance", l.food); h(p + "TotalCost", r2(l.cost + l.food));
            h(p + "TravelSite", l.travelSite); h(p + "TravelStart", l.travelStart); h(p + "TravelEnd", l.travelEnd); h(p + "TravelHours", l.travelHours);
        });
        $("hiddenInputs").innerHTML = html.join("");
    }

    // ---------- seed ----------
    (FS.lines || []).forEach(function (x) {
        lines.push({
            type: x.type === "TRAVEL" ? "TRAVEL" : "WORK", workDate: x.workDate, startTime: x.startTime, endTime: x.endTime,
            lunch: num(x.lunch), idle: num(x.idle), idleReason: x.idleReason || "",
            taskCode: x.taskCode || "", taskName: x.taskName || "", stdHours: num(x.stdHours),
            techCode: x.techCode, techName: x.techName || "", skill: x.skill || "",
            net: num(x.net), normal: num(x.normal), ot: num(x.ot), rate: num(x.rate), otRate: num(x.otRate), cost: num(x.cost), food: num(x.food),
            travelSite: x.travelSite || "", travelStart: x.travelStart || "", travelEnd: x.travelEnd || "", travelHours: num(x.travelHours)
        });
    });
    render();
})();
