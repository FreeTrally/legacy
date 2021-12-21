using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using ProviderProcessing.ProcessReports;
using ProviderProcessing.ProviderDatas;
using ProviderProcessing.References;
using ProviderProcessor;

namespace ProviderProcessing
{
    public class ProviderProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProviderProcessor));
        private readonly ProviderRepository repo;
        private readonly ProductValidator validator;

        public ProviderProcessor(ProviderRepository repo, ProductValidator validator)
        {
            this.repo = repo ?? new ProviderRepository();
            this.validator = validator ?? new ProductValidator();
        }

        public ProcessReport ProcessProviderData(string message)
        {
            var data = JsonConvert.DeserializeObject<ProviderData>(message);
            var currentData = repo.FindByProviderId(data.ProviderId);
            return ProcessProviderData(data, currentData);
        }

        public ProcessReport ProcessProviderData(ProviderData data, ProviderData current)
        {
            if (current != null && data.Timestamp < current.Timestamp)
            {
                log.InfoFormat("Outdated provider data. ProviderId {0} Received timestamp: {1} database timestamp {2}",
                    data.ProviderId, data.Timestamp, current.Timestamp);
                return new ProcessReport(false, "Outdated data");
            }
            //var errors = ValidateNames(data.Products)
            //    .Concat(data.Products.SelectMany(ValidatePricesAndMeasureUnitCodes))
            //    .ToArray();
            var errors = data.Products
                .SelectMany(p => validator.ValidateProduct(p))
                .ToArray();
            
            if (errors.Any())
            {
                return new ProcessReport(false, 
                    "Product validation errors", errors);
            }

            if (current == null)
            {
                repo.Save(data);
            }
            else if (data.ReplaceData)
            {
                log.InfoFormat("Provider {0} products replaced. Deleted: {1} Added: {2}",
                    data.ProviderId, current.Products.Length, data.Products.Length);
                repo.RemoveById(current.Id);
                repo.Save(data);
            }
            else
            {
                var actualProducts = current.Products.Where(p => data.Products.All(d => d.Id != p.Id)).ToList();
                var updatedCount = current.Products.Length - actualProducts.Count;
                var newCount = data.Products.Length - updatedCount;
                log.InfoFormat("Provider {0} products update. New: {1} Updated: {2}",
                    data.ProviderId, newCount, updatedCount);
                current.Products = actualProducts.Concat(data.Products).ToArray();
                repo.Update(current);
            }
            log.InfoFormat("New data {0}, Existing data {1}", FormatData(data), FormatData(current));
            return new ProcessReport(true, "OK");
        }

        //private IEnumerable<ProductValidationResult> ValidateNames(ProductData[] data)
        //{
        //    var reference = ProductsReference.GetInstance();
        //    foreach (var product in data)
        //    {
        //        if (!reference.FindCodeByName(product.Name).HasValue)
        //            yield return new ProductValidationResult(product,
        //                "Unknown product name", ProductValidationSeverity.Error);
        //    }
        //}

        //private IEnumerable<ProductValidationResult> ValidatePricesAndMeasureUnitCodes(ProductData product)
        //{
        //    if (product.Price <= 0)
        //        yield return new ProductValidationResult(product, "Bad price", ProductValidationSeverity.Warning);
        //    if (!IsValidMeasureUnitCode(product.MeasureUnitCode))
        //        yield return new ProductValidationResult(product,
        //            "Bad units of measure", ProductValidationSeverity.Warning);
        //}

        //private bool IsValidMeasureUnitCode(string measureUnitCode)
        //{
        //    var reference = MeasureUnitsReference.GetInstance();
        //    return reference.FindByCode(measureUnitCode) != null;
        //}

        private string FormatData(ProviderData data)
        {
            return data != null
                ? data.Id + " for " + data.ProviderId + " products count " + data.Products.Length
                : "null";
        }
    }
}