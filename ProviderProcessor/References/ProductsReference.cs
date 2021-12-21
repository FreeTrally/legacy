using System;

namespace ProviderProcessing.References
{
    public class ProductsReference
    {
        private static ProductsReference instance;

        internal static void SetInstance(ProductsReference newInstance)
        {
            instance = newInstance;
        }

        public static ProductsReference GetInstance()
        {
            return instance ??= LoadReference();
        }

        public static ProductsReference LoadReference()
        {
            throw new NotImplementedException("Долгая-долгая инициализация справочника.");
        }

        public virtual int? FindCodeByName(string name)
        {
            throw new NotImplementedException("Работа со справочником");
        }

        // Прочие методы
    }
}