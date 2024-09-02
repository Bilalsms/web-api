using my_books.Data.Models;
using my_books.Data.Paging;
using my_books.Data.ViewModels;
using my_books.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace my_books.Data.Services
{
    public class PublishersService
    {
        private readonly AppDbContext dbContext;

        public PublishersService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Publisher AddPublisher(PublisherVM publisher)
        {
            if(StringStartsWithNumber(publisher.Name))
            {
                throw new PublisherNameException("Name starts with number", publisher.Name);
            }

            var _publisher = new Publisher()
            {
                Name = publisher.Name
            };

            dbContext.Publishers.Add(_publisher);
            dbContext.SaveChanges();

            return _publisher;
        }


        public PublisherWithBooksAndAuthorsVM GetPublisherData(int publisherId)
        {
            var publisherData = dbContext.Publishers.Where( x => x.Id == publisherId )
                .Select( n => new PublisherWithBooksAndAuthorsVM()
                {
                    Name = n.Name,
                    BookAuthors = n.Books.Select( n => new BookAuthorVM()
                    {
                        BookName = n.Title,
                        BookAuthors = n.Book_Authors.Select(n => n.Author.FullName).ToList()
                    }).ToList()
                }).FirstOrDefault();

            return publisherData;
        }

        public void DeletePublisherById(int id)
        {
            var publisher = dbContext.Publishers.FirstOrDefault(x => x.Id == id);
            if (publisher != null)
            {
                dbContext.Publishers.Remove(publisher);
                dbContext.SaveChanges();
            }
            else
            {
                throw new Exception($"The publisher with id: {id} does not exist");
            }
        }

        public Publisher GetPublisherById(int id)
        {
            return dbContext.Publishers.FirstOrDefault(n => n.Id == id);
        }



        private bool StringStartsWithNumber(string name)
        {
            return (Regex.IsMatch(name, @"^\d"));
        }

        public List<Publisher> GetAllPublishers(string sortBy, string searchString, int? pageNumber)
        {
            var allPublishers =  dbContext.Publishers.OrderBy(n => n.Name).ToList();

            if(!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "name_desc":
                        allPublishers = allPublishers.OrderByDescending(n => n.Name).ToList();
                        break;

                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                allPublishers = allPublishers.Where(n => n.Name
                    .Contains(searchString, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            }


            // Paging
            int pageSize = 5;
            allPublishers = PaginatedList<Publisher>.Create(allPublishers.AsQueryable(), pageNumber ?? 1, pageSize);

            return allPublishers;
        }
    }
}
