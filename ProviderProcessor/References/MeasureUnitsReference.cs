using System;

namespace ProviderProcessing.References
{
    public class MeasureUnitsReference
    {
        private static MeasureUnitsReference instance;

        internal static void SetInstance(MeasureUnitsReference newInstance)
        {
            instance = newInstance;
        }

        public static MeasureUnitsReference GetInstance()
        {
            return instance ?? (instance = LoadReference());
        }

        public static MeasureUnitsReference LoadReference()
        {
            throw new NotImplementedException("Долгая-долгая инициализация справочника.");
        }

        public virtual MeasureUnit FindByCode(string measureUnitCode)
        {
            throw new NotImplementedException("Работа со справочником");
        }
        // Прочие методы
    }
}