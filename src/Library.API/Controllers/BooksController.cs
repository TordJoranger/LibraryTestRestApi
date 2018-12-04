using System;
using System.Collections.Generic;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static AutoMapper.Mapper;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private ILogger<BooksController> _logger;
        private readonly IUrlHelper _urlHelper;

        public BooksController(ILibraryRepository libraryRepository, ILogger<BooksController> logger, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _logger = logger;
            _urlHelper = urlHelper;
        }

        [HttpGet]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();


            var books = Map<IEnumerable<BookDto>>(_libraryRepository.GetBooksForAuthor(authorId));
            return Ok(books);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId) || _libraryRepository.GetBookForAuthor(authorId, id) == null)
                return NotFound();

            var bookDto = Map<BookDto>(_libraryRepository.GetBookForAuthor(authorId, id));
            return Ok(CreateLinksForBook(bookDto));
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
                return BadRequest();

            if(book.Description.Equals(book.Title))
                ModelState.AddModelError("Error title","Title cant be same as description");


            if(!ModelState.IsValid)
                return new UnProcessableEntityObjectResult(ModelState);

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookToAdd = Map<Book>(book);
            _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

            var bookToReturn = Map<BookDto>(bookToAdd);
            if (!_libraryRepository.Save())
                throw new Exception();
            return CreatedAtRoute("GetBookForAuthor",
                new {authorId, bookToReturn.Id}, CreateLinksForBook(bookToReturn));
        }

        [HttpDelete("{id}",Name = "DeleteBookForAuthor")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
                return NotFound();

            _libraryRepository.DeleteBook(bookFromRepo);

            return _libraryRepository.Save() ? Success(authorId,id) : throw new Exception("book couldn't be deleted");
        }

        private NoContentResult Success(Guid authorId, Guid id)
        {
            _logger.LogInformation(100,$"Book {id} for author {authorId} was deleted");
            return NoContent();
        }

        [HttpPut("{id}",Name = "UpdateBookForAuthor")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
                return NotFound();
            Map(book, bookFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookFromRepo);

            return _libraryRepository.Save() ? NoContent() : throw new Exception();
        }

        [HttpPatch("{id}",Name = "PartiallyUpdateBookForAuthor")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            if (patchDocument == null)
                return BadRequest();

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRepo == null)
            {
                var bookForUpdateDto = new BookForUpdateDto();
                patchDocument.ApplyTo(bookForUpdateDto,ModelState);

                TryValidateModel(bookForUpdateDto);
                if (!ModelState.IsValid)
                    return new UnProcessableEntityObjectResult(ModelState);

                var bookToAdd = Map<Book>(bookForUpdateDto);
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId,bookToAdd);

                var bookToReturn = Map<BookDto>(bookToAdd);

                return _libraryRepository.Save() ? 
                    CreatedAtRoute("GetBookForAuthor", new {authorId, id}, bookToReturn) :
                    throw new Exception();
            }


            var bookToUpdate = Map<BookForUpdateDto>(bookFromRepo);
            patchDocument.ApplyTo(bookToUpdate,ModelState);
            TryValidateModel(bookToUpdate);
            if(!ModelState.IsValid)
                return new UnProcessableEntityObjectResult(ModelState);

            Map(bookToUpdate, bookFromRepo);

            return _libraryRepository.Save() ? NoContent() : throw new Exception();
        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            book.Links.Add(new LinkDto(_urlHelper.Link("GetBookForAuthor",new {id = book.Id}),
                "self","GET"));
            book.Links.Add(new LinkDto(_urlHelper.Link("DeleteBookForAuthor", new { id = book.Id }), 
                "delete_book", "DELETE"));
            book.Links.Add(new LinkDto(_urlHelper.Link("UpdateBookForAuthor", new { id = book.Id }),
                "update_book", "PUT"));
            book.Links.Add(new LinkDto(_urlHelper.Link("PartiallyUpdateBookForAuthor", new { id = book.Id }), 
                "partially_update_book", "PATCH"));
            return book;
        }
    }
}