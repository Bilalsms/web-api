using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using my_books.ActionResults;
using my_books.Data.Models;
using my_books.Data.Services;
using my_books.Data.ViewModels;
using my_books.Data.ViewModels.Authentication;
using my_books.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Publisher + "," + UserRoles.Admin)]
    public class PublishersController : ControllerBase
    {
        private readonly PublishersService publishersService;
        private readonly ILogger<PublishersController> logger;

        public PublishersController(PublishersService publishersService, ILogger<PublishersController> logger)
        {
            this.publishersService = publishersService;
            this.logger = logger;
        }

       [HttpPost("add-publisher")]
       public IActionResult AddPublisher([FromBody] PublisherVM publisher)
       {
            try
            {
                var newPublisher = publishersService.AddPublisher(publisher);
                return Created(nameof(AddPublisher), newPublisher);
            }
            catch(PublisherNameException ex)
            {
                return BadRequest($"{ex.Message}, Publisher name: {ex.PublisherName}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
       }

        [HttpGet("get-publisher-books-with-authors/{id}")]
        //[Route("{id:int}")]
        public IActionResult GetPublisherBooksWithAuthors([FromRoute] int id)
        {
            var response = publishersService.GetPublisherData(id);
            return Ok(response);
        }


        [HttpGet("get-publisher-by-id/{id:int}")]
        //[Route("{id:int}")]
        public IActionResult GetPublisherById([FromRoute] int id)
        {
            var response = publishersService.GetPublisherById(id);

            if (response != null)
            {
                return Ok(response); 
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeletePublisherById([FromRoute] int id)
        {
            try
            {
                publishersService.DeletePublisherById(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-all-publishers")]
        public IActionResult GetAllPublishers(string sortBy, string searchString, int pageNumber)
        {
            try
            {
                logger.LogInformation("This is just a log in GetAllPublishers");
                var _publisers = publishersService.GetAllPublishers(sortBy, searchString, pageNumber);
                return Ok(_publisers);
            }
            catch (Exception ex)
            {
                return BadRequest("Sorry, we could not load the publishers" + ex.Message);
            }
        }


    }
}
