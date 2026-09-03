# ObjectInfo Property Assignment Guard

## Inputs
- Target the .NET 10 signed library `Common/VCService30/CommonUtility30/DocAve.CommonUtility30.csproj`, whose property-accessor migration is currently incomplete.
- Cover the complete `ObjectInfoBase` inheritance tree defined under `FilterEngine/ObjectInfos`; exclude non-descendants such as `MemberInfo` and the separate `NewWrapperCommon` implementation.
- Treat `FilterEngine.IsQualified(ObjectInfoBase)` as the common Check Rule boundary; account for business-layer catch blocks that currently wrap exceptions.
- Preserve the business rule that DTO builders assign only properties required by the active filter policies and that language short-circuiting controls which getters execute.
- The operating role is the developer extending rule DTOs or property policies; the primary risk is silently evaluating a rule with an unassigned property and producing a wrong archival, retention, or disposal result.
- No authorization boundary or external-service contract applies to this in-process feature.

## Expected Output
- Add a public property-check policy contract, immutable check context, and dedicated unassigned-property exception in the target library.
- Extend `ObjectInfoBase` with policy registration, setter-call tracking, and a publicly usable scoped check lifecycle.
- Convert every public read/write property in the target inheritance tree to transparent intercepted accessors while preserving its public signature.
- Activate the scoped check automatically around `FilterEngine.IsQualified` and preserve the dedicated exception through relevant business entry points.
- Add a focused xUnit project under `Common/VCService30`, register it in the `UnitTest` folder of `RAOnline.DEV.sln`, and include behavioral and inheritance-tree contract tests.

## Constraints
- Modify only the `Common/VCService30` implementation; do not synchronize `NewWrapperCommon`.
- Preserve existing Rule branches, condition expressions, DTO construction, property call syntax, and public serialization surface.
- Give each target property an independent backing field; use a case-sensitive property-name set in the base class only to record setter execution.
- A setter must store the value and mark the property assigned; explicit `null`, `false`, zero, empty string, enum defaults, and other CLR defaults count as assigned.
- A getter with checking disabled must return its backing field without allocating a context or iterating policies.
- Each getter actually executed while checking is enabled must run the fixed assignment policy first and then custom policies in registration order on every read; skipped short-circuit operands are not checked.
- Keep the assignment policy installed and non-removable; allow add/remove operations only for custom policies.
- Reject null policies, null context targets, and empty property names; ignore repeated registration of the same policy instance.
- Support nested check scopes with reference counting and guaranteed restoration after success or exception; retain assignment state for the object's lifetime.
- A custom policy must receive all data through the immutable context, return through a throwing `void` check contract, and not read intercepted properties from the target.
- Throw `PropertyNotAssignedException : InvalidOperationException` with the runtime object type and property name when an executed getter has never been assigned.
- Re-throw that dedicated exception unchanged at wrapping business boundaries while preserving existing handling for unrelated exceptions.
- Remove the duplicate `FSFileInfo.Name` and `FSFolderInfo.Name` declarations so both types reuse `CommonInfoBase.Name`.
- Do not promise thread-safe concurrent access to the same DTO instance.
- Do not add authorization checks, network calls, retries, or timeout behavior because the feature is entirely in-process.

## Edge Cases
- Guard API receives a null target or empty property name -> reject it before changing check state.
- Checking is disabled and an unassigned property is read -> return the CLR default and invoke no policy.
- A property is explicitly assigned `null` or a CLR default -> treat it as assigned and return that value during checking.
- A property is assigned repeatedly or the same custom policy instance is added repeatedly -> return the latest value and execute that policy once per getter.
- An inner nested scope exits or throws -> keep the outer scope active; after the outer scope exits, ordinary reads remain unchecked.
- An OR/AND expression skips an unassigned property's getter -> do not fail until that getter is actually executed in a checked scope.
- An unauthorized caller invokes the API -> no special behavior applies because this library exposes no authorization boundary.

## Acceptance Criteria
- Do assigned properties retain their values, serialization shape, and existing qualified/not-qualified Rule outcomes?
- Does an actually executed, never-assigned getter throw the dedicated exception only while checking is enabled, with the correct runtime type and property name?
- Do explicit `null`, empty, zero, `false`, enum-default, and other default assignments pass the assignment policy?
- Are policy validation, fixed-first ordering, registration ordering, duplicate suppression, and removal rules independently verified?
- Do nested, successful, and exceptional check scopes restore the exact prior state without leaking validation into later reads?
- Does an assembly-reflection contract test cover every current public read/write property and automatically expose future unconverted descendants or properties?
- Do all relevant wrapping entry points preserve the dedicated exception type, instance, and stack while leaving unrelated exception behavior unchanged?
- Do focused tests and the solution build pass with no `NewWrapperCommon` changes, and does a release benchmark confirm zero allocations on the disabled getter path?

## Review Notes
- Setter-call tracking separates assignment state from stored values, preventing legal CLR defaults from being reported as missing.
- Getter-triggered validation and scope restoration target the stated silent-wrong-result risk without changing Rule logic or reads outside Check Rule execution; the key risk-derived cases are skipped getters, explicit defaults, duplicate registration, and exceptional scope exit.
- A useful .NET-specific addition is a BenchmarkDotNet regression case for disabled and enabled getter paths alongside the reflection-based coverage test.