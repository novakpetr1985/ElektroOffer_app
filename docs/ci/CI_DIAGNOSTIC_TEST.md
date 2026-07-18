# Ruční ověření diagnostického CI logu

Diagnostiku lze bezpečně vyzkoušet pouze ručním spuštěním workflow `ElektroOffer CI Pipeline`.

1. Na GitHubu otevřete `Actions` a vyberte `ElektroOffer CI Pipeline`.
2. Zvolte `Run workflow` na pracovní větvi.
3. Zapněte parametr `simulate_failure` a spusťte workflow.
4. Krok `Simulate diagnostic failure` úmyslně ukončí pouze tento ruční běh jako neúspěšný. Při pushi, Pull Requestu ani tagu se tento krok nespouští.
5. Po dokončení otevřete job `Diagnostic log` a stáhněte artifact `elektrooffer-ci-error-log`.
6. Log musí obsahovat workflow, číslo a URL běhu, čas, ref, commit, runner, SDK, výsledky CI/release a podrobné výstupy restore, buildu a obou testovacích sad.
7. Artifact `elektrooffer-coverage` musí obsahovat Cobertura reporty unit i integračních testů.

Po testu spusťte workflow znovu s `simulate_failure = false`. Krok simulace musí být přeskočen a běžné CI musí projít. Simulace nemění zdrojový kód, testy ani release podmínky.
