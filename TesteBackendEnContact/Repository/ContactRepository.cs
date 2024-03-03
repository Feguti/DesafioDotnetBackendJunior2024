using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Company;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactRepository : IContactRepository
    {
        private readonly IDbConnection _connection;  

        public ContactRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IContact> SaveAsync(IContact contact)
        {
            var dao = new ContactDao(contact);

            dao.Id = await _connection.InsertAsync(dao);

            return dao.Export();
        }

        public async Task DeleteAsync(int id)
        {
            var query = "DELETE FROM Contact WHERE Id = @id;";
          
            await _connection.ExecuteAsync(query.ToString(), new { id });
        }

        public async Task<IEnumerable<IContact>> GetAllAsync()
        {
            var query = "SELECT * FROM Contact";
            var result = await _connection.QueryAsync<ContactDao>(query);

            return result?.Select(item => item.Export());
        }

        public async Task<IContact> GetAsync(int id)
        {
            var query = "SELECT * FROM Contact where Id = @id";  
            var result = await _connection.QuerySingleOrDefaultAsync<ContactDao>(query, new { id });

            return result?.Export();
        }

        public async Task<IContact> GetCompanyByIdAsync(int id)
        {
            var query = "SELECT * FROM Company where Id = @id";
            var result = await _connection.QuerySingleOrDefaultAsync<ContactDao>(query, new { id });

            return result?.Export();
        }

        public async Task<IContact> GetContactBookByIdAsync(int id)
        {
            var query = "SELECT * FROM ContactBook where Id = @id";
            var result = await _connection.QuerySingleOrDefaultAsync<ContactDao>(query, new { id });

            return result?.Export();
        }

        public async Task<IEnumerable<IContact>> SearchAsync(string query)
        {
            var sql = @"SELECT Contact.* FROM Contact LEFT JOIN Company ON Contact.CompanyId = Company.Id WHERE Contact.Name LIKE @Query OR 
                      Contact.Phone LIKE @Query OR Contact.Email LIKE @Query OR Contact.Address LIKE @Query OR Company.Name LIKE @Query";

            var contacts = await _connection.QueryAsync<Contact>(sql, new { Query = $"%{query}%" });

            return contacts;
        }

        public async Task<IEnumerable<IContact>> GetContactsByContactBookIdAsync(int contactBookId)
        {
            var sql = @"SELECT * FROM Contact WHERE ContactBookId = @ContactBookId";

            var contacts = await _connection.QueryAsync<Contact>(sql, new { ContactBookId = contactBookId });

            return contacts;
        }

    }

    [Table("Contact")]
    public class ContactDao : IContact
    {
        [Key]
        public int Id { get; set; }
        public int ContactBookId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public ContactDao()
        {
        }

        public ContactDao(IContact contact)
        {
            Id = contact.Id;
            ContactBookId = contact.ContactBookId;
            CompanyId = contact.CompanyId;
            Name = contact.Name;
            Phone = contact.Phone;
            Email = contact.Email;
            Address = contact.Address;
            
        }

        public IContact Export() => new Contact(Id, ContactBookId, CompanyId, Name, Phone, Email, Address);
    }
}