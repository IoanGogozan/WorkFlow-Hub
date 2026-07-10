"use client";

import { useState } from "react";

type CalculatorValues = {
  requestsPerWeek: number;
  minutesPerRequest: number;
  reductionPercentage: number;
};

const defaults: CalculatorValues = {
  requestsPerWeek: 40,
  minutesPerRequest: 15,
  reductionPercentage: 70,
};

const numberFormatter = new Intl.NumberFormat("nb-NO", {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
});

export function TimeSavingsCalculator() {
  const [values, setValues] = useState(defaults);
  const validationError = validate(values);
  const monthlyHours = validationError
    ? null
    : (values.requestsPerWeek *
        4.33 *
        values.minutesPerRequest *
        (values.reductionPercentage / 100)) /
      60;
  const workdays = monthlyHours === null ? null : monthlyHours / 7.5;

  function updateValue(field: keyof CalculatorValues, value: number) {
    setValues((current) => ({ ...current, [field]: value }));
  }

  return (
    <section aria-labelledby="savings-heading" className="py-10 sm:py-14">
      <div className="rounded-xl border border-[#d8dee8] bg-white p-5 shadow-sm sm:p-8">
        <p className="text-sm font-semibold text-[#315ea8]">Eksempelberegning</p>
        <h2
          className="mt-2 text-3xl font-semibold tracking-tight text-[#172033]"
          id="savings-heading"
        >
          Hva kan mindre manuelt arbeid bety?
        </h2>
        <p className="mt-3 max-w-3xl text-base leading-7 text-[#526075]">
          Juster forutsetningene for å se et enkelt estimat. Verdiene lagres ikke
          og sendes ikke til serveren.
        </p>

        <div className="mt-7 grid gap-5 md:grid-cols-3">
          <NumberField
            label="Henvendelser per uke"
            max={10_000}
            min={1}
            onChange={(value) => updateValue("requestsPerWeek", value)}
            value={values.requestsPerWeek}
          />
          <NumberField
            label="Manuelle minutter per henvendelse"
            max={240}
            min={1}
            onChange={(value) => updateValue("minutesPerRequest", value)}
            value={values.minutesPerRequest}
          />
          <NumberField
            label="Estimert reduksjon i prosent"
            max={95}
            min={1}
            onChange={(value) => updateValue("reductionPercentage", value)}
            suffix="%"
            value={values.reductionPercentage}
          />
        </div>

        {validationError ? (
          <p
            className="mt-5 rounded-md border border-[#f3b7b7] bg-[#fff2f2] p-4 text-sm text-[#8f2525]"
            role="alert"
          >
            {validationError}
          </p>
        ) : (
          <div
            aria-atomic="true"
            aria-live="polite"
            className="mt-7 grid gap-4 sm:grid-cols-2"
          >
            <ResultCard
              label="Estimerte timer spart per måned"
              value={`${numberFormatter.format(monthlyHours!)} timer`}
            />
            <ResultCard
              label="Tilsvarer omtrent"
              value={`${numberFormatter.format(workdays!)} arbeidsdager`}
            />
          </div>
        )}

        <p className="mt-6 border-t border-[#e2e6ec] pt-5 text-sm leading-6 text-[#64748b]">
          Eksempelberegning basert på valgte forutsetninger. Faktisk effekt må
          måles i en avgrenset pilot.
        </p>
      </div>
    </section>
  );
}

type NumberFieldProps = {
  label: string;
  min: number;
  max: number;
  value: number;
  suffix?: string;
  onChange: (value: number) => void;
};

function NumberField({
  label,
  min,
  max,
  value,
  suffix,
  onChange,
}: NumberFieldProps) {
  const descriptionId = `calculator-${label.toLowerCase().replace(/[^a-z0-9]+/g, "-")}-help`;

  return (
    <label className="block text-sm font-semibold text-[#344258]">
      {label}
      <span className="relative mt-2 block">
        <input
          aria-describedby={descriptionId}
          className="w-full rounded-md border border-[#c9d2df] bg-white px-3 py-2.5 pr-10 text-base font-medium text-[#172033] focus:border-[#315ea8] focus:outline-none focus:ring-2 focus:ring-[#b8ccea]"
          inputMode="numeric"
          max={max}
          min={min}
          onChange={(event) => onChange(event.currentTarget.valueAsNumber)}
          type="number"
          value={Number.isNaN(value) ? "" : value}
        />
        {suffix ? (
          <span className="pointer-events-none absolute inset-y-0 right-3 flex items-center text-sm text-[#64748b]">
            {suffix}
          </span>
        ) : null}
      </span>
      <span
        className="mt-1.5 block text-xs font-normal text-[#64748b]"
        id={descriptionId}
      >
        Tillatt verdi: {min}–{numberFormatter.format(max).replace(",0", "")}
      </span>
    </label>
  );
}

function ResultCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-[#cddfd3] bg-[#f3faf5] p-5">
      <p className="text-xs font-semibold uppercase tracking-[0.1em] text-[#4f6f5b]">
        {label}
      </p>
      <p className="mt-2 text-3xl font-semibold tracking-tight text-[#24543a]">
        {value}
      </p>
    </div>
  );
}

function validate(values: CalculatorValues) {
  if (!within(values.requestsPerWeek, 1, 10_000)) {
    return "Henvendelser per uke må være mellom 1 og 10 000.";
  }
  if (!within(values.minutesPerRequest, 1, 240)) {
    return "Minutter per henvendelse må være mellom 1 og 240.";
  }
  if (!within(values.reductionPercentage, 1, 95)) {
    return "Estimert reduksjon må være mellom 1 og 95 prosent.";
  }
  return null;
}

function within(value: number, min: number, max: number) {
  return Number.isFinite(value) && value >= min && value <= max;
}
