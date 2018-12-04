using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
    {
    [Route("api/authorcollection")]
    public class AuthorCollectionController : Controller
        {
        private ILibraryRepository libraryRepository;

        public AuthorCollectionController(ILibraryRepository libraryRepository)
            {
            this.libraryRepository = libraryRepository;
            }

        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
            {
            if (authorCollection == null)
                return BadRequest();

            var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (var authorEntity in authorEntities)
                libraryRepository.AddAuthor(authorEntity);

                var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
                var idsAsString = string.Join(",", authorCollectionToReturn.Select(a => a.Id));

            return libraryRepository.Save()
                ? CreatedAtRoute("GetAuthorCollection", new {ids = idsAsString},authorCollectionToReturn) 
                : throw new Exception();
            }

        [HttpGet("(ids)",Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
            {
            if(ids == null)
                return BadRequest();

                var authorEntities = libraryRepository.GetAuthors(ids);
                if (ids.Count() != authorEntities.Count())
                    return NotFound();

                var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
                return Ok(authorsToReturn);

            }
            
        }
    }
