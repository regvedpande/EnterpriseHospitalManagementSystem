
using CsvHelper;
using CsvHelper.Configuration;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hospital.Services
{
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContactService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedResult<ContactViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = _unitOfWork.Repository<Contact>().GetAll(includeProperties: "Hospital").AsQueryable();
            var totalCount = query.Count();
            var list = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<ContactViewModel>
            {
                Data = list.Select(c => new ContactViewModel(c)).ToList(),
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public ContactViewModel GetContactById(int contactId)
        {
            var model = _unitOfWork.Repository<Contact>().GetById(contactId);
            return model == null ? null : new ContactViewModel(model);
        }

        public void InsertContact(ContactViewModel contact)
        {
            var model = contact.ConvertViewModel();
            _unitOfWork.Repository<Contact>().Add(model);
            _unitOfWork.Save();
        }

        public void UpdateContact(ContactViewModel contact)
        {
            var model = contact.ConvertViewModel();
            _unitOfWork.Repository<Contact>().Update(model);
            _unitOfWork.Save();
        }

        public void DeleteContact(int id)
        {
            var model = _unitOfWork.Repository<Contact>().GetById(id);
            if (model != null)
            {
                _unitOfWork.Repository<Contact>().Delete(model);
                _unitOfWork.Save();
            }
        }

        public byte[] ExportContactsCsv()
        {
            var data = _unitOfWork.Repository<Contact>().GetAll(includeProperties: "Hospital").ToList();
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms, Encoding.UTF8, 1024, true);
            using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
            csv.WriteRecords(data.Select(c => new {
                c.Id,
                c.Email,
                c.Phone,
                Hospital = c.Hospital?.Name
            }));
            sw.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }

        public byte[] ExportContactsPdf()
        {
            var data = _unitOfWork.Repository<Contact>().GetAll(includeProperties: "Hospital").ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.Header().Text("Contacts Report").FontSize(20).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Id");
                            header.Cell().Element(CellStyle).Text("Email");
                            header.Cell().Element(CellStyle).Text("Phone");
                            header.Cell().Element(CellStyle).Text("Hospital");
                        });

                        foreach (var c in data)
                        {
                            table.Cell().Element(CellStyle).Text(c.Id.ToString());
                            table.Cell().Element(CellStyle).Text(c.Email);
                            table.Cell().Element(CellStyle).Text(c.Phone);
                            table.Cell().Element(CellStyle).Text(c.Hospital?.Name ?? "");
                        }

                        static IContainer CellStyle(IContainer c) => c.Padding(5);
                    });
                    page.Footer().AlignCenter().Text(x => x.Span("Generated on ").Append(DateTime.Now.ToString("g")));
                });
            });

            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }
    }
}
