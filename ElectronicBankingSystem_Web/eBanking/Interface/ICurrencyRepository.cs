using eBanking.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace eBanking.Interface
{
  public interface ICurrencyRepository
    {
        IEnumerable<Currency> GetAll();
        IQueryable<Currency> GetAllQueryable();
        
        Currency FindById(int? Id);

        Currency FindByName(string Name);
        Currency FindByDestinationID(int DestId);
        CurrencyViewModel GetCurrencyVMById(int? Id);
        bool Add(Currency entity);
        Currency Delete(int Id);
        bool Edit(Currency entity);
        void Save();
        IEnumerable<CurrencyViewModel> ActiveCurrency(string Request);
        SelectList GetDestinationSelectList();
        decimal ConvertToLocal(decimal Amount, int FromCurrencyId);
        decimal GetConversionRate(int CurrencyId);
    }
}
