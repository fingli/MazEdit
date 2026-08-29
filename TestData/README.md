Copy a real program here for golden tests (not committed):

  TestData\TEST.MAZ

Or set environment variable MAZEDIT_TEST_MAZ to the full path.

Then:

  dotnet test MazEdit.Tests\MazEdit.Tests.csproj

If the file is missing, ParseSubProgram_RealTestMaz_MatchesControlListing is skipped.
