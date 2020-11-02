using System;
using Editor.Scripts;
using NUnit.Framework;

namespace Editor.Tests
{
    public class MarkdownTableGeneratorTests
    {
	    #region Header Tests

	    [Test]
	    public void MarkdownTableGenerator2Headers()
	    {
		    MarkdownTableGenerator generator = new MarkdownTableGenerator("Test", "Header1", "Header2");
		    StringAssert.AreEqualIgnoringCase("## Test\n\n|Header1|Header2|\n|-------|-------|\n", generator.ToString());
	    }

	    [Test]
	    public void MarkdownTableGenerator3Headers()
	    {
		    MarkdownTableGenerator generator = new MarkdownTableGenerator("Test", "Header1", "Header2", "Header3");
		    StringAssert.AreEqualIgnoringCase("## Test\n\n|Header1|Header2|Header3|\n|-------|-------|-------|\n", generator.ToString());
	    }

		
	    #endregion

	    #region Option Tests

	    [Test]
	    public void MarkdownTableGeneratorAdd1Option2Headers()
	    {
		    MarkdownTableGenerator generator = new MarkdownTableGenerator("Test", "Header1", "Header2");
			generator.AddOption("Test1", "Test2");
			StringAssert.AreEqualIgnoringCase("## Test\n\n|Header1|Header2|\n|-------|-------|\n|Test1|Test2|\n", generator.ToString());
	    }

	    [Test]
	    public void MarkdownTableGeneratorAdd2Option2Headers()
	    {
		    MarkdownTableGenerator generator = new MarkdownTableGenerator("Test", "Header1", "Header2");
		    generator.AddOption("Test1", "Test2");
		    generator.AddOption("Test3", "Test4");
		    StringAssert.AreEqualIgnoringCase("## Test\n\n|Header1|Header2|\n|-------|-------|\n|Test1|Test2|\n|Test3|Test4|\n", generator.ToString());
	    }

	    [Test]
	    public void MarkdownTableGeneratorAdd1Option3Headers()
	    {
		    MarkdownTableGenerator generator = new MarkdownTableGenerator("Test", "Header1", "Header2", "Header3");
		    generator.AddOption("Test1", "Test2", "Test3");
		    StringAssert.AreEqualIgnoringCase("## Test\n\n|Header1|Header2|Header3|\n|-------|-------|-------|\n|Test1|Test2|Test3|\n", generator.ToString());
	    }

	    [Test]
	    public void MarkdownTableGeneratorAdd2Option3Headers()
	    {
		    MarkdownTableGenerator generator = new MarkdownTableGenerator("Test", "Header1", "Header2", "Header3");
		    generator.AddOption("Test1", "Test2", "Test3");
		    generator.AddOption("Test4", "Test5", "Test6");
		    StringAssert.AreEqualIgnoringCase("## Test\n\n|Header1|Header2|Header3|\n|-------|-------|-------|\n|Test1|Test2|Test3|\n|Test4|Test5|Test6|\n", generator.ToString());
	    }

	    [Test]
	    public void MarkdownTableGeneratorTooManyOptions()
	    {
		    Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
		    {
			    MarkdownTableGenerator generator = new MarkdownTableGenerator("Test", "Header1", "Header2", "Header3");
				generator.AddOption("Test1", "Test2");
		    });
	    }
		
	    #endregion
    }
}