using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_books.Data.Models;
using my_books.Data.Services;
using my_books.Data.ViewModels;
using my_books.Data.ViewModels.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Author)]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorsService authorsService;

        public AuthorsController(AuthorsService authorsService)
        {
            this.authorsService = authorsService;
        }

       [HttpPost]
       public IActionResult AddAuthor([FromBody] AuthorVM author)
       {
            var  newAuthor = authorsService.AddAuthor(author);
            return Ok(newAuthor);
       }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetAuthorWithBooks([FromRoute] int id)
        {
            var response = authorsService.GetAuthorWithBooks(id);
            return Ok(response);
        }


    }
}
