using System;

namespace ProviderProcessing.ProviderDatas
{
    public class ProviderRepository
    {
        public virtual ProviderData FindByProviderId(Guid providerId)
        {
            throw new NotImplementedException("Работа с базой данных");
        }

        public virtual void RemoveById(Guid id)
        {
            throw new NotImplementedException("Работа с базой данных");
        }

        public virtual void Save(ProviderData data)
        {
            throw new NotImplementedException("Работа с базой данных");
        }

        public virtual void Update(ProviderData existingData)
        {
            throw new NotImplementedException("Работа с базой данных");
        }
    }
}