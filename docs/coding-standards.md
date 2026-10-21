# Project D — Coding Standards

> Codename **Zantetsuken** · CONFIDENTIAL

1. **Server decides, client displays.** No damage, cooldown, gold, or objective
   ownership is trusted client-side (GCD 14.3). Casts are predicted then reconciled.
2. **Data over code.** Balance numbers live in ScriptableObjects / remote-config.
3. **Fairness logic is Unity-free.** DamageSystem, objective + draft controllers,
   StatBlock, SeededRandom avoid UnityEngine so the server reuses them and tests run headlessly.
4. **Disclosed RNG only.** Competitive randomness is server-seeded and revealed.

Conventions: namespaces mirror folders; PascalCase types/methods, _camelCase private
fields; structs for small net payloads to cut GC; pool high-churn objects; every
fairness-critical method gets an NUnit test. Never commit unverified art.
