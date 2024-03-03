using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Domain.ContactBook.Company;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly IDbConnection _connection;  //Agora não é mais necessário abrir uma conexão dentro de cada operação

        public CompanyRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ICompany> SaveAsync(ICompany company)
        {
            
            var dao = new CompanyDao(company);

            if (dao.Id == 0)
                dao.Id = await _connection.InsertAsync(dao);
            else
                await _connection.UpdateAsync(dao);

            return dao.Export();
        }

        public async Task DeleteAsync(int id)
        {

            var sql = new StringBuilder();
            sql.AppendLine("DELETE FROM Company WHERE Id = @id;");
            sql.AppendLine("UPDATE Contact SET CompanyId = null WHERE CompanyId = @id;");

            await _connection.ExecuteAsync(sql.ToString(), new { id });
        }

        public async Task<IEnumerable<ICompany>> GetAllAsync()
        {

            var query = "SELECT * FROM Company";
            var result = await _connection.QueryAsync<CompanyDao>(query);

            return result?.Select(item => item.Export());
        }

        public async Task<ICompany> GetAsync(int id)
        {
            var query = "SELECT * FROM Company where Id = @id";  //Erro na digitação da table "Company" corrigido
            var result = await _connection.QuerySingleOrDefaultAsync<CompanyDao>(query, new { id });

            return result?.Export();
        }

        public async Task<IEnumerable<ICompany>> SearchByNameAsync(string companyName)
        {
            var sql = @"SELECT Company.*, ContactBook.* 
                        FROM Company 
                        LEFT OUTER JOIN ContactBook ON Company.ContactBookId = ContactBook.Id
                        WHERE Company.Name LIKE @CompanyName";

            var companies = new Dictionary<int, Company>();

            await _connection.QueryAsync<Company, ContactBook, Company>(
                sql,
                (company, contactBook) =>
                {
                    if (!companies.TryGetValue(company.Id, out var currentCompany))
                    {
                        currentCompany = company;
                        currentCompany.ContactBook = contactBook;
                        companies.Add(currentCompany.Id, currentCompany);
                    }
                    return currentCompany;
                },
                new { CompanyName = $"%{companyName}%" },
                splitOn: "Id"
            );

            foreach (var company in companies.Values)
            {
                var contacts = await GetContactsByCompanyIdAsync(company.ContactBookId);
                company.Contacts = contacts.ToList();
            }

            return companies.Values;
        }

        private async Task<IEnumerable<Contact>> GetContactsByCompanyIdAsync(int companyId)
        {
            var sql = "SELECT * FROM Contact WHERE ContactBookId = @ContactBookId";
            return await _connection.QueryAsync<Contact>(sql, new { ContactBookId = companyId });
        }
    }

    [Table("Company")]
    public class CompanyDao : ICompany
    {
        [Key]
        public int Id { get; set; }
        public int ContactBookId { get; set; }
        public string Name { get; set; }

        public CompanyDao()
        {
        }

        public CompanyDao(ICompany company)
        {
            Id = company.Id;
            ContactBookId = company.ContactBookId;
            Name = company.Name;
        }

        public ICompany Export() => new Company(Id, ContactBookId, Name);
    }
}
