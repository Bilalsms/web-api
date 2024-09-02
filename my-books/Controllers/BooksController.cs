using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_books.Data.Models;
using my_books.Data.Models.Services;
using my_books.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BooksService booksService;

        public BooksController(BooksService booksService)
        {
            this.booksService = booksService;
        }

        [HttpPost]
        public IActionResult AddNewBook([FromBody] BookVM book)
        {
            booksService.AddBookWithAuthors(book);
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAllBooks()
        {
            var books = booksService.GetAllBooks();
            if (books == null)
            {
                return NotFound("No books found");
            }
            return Ok(books);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            var book = booksService.GetBookById(id);
            if(book == null)
                return NotFound($"Book with Id {id} does not exist");
            return Ok(book);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateBookById([FromRoute]int id, BookVM bookVm)
        {
            // convert to Book
            var book = new Book()
            {
                Id = id,
                Title = bookVm.Title,
                Description = bookVm.Description,
                IsRead = bookVm.IsRead,
                DateRead = bookVm.IsRead ? bookVm.DateRead.Value : null,
                Rate = bookVm.IsRead ? bookVm.Rate.Value : null,
                Genre = bookVm.Genre,
                CoverUrl = bookVm.CoverUrl,
            };

            // call to db
            var updatedBook = booksService.UpdateBook(id, book);

            // convert back to bookVm
            // Not now 

            // return 
            if (updatedBook == null) return NotFound(null);
            return Ok(updatedBook);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteBookById([FromRoute] int id)
        {
            var deletedBook = booksService.DeleteBook(id);

            if (deletedBook == null) return NotFound(deletedBook);
            return Ok(deletedBook);
        }
    }
}
