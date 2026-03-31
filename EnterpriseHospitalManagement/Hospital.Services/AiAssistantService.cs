using System.Text.RegularExpressions;
using Hospital.Models.Enums;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly ApplicationDbContext _db;

        public AiAssistantService(ApplicationDbContext db)
        {
            _db = db;
        }

        public AiAssistantPageViewModel Build(AiAssistantRole role, string userId, string? userName, string? prompt)
        {
            var cleanPrompt = (prompt ?? string.Empty).Trim();

            return role switch
            {
                AiAssistantRole.Doctor => BuildDoctor(userId, userName, cleanPrompt),
                AiAssistantRole.Pharmacist => BuildPharmacist(userName, cleanPrompt),
                AiAssistantRole.Nurse => BuildNurse(cleanPrompt),
                AiAssistantRole.LabTech => BuildLabTech(userId, cleanPrompt),
                AiAssistantRole.Receptionist => BuildReceptionist(cleanPrompt),
                AiAssistantRole.Admin => BuildAdmin(cleanPrompt),
                AiAssistantRole.Patient => BuildPatient(userId, userName, cleanPrompt),
                _ => new AiAssistantPageViewModel()
            };
        }

        private AiAssistantPageViewModel BuildDoctor(string userId, string? userName, string prompt)
        {
            var appointments = _db.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(250)
                .ToList();

            var reports = _db.PatientReports
                .Include(r => r.Patient)
                .Include(r => r.PrescribedMedicines)
                .ThenInclude(pm => pm.Medicine)
                .Where(r => r.DoctorId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .Take(100)
                .ToList();

            var todayCount = appointments.Count(a => a.AppointmentDate.Date == DateTime.Today);
            var upcoming = appointments.Count(a => a.AppointmentDate >= DateTime.Now && a.AppointmentDate <= DateTime.Now.AddDays(3));
            var recentDiagnoses = reports
                .Where(r => !string.IsNullOrWhiteSpace(r.Diagnose))
                .GroupBy(r => r.Diagnose.Trim())
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key} ({g.Count()})")
                .ToList();

            var model = CreateBaseModel(
                AiAssistantRole.Doctor,
                "Doctor",
                "AI Clinical Assistant",
                $"Clinical decision support for {FriendlyName(userName)} with diagnosis cues, medication checks, treatment planning, and conversational follow-up.",
                "Describe symptoms, suspected diagnosis, medicines, risks, or treatment goals.",
                "Generate clinical guidance",
                "AI guidance supports clinician review and should be validated against the patient chart, labs, and hospital protocol.");

            model.Capabilities.AddRange(new[]
            {
                "Diagnosis support from symptoms and recent history",
                "Medicine recommendation and safety review",
                "Drug interaction and duplicate therapy checks",
                "Early warning cues from case wording",
                "Treatment plan outline with follow-up steps",
                "Clinical chat grounded in recent appointment activity"
            });

            model.SuggestedPrompts.AddRange(new[]
            {
                "Fever, productive cough, and low appetite for 3 days. What should I rule out first?",
                "Review hypertension treatment options for a diabetic patient already taking metformin.",
                "Build a follow-up plan for a patient with repeat dizziness and borderline high BP."
            });

            model.Metrics.AddRange(new[]
            {
                Metric("Today's patients", todayCount, "fa-calendar-day", "blue"),
                Metric("Upcoming 72h", upcoming, "fa-user-clock", "teal"),
                Metric("Reports authored", reports.Count, "fa-file-medical", "green"),
                Metric("Common diagnoses", recentDiagnoses.Count == 0 ? "None yet" : string.Join(", ", recentDiagnoses), "fa-stethoscope", "orange")
            });

            model.LiveInsights.Add($"You have {todayCount} appointments scheduled today and {upcoming} more in the next 72 hours.");
            if (recentDiagnoses.Count > 0)
                model.LiveInsights.Add($"Recent chart trend: {string.Join(", ", recentDiagnoses)}.");
            model.LiveInsights.Add($"Completed reports on record: {reports.Count}. The assistant uses these trends to shape recommendations.");

            if (string.IsNullOrWhiteSpace(prompt))
            {
                model.ResponseStatusLabel = "Ready";
                model.ResponseStatusTone = "info";
                model.ResponseFacts.AddRange(new[]
                {
                    Fact("Mode", "Clinical overview"),
                    Fact("Data sources", $"{appointments.Count} appointments, {reports.Count} reports"),
                    Fact("Interaction checks", "Enabled")
                });
                model.ResponseTitle = "Clinical snapshot";
                model.ResponseSummary = "The assistant is ready for diagnosis support, medication review, and treatment planning.";
                model.Sections.Add(Section("Suggested focus today", new[]
                {
                    todayCount > 6 ? "Clinic load is high today. Prioritize triage for respiratory, chest pain, and repeat-visit complaints." : "Clinic load is manageable. You can use the assistant for deeper treatment planning.",
                    recentDiagnoses.Count > 0 ? $"Most frequent recent diagnoses: {string.Join(", ", recentDiagnoses)}." : "Once reports accumulate, common diagnoses will surface here automatically.",
                    "Ask about symptoms, red flags, medicines, or a treatment plan to get role-specific guidance."
                }));
                return model;
            }

            var considerations = new List<string>();
            var tests = new List<string>();
            var medications = new List<string>();
            var plan = new List<string>();

            if (ContainsAny(prompt, "chest pain", "shortness of breath", "unconscious", "seizure"))
            {
                considerations.Add("This wording contains high-acuity symptoms. Consider emergency escalation and immediate vitals or ECG review.");
                tests.Add("Obtain urgent cardiac and respiratory workup with pulse oximetry, ECG, and focused exam.");
                plan.Add("Do not rely on outpatient follow-up alone if instability or active distress is present.");
            }
            if (ContainsAny(prompt, "fever", "cough", "sputum", "pneumonia"))
            {
                considerations.Add("Infectious respiratory illness is a leading consideration given fever and cough language.");
                tests.Add("Consider CBC, inflammatory markers, chest imaging, and oxygen saturation if symptoms are significant.");
                medications.Add("Check for antibiotic allergy, renal status, and duplicate symptomatic therapy before prescribing.");
                plan.Add("Include hydration advice, return precautions for dyspnea, and a 48-72 hour reassessment plan.");
            }
            if (ContainsAny(prompt, "dizzy", "dizziness", "syncope", "vertigo"))
            {
                considerations.Add("Dizziness should be risk-stratified for orthostatic, vestibular, metabolic, and cardiovascular causes.");
                tests.Add("Review orthostatic vitals, glucose, hydration status, and medication burden.");
                plan.Add("If recurrent, document fall risk and arrange timely follow-up.");
            }
            if (ContainsAny(prompt, "diabetes", "glucose", "metformin"))
            {
                considerations.Add("Diabetes-related complications and medication tolerance should be reviewed.");
                medications.Add("If metformin is already in use, check renal function and GI tolerance before intensifying therapy.");
                plan.Add("Tie treatment changes to glucose trends, diet counseling, and interval lab follow-up.");
            }
            if (ContainsAny(prompt, "warfarin", "aspirin", "ibuprofen", "diclofenac"))
                medications.Add("Potential anticoagulant or NSAID bleeding risk detected. Review GI bleed history and need for gastroprotection.");

            model.DetectedSignals.AddRange(CollectSignals(prompt, "chest pain", "shortness of breath", "fever", "cough", "dizziness", "diabetes", "metformin", "warfarin", "aspirin", "ibuprofen"));

            if (considerations.Count == 0)
                considerations.Add("Primary differential should be refined with symptom duration, severity, vitals, and recent labs from the chart.");
            if (tests.Count == 0)
                tests.Add("Match investigations to the dominant system involved and verify whether recent labs or imaging already exist.");
            if (medications.Count == 0)
                medications.Add("Screen for interactions, duplicate therapy, renal dosing, and allergy history before finalizing medicines.");
            if (plan.Count == 0)
                plan.Add("Document a phased plan: immediate stabilization, targeted treatment, patient education, and follow-up timing.");

            model.ResponseStatusLabel = model.DetectedSignals.Any(s => s is "chest pain" or "shortness of breath") ? "High risk" : "Review needed";
            model.ResponseStatusTone = model.DetectedSignals.Any(s => s is "chest pain" or "shortness of breath") ? "danger" : "warning";
            model.ResponseFacts.AddRange(new[]
            {
                Fact("Signals detected", model.DetectedSignals.Count == 0 ? "General clinical query" : string.Join(", ", model.DetectedSignals)),
                Fact("Workup items", tests.Count),
                Fact("Plan items", plan.Count)
            });
            model.ResponseTitle = "Clinical guidance generated";
            model.ResponseSummary = "The assistant reviewed the case wording and recent portal activity to outline differential, workup, medicine safety, and next steps.";
            model.Sections.Add(Section("Differential and red flags", considerations));
            model.Sections.Add(Section("Recommended workup", tests));
            model.Sections.Add(Section("Medication and safety review", medications));
            model.Sections.Add(Section("Treatment plan outline", plan));
            return model;
        }

        private AiAssistantPageViewModel BuildPharmacist(string? userName, string prompt)
        {
            var medicines = _db.Medicines.ToList();
            var prescriptions = _db.PrescribedMedicines
                .Include(pm => pm.Medicine)
                .OrderByDescending(pm => pm.Id)
                .Take(150)
                .ToList();

            var commonMeds = prescriptions
                .Where(p => p.Medicine != null)
                .GroupBy(p => p.Medicine!.Name)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key} ({g.Count()})")
                .ToList();

            var model = CreateBaseModel(
                AiAssistantRole.Pharmacist,
                "Pharmacist",
                "AI Pharmacy Assistant",
                $"Pharmacy support for {FriendlyName(userName)} with interaction checks, dosage guardrails, therapeutic substitution cues, and chat support.",
                "List medicines, condition details, dosage concerns, or substitution needs.",
                "Review therapy",
                "Use this output as a pharmacy safety checklist and confirm final decisions against formulary, patient allergies, and prescriber intent.");

            model.Capabilities.AddRange(new[]
            {
                "Drug interaction screening",
                "Dosage guide and timing reminders",
                "Therapeutic substitution ideas",
                "Duplicate therapy warnings",
                "High-level dispensing chat support"
            });

            model.SuggestedPrompts.AddRange(new[]
            {
                "Check interactions for warfarin, aspirin, and ibuprofen.",
                "Suggest a therapeutic substitute for a proton pump inhibitor shortage.",
                "Review dosage concerns for amoxicillin in an older adult."
            });

            model.Metrics.AddRange(new[]
            {
                Metric("Medicines catalog", medicines.Count, "fa-pills", "blue"),
                Metric("Recent prescriptions", prescriptions.Count, "fa-prescription-bottle-medical", "green"),
                Metric("Top dispensed", commonMeds.Count == 0 ? "No data" : string.Join(", ", commonMeds), "fa-capsules", "orange")
            });

            model.LiveInsights.Add($"The pharmacy catalog currently contains {medicines.Count} medicines.");
            if (commonMeds.Count > 0)
                model.LiveInsights.Add($"Most frequent recent prescription items: {string.Join(", ", commonMeds)}.");
            model.LiveInsights.Add("Ask about combinations, dose questions, or substitution strategy to get a pharmacy-focused review.");

            if (string.IsNullOrWhiteSpace(prompt))
            {
                model.ResponseStatusLabel = "Ready";
                model.ResponseStatusTone = "info";
                model.ResponseFacts.AddRange(new[]
                {
                    Fact("Mode", "Pharmacy review"),
                    Fact("Catalog size", medicines.Count),
                    Fact("Recent prescription sample", prescriptions.Count)
                });
                model.ResponseTitle = "Pharmacy snapshot";
                model.ResponseSummary = "The assistant is ready to review interactions, substitutions, and dosing concerns.";
                model.Sections.Add(Section("What it will check", new[]
                {
                    "Interaction pairs such as anticoagulants with NSAIDs, sedative stacking, and duplicate acid suppression.",
                    "Dose clarity, administration timing, and whether instructions are missing from recent prescriptions.",
                    "Therapeutic alternatives when stock, tolerance, or formulary concerns arise."
                }));
                return model;
            }

            var therapy = new List<string>();
            var interactions = new List<string>();
            var dosing = new List<string>();

            if (ContainsAny(prompt, "warfarin") && ContainsAny(prompt, "aspirin", "ibuprofen", "diclofenac"))
                interactions.Add("Warfarin combined with aspirin or NSAIDs increases bleeding risk and should trigger escalation to the prescriber.");
            if (ContainsAny(prompt, "metformin") && ContainsAny(prompt, "contrast", "ct scan"))
                interactions.Add("Metformin plus iodinated contrast should prompt renal-function review and temporary hold guidance if indicated.");
            if (ContainsAny(prompt, "amoxicillin", "antibiotic"))
                dosing.Add("Verify renal function, allergy history, and intended duration before confirming antibiotic supply.");
            if (ContainsAny(prompt, "ppi", "omeprazole", "pantoprazole"))
                therapy.Add("If formulary pressure exists, a same-class proton pump inhibitor is a reasonable therapeutic substitution after prescriber confirmation.");
            if (ContainsAny(prompt, "statin"))
                therapy.Add("Confirm duplicate statin therapy is not being dispensed unintentionally during transitions of care.");

            model.DetectedSignals.AddRange(CollectSignals(prompt, "warfarin", "aspirin", "ibuprofen", "diclofenac", "metformin", "contrast", "amoxicillin", "ppi", "statin"));

            if (interactions.Count == 0)
                interactions.Add("No high-risk interaction keyword matched immediately. Review anticoagulants, sedatives, NSAIDs, and duplicate chronic therapy manually.");
            if (dosing.Count == 0)
                dosing.Add("Confirm age, renal function, hepatic status, and administration schedule before dispensing.");
            if (therapy.Count == 0)
                therapy.Add("Substitution decisions should stay within formulary and therapeutic equivalence guidance.");

            model.ResponseStatusLabel = interactions.Any(i => i.Contains("bleeding risk", StringComparison.OrdinalIgnoreCase)) ? "High interaction risk" : "Standard review";
            model.ResponseStatusTone = interactions.Any(i => i.Contains("bleeding risk", StringComparison.OrdinalIgnoreCase)) ? "danger" : "info";
            model.ResponseFacts.AddRange(new[]
            {
                Fact("Signals detected", model.DetectedSignals.Count == 0 ? "General medication query" : string.Join(", ", model.DetectedSignals)),
                Fact("Interaction items", interactions.Count),
                Fact("Substitution items", therapy.Count)
            });
            model.ResponseTitle = "Pharmacy review generated";
            model.ResponseSummary = "The assistant checked the request for interaction risk, dosing clarity, and substitution opportunities.";
            model.Sections.Add(Section("Interaction risks", interactions));
            model.Sections.Add(Section("Dosage and administration", dosing));
            model.Sections.Add(Section("Therapeutic substitution", therapy));
            return model;
        }

        private AiAssistantPageViewModel BuildNurse(string prompt)
        {
            var appointments = _db.Appointments
                .OrderByDescending(a => a.AppointmentDate)
                .Take(250)
                .ToList();
            var activePatients = appointments
                .Where(a => a.AppointmentDate.Date >= DateTime.Today.AddDays(-1))
                .Select(a => a.PatientId)
                .Distinct()
                .Count();

            var model = CreateBaseModel(
                AiAssistantRole.Nurse,
                "Nurse",
                "AI Nursing Assistant",
                "Nursing support for vitals interpretation, care prioritization, bedside escalation cues, and care-plan chat.",
                "Enter vitals, symptoms, bedside concern, or care planning notes. Example: BP 178/112, temp 102, dizzy and weak.",
                "Interpret vitals",
                "This tool supports nursing judgment. Escalate to the clinician immediately for unstable patients or emergency symptoms.");

            model.Capabilities.AddRange(new[]
            {
                "Vitals interpretation with normal, warning, and critical flags",
                "Care plan drafting for routine and at-risk patients",
                "Escalation reminders for bedside deterioration",
                "Nursing handoff and observation chat"
            });

            model.SuggestedPrompts.AddRange(new[]
            {
                "BP 178/112, temp 102.4, dizzy and weak. How urgent is this?",
                "Create a nursing care plan for a patient with fever and dehydration risk.",
                "Interpret borderline high BP and mild cough before doctor review."
            });

            model.Metrics.AddRange(new[]
            {
                Metric("Recent appointments", appointments.Count, "fa-bed-pulse", "blue"),
                Metric("Active patient flow", activePatients, "fa-user-nurse", "green")
            });

            model.LiveInsights.Add($"Recent patient flow shows {activePatients} active patients across the latest appointment window.");
            model.LiveInsights.Add("Use the assistant to flag critical vitals before handoff or to draft focused care tasks.");

            var (systolic, diastolic) = ExtractBloodPressure(prompt);
            var temp = ExtractTemperature(prompt);

            if (string.IsNullOrWhiteSpace(prompt))
            {
                model.ResponseStatusLabel = "Ready";
                model.ResponseStatusTone = "info";
                model.ResponseFacts.AddRange(new[]
                {
                    Fact("Mode", "Vitals triage"),
                    Fact("Recent patient flow", activePatients),
                    Fact("Bedside escalation", "Enabled")
                });
                model.ResponseTitle = "Nursing snapshot";
                model.ResponseSummary = "The assistant is ready to classify vitals and outline bedside care priorities.";
                model.Sections.Add(Section("How to use it", new[]
                {
                    "Paste BP, temperature, symptoms, and the bedside concern in plain language.",
                    "The assistant will mark the case as critical, warning, or stable based on the values and symptom text.",
                    "It then drafts observation and care-plan bullets you can adapt for handoff."
                }));
                return model;
            }

            var severity = "Normal";
            var flags = new List<string>();
            var carePlan = new List<string>();

            if ((systolic.HasValue && systolic >= 180) || (diastolic.HasValue && diastolic >= 120) || (temp.HasValue && temp >= 103) || ContainsAny(prompt, "unresponsive", "seizure", "severe shortness of breath"))
            {
                severity = "Critical";
                flags.Add("Critical escalation threshold reached. Notify the doctor immediately and begin continuous monitoring.");
            }
            else if ((systolic.HasValue && systolic >= 140) || (diastolic.HasValue && diastolic >= 90) || (temp.HasValue && temp >= 100.4) || ContainsAny(prompt, "dizzy", "weak", "dehydrated", "confused"))
            {
                severity = "Warning";
                flags.Add("Vital signs or symptoms suggest a warning state that warrants closer reassessment and clinician update.");
            }
            else
            {
                flags.Add("No critical threshold detected from the supplied values. Continue standard observation and reassessment.");
            }

            if (systolic.HasValue && diastolic.HasValue)
                flags.Add($"Blood pressure interpreted from input: {systolic}/{diastolic}.");
            if (temp.HasValue)
                flags.Add($"Temperature interpreted from input: {temp:0.#}.");

            model.DetectedSignals.AddRange(CollectSignals(prompt, "dizzy", "weak", "dehydration", "confused", "cough", "shortness of breath", "vomiting"));
            if (systolic.HasValue && diastolic.HasValue)
                model.DetectedSignals.Add($"BP {systolic}/{diastolic}");
            if (temp.HasValue)
                model.DetectedSignals.Add($"Temp {temp:0.#}");

            carePlan.Add("Recheck vitals on a defined interval and document trend rather than relying on a single reading.");
            carePlan.Add("Watch hydration, mobility, pain, and mental-status changes during observation.");
            if (ContainsAny(prompt, "cough", "shortness of breath"))
                carePlan.Add("Add respiratory observation with pulse oximetry if available and escalate any worsening work of breathing.");
            if (ContainsAny(prompt, "dehydration", "vomiting", "poor intake"))
                carePlan.Add("Track intake/output and notify the clinician if oral intake remains poor or symptoms worsen.");

            model.ResponseStatusLabel = severity;
            model.ResponseStatusTone = severity == "Critical" ? "danger" : severity == "Warning" ? "warning" : "success";
            model.ResponseFacts.AddRange(new[]
            {
                Fact("Detected signals", model.DetectedSignals.Count == 0 ? "No key signals detected" : string.Join(", ", model.DetectedSignals)),
                Fact("Monitoring level", severity == "Critical" ? "Immediate escalation" : severity == "Warning" ? "Close reassessment" : "Routine observation"),
                Fact("Care items", carePlan.Count)
            });
            model.ResponseTitle = $"Vitals interpretation: {severity}";
            model.ResponseSummary = "The assistant classified the bedside concern and drafted immediate nursing actions.";
            model.Sections.Add(Section("Clinical flags", flags));
            model.Sections.Add(Section("Suggested nursing care plan", carePlan));
            return model;
        }

        private AiAssistantPageViewModel BuildLabTech(string userId, string prompt)
        {
            var labs = _db.Labs
                .Include(l => l.Patient)
                .Where(l => l.TechnicianId == userId || l.TechnicianId == null)
                .OrderByDescending(l => l.CreatedDate)
                .Take(200)
                .ToList();

            var pending = labs.Count(l => l.Status != LabTestStatus.Completed && l.Status != LabTestStatus.Cancelled);
            var completed = labs.Count(l => l.Status == LabTestStatus.Completed);

            var model = CreateBaseModel(
                AiAssistantRole.LabTech,
                "LabTech",
                "AI Lab Interpreter",
                "Lab support for interpreting result wording, suggesting likely follow-up panels, and helping technicians triage notable findings.",
                "Paste the test type and result summary. Example: glucose 168 fasting, Hb 10.5, fatigue.",
                "Interpret result",
                "This assistant supports lab workflow only. Final interpretation should follow clinician review and local laboratory standards.");

            model.Capabilities.AddRange(new[]
            {
                "Result interpretation guidance",
                "Abnormality flagging from plain-text result entry",
                "Suggested follow-up test panels",
                "Contextual lab chat"
            });

            model.SuggestedPrompts.AddRange(new[]
            {
                "Fasting glucose 168 and HbA1c pending. How should I flag this?",
                "Hemoglobin 10.2 with fatigue. What follow-up tests are commonly paired?",
                "Creatinine 1.6 and BP 160/100. What panels should be suggested?"
            });

            model.Metrics.AddRange(new[]
            {
                Metric("Pending tests", pending, "fa-flask-vial", "orange"),
                Metric("Completed tests", completed, "fa-vial-circle-check", "green"),
                Metric("Recent queue", labs.Count, "fa-list-check", "blue")
            });

            model.LiveInsights.Add($"The lab queue currently shows {pending} pending tests and {completed} completed items in the recent sample.");
            model.LiveInsights.Add("Ask about a result phrase or numeric marker to get interpretation and panel suggestions.");

            if (string.IsNullOrWhiteSpace(prompt))
            {
                model.ResponseStatusLabel = "Ready";
                model.ResponseStatusTone = "info";
                model.ResponseFacts.AddRange(new[]
                {
                    Fact("Mode", "Lab interpretation"),
                    Fact("Pending queue", pending),
                    Fact("Completed sample", completed)
                });
                model.ResponseTitle = "Lab snapshot";
                model.ResponseSummary = "The assistant is ready to interpret result text and suggest likely next panels.";
                model.Sections.Add(Section("What it reviews", new[]
                {
                    "Common lab markers such as glucose, hemoglobin, white cell count, cholesterol, and creatinine.",
                    "Whether the wording suggests a mild, moderate, or important abnormality flag.",
                    "Commonly paired panels that can help complete the picture before clinician review."
                }));
                return model;
            }

            var interpretation = new List<string>();
            var panels = new List<string>();
            var number = ExtractFirstNumber(prompt);

            if (ContainsAny(prompt, "glucose") && number.HasValue)
            {
                interpretation.Add(number >= 126 ? $"Glucose value {number:0.#} is elevated for fasting context and should be flagged for diabetic review." : $"Glucose value {number:0.#} is not obviously critical from the supplied wording.");
                panels.Add("Pair with HbA1c, renal profile, and urine testing if diabetes assessment is underway.");
            }
            if (ContainsAny(prompt, "hemoglobin", "hb") && number.HasValue)
            {
                interpretation.Add(number < 12 ? $"Hemoglobin {number:0.#} suggests anemia and should prompt severity/context review." : $"Hemoglobin {number:0.#} is not clearly low by the supplied value.");
                panels.Add("Consider CBC indices, ferritin, iron studies, and reticulocyte count if clinically appropriate.");
            }
            if (ContainsAny(prompt, "creatinine") && number.HasValue)
            {
                interpretation.Add(number > 1.3 ? $"Creatinine {number:0.#} is elevated and may indicate renal impairment or dehydration." : $"Creatinine {number:0.#} is not clearly elevated from the supplied threshold.");
                panels.Add("Renal function panel, electrolytes, urine protein, and medication review are common next checks.");
            }
            if (ContainsAny(prompt, "cholesterol") && number.HasValue)
            {
                interpretation.Add(number > 200 ? $"Cholesterol {number:0.#} is above the usual desirable range and should be flagged as high." : $"Cholesterol {number:0.#} is not clearly high from the supplied threshold.");
                panels.Add("A full lipid profile and cardiovascular risk review commonly follow.");
            }

            model.DetectedSignals.AddRange(CollectSignals(prompt, "glucose", "hemoglobin", "hb", "creatinine", "cholesterol", "fatigue"));
            if (number.HasValue)
                model.DetectedSignals.Add($"Value {number:0.#}");

            if (interpretation.Count == 0)
                interpretation.Add("No direct lab marker rule matched. The assistant can still summarize the result wording and suggest whether clinician review should be expedited.");
            if (panels.Count == 0)
                panels.Add("Suggested next panels depend on the dominant organ system, symptoms, and previous abnormal results.");

            model.ResponseStatusLabel = interpretation.Any(i => i.Contains("elevated", StringComparison.OrdinalIgnoreCase) || i.Contains("anemia", StringComparison.OrdinalIgnoreCase) || i.Contains("high", StringComparison.OrdinalIgnoreCase))
                ? "Abnormal flag"
                : "Needs review";
            model.ResponseStatusTone = model.ResponseStatusLabel == "Abnormal flag" ? "warning" : "info";
            model.ResponseFacts.AddRange(new[]
            {
                Fact("Detected signals", model.DetectedSignals.Count == 0 ? "General result query" : string.Join(", ", model.DetectedSignals)),
                Fact("Interpretation items", interpretation.Count),
                Fact("Follow-up panels", panels.Distinct().Count())
            });
            model.ResponseTitle = "Lab interpretation generated";
            model.ResponseSummary = "The assistant reviewed the supplied marker text and suggested flagging plus follow-up panels.";
            model.Sections.Add(Section("Interpretation", interpretation));
            model.Sections.Add(Section("Follow-up panel suggestions", panels.Distinct().ToList()));
            return model;
        }

        private AiAssistantPageViewModel BuildReceptionist(string prompt)
        {
            var recentAppointments = _db.Appointments
                .Where(a => a.AppointmentDate >= DateTime.Today.AddDays(-7))
                .OrderByDescending(a => a.AppointmentDate)
                .Take(250)
                .ToList();

            var todayCount = recentAppointments.Count(a => a.AppointmentDate.Date == DateTime.Today);
            var openBookings = recentAppointments.Count(a => a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed);

            var model = CreateBaseModel(
                AiAssistantRole.Receptionist,
                "Receptionist",
                "AI Triage Assistant",
                "Front-desk triage support for symptom urgency, booking priority, and escalation messaging before the patient reaches clinical staff.",
                "Describe symptoms in plain language. Example: chest pain and shortness of breath for 20 minutes.",
                "Triage request",
                "Front-desk triage is not a diagnosis. Send emergency symptoms for immediate clinical or emergency response.");

            model.Capabilities.AddRange(new[]
            {
                "Symptom triage into emergency, urgent, or routine",
                "Booking priority guidance",
                "Escalation language for front-desk staff",
                "Reception chat support"
            });

            model.SuggestedPrompts.AddRange(new[]
            {
                "Chest pain and sweating for 20 minutes.",
                "High fever, vomiting, and weakness since yesterday.",
                "Mild rash for 1 week and wants the next available appointment."
            });

            model.Metrics.AddRange(new[]
            {
                Metric("Today's bookings", todayCount, "fa-calendar-check", "blue"),
                Metric("Open appointment load", openBookings, "fa-user-clock", "orange")
            });

            model.LiveInsights.Add($"Reception currently has {todayCount} appointments today and {openBookings} open scheduled or confirmed bookings in the recent queue.");
            model.LiveInsights.Add("Use the assistant to turn symptom descriptions into booking urgency and escalation guidance.");

            if (string.IsNullOrWhiteSpace(prompt))
            {
                model.ResponseStatusLabel = "Ready";
                model.ResponseStatusTone = "info";
                model.ResponseFacts.AddRange(new[]
                {
                    Fact("Mode", "Front desk triage"),
                    Fact("Today's bookings", todayCount),
                    Fact("Open bookings", openBookings)
                });
                model.ResponseTitle = "Triage snapshot";
                model.ResponseSummary = "The assistant is ready to classify symptom urgency for front-desk handling.";
                model.Sections.Add(Section("Triage logic", new[]
                {
                    "Emergency for chest pain, severe breathing trouble, stroke signs, heavy bleeding, seizures, or loss of consciousness.",
                    "Urgent for high fever, severe pain, dehydration, worsening symptoms, or same-day clinician review needs.",
                    "Routine for mild stable symptoms without clear danger language."
                }));
                return model;
            }

            var level = "Routine";
            var actions = new List<string>();

            if (ContainsAny(prompt, "chest pain", "shortness of breath", "stroke", "unconscious", "seizure", "severe bleeding"))
            {
                level = "Emergency";
                actions.Add("Do not wait for a routine slot. Escalate immediately to emergency response or onsite clinical staff.");
                actions.Add("Keep communication short and direct while arranging urgent help.");
            }
            else if (ContainsAny(prompt, "high fever", "vomiting", "dehydration", "severe pain", "pregnant", "worsening", "urgent"))
            {
                level = "Urgent";
                actions.Add("Book same-day or earliest available clinician review and alert nursing or doctor staff if symptoms worsen.");
                actions.Add("Advise the patient not to delay care if new emergency symptoms appear.");
            }
            else
            {
                actions.Add("A routine appointment appears reasonable if symptoms are stable and no danger language is present.");
                actions.Add("Offer the next suitable slot and give return advice if symptoms worsen before the visit.");
            }

            model.DetectedSignals.AddRange(CollectSignals(prompt, "chest pain", "shortness of breath", "stroke", "unconscious", "seizure", "severe bleeding", "high fever", "vomiting", "dehydration", "severe pain", "pregnant", "worsening"));
            model.ResponseStatusLabel = level;
            model.ResponseStatusTone = level == "Emergency" ? "danger" : level == "Urgent" ? "warning" : "success";
            model.ResponseFacts.AddRange(new[]
            {
                Fact("Detected signals", model.DetectedSignals.Count == 0 ? "General symptom query" : string.Join(", ", model.DetectedSignals)),
                Fact("Booking target", level == "Emergency" ? "Immediate escalation" : level == "Urgent" ? "Same day" : "Next routine slot"),
                Fact("Front desk action count", actions.Count)
            });
            model.ResponseTitle = $"Triage classification: {level}";
            model.ResponseSummary = "The assistant translated the symptom wording into a front-desk urgency level and next actions.";
            model.Sections.Add(Section("Recommended action", actions));
            return model;
        }

        private AiAssistantPageViewModel BuildAdmin(string prompt)
        {
            var appointments = _db.Appointments.ToList();
            var labs = _db.Labs.ToList();
            var bills = _db.Bills.ToList();
            var rooms = _db.Rooms.ToList();
            var users = _db.Users.ToList();

            var revenue = bills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.TotalBill);
            var pendingLabs = labs.Count(l => l.Status != LabTestStatus.Completed && l.Status != LabTestStatus.Cancelled);
            var occupancy = rooms.Count == 0 ? 0 : rooms.Count(r => r.Status != 0) * 100 / rooms.Count;

            var model = CreateBaseModel(
                AiAssistantRole.Admin,
                "Admin",
                "Hospital AI Analytics",
                "Live hospital analytics from the real database with operational summaries, bottleneck detection, and admin chat support.",
                "Ask about revenue, patient flow, lab bottlenecks, staffing load, or hospital trends.",
                "Analyze hospital data",
                "Analytics reflect live application data but should still be reviewed before operational or financial decisions.");

            model.Capabilities.AddRange(new[]
            {
                "Live hospital stats analysis from real DB data",
                "Operational bottleneck summaries",
                "Financial and billing trend prompts",
                "Admin chat support"
            });

            model.SuggestedPrompts.AddRange(new[]
            {
                "Where are the current operational bottlenecks?",
                "Summarize patient flow and pending lab pressure.",
                "What should admins focus on today based on revenue and occupancy?"
            });

            model.Metrics.AddRange(new[]
            {
                Metric("Total users", users.Count, "fa-users", "blue"),
                Metric("Appointments", appointments.Count, "fa-calendar-days", "green"),
                Metric("Pending labs", pendingLabs, "fa-flask", "orange"),
                Metric("Paid revenue", revenue.ToString("C0"), "fa-money-bill-wave", "teal"),
                Metric("Room occupancy", $"{occupancy}%", "fa-bed", "orange")
            });

            model.LiveInsights.Add($"Live DB snapshot: {appointments.Count} appointments, {pendingLabs} pending labs, {bills.Count} bills, and {rooms.Count} rooms.");
            model.LiveInsights.Add($"Current paid revenue captured in-app: {revenue:C0}. Estimated occupancy: {occupancy}%.");

            var bottlenecks = new List<string>();
            if (pendingLabs > 5)
                bottlenecks.Add("Pending lab volume is elevated and may slow discharge or consultation decisions.");
            if (occupancy >= 80)
                bottlenecks.Add("Room occupancy is high, which may constrain admissions and transfers.");
            if (appointments.Count(a => a.Status == AppointmentStatus.Cancelled || a.Status == AppointmentStatus.NoShow) > Math.Max(3, appointments.Count / 5))
                bottlenecks.Add("Cancellation and no-show volume is noticeable and may be affecting clinic efficiency.");
            if (bottlenecks.Count == 0)
                bottlenecks.Add("No major operational bottleneck stands out from the current DB snapshot.");

            var focus = new List<string>
            {
                $"Monitor pending labs ({pendingLabs}) against clinician demand.",
                $"Review room utilization at {occupancy}% occupancy for capacity planning.",
                $"Track paid revenue of {revenue:C0} against pending and overdue billing queues."
            };

            model.ResponseTitle = "Hospital analytics overview";
            model.ResponseSummary = string.IsNullOrWhiteSpace(prompt)
                ? "The assistant summarized current hospital pressure points directly from the live database."
                : "The assistant used live database counts to answer the operational analytics request.";
            model.ResponseStatusLabel = bottlenecks.Count > 1 ? "Operational pressure" : "Stable snapshot";
            model.ResponseStatusTone = bottlenecks.Count > 1 ? "warning" : "info";
            model.ResponseFacts.AddRange(new[]
            {
                Fact("Pending labs", pendingLabs),
                Fact("Occupancy", $"{occupancy}%"),
                Fact("Revenue", revenue.ToString("C0"))
            });
            model.DetectedSignals.AddRange(CollectSignals(prompt, "revenue", "patient flow", "labs", "staffing", "occupancy", "billing", "bottlenecks"));
            model.Sections.Add(Section("Detected bottlenecks", bottlenecks));
            model.Sections.Add(Section("Recommended admin focus", focus));
            return model;
        }

        private AiAssistantPageViewModel BuildPatient(string userId, string? userName, string prompt)
        {
            var labs = _db.Labs
                .Where(l => l.PatientId == userId)
                .OrderByDescending(l => l.CreatedDate)
                .Take(25)
                .ToList();
            var reports = _db.PatientReports
                .Where(r => r.PatientId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .Take(25)
                .ToList();
            var docs = _db.PatientDocuments
                .Where(d => d.PatientId == userId)
                .OrderByDescending(d => d.UploadedDate)
                .Take(25)
                .ToList();

            var model = CreateBaseModel(
                AiAssistantRole.Patient,
                "Patient",
                "Health AI Assistant",
                $"Health guidance for {FriendlyName(userName)} with symptom Q&A, recent record awareness, and document-oriented support.",
                "Describe symptoms or ask about a recent report or uploaded document.",
                "Get guidance",
                "This assistant gives general health guidance only and is not a substitute for a clinician or emergency care.");

            model.Capabilities.AddRange(new[]
            {
                "Health Q and A in plain language",
                "Symptom guidance and next-step suggestions",
                "Recent lab and report awareness",
                "Document analysis support from uploaded file metadata and user-provided summaries",
                "Patient chat"
            });

            model.SuggestedPrompts.AddRange(new[]
            {
                "I have cough and fever for 2 days. When should I seek urgent care?",
                "Explain my recent lab result showing glucose 168 in simple language.",
                "I uploaded a discharge summary. What questions should I ask at follow-up?"
            });

            model.Metrics.AddRange(new[]
            {
                Metric("Recent labs", labs.Count, "fa-flask", "blue"),
                Metric("Reports", reports.Count, "fa-file-medical", "green"),
                Metric("Documents", docs.Count, "fa-folder-open", "orange")
            });

            model.LiveInsights.Add($"Your portal currently has {labs.Count} recent labs, {reports.Count} reports, and {docs.Count} uploaded documents.");
            if (docs.Count > 0)
                model.LiveInsights.Add($"Latest uploaded file: {docs.First().OriginalFileName} ({docs.First().DocumentType}).");
            model.LiveInsights.Add("Ask in everyday language and the assistant will turn that into next-step guidance.");

            if (string.IsNullOrWhiteSpace(prompt))
            {
                model.ResponseStatusLabel = "Ready";
                model.ResponseStatusTone = "info";
                model.ResponseFacts.AddRange(new[]
                {
                    Fact("Mode", "Patient guidance"),
                    Fact("Recent records", $"{labs.Count} labs, {reports.Count} reports"),
                    Fact("Uploaded documents", docs.Count)
                });
                model.ResponseTitle = "Personal health snapshot";
                model.ResponseSummary = "The assistant is ready to answer symptom questions and help you understand portal records.";
                model.Sections.Add(Section("What it can help with", new[]
                {
                    "Explain recent labs or reports in simpler language.",
                    "Suggest whether symptoms sound routine, urgent, or emergency-level.",
                    "Help prepare follow-up questions after a document upload or clinic visit."
                }));
                return model;
            }

            var guidance = new List<string>();
            if (ContainsAny(prompt, "chest pain", "shortness of breath", "stroke", "unconscious", "seizure"))
            {
                guidance.Add("These symptoms can be emergencies. Seek immediate emergency care rather than waiting for a clinic reply.");
            }
            else if (ContainsAny(prompt, "fever", "vomiting", "dehydration", "worsening", "severe pain"))
            {
                guidance.Add("These symptoms may need urgent same-day clinical review, especially if they are worsening or hard to manage at home.");
            }
            else
            {
                guidance.Add("The symptom wording sounds less urgent, but monitor closely and book a routine review if symptoms persist.");
            }

            if (ContainsAny(prompt, "document", "summary", "report", "lab"))
                guidance.Add("Bring the document, symptom timeline, current medicines, and any questions about follow-up tests to your next appointment.");

            guidance.Add("If new red-flag symptoms appear, move from routine guidance to urgent or emergency care immediately.");

            model.DetectedSignals.AddRange(CollectSignals(prompt, "chest pain", "shortness of breath", "stroke", "unconscious", "seizure", "fever", "vomiting", "dehydration", "worsening", "severe pain", "document", "summary", "report", "lab"));
            var patientLevel = guidance.Any(g => g.Contains("emergencies", StringComparison.OrdinalIgnoreCase)) ? "Emergency advice" :
                guidance.Any(g => g.Contains("urgent", StringComparison.OrdinalIgnoreCase)) ? "Urgent advice" : "Routine advice";
            model.ResponseStatusLabel = patientLevel;
            model.ResponseStatusTone = patientLevel == "Emergency advice" ? "danger" : patientLevel == "Urgent advice" ? "warning" : "success";
            model.ResponseFacts.AddRange(new[]
            {
                Fact("Detected signals", model.DetectedSignals.Count == 0 ? "General health question" : string.Join(", ", model.DetectedSignals)),
                Fact("Recent portal context", $"{labs.Count} labs, {reports.Count} reports, {docs.Count} documents"),
                Fact("Next-step items", guidance.Count)
            });
            model.ResponseTitle = "Health guidance generated";
            model.ResponseSummary = "The assistant translated your question into plain-language next steps and follow-up advice.";
            model.Sections.Add(Section("Suggested next steps", guidance));
            return model;
        }

        private static AiAssistantPageViewModel CreateBaseModel(
            AiAssistantRole role,
            string areaName,
            string sidebarTitle,
            string subtitle,
            string placeholder,
            string submitLabel,
            string disclaimer)
        {
            return new AiAssistantPageViewModel
            {
                Role = role,
                AreaName = areaName,
                PageTitle = sidebarTitle,
                SidebarTitle = sidebarTitle,
                Subtitle = subtitle,
                PromptPlaceholder = placeholder,
                SubmitLabel = submitLabel,
                Disclaimer = disclaimer
            };
        }

        private static AiAssistantMetricViewModel Metric(string label, object value, string icon, string tone)
        {
            return new AiAssistantMetricViewModel
            {
                Label = label,
                Value = value.ToString() ?? "0",
                Icon = icon,
                Tone = tone
            };
        }

        private static AiAssistantFactViewModel Fact(string label, object value)
        {
            return new AiAssistantFactViewModel
            {
                Label = label,
                Value = value.ToString() ?? ""
            };
        }

        private static AiAssistantSectionViewModel Section(string title, IEnumerable<string> items)
        {
            return new AiAssistantSectionViewModel
            {
                Title = title,
                Items = items.Where(i => !string.IsNullOrWhiteSpace(i)).Distinct().ToList()
            };
        }

        private static bool ContainsAny(string text, params string[] tokens)
        {
            return tokens.Any(token => text.Contains(token, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<string> CollectSignals(string text, params string[] tokens)
        {
            return tokens
                .Where(token => text.Contains(token, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static (int? systolic, int? diastolic) ExtractBloodPressure(string text)
        {
            var match = Regex.Match(text, @"(?<sys>\d{2,3})\s*/\s*(?<dia>\d{2,3})");
            if (!match.Success)
                return (null, null);

            return (int.Parse(match.Groups["sys"].Value), int.Parse(match.Groups["dia"].Value));
        }

        private static double? ExtractTemperature(string text)
        {
            var match = Regex.Match(text, @"(?:(?:temp|temperature)\s*)?(?<temp>\d{2,3}(?:\.\d+)?)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            return double.TryParse(match.Groups["temp"].Value, out var value) ? value : null;
        }

        private static double? ExtractFirstNumber(string text)
        {
            var match = Regex.Match(text, @"\d+(?:\.\d+)?");
            if (!match.Success)
                return null;

            return double.TryParse(match.Value, out var value) ? value : null;
        }

        private static string FriendlyName(string? userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "your team";

            return userName.Contains('@')
                ? userName.Split('@')[0]
                : userName;
        }
    }
}
