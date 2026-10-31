using EmployeeTracking.Application.ScreenCapture;

namespace EmployeeTracking.Infrastructure.ScreenCapture
{
    public class SimulatedScreenAiAnalyzer : IScreenAiAnalyzer
    {
        private static readonly string[] Findings =
        [
            "The shared screen showed a document and a browser for most of the session.",
            "Work looked steady: email, a spreadsheet, and the time tracking app.",
            "A chat tool stayed open beside the main work window.",
            "The screen was idle for a stretch, then returned to a browser.",
            "A code editor and a terminal were the main windows in view.",
            "The session moved between a calendar, email, and a document."
        ];

        public Task<IReadOnlyList<string>> AnalyzeAsync(ScreenAiRequest request, CancellationToken ct = default)
        {
            if (request.Images.Count == 0)
                return Task.FromResult<IReadOnlyList<string>>(
                    ["No screen snapshots were captured, so there is nothing to review."]);

            var index = Random.Shared.Next(Findings.Length);
            return Task.FromResult<IReadOnlyList<string>>([Findings[index]]);
        }
    }
}
