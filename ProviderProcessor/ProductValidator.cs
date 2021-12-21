using System.Collections.Generic;
using System.Linq;
using ProviderProcessing.ProcessReports;
using ProviderProcessing.ProviderDatas;
using ProviderProcessing.References;

namespace ProviderProcessor
{
    public class ProductValidator
    {
        public IList<ProductValidationResult> ValidateProduct(ProductData product)
        {
            var data = new[] {product};
            return ValidateNames(data)
                .Concat(data.SelectMany(ValidatePricesAndMeasureUnitCodes))
                .ToList();
        }

        private IEnumerable<ProductValidationResult> ValidateNames(ProductData[] data)
        {
            var reference = ProductsReference.GetInstance();
            foreach (var product in data)
            {
                if (!reference.FindCodeByName(product.Name).HasValue)
                    yield return new ProductValidationResult(product,
                        "Unknown product name", ProductValidationSeverity.Error);
            }
        }

        private IEnumerable<ProductValidationResult> ValidatePricesAndMeasureUnitCodes(ProductData product)
        {
            if (product.Price <= 0)
                yield return new ProductValidationResult(product, "Bad price", ProductValidationSeverity.Warning);
            if (!IsValidMeasureUnitCode(product.MeasureUnitCode))
                yield return new ProductValidationResult(product,
                    "Bad units of measure", ProductValidationSeverity.Warning);
        }

        private bool IsValidMeasureUnitCode(string measureUnitCode)
        {
            var reference = MeasureUnitsReference.GetInstance();
            return reference.FindByCode(measureUnitCode) != null;
        }
    }
}
