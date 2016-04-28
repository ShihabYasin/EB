using eBanking.Models;
using System.Collections.Generic;
using System.Linq;
using System;


namespace eBanking.Interface
{
   public interface IDestinationRepository
    {
        IEnumerable<Destination> GetAll();        
        IQueryable<Destination> GetAllQueryable();
        
        Destination FindById(int? Id);

        Destination FindByName(string Name);
        Destination FindByCountryCode(string CountryCode);
        //IEnumerable<Destination> GetDestinationForTopUp();
        bool Add(Destination entity);
        Destination Delete(int Id);
        bool Edit(Destination entity);
        void Save();
    }
}
