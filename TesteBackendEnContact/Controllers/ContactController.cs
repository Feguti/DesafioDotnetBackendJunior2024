using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Repository;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportContacts(IFormFile file, [FromServices] IContactRepository contactRepository)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Arquivo não fornecido");

                if (!file.FileName.EndsWith(".csv"))
                    return BadRequest("Formato de arquivo inválido. Apenas arquivos CSV são permitidos.");

                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {

                    var contacts = csv.GetRecords<ContactDao>().ToList();

                    foreach (var contact in contacts)
                    {
                        // Caso os contatos não estejam atrelados a uma agenda, eles serão ignorados
                        var existingContactBook = await contactRepository.GetContactBookByIdAsync(contact.ContactBookId);
                        if (existingContactBook == null)
                        {
                            continue;
                        }

                        var existingCompany = await contactRepository.GetCompanyByIdAsync(contact.CompanyId); // Verifica se a empresa existe
                        if (existingCompany == null)
                        {
                            // Se a empresa não existir, define CompanyId como 0 e deixa o contato sem vínculo com empresas
                            contact.CompanyId = 0;
                        }
                        await contactRepository.SaveAsync(contact);
                    }

                    return Ok("Contatos importados com sucesso.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao importar contatos: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchContacts([FromQuery] string query, [FromServices] IContactRepository contactRepository)
        {
            try
            {
                // Executa a pesquisa de contatos com base na consulta fornecida
                var contacts = await contactRepository.SearchAsync(query);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar contatos: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IContact> Post(Contact contact, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.SaveAsync(contact);
        }

        [HttpDelete]
        public async Task Delete(int id, [FromServices] IContactRepository contactRepository)
        {
            await contactRepository.DeleteAsync(id);
        }

        [HttpGet]
        public async Task<IEnumerable<IContact>> Get([FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<IContact> Get(int id, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.GetAsync(id);
        }


    }
}
