using my_books.Data.Models;
using my_books.Data.ViewModels;
using System.Linq;

namespace my_books.Data.Services
{
    public class AuthorsService
    {
        private readonly AppDbContext dbContext;

        public AuthorsService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Author AddAuthor(AuthorVM author)
        {
            var _author = new Author()
            {
                FullName = author.FullName
            };

            dbContext.Authors.Add(_author);
            dbContext.SaveChanges();

            return _author;
        }

        public AuthorWithBooksVM GetAuthorWithBooks(int authorId)
        {
            var _author = dbContext.Authors.Where(n => n.Id == authorId).Select(n => new AuthorWithBooksVM()
            {
                FullName = n.FullName,
                BookTitles = n.Book_Authors.Select( n => n.Book.Title).ToList()
            }).FirstOrDefault();

            return _author;
        }
    }
}
