using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactBookController : ControllerBase
    {
        private readonly ILogger<ContactBookController> _logger;

        public ContactBookController(ILogger<ContactBookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IContactBook> Post(ContactBook contactBook, [FromServices] IContactBookRepository contactBookRepository)
        {
            return await contactBookRepository.SaveAsync(contactBook);
        }

        [HttpDelete]
        public async Task Delete(int id, [FromServices] IContactBookRepository contactBookRepository)
        {
            await contactBookRepository.DeleteAsync(id);
        }

        [HttpGet]
        public async Task<IEnumerable<IContactBook>> Get([FromServices] IContactBookRepository contactBookRepository)
        {
            return await contactBookRepository.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<IContactBook> Get(int id, [FromServices] IContactBookRepository contactBookRepository)
        {
            return await contactBookRepository.GetAsync(id);
        }

        [HttpGet("{id}/export")]
        public async Task<IActionResult> ExportContactBook(int id, [FromServices] IContactBookRepository contactBookRepository, [FromServices] IContactRepository contactRepository)
        {
            try
            {
                var contactBook = await contactBookRepository.GetAsync(id);
                if (contactBook == null)
                {
                    return NotFound("Agenda não encontrada.");
                }

                var contacts = await contactRepository.GetContactsByContactBookIdAsync(id);

                var csvData = new StringBuilder();
                csvData.AppendLine("Name,Phone,Email,Address");
                foreach (var contact in contacts)
                {
                    csvData.AppendLine($"{contact.Name},{contact.Phone},{contact.Email},{contact.Address}");
                }

                var byteArray = Encoding.UTF8.GetBytes(csvData.ToString());

                return File(byteArray, "text/csv", "contacts.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao exportar agenda e contatos: {ex.Message}");
            }
        }
    }
}
