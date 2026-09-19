using System.Diagnostics.CodeAnalysis;
using Microsoft.Testing.Extensions.TrxReport.Abstractions;

namespace DependencyGraphTests;

using DependencyGraph;

/// <summary>
///   This is a test class for DependencyGraphTest and is intended
///   to contain all DependencyGraphTest Unit Tests
/// </summary>
[TestClass]
public class DependencyGraphTests
{
    /// <summary>
    /// This stress test will create a graph of 200 dependencies, adds and removes multiple times
    /// and checks that the result is to be expected each step of the way. It also ensures that the
    /// methods do not timeout.
    /// </summary>
    [TestMethod]
    [Timeout(2000, CooperativeCancellation = true)] // 2 second run time limit
    public void StressTest()
    {
        // create the DG
        DependencyGraph dg = new();

        // create an array of 200 variables
        const int size = 200;
        string[] letters = new string[size];
        for (int i = 0; i < size; i++)
        {
            letters[i] = string.Empty + ((char)('a' + i));
        }

        // The correct answers
        // creating empty dependents/dependees HashSets
        HashSet<string>[] dependents = new HashSet<string>[size];
        HashSet<string>[] dependees = new HashSet<string>[size];
        for (int i = 0; i < size; i++)
        {
            dependents[i] = [];
            dependees[i] = [];
        }

        // Add a bunch of dependencies using the variables we created
        for (int i = 0; i < size; i++)
        {
            for (int j = i + 1; j < size; j++)
            {
                dg.AddDependency(letters[i], letters[j]);
                dependents[i].Add(letters[j]);
                dependees[j].Add(letters[i]);
            }
        }

        // Remove a bunch of these dependencies
        for (int i = 0; i < size; i++)
        {
            for (int j = i + 4; j < size; j += 4)
            {
                dg.RemoveDependency(letters[i], letters[j]);
                dependents[i].Remove(letters[j]);
                dependees[j].Remove(letters[i]);
            }
        }

        // Add some back
        for (int i = 0; i < size; i++)
        {
            for (int j = i + 1; j < size; j += 2)
            {
                dg.AddDependency(letters[i], letters[j]);
                dependents[i].Add(letters[j]);
                dependees[j].Add(letters[i]);
            }
        }

        // Remove some more
        for (int i = 0; i < size; i += 2)
        {
            for (int j = i + 3; j < size; j += 3)
            {
                dg.RemoveDependency(letters[i], letters[j]);
                dependents[i].Remove(letters[j]);
                dependees[j].Remove(letters[i]);
            }
        }

        // Make sure everything is right
        for (int i = 0; i < size; i++)
        {
            Assert.IsTrue(dependents[i].SetEquals(new HashSet<string>(dg.GetDependents(letters[i]))),
                "first assert failed at " + i);
            Assert.IsTrue(dependees[i].SetEquals(new HashSet<string>(dg.GetDependees(letters[i]))),
                "second assert failed at " + i);
        }
    }

    [TestMethod]
    public void DependencyGraph_SingleAdd()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        Assert.AreEqual(1, dg.Size);
    }
    
    [TestMethod]
    public void DependencyGraph_Empty()
    {
        DependencyGraph dg = new();
        Assert.AreEqual(0, dg.Size);
    }
    
    [TestMethod]
    public void DependencyGraph_AddDuplicates()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("a1", "b1");
        Assert.AreEqual(2, dg.Size);
    }
    
    [TestMethod]
    public void DependencyGraph_AddDependentIsDependee()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "a1");
        Assert.AreEqual(1, dg.Size);
    }
    
    [TestMethod]
    public void DependencyGraph_AddCyclical()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("b1", "a1");
        dg.AddDependency("b2", "a1");
        
        Assert.AreEqual(4, dg.Size);
    }
    
    [TestMethod]
    public void DependencyGraph_HasDependents()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        Assert.IsTrue(dg.HasDependents("a1"));
        Assert.IsFalse(dg.HasDependents("b1"));
    }

    [TestMethod]
    public void DependencyGraph_HasDependees()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        Assert.IsTrue(dg.HasDependees("b1"));
        Assert.IsFalse(dg.HasDependees("a1"));
    }
    
    [TestMethod]
    public void DependencyGraph_RemoveDependency_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("a1", "b3");
        dg.RemoveDependency("a1", "b2");
        Assert.AreEqual(2, dg.Size);
    }
    
    [TestMethod]
    public void DependencyGraph_RemoveDependency_Invalid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.RemoveDependency("a1", "b3");
        dg.RemoveDependency("a2", "b1");
        Assert.AreEqual(2, dg.Size);
    }
    
    [TestMethod]
    public void DependencyGraph_AddAndRemoveEmpty()
    {
        DependencyGraph dg = new();
        dg.AddDependency(string.Empty, string.Empty);
        Assert.AreEqual(1, dg.Size);
        dg.RemoveDependency(string.Empty, string.Empty);
        Assert.AreEqual(0,dg.Size);
    }

    [TestMethod]
    public void DependencyGraph_GetDependents_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("a1", "b3");
        dg.AddDependency("a1", "b4");

        HashSet<string> expected = new HashSet<string>();
        expected.Add("b1");
        expected.Add("b2");
        expected.Add("b3");
        expected.Add("b4");
        
        Assert.IsTrue(expected.SetEquals(dg.GetDependents("a1")));
    }
    
    [TestMethod]
    public void DependencyGraph_GetDependents_Invalid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");

        Assert.IsEmpty(dg.GetDependents("c1"));
        Assert.IsEmpty(dg.GetDependents("b1"));
    }
    
    [TestMethod]
    public void DependencyGraph_GetDependees_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a2", "b1");
        dg.AddDependency("a3", "b1");
        dg.AddDependency("a4", "b1");

        HashSet<string> expected = new HashSet<string>();
        expected.Add("a1");
        expected.Add("a2");
        expected.Add("a3");
        expected.Add("a4");

        Assert.IsTrue(expected.SetEquals(dg.GetDependees("b1")));
    }
    
    [TestMethod]
    public void DependencyGraph_GetDependees_Invalid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        
        Assert.IsEmpty(dg.GetDependees("a3"));
        Assert.IsEmpty(dg.GetDependees("a1"));
    }
    
    [TestMethod]
    public void DependencyGraph_ReplaceDependents_ValidNode()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("a1", "b3");
        dg.AddDependency("a1", "b4");

        HashSet<string> newDependents = new HashSet<string>();
        newDependents.Add("c1");
        newDependents.Add("c2");

        dg.ReplaceDependents("a1", newDependents);
        
        Assert.AreEqual(2, dg.Size);
        Assert.IsTrue(newDependents.SetEquals(dg.GetDependents("a1")));
    }
    
    [TestMethod]
    public void DependencyGraph_ReplaceDependents_InvalidNode()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        
        HashSet<string> newDependents = new HashSet<string>();
        newDependents.Add("c1");
        newDependents.Add("c2");
        
        //replace method should 1: remove all existing ordered pairs (nodeName, *)
        //and 2: add all ordered pairs (nodeName, newDependents)
        
        //since the given nodeName is not a valid one, step 1 will be skipped, and step 2 will be executed
        dg.ReplaceDependents("a2", newDependents);
        
        DependencyGraph expected = new();
        expected.AddDependency("a1", "b1");
        expected.AddDependency("a1", "b2");
        expected.AddDependency("a2", "c1");
        expected.AddDependency("a2", "c2");
        
        Assert.AreEqual(4, dg.Size);
    }
    
    [TestMethod]
    public void DependencyGraph_ReplaceDependees_ValidNode()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a2", "b1");
        dg.AddDependency("a3", "b1");
        dg.AddDependency("a4", "b1");
        
        HashSet<string> newDependees = new HashSet<string>();
        newDependees.Add("c1");
        newDependees.Add("c2");

        dg.ReplaceDependees("b1", newDependees);
        Assert.AreEqual(2, dg.Size);
        
        int i = 1;

        foreach (string str in dg.GetDependees("b1"))
        {
            Assert.AreEqual("c" + i, str, "failed at " + i);
            i++;
        }
    }

    
    [TestMethod]
    public void DependencyGraph_ReplaceDependees_InvalidNode()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a2", "b1");

        HashSet<string> newDependees = new HashSet<string>();
        newDependees.Add("c1");
        newDependees.Add("c2");

        //replace method should 1: remove all existing ordered pairs (*, nodeName)
        //and 2: add all ordered pairs (newDependees, nodeName)
        
        //since the given nodeName is not a valid one, step 1 will be skipped, and step 2 will be executed
        dg.ReplaceDependees("d1", newDependees);
        Assert.AreEqual(4, dg.Size);
    }
    
        [TestMethod]
    public void DependencyGraph_ReplaceDependents_SimpleCase()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("z1", "b1");
 
        dg.ReplaceDependents("a1", ["c1"]);
 
        Assert.AreEqual(2, dg.Size);
        Assert.IsTrue(new HashSet<string> { "c1" }.SetEquals(dg.GetDependents("a1")));
 
        // b1 and b2 should no longer list a1 as a dependee
        Assert.IsTrue(new HashSet<string> { "z1" }.SetEquals(dg.GetDependees("b1")));
        Assert.IsEmpty(dg.GetDependees("b2"));
 
        // the unrelated pair will stay
        Assert.IsTrue(new HashSet<string> { "b1" }.SetEquals(dg.GetDependents("z1")));
    }
 

    [TestMethod]
    public void DependencyGraph_ReplaceDependents_NothingToReplace()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
 
        // b1 exists in the graph but has no dependents of its own
        dg.ReplaceDependents("b1", ["c1", "c2"]);
 
        Assert.AreEqual(3, dg.Size);
        Assert.IsTrue(new HashSet<string> { "c1", "c2" }.SetEquals(dg.GetDependents("b1")));
        Assert.IsTrue(dg.HasDependents("b1"));
 
        // the original pair is undisturbed
        Assert.IsTrue(new HashSet<string> { "b1" }.SetEquals(dg.GetDependents("a1")));
    }
    
    [TestMethod]
    public void DependencyGraph_ReplaceDependents_WithEmptyCollection()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("a1", "b3");
 
        dg.ReplaceDependents("a1", []);
 
        Assert.AreEqual(0, dg.Size);
        Assert.IsFalse(dg.HasDependents("a1"));
        Assert.IsEmpty(dg.GetDependents("a1"));
        Assert.IsFalse(dg.HasDependees("b1"));
        Assert.IsFalse(dg.HasDependees("b2"));
        Assert.IsFalse(dg.HasDependees("b3"));
    }
 
    [TestMethod]
    public void DependencyGraph_ReplaceDependents_OverlappingCollection()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
 
        dg.ReplaceDependents("a1", ["b2", "b3"]);
 
        Assert.AreEqual(2, dg.Size);
        Assert.IsTrue(new HashSet<string> { "b2", "b3" }.SetEquals(dg.GetDependents("a1")));
        Assert.IsTrue(new HashSet<string> { "a1" }.SetEquals(dg.GetDependees("b2")));
        Assert.IsEmpty(dg.GetDependees("b1"));
    }
 

    [TestMethod]
    public void DependencyGraph_ReplaceDependents_DuplicatesInCollection()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
 
        dg.ReplaceDependents("a1", ["c1", "c1", "c2", "c1"]);
 
        Assert.AreEqual(2, dg.Size);
        Assert.IsTrue(new HashSet<string> { "c1", "c2" }.SetEquals(dg.GetDependents("a1")));
    }
    
    [TestMethod]
    public void DependencyGraph_ReplaceDependees_SimpleCase()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a2", "b1");
        dg.AddDependency("a1", "z1");
 
        dg.ReplaceDependees("b1", ["c1"]);
 
        Assert.AreEqual(2, dg.Size);
        Assert.IsTrue(new HashSet<string> { "c1" }.SetEquals(dg.GetDependees("b1")));
 
        // a1 keeps its other dependent, a2 loses its only one
        Assert.IsTrue(new HashSet<string> { "z1" }.SetEquals(dg.GetDependents("a1")));
        Assert.IsFalse(dg.HasDependents("a2"));
    }
 
    [TestMethod]
    public void DependencyGraph_ReplaceDependees_NothingToReplace()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
 
        // a1 exists in the graph but nothing depends on it
        dg.ReplaceDependees("a1", ["c1", "c2"]);
 
        Assert.AreEqual(3, dg.Size);
        Assert.IsTrue(new HashSet<string> { "c1", "c2" }.SetEquals(dg.GetDependees("a1")));
        Assert.IsTrue(dg.HasDependees("a1"));
        Assert.IsTrue(new HashSet<string> { "b1" }.SetEquals(dg.GetDependents("a1")));
    }
 
    [TestMethod]
    public void DependencyGraph_ReplaceDependees_WithEmptyCollection()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a2", "b1");
        dg.AddDependency("a3", "b1");
 
        dg.ReplaceDependees("b1", []);
 
        Assert.AreEqual(0, dg.Size);
        Assert.IsFalse(dg.HasDependees("b1"));
        Assert.IsEmpty(dg.GetDependees("b1"));
        Assert.IsFalse(dg.HasDependents("a1"));
        Assert.IsFalse(dg.HasDependents("a2"));
        Assert.IsFalse(dg.HasDependents("a3"));
    }
    
    [TestMethod]
    public void DependencyGraph_ReplaceDependees_SelfLoop()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "a1");
 
        dg.ReplaceDependees("a1", ["b1"]);
 
        Assert.AreEqual(1, dg.Size);
        Assert.IsTrue(new HashSet<string> { "b1" }.SetEquals(dg.GetDependees("a1")));
        Assert.IsFalse(dg.HasDependents("a1"));
    }
 
    [TestMethod]
    public void DependencyGraph_AddThenRemoveAllDependents()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
        dg.AddDependency("a1", "b3");
        Assert.AreEqual(3, dg.Size);
 
        dg.RemoveDependency("a1", "b1");
        Assert.AreEqual(2, dg.Size);
        dg.RemoveDependency("a1", "b2");
        Assert.AreEqual(1, dg.Size);
        dg.RemoveDependency("a1", "b3");
        Assert.AreEqual(0, dg.Size);
 
        Assert.IsFalse(dg.HasDependents("a1"));
        Assert.IsEmpty(dg.GetDependents("a1"));
        Assert.IsFalse(dg.HasDependees("b2"));
    }
    
    [TestMethod]
    public void DependencyGraph_AddThenRemoveAllDependees()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a2", "b1");
        dg.AddDependency("a3", "b1");
        Assert.AreEqual(3, dg.Size);
 
        dg.RemoveDependency("a1", "b1");
        Assert.AreEqual(2, dg.Size);
        dg.RemoveDependency("a2", "b1");
        Assert.AreEqual(1, dg.Size);
        dg.RemoveDependency("a3", "b1");
        Assert.AreEqual(0, dg.Size);
 
        Assert.IsFalse(dg.HasDependees("b1"));
        Assert.IsEmpty(dg.GetDependees("b1"));
        Assert.IsFalse(dg.HasDependents("a2"));
    }
    
    [TestMethod]
    public void DependencyGraph_RemoveThenReAdd()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
 
        dg.RemoveDependency("a1", "b1");
        Assert.AreEqual(1, dg.Size);
 
        dg.AddDependency("a1", "b1");
        Assert.AreEqual(2, dg.Size);
        Assert.IsTrue(new HashSet<string> { "b1", "b2" }.SetEquals(dg.GetDependents("a1")));
        Assert.IsTrue(new HashSet<string> { "a1" }.SetEquals(dg.GetDependees("b1")));
    }
 
    [TestMethod]
    public void DependencyGraph_RemoveSamePairTwice()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("a1", "b2");
 
        dg.RemoveDependency("a1", "b1");
        dg.RemoveDependency("a1", "b1");
 
        Assert.AreEqual(1, dg.Size);
        Assert.IsTrue(new HashSet<string> { "b2" }.SetEquals(dg.GetDependents("a1")));
    }
 
    [TestMethod]
    public void DependencyGraph_RemoveFromEmptyGraph()
    {
        DependencyGraph dg = new();
        dg.RemoveDependency("a1", "b1");
 
        Assert.AreEqual(0, dg.Size);
        Assert.IsFalse(dg.HasDependents("a1"));
        Assert.IsFalse(dg.HasDependees("b1"));
    }
    
    [TestMethod]
    public void DependencyGraph_RemoveSelfReferentialPair()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "a1");
        Assert.IsTrue(dg.HasDependents("a1"));
        Assert.IsTrue(dg.HasDependees("a1"));
 
        dg.RemoveDependency("a1", "a1");
 
        Assert.AreEqual(0, dg.Size);
        Assert.IsFalse(dg.HasDependents("a1"));
        Assert.IsFalse(dg.HasDependees("a1"));
    }
 
    [TestMethod]
    public void DependencyGraph_RelationshipIsDirected()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
 
        Assert.IsTrue(new HashSet<string> { "b1" }.SetEquals(dg.GetDependents("a1")));
        Assert.IsEmpty(dg.GetDependents("b1"));
        Assert.IsTrue(new HashSet<string> { "a1" }.SetEquals(dg.GetDependees("b1")));
        Assert.IsEmpty(dg.GetDependees("a1"));
    }
    
    [TestMethod]
    public void DependencyGraph_QueryUnknownNode()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
 
        Assert.IsEmpty(dg.GetDependents("does_not_exist"));
        Assert.IsEmpty(dg.GetDependees("does_not_exist"));
        Assert.IsFalse(dg.HasDependents("does_not_exist"));
        Assert.IsFalse(dg.HasDependees("does_not_exist"));
        Assert.AreEqual(1, dg.Size);
    }
 
    [TestMethod]
    public void DependencyGraph_NodeNamesAreCaseSensitive()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
        dg.AddDependency("A1", "b1");
 
        Assert.AreEqual(2, dg.Size);
        Assert.IsTrue(new HashSet<string> { "a1", "A1" }.SetEquals(dg.GetDependees("b1")));
    }
    
    [TestMethod]
    public void DependencyGraph_QueryReflectsLaterModifications()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "b1");
 
        List<string> snapshot = [.. dg.GetDependents("a1")];
        dg.AddDependency("a1", "b2");
 
        Assert.AreEqual(1, snapshot.Count);
        Assert.IsTrue(new HashSet<string> { "b1", "b2" }.SetEquals(dg.GetDependents("a1")));
    }

    [TestMethod]
    public void DependencyGraph_InstancesAreIndependent()
    {
        DependencyGraph first = new();
        DependencyGraph second = new();
 
        first.AddDependency("a1", "b1");
 
        Assert.AreEqual(1, first.Size);
        Assert.AreEqual(0, second.Size);
        Assert.IsEmpty(second.GetDependents("a1"));
    }
}