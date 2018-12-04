using System;
using System.Collections.Generic;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using Library.API.Models;
using Library.API.Entities;
using Library.API.Helpers;

namespace Library.API.Controllers
    {
    [Route("api/[controller]")]
    public class AuthorsController : Controller
        {
        private ILibraryRepository libraryRepository;
        private IUrlHelper _urlHelper;


            public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper)
            {
            this.libraryRepository = libraryRepository;
                _urlHelper = urlHelper;
            }
        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {

            var authorsFromRepo = libraryRepository.GetAuthors(authorsResourceParameters);
            var previousLink = authorsFromRepo.HasPrevious
                ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage)
                : null;
            var nextLink = authorsFromRepo.HasNext
                ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage)
                : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousLink,
                nextPageLink = nextLink
            };

            Response.Headers.Add("X-Pagination",Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));


            var authors = AutoMapper.Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            return Ok(authors);
            }

            private string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType pageType)
            {
                switch (pageType)
                {
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber-1,
                            pageSize = authorsResourceParameters.PageSize

                            });
                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize

                        });

                }
            }

            [HttpGet("{id}",Name ="GetAuthor")]
        public IActionResult GetAuthor(Guid id)
            {
            if (!ModelState.IsValid)
                return BadRequest();
            var authorFromRepo = libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
                return NotFound();

                var authorDto = AutoMapper.Mapper.Map<AuthorDto>(authorFromRepo);

                return Ok(CreateLinkForAuthor(authorDto));
            }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
            {
            if (author == null)
                return BadRequest();
           
            var authorToSave = AutoMapper.Mapper.Map<Author>(author);
            libraryRepository.AddAuthor(authorToSave);

            if (libraryRepository.Save()) {
                var authorToReturn = AutoMapper.Mapper.Map<AuthorDto>(authorToSave);
                return CreatedAtRoute("GetAuthor", new {authorToReturn.Id },authorToReturn);
            }
             throw new Exception();
            }

            private AuthorDto CreateLinkForAuthor(AuthorDto author)
            {
                author.Links.Add(new LinkDto(_urlHelper.Link("GetAuthor", new { id = author.Id }),
                    "self", "GET"));
                author.Links.Add(new LinkDto(_urlHelper.Link("DeleteAuthor", new { id = author.Id }),
                    "delete_author", "DELETE"));
                author.Links.Add(new LinkDto(_urlHelper.Link("GetAuthor", new { id = author.Id }),
                    "self", "GET"));

            return author;
            }

        }


    }
