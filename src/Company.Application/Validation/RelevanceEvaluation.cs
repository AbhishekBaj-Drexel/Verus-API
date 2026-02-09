namespace Company.Application.Validation;

public sealed record RelevanceEvaluation(
    bool IsRelevant,
    double Score,
    IReadOnlyList<string> Reasons);
