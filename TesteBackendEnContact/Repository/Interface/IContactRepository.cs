using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface IContactRepository
    {
        Task<IContact> SaveAsync(IContact contact);
        Task DeleteAsync(int id);
        Task<IEnumerable<IContact>> GetAllAsync();
        Task<IContact> GetAsync(int id);
        Task<IContact> GetCompanyByIdAsync(int id);
        Task<IContact> GetContactBookByIdAsync(int id);
        Task<IEnumerable<IContact>> SearchAsync(string query);
        Task<IEnumerable<IContact>> GetContactsByContactBookIdAsync(int contactBookId);
    }
}