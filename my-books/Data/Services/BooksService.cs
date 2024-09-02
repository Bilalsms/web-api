using my_books.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_books.Data.Models.Services
{
    public class BooksService
    {
        private readonly AppDbContext dbContext;

        public BooksService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddBookWithAuthors(BookVM book)
        {
            var _book = new Book()
            {
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.IsRead ? book.DateRead.Value : null,
                Rate = book.IsRead ? book.Rate.Value : null,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = DateTime.Now,
                PublisherId = book.PublisherId
            };

            dbContext.Books.Add(_book);
            dbContext.SaveChanges();

            foreach (var id in book.AuthorIds)
            {
                var _book_author = new Book_Author()
                {
                    BookId = _book.Id,
                    AuthorId = id
                };
                dbContext.Books_Authors.Add(_book_author);
                dbContext.SaveChanges();
            }
        }

        public List<Book> GetAllBooks()
        {
            return dbContext.Books.ToList();
        }

        public BookWithAuthorsVM GetBookById(int bookId)
        {
            var bookWithAuthors = dbContext.Books.Where(n=> n.Id == bookId).Select( book => new BookWithAuthorsVM()
            {
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.IsRead ? book.DateRead.Value : null,
                Rate = book.IsRead ? book.Rate.Value : null,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                PublisherName = book.Publisher.Name,
                AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).FirstOrDefault();

            return bookWithAuthors;

        }

        public Book UpdateBook(int bookId, Book updatedBook)
        {
            var oldBook = dbContext.Books.FirstOrDefault(x => x.Id == bookId);

            if(oldBook == null)
            {
                return null;
            }
            // Update Book

            /* Business rules => can not modify 
             *                           Id,
             *                           DateAdded 
             * attributes of a book */

            oldBook.Title       = updatedBook.Title;
            oldBook.Description = updatedBook.Description;
            oldBook.IsRead      = updatedBook.IsRead;
            oldBook.DateRead    = updatedBook.IsRead ? updatedBook.DateRead : null;
            oldBook.Rate        = updatedBook.IsRead ? updatedBook.Rate : null;
            oldBook.Genre       = updatedBook.Genre;
            oldBook.CoverUrl    = updatedBook.CoverUrl;

            dbContext.SaveChanges();

            return oldBook;
        }

        public Book DeleteBook(int bookId)
        {
            var book = dbContext.Books.FirstOrDefault(x => x.Id == bookId);

            if (book == null) return null;

            dbContext.Books.Remove(book);
            dbContext.SaveChanges();

            return book;
        }
    }
}
