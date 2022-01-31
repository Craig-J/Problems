namespace ValueOptimizer;

public class Rule
{
    private readonly Func<ItemInfoLookup, Context, Result> _func;

    public Rule(Func<ItemInfoLookup, Context, Result> func) => _func = func;
    
    /// <summary>
    /// Represents the result of a rule application.
    /// </summary>
    public class Result
    {
        public static Result None { get; } = new();

        private Result()
        {
            TimesApplied = 0;
            Offset = 0;
            AffectedIds = Enumerable.Empty<int>();
        }
        
        public Result(int timesApplied, int offset, IEnumerable<int> affectedIds)
        {
            TimesApplied = timesApplied;
            Offset = offset;
            AffectedIds = affectedIds;
        }

        public bool WasApplied => TimesApplied > 0;
        public int TimesApplied { get; }
        public int Offset { get; }
        public IEnumerable<int> AffectedIds { get; }
    }
    
    /// <summary>
    /// Represents contextual state shared between multiple rule applications.
    /// Immutable so that rule implementations can't partially modify state during their execution.
    /// </summary>
    public class Context
    {
        public Context(IReadOnlySet<int> excludedIds)
        {
            ExcludedIds = excludedIds;
        }

        public IReadOnlySet<int> ExcludedIds { get; }
    }

    /// <summary>
    /// Applies all rules in the list in sequence to the lookup, returning the total value of all items +/- the 
    /// </summary>
    public static int EvaluateAll(ItemInfoLookup items, IEnumerable<Rule> rules)
    {
        var context = new Context(new HashSet<int>());
        var totalWithoutRules = items.Sum;
        var ruleOffset = rules.Sum(rule => rule.Evaluate(items, ref context).Offset);
        return totalWithoutRules + ruleOffset;
    }

    /// <summary>
    /// Applies a rule to items in the given lookup.
    /// If the rule was applied, context will be replaced with an updated context.
    /// </summary>
    public Result Evaluate(ItemInfoLookup items, ref Context context)
    {
        var result = _func(items, context);
        if (result.WasApplied)
        {
            context = new Context(context.ExcludedIds.Union(result.AffectedIds).ToHashSet());
        }

        return result;
    }
}

public static class CommonRules
{
    public static Rule SingleItemCountConstantOffset(int id, int countRequired, int offset) => new Rule((lookup, context) =>
    {
        if (context.ExcludedIds.Contains(id)) return Rule.Result.None;
        return lookup.Lookup(id, out var info) && info.Count >= countRequired
            ? new Rule.Result(info.Count / countRequired, info.Count / countRequired * offset, new[] { id })
            : Rule.Result.None;
    });

    public static Rule MultipleItemCountsConstantOffset((int id, int countRequired)[] items, int offset) => new Rule((lookup, context) =>
    {
        if (items.Any(i => context.ExcludedIds.Contains(i.id))) return Rule.Result.None;
        // Number of times each specific item matches their count requirement
        var itemCountMatches = items.Select(i => lookup.Lookup(i.id, out var info) && info.Count >= i.countRequired ? info.Count / i.countRequired : 0);
        // We take the minimum of matches as it's is the maximum number of times the rule can be applied across all items
        var minimumRuleMatches = itemCountMatches.Min();
        return minimumRuleMatches > 0
            ? new Rule.Result(minimumRuleMatches, minimumRuleMatches * offset, items.Select(i => i.id).ToArray())
            : Rule.Result.None;
    });
}