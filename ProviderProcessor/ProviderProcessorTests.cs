using System;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FakeItEasy;
using NUnit.Framework;
using ProviderProcessing.ProviderDatas;
using ProviderProcessing.References;

namespace ProviderProcessing
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class ProviderProcessorTests
    {
        private ProductsReference productsReference;
        private MeasureUnitsReference measureUnitsReference;
        private ProviderProcessor processor;

        [SetUp]
        public void SetUp()
        {
            measureUnitsReference = A.Fake<MeasureUnitsReference>();
            A.CallTo(() => measureUnitsReference.FindByCode("a")).Returns(new MeasureUnit());
            A.CallTo(() => measureUnitsReference.FindByCode("b")).Returns(null);
            MeasureUnitsReference.SetInstance(measureUnitsReference);

            productsReference = A.Fake<ProductsReference>();
            A.CallTo(() => productsReference.FindCodeByName("a")).Returns(10);
            A.CallTo(() => productsReference.FindCodeByName("b")).Returns(null);
            ProductsReference.SetInstance(productsReference);

            processor = new ProviderProcessor(A.Fake<ProviderRepository>(), null);
        }

        private static IEnumerable<TestCaseData> GetTestCases()
        {
            yield return new TestCaseData(getSampleData(), null) { TestName = "New data" };

            var withUnknownName = getSampleData();
            withUnknownName.Products = new[] { new ProductData() { Name = "b" } };
            yield return new TestCaseData(withUnknownName, null) { TestName = "Unknown name" };

            var withBadPrice = getSampleData();
            withBadPrice.Products = new[] { new ProductData() { Name = "a", Price = -1, MeasureUnitCode = "a" } };
            yield return new TestCaseData(withBadPrice, null) { TestName = "Bad price" };

            var withBadUnits = getSampleData();
            withBadUnits.Products = new[] { new ProductData() { Name = "a", Price = 1, MeasureUnitCode = "b" } };
            yield return new TestCaseData(withBadUnits, null) { TestName = "Bad unit" };

            var data = getSampleData();
            data.Products = new[]
            {
                new ProductData { Name = "a", Price = -1, MeasureUnitCode = "a" },
                new ProductData { Name = "b", Price = 1, MeasureUnitCode = "a" }
            };
            yield return new TestCaseData(data, null) { TestName = "Multiple errors" };

            yield return new TestCaseData(getSampleData(), getSampleData()) { TestName = "Same empty data" };

            var withReplace = getSampleData();
            withReplace.ReplaceData = true;
            yield return new TestCaseData(withReplace, getSampleData()) { TestName = "Data replace" };
        }

        [TestCaseSource(nameof(GetTestCases))]
        [Test]
        public void CharacterizationTest(ProviderData receivedData, ProviderData existedData)
        {
            using (ApprovalResults.ForScenario(TestContext.CurrentContext.Test.Name))
            {
                var res = processor
                    .ProcessProviderData(receivedData, existedData);
                Approvals.Verify(res);
            }
        }

        private static readonly Func<ProviderData> getSampleData = () => new ProviderData
        {
            Id = new Guid(),
            ProviderId = new Guid(),
            ReplaceData = false,
            Timestamp = new DateTime(2021, 12, 21),
            Products = new ProductData[] { }
        };
    }
}