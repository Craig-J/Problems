using NUnit.Framework;
using System.Collections.Generic;

namespace ValueOptimizer.Tests;

public class Tests
{
    public static readonly Dictionary<int, int> ItemValues = new Dictionary<int, int>
    {
        { 0, 50 },
        { 1, 30 },
        { 2, 20 },
        { 3, 15 }
    };

    public static readonly List<Rule> Ruleset = new()
    {
        CommonRules.SingleItemCountConstantOffset(0, 3, -20),
        CommonRules.SingleItemCountConstantOffset(1, 2, -15),
        CommonRules.MultipleItemCountsConstantOffset(new []{(2, 1), (3, 1)}, -5),
    };

    [Test]
    public void NoRulesMatch()
    {
        var items = new ItemInfoLookup(ItemValues, new Dictionary<int, int>
        {
            { 0, 1 },
            { 1, 1 },
            { 2, 1 }
        });
        
        Assert.That(Rule.EvaluateAll(items,Ruleset), Is.EqualTo(100));
    }

    [Test]
    public void SingleItemRulesMatch()
    {
        var items = new ItemInfoLookup(ItemValues, new Dictionary<int, int>
        {
            { 0, 5 },
            { 1, 5 },
            { 2, 1 }
        });
        
        Assert.That(Rule.EvaluateAll(items,Ruleset), Is.EqualTo(370));
    }

    [Test]
    public void MultipleItemRuleMatch()
    {
        var items = new ItemInfoLookup(ItemValues, new Dictionary<int, int>
        {
            { 0, 3 },
            { 1, 5 },
            { 2, 1 },
            { 3, 1}
        });
        
        Assert.That(Rule.EvaluateAll(items,Ruleset), Is.EqualTo(280));
    }
    
    [Test]
    public void MultipleItemRuleDoesntGoOverMinimumMatchCount()
    {
        // There's definitely a better name for this test
        // To summarize, if the rule has conditions on some items that match many times
        // it should not exceed the number of rule matches of the least-matched item.
        // Below there are 3x item 2 but only 2x item 3.
        // The rule is any pairs of item 2+3 so it should only match twice. 

        var items = new ItemInfoLookup(ItemValues, new Dictionary<int, int>
        {
            { 2, 3 },
            { 3, 2 }
        });
        
        Assert.That(Rule.EvaluateAll(items,Ruleset), Is.EqualTo(80));
    }
}