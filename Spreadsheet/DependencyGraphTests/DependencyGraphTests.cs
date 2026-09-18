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
    ///   TODO:  Explain carefully what this code tests.
    ///          Also, update in-line comments as appropriate.
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

        String[] expected = { "b1","b2","b3","b4" };
        
        int i = 0;

        foreach (string str in dg.GetDependents("a1"))
        {
            Assert.AreEqual(expected[i], str, "failed at " + i);
            i++;
        }
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

        String[] expected = { "a1","a2","a3","a4" };
        
        int i = 0;

        foreach (string str in dg.GetDependees("b1"))
        {
            Assert.AreEqual(expected[i], str, "failed at " + i);
            i++;
        }
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
        
        int i = 2;

        foreach (string str in dg.GetDependents("a1"))
        {
            Assert.AreEqual("c" + i, str, "failed at " + i);
            i--;
        }
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
        
        int i = 2;

        foreach (string str in dg.GetDependees("a1"))
        {
            Assert.AreEqual("c" + i, str, "failed at " + i);
            i--;
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


}