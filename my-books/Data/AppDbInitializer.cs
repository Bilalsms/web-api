using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using my_books.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_books.Data
{
    public class AppDbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AppDbContext>();

                if(!context.Books.Any())
                {
                    var books = new List<Book>()
                    {
                        new Book()
                        {
                            Title = "First Book Title",
                            Description = "First Book Description",
                            IsRead = true,
                            DateRead = DateTime.Now.AddDays(-10),
                            Rate = 4,
                            Genre = "First Book Genre",
                            CoverUrl = "First Book CoverUrl",
                            DateAdded = DateTime.Now.AddDays(-30)
                        },
                        new Book()
                        {
                            Title = "Second Book Title",
                            Description = "Second Book Description",
                            IsRead = false,
                            Genre = "Second Book Genre",
                            CoverUrl = "Second Book CoverUrl",
                            DateAdded = DateTime.Now.AddDays(-20)
                        },
                    };

                    context.Books.AddRange(books);
                    context.SaveChanges();
                }    
            }
        }
    }
}
