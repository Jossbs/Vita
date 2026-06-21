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

// Required fields validated in the browser before saving.
const requiredFields = ["firstName", "lastName", "birthDate"];

// Fields counted toward the persuasive completion meter.
const trackedFields = [
  "firstName",
  "lastName",
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

const state = {
  patients: [],
  selectedId: null,
  searchTerm: ""
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
  sections: Array.from(document.querySelectorAll(".form-section")),
  progressValue: document.querySelector("#progressValue"),
  progressFill: document.querySelector("#progressFill"),
  progressHint: document.querySelector("#progressHint")
};

document.addEventListener("DOMContentLoaded", () => {
  elements.form.addEventListener("submit", handleSubmit);
  elements.form.addEventListener("input", handleFormInput);
  elements.search.addEventListener("input", handleSearch);
  elements.refresh.addEventListener("click", loadPatients);
  elements.createNew.addEventListener("click", resetForm);
  elements.clear.addEventListener("click", resetForm);
  elements.delete.addEventListener("click", deleteSelectedPatient);

  // Validate required fields as the user leaves them.
  for (const name of requiredFields) {
    elements.form.elements[name].addEventListener("blur", () => validateField(name));
  }

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
  resetSections();
  updateProgress();
  renderPatientList();
  hideMessage();
}

function fillForm(patient) {
  elements.patientId.value = patient.id;
  setField("firstName", patient.firstName);
  setField("lastName", patient.lastName);
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

// Collapse every section except the first (Identificación).
function resetSections() {
  elements.sections.forEach((section, index) => {
    section.open = index === 0;
  });
}

// --- Validation ---------------------------------------------------------

function validateForm() {
  let firstInvalid = null;

  for (const name of requiredFields) {
    if (!validateField(name) && !firstInvalid) {
      firstInvalid = name;
    }
  }

  if (firstInvalid) {
    const field = elements.form.elements[firstInvalid];
    const section = field.closest(".form-section");
    if (section) {
      section.open = true;
    }
    field.focus();
  }

  return firstInvalid === null;
}

function validateField(name) {
  const field = elements.form.elements[name];
  const value = String(field.value || "").trim();
  let error = "";

  if (name === "firstName") {
    if (value.length === 0) {
      error = "El nombre es obligatorio.";
    } else if (value.length < 2) {
      error = "El nombre debe tener al menos 2 caracteres.";
    }
  }

  if (name === "lastName") {
    if (value.length === 0) {
      error = "Los apellidos son obligatorios.";
    } else if (value.length < 2) {
      error = "Los apellidos deben tener al menos 2 caracteres.";
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
  const label = event.target.closest("label");
  if (requiredFields.includes(name) && label && label.classList.contains("has-error")) {
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

// --- Persistence --------------------------------------------------------

async function handleSubmit(event) {
  event.preventDefault();
  hideMessage();

  if (!validateForm()) {
    showMessage("Revisa los campos obligatorios antes de guardar.", true);
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
    firstName: valueOrNull(formData.get("firstName")),
    lastName: valueOrNull(formData.get("lastName")),
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
  resetSections();
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
