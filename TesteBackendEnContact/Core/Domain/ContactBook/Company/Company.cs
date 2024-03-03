using System.Collections.Generic;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;

namespace TesteBackendEnContact.Core.Domain.ContactBook.Company
{
    public class Company : ICompany
    {
        public int Id { get; private set; }
        public int ContactBookId { get; private set; }
        public string Name { get; private set; }
        public ContactBook ContactBook { get; set; }
        public IEnumerable<Contact.Contact> Contacts { get; internal set; }

        public Company()
        {
        }


        public Company(int id, int contactBookId, string name)
        {
            Id = id;
            ContactBookId = contactBookId;
            Name = name;
        }
    }
}
