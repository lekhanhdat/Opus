---
name: development-document-writer
description: Write development documents from the current plan and the TDD template. Use when the user wants a development document, TDD document, solution design document, or asks to generate a concise English doc for PM or QA from current context or plan.
---

# Development Document Writer

Generate a development document from the current workspace context, the active plan, and the template at `docs/templates/tdd_template.md`.

## Scope

Use this skill when the user wants a development document written for PM, QA, or test developers.

The output must:

- be written in simple, concise English
- focus on scope, behavior, flow, risks, and validation
- avoid code-level details, class names, method names, and internal implementation trivia unless the user explicitly asks for them
- include API design details when API behavior is in scope (endpoint, purpose, input/output summary, and change type)
- include concrete database table design when database schema changes are in scope (table name, columns, types, keys, indexes, and notes)
- be saved under `docs/tdd`
- use automatic numbering in the file name

## Steps

1. Inspect the available context.
   - Read the current plan, requirement notes, active editor context, and any user-provided details.
   - Read `docs/templates/tdd_template.md`.
   - Identify the minimum set of facts needed to fill the template.
   Completion criterion: You can name the target feature, the intended audience, the template path, and the missing required fields.

2. Resolve missing business inputs.
   - Compare the template's `Basic Info` rows with the available context before drafting.
   - Treat an empty value, `TBD`, `TBF`, `N/A`, or a template placeholder as missing when checking required fields.
   - `Reviewer` and `Feature Jira` are required business inputs. Ask for them one at a time, in template order, and wait for the answer before asking the next one.
   - Populate other Basic Info fields from context when available; otherwise keep them as `TBD` without asking.
   - Do not ask again for fields already supplied by the user or current workspace context.
   Completion criterion: `Reviewer` and `Feature Jira` have explicit values before drafting; all other unresolved Basic Info fields may remain `TBD`.

3. Draft the document from the template.
   - Keep the structure of `docs/templates/tdd_template.md`.
   - Write for PM and QA readers.
   - Explain what the feature changes, why it matters, the expected outcome, high-level implementation approach, system interactions, rollout impact, and QA suggestions.
   - Replace technical internals with high-level component language.
   - If API changes exist, add a clear API design subsection in `How`.
   - If DB changes exist, add a concrete DB table design subsection in `How`.
   - If ADRs are not requested by the user, remove ADR content instead of adding generic decisions.
   Completion criterion: Every relevant section in the template has concise, reader-safe content or an intentional placeholder that depends on user-owned business data.

4. Choose the output file path.
   - Save the document under `docs/tdd`.
   - Use the next available numeric prefix followed by a short kebab-case topic name.
   - Example: `docs/tdd/001-file-share-archive.md`
   Completion criterion: A single target path under `docs/tdd` is chosen and does not collide with an existing numbered document.

5. Write and verify.
   - Create or update the document.
   - Re-read the saved file for a quick check.
   - Confirm the writing is concise, in English, and free of code-detail leakage.
   Completion criterion: The saved file matches the template structure, uses simple English, and is appropriate for PM and QA readers.

## Writing Rules

- Prefer short sentences and plain words.
- Keep each section focused on decisions and outcomes.
- Do not expose code snippets, namespaces, data model internals, or low-level processing logic.
- For API sections, list stable product-facing contract details, not code symbols.
- For API sections, list only new or changed APIs. Do not list unchanged APIs.
- For DB sections, provide table-level design details in plain language and concise tabular form.
- If exact implementation details are uncertain, describe the behavior and boundary instead of inventing internals.
- Keep diagrams simple and readable.

## Output Naming

When choosing the number:

- scan `docs/tdd` for existing numeric prefixes
- use three digits
- increment from the current maximum
- if no file exists yet, start from `001`

## Failure Mode Guardrails

- Do not dump plan text into the document without rewriting it for PM and QA readers.
- Do not leave the document in mixed Chinese and English unless the user asks for bilingual output.
- Do not create more than one candidate file unless the user asks for alternatives.
- Do not expose source-code details just because they are available in the repo.