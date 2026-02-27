### Issue to Fix Test Failures

**Description:** Certain tests are failing because line height and spacing calculations return null for radio button forms. This can be seen in the logs here: [GitHub Actions Run](https://github.com/MaiRat/Broiler/actions/runs/22493682733/job/65162393853). Specifically, the issue is prevalent in `Acid1ProgrammaticTests.cs` at line 1608.  

**Proposed Solution:** Ensure that non-null tuples are always returned during these calculations, or adjust the tests to expect a valid failure state if applicable.

**Assigned to:** @Copilot

**Date Created:** 2026-02-27