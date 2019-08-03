using Xunit;

namespace prismic.AspNetCore.Tests
{
	public class PredicatesTest
	{
		[Fact]
		public void TestAtPredicate() {
			var p = Predicates.at("document.type", "blog-post");
			Assert.Equal("[:d = at(document.type, \"blog-post\")]", p.q());
		}

		[Fact]
		public void TestAnyPredicate() {
			var p = Predicates.any("document.tags", new string[] { "Macaron", "Cupcakes" });
			Assert.Equal("[:d = any(document.tags, [\"Macaron\",\"Cupcakes\"])]", p.q());
		}

		[Fact]
		public void TestNumberLT() {
			var p = Predicates.lt("my.product.price", 4.2);
			Assert.Equal("[:d = number.lt(my.product.price, 4.2)]", p.q());
		}

		[Fact]
		public void TestNumberInRange() {
			var p = Predicates.inRange("my.product.price", 2, 4);
			Assert.Equal("[:d = number.inRange(my.product.price, 2, 4)]", p.q());
		}

		[Fact]
		public void TestMonthAfter() {
			var p = Predicates.monthAfter("my.blog-post.publication-date", Predicates.Months.April);
			Assert.Equal("[:d = date.month-after(my.blog-post.publication-date, \"April\")]", p.q());
		}

		[Fact]
		public void TestGeopointNear() {
			var p = Predicates.near("my.store.coordinates", 40.689757, -74.0451453, 15);
			Assert.Equal("[:d = geopoint.near(my.store.coordinates, 40.689757, -74.0451453, 15)]", p.q());
		}

		[Fact]
		public void TestNot() {
			var p = Predicates.not("my.store.coordinates", "value");
			Assert.Equal("[:d = not(my.store.coordinates, \"value\")]", p.q());
		}

		[Fact]
		public void TestHas() {
			var p = Predicates.has("my.store.coordinates");
			Assert.Equal("[:d = has(my.store.coordinates)]", p.q());
		}

		[Fact]
		public void TestSimilar() {
			var p = Predicates.similar("VkRmhykAAFA6PoBj", 10);
			Assert.Equal("[:d = similar(\"VkRmhykAAFA6PoBj\", 10)]", p.q());
		}
	}
}

