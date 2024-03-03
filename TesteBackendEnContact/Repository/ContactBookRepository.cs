using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactBookRepository : IContactBookRepository
    {
        private readonly IDbConnection _connection;  //Agora não é mais necessário abrir uma conexão dentro de cada operação

        public ContactBookRepository(IDbConnection connection)
        {
            _connection = connection;
        }


        public async Task<IContactBook> SaveAsync(IContactBook contactBook)
        {
            var dao = new ContactBookDao(contactBook);

            dao.Id = await _connection.InsertAsync(dao);

            return dao.Export();
        }


        public async Task DeleteAsync(int id)
        {
            // TODO - DONE

            var sql = new StringBuilder();
            sql.AppendLine("DELETE FROM ContactBook WHERE Id = @id;");
            sql.AppendLine("UPDATE Company SET ContactBookId = null WHERE ContactBookId = @id;");

            await _connection.ExecuteAsync(sql.ToString(), new { id });
        }


        public async Task<IEnumerable<IContactBook>> GetAllAsync()
        {
            var query = "SELECT * FROM ContactBook";
            var result = await _connection.QueryAsync<ContactBookDao>(query);

            var returnList = new List<IContactBook>();

            foreach (var AgendaSalva in result.ToList())
            {
                IContactBook Agenda = new ContactBook(AgendaSalva.Id, AgendaSalva.Name.ToString());
                returnList.Add(Agenda);
            }

            return returnList.ToList();
        }

        public async Task<IContactBook> GetAsync(int id)
        {
            var list = await GetAllAsync();

            return list.ToList().Where(item => item.Id == id).FirstOrDefault();
        }
    }

    [Table("ContactBook")]
    public class ContactBookDao : IContactBook
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public ContactBookDao()
        {
        }

        public ContactBookDao(IContactBook contactBook)
        {
            Id = contactBook.Id;
            Name = contactBook.Name; //Erro de digitação corrigido
        }

        public IContactBook Export() => new ContactBook(Id, Name);
    }
}
