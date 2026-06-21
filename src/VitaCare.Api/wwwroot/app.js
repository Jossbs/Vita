const apiUrl = "/api/patients";

const careLabels = {
  Basic: "Básico",
  Moderate: "Moderado",
  High: "Alto",
  Intensive: "Intensivo"
};

const mobilityLabels = {
  Independent: "Independiente",
  NeedsAssistance: "Requiere asistencia",
  UsesWheelchair: "Usa silla de ruedas",
  BedBound: "Permanece en cama"
};

// Fields counted toward the persuasive completion meter.
const trackedFields = [
  "fullName",
  "preferredName",
  "birthDate",
  "bloodType",
  "primaryCondition",
  "allergies",
  "currentMedications",
  "careInstructions",
  "medicalNotes",
  "emergencyContactName",
  "emergencyContactPhone",
  "emergencyContactRelationship"
];

const totalSteps = 5;

const state = {
  patients: [],
  selectedId: null,
  searchTerm: "",
  currentStep: 0
};

const elements = {
  status: document.querySelector("#systemStatus"),
  list: document.querySelector("#patientList"),
  form: document.querySelector("#patientForm"),
  patientId: document.querySelector("#patientId"),
  search: document.querySelector("#searchInput"),
  refresh: document.querySelector("#refreshButton"),
  createNew: document.querySelector("#newPatientButton"),
  clear: document.querySelector("#clearButton"),
  delete: document.querySelector("#deleteButton"),
  message: document.querySelector("#messageArea"),
  formMode: document.querySelector("#formModeLabel"),
  formTitle: document.querySelector("#formTitle"),
  formLead: document.querySelector("#formLead"),
  totalPatients: document.querySelector("#totalPatients"),
  highCarePatients: document.querySelector("#highCarePatients"),
  supervisionPatients: document.querySelector("#supervisionPatients"),
  stepper: document.querySelector("#formStepper"),
  steps: Array.from(document.querySelectorAll(".form-step")),
  stepButtons: Array.from(document.querySelectorAll(".form-stepper .step")),
  prevStep: document.querySelector("#prevStepButton"),
  nextStep: document.querySelector("#nextStepButton"),
  submit: document.querySelector("#submitButton"),
  progressValue: document.querySelector("#progressValue"),
  progressFill: document.querySelector("#progressFill"),
  progressHint: document.querySelector("#progressHint"),
  review: document.querySelector("#reviewSummary")
};

document.addEventListener("DOMContentLoaded", () => {
  elements.form.addEventListener("submit", handleSubmit);
  elements.form.addEventListener("input", handleFormInput);
  elements.search.addEventListener("input", handleSearch);
  elements.refresh.addEventListener("click", loadPatients);
  elements.createNew.addEventListener("click", resetForm);
  elements.clear.addEventListener("click", resetForm);
  elements.delete.addEventListener("click", deleteSelectedPatient);
  elements.prevStep.addEventListener("click", () => goToStep(state.currentStep - 1));
  elements.nextStep.addEventListener("click", handleNextStep);

  for (const button of elements.stepButtons) {
    button.addEventListener("click", () => goToStep(Number(button.dataset.step)));
  }

  // Validate required fields as the user leaves them.
  for (const name of ["fullName", "birthDate"]) {
    const field = elements.form.elements[name];
    field.addEventListener("blur", () => validateField(name));
  }

  goToStep(0);
  updateProgress();
  checkHealth();
  loadPatients();
});

async function checkHealth() {
  try {
    const response = await fetch("/health");
    if (!response.ok) {
      throw new Error("Health check failed");
    }

    elements.status.classList.remove("is-offline");
    elements.status.classList.add("is-online");
    elements.status.lastChild.textContent = " Servicio disponible";
  } catch {
    elements.status.classList.remove("is-online");
    elements.status.classList.add("is-offline");
    elements.status.lastChild.textContent = " Servicio no disponible";
  }
}

async function loadPatients() {
  showMessage("Cargando pacientes...");

  try {
    const response = await fetch(apiUrl);
    if (!response.ok) {
      throw await readProblem(response);
    }

    state.patients = await response.json();
    renderSummary();
    renderPatientList();
    hideMessage();
  } catch (error) {
    showMessage(error.message || "No se pudieron cargar los pacientes.", true);
  }
}

function renderPatientList() {
  elements.list.replaceChildren();

  const filteredPatients = state.patients.filter(patient => {
    const text = [
      patient.fullName,
      patient.preferredName,
      patient.primaryCondition,
      patient.emergencyContactName
    ].filter(Boolean).join(" ").toLowerCase();

    return text.includes(state.searchTerm);
  });

  if (filteredPatients.length === 0) {
    const empty = document.createElement("div");
    empty.className = "empty-state";
    empty.textContent = state.searchTerm
      ? "No hay pacientes que coincidan con la búsqueda."
      : "Todavía no hay pacientes registrados.";
    elements.list.append(empty);
    return;
  }

  for (const patient of filteredPatients) {
    elements.list.append(createPatientRow(patient));
  }
}

function renderSummary() {
  const highCarePatients = state.patients.filter(patient =>
    patient.careLevel === "High" || patient.careLevel === "Intensive").length;
  const supervisionPatients = state.patients.filter(patient =>
    patient.requiresContinuousSupervision).length;

  elements.totalPatients.textContent = state.patients.length;
  elements.highCarePatients.textContent = highCarePatients;
  elements.supervisionPatients.textContent = supervisionPatients;
}

function createPatientRow(patient) {
  const row = document.createElement("button");
  row.type = "button";
  row.className = "patient-row";
  row.dataset.id = patient.id;
  row.setAttribute("aria-label", `Editar ${patient.fullName}`);

  if (patient.id === state.selectedId) {
    row.classList.add("is-selected");
  }

  const summary = document.createElement("span");
  const name = document.createElement("strong");
  name.textContent = patient.fullName;

  const detail = document.createElement("span");
  const condition = patient.primaryCondition || "Sin condición principal";
  detail.textContent = `${condition} · ${mobilityLabels[patient.mobilityStatus] || patient.mobilityStatus}`;

  const contact = document.createElement("span");
  contact.textContent = patient.emergencyContactName
    ? `Contacto: ${patient.emergencyContactName}`
    : "Sin contacto de emergencia";

  summary.append(name, detail, contact);

  const badge = document.createElement("span");
  badge.className = "care-badge";
  if (patient.careLevel === "High" || patient.careLevel === "Intensive") {
    badge.classList.add(patient.careLevel === "High" ? "is-high" : "is-intensive");
  }
  badge.textContent = careLabels[patient.careLevel] || patient.careLevel;

  row.append(summary, badge);
  row.addEventListener("click", () => selectPatient(patient.id));
  return row;
}

function selectPatient(id) {
  const patient = state.patients.find(item => item.id === id);
  if (!patient) {
    return;
  }

  state.selectedId = id;
  elements.formMode.textContent = "Editando paciente";
  elements.formTitle.textContent = patient.fullName;
  elements.formLead.textContent = "Actualiza la información para mantener el cuidado al día.";
  elements.delete.hidden = false;
  fillForm(patient);
  clearAllFieldErrors();
  goToStep(0);
  updateProgress();
  renderPatientList();
  hideMessage();
}

function fillForm(patient) {
  elements.patientId.value = patient.id;
  setField("fullName", patient.fullName);
  setField("preferredName", patient.preferredName);
  setField("birthDate", patient.birthDate?.slice(0, 10));
  setField("bloodType", patient.bloodType);
  setField("careLevel", patient.careLevel);
  setField("mobilityStatus", patient.mobilityStatus);
  setField("primaryCondition", patient.primaryCondition);
  setField("allergies", patient.allergies);
  setField("currentMedications", patient.currentMedications);
  setField("careInstructions", patient.careInstructions);
  setField("emergencyContactName", patient.emergencyContactName);
  setField("emergencyContactPhone", patient.emergencyContactPhone);
  setField("emergencyContactRelationship", patient.emergencyContactRelationship);
  setField("medicalNotes", patient.medicalNotes);
  setField("requiresContinuousSupervision", patient.requiresContinuousSupervision);
  setField("requiresMedicationAssistance", patient.requiresMedicationAssistance);
  setField("requiresFeedingAssistance", patient.requiresFeedingAssistance);
}

function setField(name, value) {
  const field = elements.form.elements[name];
  if (!field) {
    return;
  }

  if (field.type === "checkbox") {
    field.checked = Boolean(value);
    return;
  }

  field.value = value || "";
}

// --- Stepper navigation -------------------------------------------------

function goToStep(index) {
  const target = Math.max(0, Math.min(totalSteps - 1, index));
  state.currentStep = target;

  elements.steps.forEach((step, position) => {
    step.classList.toggle("is-active", position === target);
  });

  elements.stepButtons.forEach((button, position) => {
    button.classList.toggle("is-active", position === target);
    button.classList.toggle("is-complete", position < target);
    if (position === target) {
      button.setAttribute("aria-current", "step");
    } else {
      button.removeAttribute("aria-current");
    }
  });

  const isLastStep = target === totalSteps - 1;
  elements.prevStep.hidden = target === 0;
  elements.nextStep.hidden = isLastStep;
  elements.submit.hidden = !isLastStep;

  if (isLastStep) {
    renderReview();
  }
}

function handleNextStep() {
  if (!validateStep(state.currentStep)) {
    return;
  }

  goToStep(state.currentStep + 1);
}

function validateStep(index) {
  if (index === 0) {
    const validName = validateField("fullName");
    const validBirth = validateField("birthDate");
    return validName && validBirth;
  }

  return true;
}

function validateField(name) {
  const field = elements.form.elements[name];
  const value = String(field.value || "").trim();
  let error = "";

  if (name === "fullName") {
    if (value.length === 0) {
      error = "El nombre completo es obligatorio.";
    } else if (value.length < 2) {
      error = "El nombre completo debe tener al menos 2 caracteres.";
    }
  }

  if (name === "birthDate") {
    if (value.length === 0) {
      error = "La fecha de nacimiento es obligatoria.";
    } else if (value > todayIso()) {
      error = "La fecha de nacimiento no puede estar en el futuro.";
    }
  }

  setFieldError(name, error);
  return error.length === 0;
}

function setFieldError(name, error) {
  const message = elements.form.querySelector(`.field-message[data-for="${name}"]`);
  const label = elements.form.elements[name].closest("label");

  if (message) {
    message.textContent = error;
  }

  if (label) {
    label.classList.toggle("has-error", error.length > 0);
  }
}

function clearAllFieldErrors() {
  for (const message of elements.form.querySelectorAll(".field-message")) {
    message.textContent = "";
  }
  for (const label of elements.form.querySelectorAll("label.has-error")) {
    label.classList.remove("has-error");
  }
}

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

// --- Progress meter -----------------------------------------------------

function handleFormInput(event) {
  updateProgress();

  const name = event.target.name;
  if ((name === "fullName" || name === "birthDate") && event.target.closest("label").classList.contains("has-error")) {
    validateField(name);
  }
}

function updateProgress() {
  const filled = trackedFields.filter(name => {
    const field = elements.form.elements[name];
    return field && String(field.value || "").trim().length > 0;
  }).length;

  const percent = Math.round((filled / trackedFields.length) * 100);
  elements.progressValue.textContent = percent;
  elements.progressFill.style.width = `${percent}%`;
  elements.progressHint.textContent = progressHintFor(percent);
}

function progressHintFor(percent) {
  if (percent === 0) {
    return "Empieza por lo esencial: ¿a quién vamos a cuidar?";
  }
  if (percent < 40) {
    return "Buen comienzo. Cada dato suma a un cuidado más seguro.";
  }
  if (percent < 70) {
    return "Vas muy bien. La información clínica marca la diferencia en una emergencia.";
  }
  if (percent < 100) {
    return "Casi listo. Un contacto de emergencia completa el perfil.";
  }
  return "Perfil completo. ¡Excelente trabajo!";
}

// --- Review step --------------------------------------------------------

function renderReview() {
  const payload = readFormPayload();
  const rows = [
    ["Nombre completo", payload.fullName],
    ["Nombre preferido", payload.preferredName],
    ["Fecha de nacimiento", formatDate(payload.birthDate)],
    ["Tipo de sangre", payload.bloodType],
    ["Nivel de cuidado", careLabels[payload.careLevel]],
    ["Movilidad", mobilityLabels[payload.mobilityStatus]],
    ["Apoyos", supportSummary(payload)],
    ["Condición principal", payload.primaryCondition],
    ["Alergias", payload.allergies],
    ["Medicamentos actuales", payload.currentMedications],
    ["Instrucciones de cuidado", payload.careInstructions],
    ["Notas médicas", payload.medicalNotes],
    ["Contacto de emergencia", contactSummary(payload)]
  ];

  elements.review.replaceChildren();

  for (const [label, value] of rows) {
    const item = document.createElement("div");
    item.className = "review-item";

    const term = document.createElement("span");
    term.className = "review-term";
    term.textContent = label;

    const detail = document.createElement("span");
    detail.className = "review-value";
    if (value) {
      detail.textContent = value;
    } else {
      detail.textContent = "Sin registrar";
      detail.classList.add("is-empty");
    }

    item.append(term, detail);
    elements.review.append(item);
  }
}

function supportSummary(payload) {
  const supports = [];
  if (payload.requiresContinuousSupervision) supports.push("Supervisión continua");
  if (payload.requiresMedicationAssistance) supports.push("Ayuda con medicamentos");
  if (payload.requiresFeedingAssistance) supports.push("Ayuda con alimentación");
  return supports.join(", ");
}

function contactSummary(payload) {
  if (!payload.emergencyContactName) {
    return "";
  }

  const parts = [payload.emergencyContactName];
  if (payload.emergencyContactRelationship) parts.push(`(${payload.emergencyContactRelationship})`);
  if (payload.emergencyContactPhone) parts.push(`· ${payload.emergencyContactPhone}`);
  return parts.join(" ");
}

function formatDate(value) {
  if (!value) {
    return "";
  }

  const date = new Date(`${value}T00:00:00`);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleDateString("es", { day: "2-digit", month: "long", year: "numeric" });
}

// --- Persistence --------------------------------------------------------

async function handleSubmit(event) {
  event.preventDefault();
  hideMessage();

  if (!validateStep(0)) {
    goToStep(0);
    showMessage("Revisa los datos de identificación antes de guardar.", true);
    return;
  }

  const payload = readFormPayload();
  const isUpdate = Boolean(state.selectedId);
  const url = isUpdate ? `${apiUrl}/${state.selectedId}` : apiUrl;
  const method = isUpdate ? "PUT" : "POST";

  try {
    const response = await fetch(url, {
      method,
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      throw await readProblem(response);
    }

    const savedPatient = await response.json();
    await loadPatients();
    selectPatient(savedPatient.id);
    showMessage(isUpdate ? "Paciente actualizado." : "Paciente registrado.");
  } catch (error) {
    showMessage(error.message || "No se pudo guardar el paciente.", true);
  }
}

function readFormPayload() {
  const formData = new FormData(elements.form);

  return {
    fullName: valueOrNull(formData.get("fullName")),
    preferredName: valueOrNull(formData.get("preferredName")),
    birthDate: valueOrNull(formData.get("birthDate")),
    bloodType: valueOrNull(formData.get("bloodType")),
    careLevel: formData.get("careLevel"),
    mobilityStatus: formData.get("mobilityStatus"),
    requiresContinuousSupervision: formData.has("requiresContinuousSupervision"),
    requiresMedicationAssistance: formData.has("requiresMedicationAssistance"),
    requiresFeedingAssistance: formData.has("requiresFeedingAssistance"),
    primaryCondition: valueOrNull(formData.get("primaryCondition")),
    allergies: valueOrNull(formData.get("allergies")),
    currentMedications: valueOrNull(formData.get("currentMedications")),
    careInstructions: valueOrNull(formData.get("careInstructions")),
    emergencyContactName: valueOrNull(formData.get("emergencyContactName")),
    emergencyContactPhone: valueOrNull(formData.get("emergencyContactPhone")),
    emergencyContactRelationship: valueOrNull(formData.get("emergencyContactRelationship")),
    medicalNotes: valueOrNull(formData.get("medicalNotes"))
  };
}

function valueOrNull(value) {
  const normalized = String(value || "").trim();
  return normalized.length === 0 ? null : normalized;
}

async function deleteSelectedPatient() {
  if (!state.selectedId) {
    return;
  }

  const selected = state.patients.find(patient => patient.id === state.selectedId);
  const shouldDelete = confirm(`¿Eliminar el registro de ${selected?.fullName || "este paciente"}?`);

  if (!shouldDelete) {
    return;
  }

  try {
    const response = await fetch(`${apiUrl}/${state.selectedId}`, { method: "DELETE" });
    if (!response.ok) {
      throw await readProblem(response);
    }

    resetForm();
    await loadPatients();
    showMessage("Paciente eliminado.");
  } catch (error) {
    showMessage(error.message || "No se pudo eliminar el paciente.", true);
  }
}

function handleSearch(event) {
  state.searchTerm = event.target.value.trim().toLowerCase();
  renderPatientList();
}

function resetForm() {
  state.selectedId = null;
  elements.form.reset();
  elements.patientId.value = "";
  elements.formMode.textContent = "Nuevo registro";
  elements.formTitle.textContent = "Datos del paciente";
  elements.formLead.textContent = "Cada dato que registras ayuda a brindar un cuidado más seguro y humano.";
  elements.delete.hidden = true;
  clearAllFieldErrors();
  goToStep(0);
  updateProgress();
  renderPatientList();
  hideMessage();
}

async function readProblem(response) {
  let problem;

  try {
    problem = await response.json();
  } catch {
    return new Error("Ocurrió un error inesperado.");
  }

  if (problem.errors) {
    const messages = Object.values(problem.errors).flat();
    return new Error(messages.join(" "));
  }

  return new Error(problem.detail || problem.title || "Ocurrió un error inesperado.");
}

function showMessage(text, isError = false) {
  elements.message.hidden = false;
  elements.message.textContent = text;
  elements.message.classList.toggle("is-error", isError);
}

function hideMessage() {
  elements.message.hidden = true;
  elements.message.textContent = "";
  elements.message.classList.remove("is-error");
}
