---
name: prompt-contract-generator
description: 'Generate a reusable implementation prompt contract from a feature interview. Use when users ask to turn a feature idea into an AI coding prompt, prompt contract, structured engineering brief, or implementation specification.'
argument-hint: 'Describe the feature you want to specify'
user-invocable: true
---

# Prompt Contract Generator

Create a stack-aware, implementation-neutral prompt contract that a coding assistant can use to build one focused feature.

## When to Use

- Turn a feature idea into a structured prompt for an AI coding assistant.
- Define implementation constraints, edge cases, and testable acceptance criteria before coding.
- Refine an incomplete feature brief into a small, self-contained contract.

## Interview

1. Read the user's initial description and extract answers already supplied.
2. Ask only unanswered questions below, one at a time, and wait for each answer before asking the next.
   - What does this feature do? Ask for one to three plain-language sentences.
   - Which product roles will use or operate this feature? Ask with role-oriented options such as tenant admin, records manager, and end user.
   - What is the worst thing that could go wrong? Offer examples such as data loss, security breach, silent failure, wrong output, or nothing serious.
3. Do not infer a missing answer when the user has not provided enough information. Ask the corresponding question instead.

## Generate the Contract

After the interview, silently check that every constraint has one concern, acceptance criteria are independently testable, fear-derived risks have edge cases, and implementation choices remain open unless the user supplied them.

Present a Markdown contract of no more than 60 lines with exactly these sections:

## Inputs

- List all context required before implementation: relevant existing files, data schemas, business rules, stack details, user roles, and external-service contracts.
- When the stack is unknown, state stack-agnostic constraints rather than choosing a language, framework, or library.

## Expected Output

- Define deliverables, file types and locations when known, public exports or endpoints, and the expected high-level structure.
- Make the result specific enough that independent coding assistants create compatible shapes without prescribing internal names, helpers, imports, or algorithms.

## Constraints

- State one mandatory or forbidden behavior per bullet.
- Include input validation.
- Include an error-handling approach.
- Require authorization checks when the identified users or operation require them.
- Define external-service timeouts, failure behavior, and response validation when an external service is involved.

## Edge Cases

- Provide at least three `scenario -> expected behavior` entries derived from the user's stated worst-case outcome.
- Cover missing or empty input.
- Cover duplicate or repeated data.
- Cover unauthorized access when authorization applies.

## Acceptance Criteria

- Use yes-or-no, externally verifiable conditions.
- Include a happy-path condition and an error-path condition.
- Add conditions for each high-risk failure mode, authorization rule, and external-service behavior that applies.

## Review Notes

After the contract, explain the technical guardrails included and why, identify the edge cases that stem from the stated fear scenario, and name one useful stack-specific addition.

## Follow-up

Identify the single most ambiguous or weakest contract assumption and ask a focused question to resolve it. Once the user answers, revise the contract and repeat the review notes. When the contract is finalized, summarize its output, provide two example prompts for invoking this skill, and suggest relevant follow-up customizations such as a code-review skill or a testing checklist.

## Save the Contract

- Save the finalized contract under `docs/prompts`.
- If `docs/prompts` does not exist, create it before saving.
- Scan `docs/prompts` for existing numeric prefixes and use the next available three-digit prefix.
- Name the file `docs/prompts/NNN-<short-kebab-case-topic>.md`; for example, `docs/prompts/001-excel-preview-to-csv.md`.
- Do not create a file for an unfinished interview or draft contract.
- After saving, re-read the file and confirm that it contains the finalized contract and all required sections.